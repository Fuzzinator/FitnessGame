using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectScroller : MonoBehaviour
{
    [SerializeField] 
    private ScrollRect _scrollRect;

    [SerializeField]
    private EnhancedScroller _scroller;
    
    private bool _scroll = false;

    private void OnValidate()
    {
        if (_scrollRect != null && _scroller == null)
        {
            _scrollRect.TryGetComponent(out _scrollRect);
        }
    }

    public async void Scroll(float value)
    {
        if (_scroll)
        {
            return;
        }
        _scroll = true;
        await AsyncScroll(value);
    }

    public void StopScroll()
    {
        _scroll = false;
    }

    private void OnDestroy()
    {
        _scroll = false;
    }

    private async UniTask AsyncScroll(float value)
    {
        if (_scroller == null)
        {
            /*var rectHeight = _scrollRect.content.rect.height;
            value = (rectHeight * value) / rectHeight;*/
        }
        else
        {
            value /= _scroller.NumberOfCells;
        }
        while (_scroll)
        {
            await UniTask.DelayFrame(1);
            var position = Mathf.Clamp(_scrollRect.verticalNormalizedPosition - value, 0,1);
            _scrollRect.verticalNormalizedPosition = position;
        }
    }
}
