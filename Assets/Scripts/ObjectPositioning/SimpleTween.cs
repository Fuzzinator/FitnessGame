using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace SimpleTweens
{
    public class SimpleTween
    {
        internal Data data;

        private readonly SimpleTweenPool _pool;
        private CancellationTokenSource _internalCancelTokenSource;

        public SimpleTween(SimpleTweenPool pool, Data data, CancellationTokenSource tokenSource)
        {
            _pool = pool;
            this.data = data;
            
            _internalCancelTokenSource = tokenSource;
        }
        
        public async UniTask DelayTweenStart(float delayTime)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime),
                    cancellationToken: _internalCancelTokenSource.Token);
                if (_internalCancelTokenSource.Token.IsCancellationRequested)
                {
                    InternalCancel();
                    return;
                }

                data.OnStart?.Invoke();
                
                _pool._datasToAdd.Add(data);
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                InternalCancel();
            }
        }  
        
        public void Complete()
        {
            data.OnComplete?.Invoke();
        }

        public void ForceComplete()
        {
            data.OnComplete?.Invoke();
            Cancel();
        }
        
        public void Cancel()
        {
            _internalCancelTokenSource?.Cancel();
        }

        internal void InternalCancel()
        {
            _pool.CompleteTween(this);
        }

        private async UniTask DoTween()
        {
            DoTweenJob job;
            JobHandle handle = new JobHandle();
            TransformAccessArray access = new TransformAccessArray();
            while (!_internalCancelTokenSource.Token.IsCancellationRequested &&
                   Vector3.Distance(data.MyTransform.position, data.EndPosition) > .01f)
            {
                handle.Complete();
                access.RemoveAtSwapBack(0);
                access.Add(data.MyTransform);
                var time = Time.time;
                await UniTask.DelayFrame(1, cancellationToken: _internalCancelTokenSource.Token);
                job = new DoTweenJob(Time.time - time, data.Speed, data.EndPosition);
                handle = job.Schedule(access);
            }
        }

        private struct DoTweenJob : IJobParallelForTransform
        {
            private float _deltaTime;
            private float _speed;
            private Vector3 _endPosition;
            public DoTweenJob(float deltaTime, float speed, Vector3 endPosition)
            {
                _deltaTime = deltaTime;
                _speed = speed;
                _endPosition = endPosition;
            }
            public void Execute(int index, TransformAccess transform)
            {
                var step = _speed * (_deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, _endPosition, step);
                transform = new TransformAccess();
            }
        }

        [Serializable]
        public struct Data
        {
            public Transform MyTransform { get; }
            public Action OnStart { get; }
            public Action OnComplete { get; }
            public Vector3 EndPosition { get; }
            public float Speed { get; }

            public Data(Transform t, Action start, Action complete, Vector3 endPos, float speed)
            {
                MyTransform = t;
                OnStart = start;
                OnComplete = complete;
                EndPosition = endPos;
                Speed = speed;
            }
        }

        
    }
}