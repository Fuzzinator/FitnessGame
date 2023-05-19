using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNotificationRequester : MonoBehaviour
{
    [SerializeField]
    private TransitionController _transitionController;
    
    private readonly Action _mainMenuAction;
    private Notification.NotificationVisualInfo _visualInfo;
    
    private const string WORKOUTCOMPLETE = "Workout Complete";
    private const string SONGCOMPLETE = "Song Complete";
    private const string FINISHBUTTON = "Finish";
    private const string TOTALSCORE = "Total Score: ";
    private const string SONGSCORE = "Song Score: ";
    private const string GOODHITS = "Good Hits: ";
    private const string MISSEDHITS = "Missed Hits: ";
    private const string HITOBSTACLES = "Hit Obstacles: ";
    private const string ENDSONGSTATSFORMAT = "{0}{1}\n{2}{3}\n{4}{5}\n{6}{7}";
    private const string ENDLEVELSTATSFORMAT = "{0}{1}\n{2}{3}\n{4}{5}\n{6}{7}";

    private LevelNotificationRequester()
    {
        _mainMenuAction = () =>
        {
            Time.timeScale = 1;
            _transitionController.RequestTransition(); 
        };//ActiveSceneManager.Instance.LoadMainMenu();};
    }

    private void Start()
    {
        _visualInfo = new Notification.NotificationVisualInfo();
    }

    public void CheckLevelStateAndDisplay()
    {
        if (PlaylistManager.Instance == null || PlaylistManager.Instance.CurrentPlaylist?.Items == null)
        {
            return;
        }
        
        /*if (PlaylistManager.Instance.CurrentIndex == PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            DisplayEndLevelStats();
        }
        else */
        if(PlaylistManager.Instance.CurrentIndex < PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            DisplayEndSongStats();
        }
    }
    
    public void DisplayEndSongStats()
    {
        var score = ScoringAndHitStatsManager.Instance.SongScore;
        var goodHits = (int)ScoringAndHitStatsManager.Instance.SongHitTargets;
        var missedHits = (int)ScoringAndHitStatsManager.Instance.SongMissedTargets;
        var hitObstacles = (int)ScoringAndHitStatsManager.Instance.SongHitObstacles;
        
        
        string message;
        //ArraySegment<char> chars;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(ENDSONGSTATSFORMAT, SONGSCORE,score,GOODHITS,goodHits,MISSEDHITS,missedHits,HITOBSTACLES,hitObstacles);
            message = sb.ToString();
        }

        _visualInfo.message = message;
        _visualInfo.header = SONGCOMPLETE;
        _visualInfo.button1Txt = null;
        _visualInfo.disableUI = false;
        _visualInfo.autoTimeOutTime = 4f;
        _visualInfo.popUp = true;
        _visualInfo.height = 0f;
        
        //var visuals = new Notification.NotificationVisuals(message, SONGCOMPLETE, disableUI:false, autoTimeOutTime:4f, popUp:true);
        NotificationManager.RequestNotification(_visualInfo,  _mainMenuAction);
    }

    public void DisplayEndLevelStats()
    {
        var totalScore = ScoringAndHitStatsManager.Instance.CurrentScore;
        var goodHits = (int)ScoringAndHitStatsManager.Instance.WorkoutHitTargets;
        var missedHits = (int)ScoringAndHitStatsManager.Instance.WorkoutMissedTargets;
        var hitObstacles = (int)ScoringAndHitStatsManager.Instance.WorkoutHitObstacles;

        string message;
        //ArraySegment<char> chars;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(ENDLEVELSTATSFORMAT, TOTALSCORE, totalScore,GOODHITS,goodHits,MISSEDHITS,missedHits,HITOBSTACLES,hitObstacles);
            message = sb.ToString();
        }

        _visualInfo.message = message;
        _visualInfo.header = WORKOUTCOMPLETE;
        _visualInfo.button1Txt = FINISHBUTTON;
        _visualInfo.disableUI = false;
        _visualInfo.autoTimeOutTime = 0f;
        _visualInfo.popUp = false;
        _visualInfo.height = 0f;
        
        NotificationManager.RequestNotification(_visualInfo,  _mainMenuAction);
    }

    public void DisplayEndLevel()
    {
        
    }
}