using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlaylistCountdownClock : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private float _timeRemaining = 0;
    private bool _clockRunning = false;
    private bool _clockEnabled = false;


    private const int MINUTE = 60;

    private void OnEnable()
    {
        _clockEnabled = true;
        GameStateManager.Instance.gameStateChanged.AddListener(GameStateListener);
    }

    private void OnDisable()
    {
        _clockEnabled = false;
        GameStateManager.Instance.gameStateChanged.RemoveListener(GameStateListener);
    }
    
    public async void InitializeClock()
    {
        _timeRemaining = PlaylistManager.Instance.CurrentPlaylist.Length;
        UpdateDisplay();
        
#pragma warning disable 4014
        UniTask.Run(RunClock);
#pragma warning restore 4014
        
        await RunDisplayUpdate().SuppressCancellationThrow();
    }

    public void StartClock()
    {
        _clockRunning = true;
    }

    public void ToggleClock(bool on)
    {
        _clockRunning = on;
    }

    public void StopClock()
    {
        _timeRemaining = 0;
        _clockRunning = false;
    }

    private void UpdateDisplay()
    {
        var minutes = _timeRemaining/ MINUTE;
        _text.SetText(minutes.ToString("0.00"));
    }

    private async UniTask RunClock()
    {
        var time = new Stopwatch();
        while (_clockEnabled && _timeRemaining > 0)
        {
            time.Restart();
            await UniTask.DelayFrame(1);
            if (!_clockEnabled || _timeRemaining <= 0)
            {
                break;
            }
            if (!_clockRunning)
            {
                continue;
            }

            time.Stop();// = DateTime.Now - time;
            _timeRemaining -= (time.ElapsedMilliseconds*0.001f);
        }
    }

    private async UniTask RunDisplayUpdate()
    {
        while (_clockEnabled && _timeRemaining > 0)
        {
            if (!_clockRunning)
            {
                await UniTask.DelayFrame(1);
                continue;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(.25f), cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
            
            if (!_clockEnabled || _timeRemaining <= 0)
            {
                break;
            }
            UpdateDisplay();
        }
    }


    private void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.Paused && newState == GameState.Playing)
        {
            ToggleClock(true);
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            ToggleClock(false);
        }
    }
}