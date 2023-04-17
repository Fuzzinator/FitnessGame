using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WatchDisplay : MonoBehaviour
{
    public GameObject container;

    private AnimationChecker _animationChecker;

    public Action OnShow;
    public Action OnHide;
    public Action OnFinish;

    private WaitForSecondsRealtime _waitForAnimation;

    private void Awake()
    {
        _animationChecker = GetComponent<AnimationChecker>();
        _waitForAnimation = new WaitForSecondsRealtime(0.01f);
    }

    public Coroutine Show()
    {
        return Show(true);
    }

    public Coroutine Hide()
    {
        return Hide(true);
    }

    public virtual Coroutine Show(bool useAnimation)
    {
        if (_animationChecker.animator != null && useAnimation)
        {
            container.SetActive(true);
            _animationChecker.animator.SetTrigger("Show");
            _animationChecker.animator.ResetTrigger("Hide");
        }
        else
        {
            container.SetActive(true);
        }
        OnShow?.Invoke();
        return StartCoroutine(WaitAnimation());
    }

    public virtual Coroutine Hide(bool useAnimation)
    {
        if (_animationChecker.animator != null && useAnimation)
        {
            _animationChecker.animator.SetTrigger("Hide");
            _animationChecker.animator.ResetTrigger("Show");
        }
        OnHide?.Invoke();
        return StartCoroutine(WaitAnimation());
    }


    private IEnumerator WaitAnimation()
    {
        yield return _waitForAnimation;

        while (_animationChecker.AnimatorIsPlaying())
        {
            yield return null;
        }
        OnFinish?.Invoke();
    }
}
