using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectScroller : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    
    private bool _scroll = false;
    
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

    private async UniTask AsyncScroll(float value)
    {
        var rectHeight = _scrollRect.content.rect.height;
        value = (rectHeight * value) / rectHeight;
        
        while (_scroll)
        {
            await UniTask.DelayFrame(1);
            var position = Mathf.Clamp(_scrollRect.verticalNormalizedPosition - value, 0,1);
            _scrollRect.verticalNormalizedPosition = position;
        }
    }
}
