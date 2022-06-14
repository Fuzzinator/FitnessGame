using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private float _transitionSpeed = 1;
    [SerializeField]
    private TransitionData[] _transitionDatas;

    [SerializeField]
    private UnityEvent _transitionStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent _transitionCompleted = new UnityEvent();

    [SerializeField]
    private GameState _resumedState;

    [SerializeField, ReadOnly]
    private float _longestClipTime;
    private int _propertyID;
    private CancellationToken _cancellationToken;
    private Func<bool> _reset;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void OnValidate()
    {
        foreach (var clip in _transitionDatas)
        {
            var length = clip.Length;
            if (length > _longestClipTime)
            {
                _longestClipTime = length;
            }
        }
    }

    public void RequestTransition()
    {
        GameStateManager.Instance.SetState(_resumedState);
        RunTransition().Forget();
    }

    private async UniTaskVoid RunTransition()
    {
        await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        //var startingValue = _sourceMaterial.GetFloat(_propertyID);
        _transitionStarted?.Invoke();
        for (var f = 0f; f < _longestClipTime; f += Time.deltaTime * _transitionSpeed)
        {
            foreach (var data in _transitionDatas)
            {
                data.Clip.SampleAnimation(data.GameObj, f);
            }
            /*_sourceMaterial.SetFloat(_propertyID,
                Mathf.Lerp(startingValue, _targetValue, _transitionCurve.Evaluate(f)));*/
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }

        _transitionCompleted?.Invoke();
    }

    [Serializable]
    private struct TransitionData
    {
        [SerializeField]
        private GameObject _gameObject;

        [SerializeField]
        private AnimationClip _animationClip;

        public GameObject GameObj => _gameObject;
        public AnimationClip Clip => _animationClip;
        public float Length => _animationClip != null ? _animationClip.length : 0;
    }
    /*    
        [SerializeField]
        public Material _sourceMaterial;

        [SerializeField]
        public string _propertyName;

        [SerializeField]
        public float _transitionSpeed;
        [SerializeField]
        public float _targetValue;
        [SerializeField]
        public float _defaultValue;

        [SerializeField]
        public AnimationCurve _transitionCurve;
    
        [SerializeField]
        public UnityEvent _transitionStarted;
        [SerializeField]
        public UnityEvent _transitionCompleted;
    }*/
}