using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class LoadingProgressUIDisplay : MonoBehaviour
{
    [SerializeField]
    private Image _loadingBar;

    public IProgress<double> ProgressDisplay
    {
        get
        {
            if(_progressDisplay == null)
            {
                _progressDisplay = new Progress<double>();
                _progressDisplay.ProgressChanged += (sender, d) => UpdateLoadingBar(d);
            }
            return _progressDisplay;
        }
    }
    private Progress<double> _progressDisplay;

    private void UpdateLoadingBar(double value)
    {
        _loadingBar.fillAmount = (float)value;
    }
}
