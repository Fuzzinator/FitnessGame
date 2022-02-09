using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FollowRotation : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransform;

    private Quaternion _previousQuaternion;

    private Tween _currentTween;

    private CancellationTokenSource _tokenSource;

    private void OnEnable()
    {
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        FollowTargetRotation().Forget();
    }

    private async UniTaskVoid FollowTargetRotation()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            _previousQuaternion = _targetTransform.rotation;
            await UniTask.Delay(TimeSpan.FromSeconds(1.5));
            if (_tokenSource.IsCancellationRequested)
            {
                return;
            }
            if (_previousQuaternion != _targetTransform.rotation)
            {
                if (_currentTween != null && _currentTween.IsActive())
                {
                    _currentTween.Kill();
                }

                _currentTween = transform.DORotateQuaternion(_targetTransform.rotation, 1.25f);
            }
        }
    }
}