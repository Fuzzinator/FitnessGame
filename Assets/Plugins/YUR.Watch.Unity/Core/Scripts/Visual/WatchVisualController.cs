using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Watch;

public class WatchVisualController : MonoBehaviour
{

    public RectTransform ScalerReference;
    public RectTransform ScalerTwoReference;
    public Transform ScaleReference;

    public float offset = 0;
    public float divisionFactor = 10;

    public Transform MetalBarScaler;
    public Transform MetalBars;

    public WatchDisplay watchDisplay;

    private bool _updateBars;

    private void Start()
    {
        watchDisplay.OnShow += ShowBars;
        watchDisplay.OnHide += HideBars;
        watchDisplay.OnFinish += FinishBars;
    }

    private void Update()
    {
        if (_updateBars)
        {
            UpdateBars();
        }
    }

    private void ShowBars()
    {
        _updateBars = true;
    }

    private void HideBars()
    {
        _updateBars = true;
    }

    private void FinishBars()
    {
        if (WatchManager.Instance.CurrentScreen == ScreenType.MainScreen)
        {
            _updateBars = false;
        }
    }

    private void UpdateBars()
    {
        float mainScalerValue = (ScalerReference.sizeDelta.x / divisionFactor);
        float secondScalerValue = ((ScalerTwoReference.sizeDelta.x / divisionFactor) + offset);

        MetalBarScaler.localScale = new Vector3(MetalBarScaler.localScale.x, MetalBarScaler.localScale.y, mainScalerValue + secondScalerValue);

        MetalBars.localScale = new Vector3(MetalBars.localScale.x, MetalBars.localScale.y, ScaleReference.localScale.x);
    }
}
