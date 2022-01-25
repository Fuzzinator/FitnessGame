using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    private bool _clockPaused = false;
    private bool _applicationPaused = false;


    private const int MINUTE = 60;
    private CancellationTokenSource _source;

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

    private void Start()
    {
        InitializeClock();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        _applicationPaused = pauseStatus;
    }

    public async void InitializeClock()
    {
        _timeRemaining = PlaylistManager.Instance.CurrentPlaylist.Length;
        UpdateDisplay();

        var token = this.GetCancellationTokenOnDestroy();
#pragma warning disable 4014
        RunClock(token);
        //UniTask.Run(() => RunClock(token), cancellationToken: token);//_source.Token));
#pragma warning restore 4014
        await RunDisplayUpdate(token).SuppressCancellationThrow();
    }

    public void ToggleClockRunning(bool on)
    {
        _clockRunning = on;
    }

    public void ToggleClockPaused(bool on)
    {
        _clockPaused = on;
    }

    public void ResetClock()
    {
        _timeRemaining = PlaylistManager.Instance.CurrentPlaylist.Length;
        UpdateDisplay();
        _clockRunning = false;
    }

    private void UpdateDisplay()
    {
        var minutes = Mathf.Floor(_timeRemaining / MINUTE);
        var seconds = Mathf.Floor(_timeRemaining % MINUTE);

        _text.SetText($"{minutes}:{seconds:00}");
    }

    public void SongFailedToLoad()
    {
        _timeRemaining-= PlaylistManager.Instance.CurrentItem.SongInfo.SongLength;
        UpdateDisplay();
    }

    private async UniTask RunClock(CancellationToken token)
    {
        var time = new Stopwatch();
        while (_clockEnabled)
        {
            try
            {
                time.Restart();
                await UniTask.DelayFrame(1, cancellationToken: token);
                if (token.IsCancellationRequested || !_clockEnabled)
                {
                    break;
                }

                if (!_clockRunning || _clockPaused || _applicationPaused || _timeRemaining <= 0)
                {
                    continue;
                }

                time.Stop();
                var timeSpan = time.Elapsed;
                _timeRemaining -= (float) timeSpan.TotalSeconds;
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                break;
            }
        }
    }

    private async UniTask RunDisplayUpdate(CancellationToken token)
    {
        var delayTime = TimeSpan.FromSeconds(.25f);
        while (_clockEnabled)
        {
            try
            {
                if (!_clockRunning || _clockPaused || _timeRemaining <= 0)
                {
                    await UniTask.DelayFrame(1, cancellationToken: token);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    continue;
                }

                await UniTask.Delay(delayTime, cancellationToken: token);

                if (token.IsCancellationRequested || !_clockEnabled)
                {
                    break;
                }

                UpdateDisplay();
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                break;
            }
        }
    }


    private void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.Paused && newState == GameState.Playing)
        {
            ToggleClockPaused(false);
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            ToggleClockPaused(true);
        }
    }
}