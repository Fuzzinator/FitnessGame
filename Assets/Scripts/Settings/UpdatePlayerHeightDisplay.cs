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
    private SettingsDisplay _settingsDisplay;
    [SerializeField]
    private TextMeshProUGUI _currentText; 
    private float _setHeight;
    private bool _pressed;

    private CancellationToken _cancellationToken;
    
    public bool SaveRequested { get; set; }

    private const string METERS = "<size=50%> Meters</size>";

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

    private void UpdateDisplay()
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            var height = Mathf.Round(_setHeight * 1000f) / 1000f;
            //var heightAsDouble = Math.Round((double)height, 2);
            sb.Append(height);
            sb.Append(METERS);
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
