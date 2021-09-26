using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class ScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
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
        _scroller.ReloadData();
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return PlaylistFilesReader.Instance.availablePlaylists.Count;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return _cellViewSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(_cellViewPrefab) as PlaylistCellView;
        cellView.SetData(PlaylistFilesReader.Instance.availablePlaylists[dataIndex]);
        return cellView;
    }
}
