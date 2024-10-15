using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlaylistCountdownClock : MonoBehaviour, IOrderedInitialize
{
    public bool Initialized { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _minutesText;

    private float _timeRemaining = 0;
    private bool _clockRunning = false;
    private bool _clockEnabled = false;
    private bool _clockPaused = false;
    private bool _applicationPaused = false;


    private const int MINUTE = 60;

    private const string DOUBLEDIGITFORMAT = "<mspace=.65em><size=85>{0:00}:{1:00}</size></mspace>";
    private const string TRIPLEDIGITFORMAT = "<mspace=.65em><size=70>{0:000}:{1:00}</size></mspace>";
    private const string STRINGFORMAT = "<size={0}>{1:00}:{2:00}</size>";//"<mspace=.65em>{0:00}:{1:00}</mspace>";
    //private const string DIVIDER = ":";

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

    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        InitializeClock();
        Initialized = true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        _applicationPaused = pauseStatus;
    }

    public async void InitializeClock()
    {
        _timeRemaining = PlaylistManager.Instance.CurrentPlaylist?.Length ?? 0;
        UpdateDisplay();

        var token = this.GetCancellationTokenOnDestroy();

        RunClock(token).Forget();

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
        _timeRemaining = PlaylistManager.Instance.CurrentPlaylist?.Length ?? 0;
        UpdateDisplay();
        _clockRunning = false;
    }

    private void UpdateDisplay()
    {
        var minutes = (int)Mathf.Floor(_timeRemaining / MINUTE);
        var seconds = (int)Mathf.Floor(_timeRemaining % MINUTE);

        using (var sb = ZString.CreateStringBuilder(true))
        {
            var format = minutes < 100 ? DOUBLEDIGITFORMAT : TRIPLEDIGITFORMAT;

            sb.AppendFormat(format, minutes, seconds);

            _minutesText.SetText(sb);
        }
    }

    public void SongFailedToLoad()
    {
        _timeRemaining -= PlaylistManager.Instance.CurrentItem.SongInfo.SongLength;
        UpdateDisplay();
    }

    private async UniTask RunClock(CancellationToken token)
    {
        var paused = false;
        while (_clockEnabled)
        {
            try
            {
                await UniTask.NextFrame(cancellationToken: token);
                if (token.IsCancellationRequested || !_clockEnabled)
                {
                    break;
                }

                if (_clockPaused || _applicationPaused)
                {
                    if (!paused)
                    {
                        paused = true;
                    }
                    continue;
                }

                if(paused)
                {
                    paused = false;
                }

                if (!_clockRunning || _timeRemaining <= 0)
                {

                    continue;
                }

                _timeRemaining -= Time.deltaTime;
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

                if (_timeRemaining < 0)
                {
                    continue;
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
        if ((oldState == GameState.Paused || oldState == GameState.PreparingToPlay) && newState == GameState.Playing)
        {
            ToggleClockPaused(false);
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            ToggleClockPaused(true);
        }
    }
}