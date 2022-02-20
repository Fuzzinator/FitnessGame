using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleTweens
{
    public class SimpleTween
    {
        internal Data data;
        internal delegate void OnReturnDelegate();
        internal event OnReturnDelegate OnReturn;
        
        private CancellationToken _destroyedCancellationToken;
        private CancellationTokenSource _internalCancelTokenSource;

        public SimpleTween(Data data, CancellationToken token)
        {
            this.data = data;
            _destroyedCancellationToken = token;
            _internalCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_destroyedCancellationToken);
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
                await DoTween();
                if (_internalCancelTokenSource.Token.IsCancellationRequested)
                {
                    InternalCancel();
                    return;
                }

                data.OnComplete?.Invoke();
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                InternalCancel();
            }
            OnReturn?.Invoke();
        }

        public void Complete()
        {
            data.OnComplete?.Invoke();
            Cancel();
        }
        
        public void Cancel()
        {
            _internalCancelTokenSource?.Cancel();
        }

        private void InternalCancel()
        {
            OnReturn?.Invoke();
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