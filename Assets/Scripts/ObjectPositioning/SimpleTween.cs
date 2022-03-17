using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleTweens
{
    [Serializable]
    public class SimpleTween
    {
        internal Data data;
        internal delegate void OnReturnDelegate();
        internal event OnReturnDelegate OnReturn;

        internal bool _isPooled = false;
        
        internal bool _active = false;

        internal float _delayTime;
        
        private CancellationToken _destroyedCancellationToken;
        private CancellationTokenSource _internalCancelTokenSource;
        public SimpleTween(Data data, CancellationToken token)
        {
            this.data = data;
            _destroyedCancellationToken = token;
            _internalCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_destroyedCancellationToken);
        }

        internal void StartTweener()
        {
            DelayTweenStartAsync().Forget();
        }

        public void DelayTweenStart(float delay)
        {
            _delayTime = delay;
            _active = true;
        }
        
        private async UniTask DelayTweenStartAsync()
        {
            while (!_internalCancelTokenSource.IsCancellationRequested)
            {
                while (!_active)
                {
                    if (_internalCancelTokenSource.IsCancellationRequested)
                    {
                        InternalCancel();
                        break;
                    }
                    try
                    {
                        await UniTask.DelayFrame(1, cancellationToken: _internalCancelTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        if (e is not OperationCanceledException)
                        {
                            Debug.LogError(e);
                        }
                        InternalCancel();
                        break;
                    }
                }
                
                if (_internalCancelTokenSource.IsCancellationRequested)
                {
                    InternalCancel();
                    continue;
                }
                
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_delayTime),
                        cancellationToken: _internalCancelTokenSource.Token);
                    if (_internalCancelTokenSource.Token.IsCancellationRequested)
                    {
                        InternalCancel();
                        continue;
                    }

                    data.OnStart?.Invoke();
                    
                    //This is the DoTween Method moved here because it reduces garbage?
                    while (!_internalCancelTokenSource.Token.IsCancellationRequested &&
                           Vector3.Distance(data.MyTransform.position, data.EndPosition) > .01f)
                    {
                        var time = Time.time;
                        await UniTask.DelayFrame(1, cancellationToken: _internalCancelTokenSource.Token);
                        var step = data.Speed * (Time.time - time);
                        data.MyTransform.position = Vector3.MoveTowards(data.MyTransform.position, data.EndPosition, step);
                    }
                    
                    //await DoTween();
                    if (_internalCancelTokenSource.Token.IsCancellationRequested)
                    {
                        InternalCancel();
                        continue;
                    }

                    data.OnComplete?.Invoke();
                }
                catch (Exception e)
                {
                    if (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                    InternalCancel();
                    continue;
                }

                _active = false;
                if (!_isPooled)
                {
                    OnReturn?.Invoke();
                }
            }
        }

        public void Complete()
        {
            data.OnComplete?.Invoke();
            Cancel();
        }
        
        public void Cancel()
        {
            _active = false;
            _internalCancelTokenSource?.Cancel();
            
            if (!_isPooled)
            {
                OnReturn?.Invoke();
            }
        }

        private void InternalCancel()
        {
            _active = false;
            if (!_isPooled)
            {
                OnReturn?.Invoke();
            }

            if (_destroyedCancellationToken.IsCancellationRequested)
            {
                OnReturn = null;
                return;
            }
            _internalCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_destroyedCancellationToken);
        }

        private async UniTask DoTween()
        {
            while (!_internalCancelTokenSource.Token.IsCancellationRequested &&
                   Vector3.Distance(data.MyTransform.position, data.EndPosition) > .01f)
            {
                var time = Time.time;
                await UniTask.DelayFrame(1, cancellationToken: _internalCancelTokenSource.Token);
                var step = data.Speed * (Time.time - time);
                data.MyTransform.position = Vector3.MoveTowards(data.MyTransform.position, data.EndPosition, step);
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