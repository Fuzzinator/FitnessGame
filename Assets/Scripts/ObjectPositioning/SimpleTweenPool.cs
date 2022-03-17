using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace SimpleTweens
{
    [Serializable]
    public class SimpleTweenPool
    {
        public List<SimpleTween> pooledTweens;
        public List<SimpleTween> activeTweens;

        private CancellationToken _cancellationToken;

        private TransformAccessArray _objectsToTween;

        public SimpleTweenPool(int initialSize, CancellationToken token)
        {
            _cancellationToken = token;

            pooledTweens = new List<SimpleTween>(initialSize);
            activeTweens = new List<SimpleTween>(initialSize);

            for (var i = 0; i < initialSize; i++)
            {
                var tween = CreateNewTween(new SimpleTween.Data());
                ReturnToPool(tween);
            }
        }

        public SimpleTween GetNewTween(SimpleTween.Data data)
        {
            if (pooledTweens.Count > 0)
            {
                var tween = pooledTweens[0];
                pooledTweens.Remove(tween);
                tween.data = data;
                tween._isPooled = false;
                activeTweens.Add(tween);
                return tween;
            }
            else
            {
                return CreateNewTween(data);
            }
        }

        public void CompleteTween(SimpleTween tween)
        {
            tween.Complete();
            ReturnToPool(tween);
        }

        private void ReturnToPool(SimpleTween tween)
        {
            tween.data = new SimpleTween.Data();

            activeTweens.Remove(tween);
            if (pooledTweens.Contains(tween))
            {
                return;
            }
            
            pooledTweens.Add(tween);
            tween._isPooled = true;
        }

        public void CompleteAllActive()
        {
            while (activeTweens.Count > 0)
            {
                var tween = activeTweens[0];
                tween.Complete();
            }
        }

        public void Destroy()
        {
            while (activeTweens.Count > 0)
            {
                var tween = activeTweens[0];
                tween.Cancel();
                activeTweens.Remove(tween);
            }

            while (pooledTweens.Count > 0)
            {
                var tween = pooledTweens[0];
                tween.Cancel();
                pooledTweens.Remove(tween);
            }
        }

        private SimpleTween CreateNewTween(SimpleTween.Data data)
        {
            var tween = new SimpleTween(data, _cancellationToken);
            tween.OnReturn += () => ReturnToPool(tween);
            tween.StartTweener();
            tween._isPooled = false;
            activeTweens.Add(tween);
            return tween;
        }
    }
}