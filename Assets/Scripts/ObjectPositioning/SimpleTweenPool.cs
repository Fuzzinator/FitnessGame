using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace SimpleTweens
{
    public class SimpleTweenPool
    {
        public List<SimpleTween> pooledTweens;
        public List<SimpleTween> activeTweens;

        private CancellationTokenSource _cancellationTokenSource;

        private NativeList<UnManagedTweenData> _tweenDatas;
        internal List<SimpleTween.Data> _datasToAdd;
        private TransformAccessArray _objectsToTween;

        private Dictionary<Transform, int> _activeDataIndex = new Dictionary<Transform, int>();

        public SimpleTweenPool(int initialSize, CancellationToken token)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            _datasToAdd = new List<SimpleTween.Data>(initialSize);
            pooledTweens = new List<SimpleTween>(initialSize);
            activeTweens = new List<SimpleTween>(initialSize);

            _tweenDatas = new NativeList<UnManagedTweenData>(initialSize, Allocator.Persistent);
            _objectsToTween = new TransformAccessArray(initialSize);

            for (var i = 0; i < initialSize; i++)
            {
                var tween = CreateNewTween(new SimpleTween.Data());
                ReturnToPool(tween);
            }

            DoTweens().Forget();
        }

        public SimpleTween GetNewTween(SimpleTween.Data data)
        {
            if (pooledTweens.Count > 0)
            {
                var tween = pooledTweens[0];
                pooledTweens.Remove(tween);
                tween.data = data;
                activeTweens.Add(tween);
                return tween;
            }
            else
            {
                return CreateNewTween(data);
            }
        }

        public async UniTask DoTweens()
        {
            DoTweenJob job;
            JobHandle handle = new JobHandle();
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                handle.Complete();
                var time = Time.time;
                if (activeTweens.Count > 0)
                {
                    //var completed = new NativeArray<bool>(_objectsToTween.length, Allocator.TempJob);
                    /*try
                    {
                        var checkCompleted = new CheckCompletedJob(_tweenDatas, completed);
                        await checkCompleted.Schedule(_objectsToTween);
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            foreach (var tween in activeTweens)
                            {
                                tween.InternalCancel();
                            }
                        }

                        for (var i = 0; i < _objectsToTween.Length; i++)
                        {
                            if (!completed[i])
                            {
                                continue;
                            }

                            CompleteTween(activeTweens[i]);
                            break;
                        }

                        completed.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        completed.Dispose();
                    }*/
                    foreach (var tween in activeTweens)
                    {
                        var transform = tween.data.MyTransform;
                        if (!_activeDataIndex.ContainsKey(transform))
                        {
                            continue;
                        }
                        
                        var shouldComplete = Vector3.Distance(transform.position, tween.data.EndPosition) < .01f;
                        if (tween.data.EndPosition !=
                            _tweenDatas[_activeDataIndex[transform]].EndPosition)
                        {
                            Debug.LogError("This is what is breaking?");
                        }
                        
                        if (shouldComplete)
                        {
                            CompleteTween(tween);
                            break;
                        }
                    }

                    foreach (var fullData in _datasToAdd)
                    {
                        var data = new UnManagedTweenData(fullData.Speed, fullData.EndPosition);
                        _activeDataIndex[fullData.MyTransform] = _tweenDatas.Length;
                        _tweenDatas.Add(data);
                        _objectsToTween.Add(fullData.MyTransform);
                    }

                    _datasToAdd.Clear();
                }

                try
                {
                    await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
                }
                catch (Exception e) when (e is OperationCanceledException)
                {
                    foreach (var tween in activeTweens)
                    {
                        tween.InternalCancel();
                    }
                }

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                job = new DoTweenJob(Time.time - time, _tweenDatas);
                handle = job.Schedule(_objectsToTween);
            }
        }

        #region Jobs

        private struct UnManagedTweenData
        {
            public float Speed { get; }
            public Vector3 EndPosition { get; }

            public UnManagedTweenData(float speed, Vector3 endPosition)
            {
                Speed = speed;
                EndPosition = endPosition;
            }
        }

        private struct CheckCompletedJob : IJobParallelForTransform
        {
            private readonly NativeList<UnManagedTweenData> _datas;
            private NativeArray<bool> _completed;

            public CheckCompletedJob(NativeList<UnManagedTweenData> datas, NativeArray<bool> completed)
            {
                _datas = datas;
                _completed = completed;
            }

            public void Execute(int index, TransformAccess transform)
            {
                _completed[index] = Vector3.Distance(transform.position, _datas[index].EndPosition) < .01f;
            }
        }

        private struct DoTweenJob : IJobParallelForTransform
        {
            private readonly float _deltaTime;
            private readonly NativeList<UnManagedTweenData> _datas;

            public DoTweenJob(float deltaTime, NativeList<UnManagedTweenData> datas)
            {
                _deltaTime = deltaTime;
                _datas = datas;
            }

            public void Execute(int index, TransformAccess transform)
            {
                var step = _datas[index].Speed * (_deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, _datas[index].EndPosition, step);
            }
        }

        #endregion

        internal void CompleteTween(SimpleTween tween)
        {
            tween.Complete();
            if (tween.data.MyTransform != null)
            {
                var index = _activeDataIndex[tween.data.MyTransform];
                _activeDataIndex.Remove(tween.data.MyTransform);

                if (index < _tweenDatas.Length)
                {
                    _tweenDatas.RemoveAtSwapBack(index);
                }

                if (index < _objectsToTween.length)
                {
                    _objectsToTween.RemoveAtSwapBack(index);
                    if (index < _objectsToTween.length)
                    {
                        _activeDataIndex[_objectsToTween[index]] = index;
                    }
                }
            }

            ReturnToPool(tween);
        }

        internal void ReturnToPool(SimpleTween tween)
        {
            activeTweens.Remove(tween);
            pooledTweens.Add(tween);
        }

        public void CompleteAllActive()
        {
            while (activeTweens.Count > 0)
            {
                var tween = activeTweens[0];
                tween.ForceComplete();
                activeTweens.Remove(tween);
            }
        }

        public void Destroy()
        {
            _cancellationTokenSource.Cancel();
            _tweenDatas.Dispose();
            _objectsToTween.Dispose();
        }

        private SimpleTween CreateNewTween(SimpleTween.Data data)
        {
            var tween = new SimpleTween(this, data, _cancellationTokenSource);
            //tween.OnReturn += () => ReturnToPool(tween);
            activeTweens.Add(tween);
            return tween;
        }
    }
}