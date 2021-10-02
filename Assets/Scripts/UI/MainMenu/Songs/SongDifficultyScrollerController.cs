using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class SongDifficultyScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private EnhancedScrollerCellView _cellViewPrefab;

    [SerializeField]
    private float _cellViewSize;

    private SongInfo _songInfo;
    // Start is called before the first frame update
    void Start()
    {
        _scroller.Delegate = this;
    }

    public void UpdateDifficultyOptions(SongInfo info)
    {
        _songInfo = info;
        _scroller.ReloadData();
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        throw new System.NotImplementedException();
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        throw new System.NotImplementedException();
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        throw new System.NotImplementedException();
    }
}
