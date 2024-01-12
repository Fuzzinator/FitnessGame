using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UpdatePlayerHeightDisplay : MonoBehaviour, ISaver
{
    [SerializeField]
    private UIToggleGroupSetting _toggleGroup;
    [SerializeField]
    private SettingsDisplay _settingsDisplay;
    [SerializeField]
    private ProfileEditor _profileEditor;
    [SerializeField]
    private UIToggleGroupSetting _metersOrFeet;
    [SerializeField]
    private TextMeshProUGUI _currentText;
    [SerializeField]
    private TextMeshProUGUI _buttonText;
    [SerializeField]
    private GameObject _calibratedTextDisplay;
    [SerializeField]
    protected bool _setSettingOnEnable = false;
    [SerializeField]
    private string _defaultCalibrationText = "Calibrate Height";
    private float _setHeight;
    private float _heightOffset;
    private bool _pressed;

    private CancellationToken _cancellationToken;
    private CancellationTokenSource _recalibratingTokenSource;

    public bool SaveRequested { get; set; }

    private const string CalibratingText = "Calibrating";
    private const string RecalibrateText = "Recalibrate Height";
    private const string Meters = "<size=50%> Meters</size>";
    private const string Feet = "<size=50%> Feet</size>";
    private const float MeterToFeet = 3.28084f;
    //private const float DisplayOffset = .075f;

    private void OnEnable()
    {
        Revert();
        SaveRequested = _setSettingOnEnable;
        if (_recalibratingTokenSource != null && _recalibratingTokenSource.IsCancellationRequested)
        {
            _recalibratingTokenSource.Dispose();
            _recalibratingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
        if (_recalibratingTokenSource == null)
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            _recalibratingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }
    }

    private void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
        if (_recalibratingTokenSource != null)
        {
            _recalibratingTokenSource.Cancel();
        }
    }
    public void ResetHeadHeight()
    {
        _setHeight = Head.Instance.transform.position.y;
        _heightOffset = 0f;
        _settingsDisplay?.ChangeWasMade(this);
        DelayAndShowCalibration().Forget();
        SaveRequested = true;
    }

    private async UniTask DelayAndShowCalibration()
    {
        _buttonText.SetText(CalibratingText);
        _calibratedTextDisplay.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f), ignoreTimeScale: true, cancellationToken: _recalibratingTokenSource.Token).SuppressCancellationThrow();

        if (_recalibratingTokenSource.IsCancellationRequested)
        {
            return;
        }

        _buttonText.SetText(RecalibrateText);
        _calibratedTextDisplay.SetActive(true);
        UpdateDisplay();
    }

    public void StartUpdating(float increment)
    {
        _pressed = true;
        WaitToUpdate(increment).Forget();
    }

    public void PressReleased()
    {
        _pressed = false;
    }

    private async UniTaskVoid WaitToUpdate(float increment)
    {
        UpdateHeadHeight(increment);
        await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken: _cancellationToken);
        while (!_cancellationToken.IsCancellationRequested && _pressed)
        {
            UpdateHeadHeight(increment);
            await UniTask.Delay(TimeSpan.FromSeconds(.5), cancellationToken: _cancellationToken);
        }

    }

    public void UpdateHeadHeight(float increment)
    {
        _heightOffset += increment;
        SaveRequested = true;
        _settingsDisplay?.ChangeWasMade(this);
        UpdateDisplay();

    }

    public void RefreshDisplayType()
    {
        SetText(_toggleGroup.CurrentValue == 0);
    }

    private void UpdateDisplay()
    {
        var useMeters = _metersOrFeet.CurrentValue == 0;
        //var useMeters = SettingsManager.GetSetting("DisplayInMeters", 0) == 0;
        SetText(useMeters);
    }

    private void SetText(bool useMeters)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            //var heightAsDouble = Math.Round((double)height, 2);

            var height = _heightOffset;
            if (useMeters)
            {
                height = Mathf.Round(height * 1000f) / 1000f;
                sb.Append(height);
                sb.Append(Meters);
            }
            else
            {
                height = Mathf.Round(height * MeterToFeet * 1000f) / 1000f;
                sb.Append(height);
                sb.Append(Feet);
            }
            _currentText.SetText(sb);
        }
    }

    public void Save(Profile overrideProfile = null)
    {
        GlobalSettings.SetUserHeight(_setHeight, overrideProfile);
        GlobalSettings.SetUserHeightOffset(_heightOffset, overrideProfile);
        SaveRequested = false;
    }

    public void Revert()
    {
        if (_profileEditor != null)
        {
            _setHeight = -1f;
            _heightOffset = 0f;

            if (_profileEditor.ActiveProfile != null)
            {
                _setHeight = GlobalSettings.GetUserHeight(_profileEditor.ActiveProfile);
                _heightOffset = GlobalSettings.GetUserHeightOffset(_profileEditor.ActiveProfile);
            }

            if (_setHeight == -1)
            {
                _setHeight = Head.Instance.transform.position.y;
                _heightOffset = 0f;
            }
        }
        else
        {
            _setHeight = GlobalSettings.UserHeight;
            _heightOffset = GlobalSettings.UserHeightOffset;
        }
        _buttonText.SetText(_defaultCalibrationText);
        _calibratedTextDisplay.SetActive(false);
        UpdateDisplay();
        SaveRequested = false;
    }
}
