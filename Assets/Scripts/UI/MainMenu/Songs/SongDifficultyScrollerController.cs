using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UIElements;

public class SongDifficultyScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private EnhancedScrollerCellView _cellViewPrefab;

    [SerializeField]
    private float _cellViewSize;

    private SongInfo.DifficultySet _difficultyInfo;

    // Start is called before the first frame update
    void Start()
    {
        _scroller.Delegate = this;
    }

    public void UpdateDifficultyOptions(SongInfo.DifficultySet info)
    {
        _difficultyInfo = info;
        _scroller.ReloadData();
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _difficultyInfo.DifficultyInfos?.Length ?? 0;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return _cellViewSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(_cellViewPrefab) as SongDifficultyCellView;
        cellView.SetData(_difficultyInfo.DifficultyInfos[dataIndex], this);
        return cellView;
    }

    public void SetInfoSelected(string difficulty)
    {
        if (PlaylistMaker.Instance != null)
        {
            PlaylistMaker.Instance.AppendPlaylistItems(PlaylistMaker.Instance.GetPlaylistItem(difficulty));
        }
    }
}