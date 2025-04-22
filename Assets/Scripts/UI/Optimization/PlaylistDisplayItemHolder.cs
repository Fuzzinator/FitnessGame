using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaylistDisplayItemHolder : MonoBehaviour
{
    [SerializeField]
    private RectTransform _displayRoot;
    [SerializeField]
    private RectTransform _imagesHolder;
    [SerializeField]
    private RectTransform _playButtonHolder;
    [SerializeField]
    private RectTransform _playButtonImageHolder;
    [SerializeField]
    private RectTransform _textHolder;

    public RectTransform DisplayRoot => _displayRoot;
    public RectTransform ImagesHolder => _imagesHolder;
    public RectTransform PlayButtonHolder => _playButtonHolder;
    public RectTransform PlayButtonImageHolder => _playButtonImageHolder;
    public RectTransform TextHolder => _textHolder;

    public void MatchPosition()
    {
        _imagesHolder.position = _displayRoot.position;
        _playButtonHolder.position = _displayRoot.position;
        _playButtonHolder.position = _displayRoot.position;
        _playButtonImageHolder.position = _displayRoot.position;
        _textHolder.position = _displayRoot.position;
    }
}
