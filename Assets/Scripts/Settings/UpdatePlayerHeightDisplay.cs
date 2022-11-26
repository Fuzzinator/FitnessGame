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
    private TextMeshProUGUI _currentText; 
    private float _setHeight;
    private bool _pressed;

    private CancellationToken _cancellationToken;
    
    public bool SaveRequested { get; set; }

    private const string METERS = "<size=50%> Meters</size>";
    private const string FEET = "<size=50%> Feet</size>";
    private const float METERTOFEET = 3.28084f;

    private void OnEnable()
    {
        SaveRequested = false;
    }

    private void OnDisable()
    {
        if (!SaveRequested)
        {
            Revert();
        }
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        
        _setHeight = GlobalSettings.UserHeight;
        if (_setHeight < 0)
        {
            _setHeight = Head.Instance.transform.position.y;
            
            GlobalSettings.UserHeight = _setHeight;
        }
        UpdateDisplay();
    }

    public void ResetHeadHeight()
    {
        _setHeight = Head.Instance.transform.position.y;
        _settingsDisplay.ChangeWasMade(this);
        UpdateDisplay();
        
        SaveRequested = true;
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
        await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken:_cancellationToken);
        while (!_cancellationToken.IsCancellationRequested && _pressed)
        {
            UpdateHeadHeight(increment);
            await UniTask.Delay(TimeSpan.FromSeconds(.5), cancellationToken:_cancellationToken);
        }
        
    }

    public void UpdateHeadHeight(float increment)
    {
        _setHeight += increment;
        _settingsDisplay.ChangeWasMade(this);
        UpdateDisplay();
    }

    public void RefreshDisplayType()
    {
        SetText(_toggleGroup.CurrentValue == 0);
    }

    private void UpdateDisplay()
    {
        var useMeters = SettingsManager.GetSetting("DisplayInMeters", 0) == 0;
        SetText(useMeters);
    }

    private void SetText(bool useMeters)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            //var heightAsDouble = Math.Round((double)height, 2);
            if (useMeters)
            {
                var height = Mathf.Round(_setHeight * 1000f) / 1000f;
                sb.Append(height);
                sb.Append(METERS);
            }
            else
            {
                var height = Mathf.Round(_setHeight * METERTOFEET * 1000f) / 1000f;
                sb.Append(height);
                sb.Append(FEET);
            }
            _currentText.SetText(sb);
        }
    }

    public void Save()
    {
        GlobalSettings.UserHeight = _setHeight;
        SaveRequested = false;
    }

    public void Revert()
    {
        _setHeight = GlobalSettings.UserHeight;
        UpdateDisplay();
        SaveRequested = false;
    }
}
