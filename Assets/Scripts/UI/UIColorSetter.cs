using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSetter : MonoBehaviour
{
    [SerializeField]
    private Image _leftGloveColor;
    
    [SerializeField]
    private Image _rightGloveColor;
    
    [SerializeField]
    private Image _blockNoteColor;
    
    [SerializeField]
    private Image _obstacleColor;


    private void OnEnable()
    {
        _leftGloveColor.color = ColorsManager.Instance.GetAppropriateColor(HitSideType.Left);
        _rightGloveColor.color = ColorsManager.Instance.GetAppropriateColor(HitSideType.Right);
        _blockNoteColor.color = ColorsManager.Instance.GetAppropriateColor(HitSideType.Block);
        _obstacleColor.color = ColorsManager.Instance.GetAppropriateColor(HitSideType.Unused);
    }
}
