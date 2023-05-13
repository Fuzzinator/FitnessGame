using System.Diagnostics;

public class TrackWorkoutTime : BaseGameStateListener
{
    private Stopwatch _workoutStopwatch;
    private bool _isLevelPlaying;
    private bool _inPlayMode;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        EnqueueTracker();
        LevelManager.Instance.playLevel.AddListener(PlayLevel);
        LevelManager.Instance.levelCompleted.AddListener(OnLevelCompleted);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        TryToggleTracker(false);
        RecordTrackerValue();
        LevelManager.Instance.playLevel.RemoveListener(PlayLevel);
        LevelManager.Instance.levelCompleted.RemoveListener(OnLevelCompleted);
    }

    private void PlayLevel()
    {
        _isLevelPlaying = true;
        TryToggleTracker( true);
    }
    
    private void OnLevelCompleted()
    {
        _isLevelPlaying = false;
        TryToggleTracker(false);
    }

    private void TryToggleTracker(bool enable)
    {
        if (_workoutStopwatch == null)
        {
            return;
        }

        if (enable && _inPlayMode && _isLevelPlaying)
        {
            _workoutStopwatch.Start();
        }
        else if(!enable)
        {
            _workoutStopwatch.Stop();
        }
    }

    private void EnqueueTracker()
    {
        _workoutStopwatch = new Stopwatch();
    }

    private void RecordTrackerValue()
    {
        StatsManager.Instance.RecordWorkoutTime(_workoutStopwatch.Elapsed.Seconds);
    }
    
    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        switch (oldState)
        {
            case GameState.Paused when newState == GameState.Playing:
                _inPlayMode = true;
                TryToggleTracker(true);
                break;
            case GameState.Playing when (newState == GameState.Paused || newState == GameState.Unfocused):
                TryToggleTracker(false);
                break;
        }
    }
}
