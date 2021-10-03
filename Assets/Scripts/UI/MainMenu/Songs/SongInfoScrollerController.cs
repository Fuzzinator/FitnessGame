using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class SongInfoScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private EnhancedScrollerCellView _cellViewPrefab;

    [SerializeField]
    private float _cellViewSize;

    [SerializeField]
    private DisplaySongInfo _displaySongInfo;

    private void Start()
    {
        _scroller.Delegate = this;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return SongInfoFilesReader.Instance.availableSongs.Count;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return _cellViewSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(_cellViewPrefab) as SongInfoCellView;
        cellView.SetData(SongInfoFilesReader.Instance.availableSongs[dataIndex], this);
        return cellView;
    }

    public void SetActiveInfo(SongInfo info)
    {
        _displaySongInfo.UpdateDisplayedInfo(info);
        PlaylistMaker.Instance.SetActiveItem(info);
    }
}
