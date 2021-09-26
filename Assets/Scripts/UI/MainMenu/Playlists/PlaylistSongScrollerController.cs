using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class PlaylistSongScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private EnhancedScrollerCellView _cellViewPrefab;

    [SerializeField]
    private float _cellViewSize = 100f;

    private void Start()
    {
        _scroller.Delegate = this;
        //_scroller.ReloadData();
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return PlaylistManager.Instance.CurrentPlaylist.Items.Length;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return _cellViewSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(_cellViewPrefab) as PlaylistSongCellView;
        cellView.SetData(PlaylistManager.Instance.CurrentPlaylist.Items[dataIndex]);
        return cellView;
    }

    public void ReloadScroller()
    {
        _scroller.ReloadData();
    }
}
