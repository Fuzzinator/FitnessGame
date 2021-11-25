using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNotificationRequester : MonoBehaviour
{
    private readonly Action _mainMenuAction;
    
    private const string WORKOUTCOMPLETE = "Workout Complete";
    private const string SONGCOMPLETE = "Song Complete";
    private const string FINISHBUTTON = "Finish";
    private const string TOTALSCORE = "Total Score: ";
    private const string SONGSCORE = "Song Score: ";
    private const string GOODHITS = "Good Hits: ";
    private const string MISSEDHITS = "Missed Hits: ";
    private const string HITOBSTACLES = "Hit Obstacles: ";

    private LevelNotificationRequester()
    {
        _mainMenuAction = () => { ActiveSceneManager.Instance.LoadMainMenu();};
    }

    public void CheckLevelStateAndDisplay()
    {
        if (PlaylistManager.Instance.CurrentIndex == PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            DisplayEndLevelStats();
        }
        else if(PlaylistManager.Instance.CurrentIndex < PlaylistManager.Instance.CurrentPlaylist.Items.Length - 1)
        {
            DisplayEndSongStats();
        }
    }
    
    public void DisplayEndSongStats()
    {
        var score = ScoringManager.Instance.ScoreThisSong;
        var goodHits = ScoringManager.Instance.GoodHitsThisSong;
        var missedHits = ScoringManager.Instance.MissedTargetsThisSong;
        var hitObstacles = ScoringManager.Instance.HitObstaclesThisSong;
        var message = $"{SONGSCORE}{score}\n{GOODHITS}{goodHits}\n{MISSEDHITS}{missedHits}\n{HITOBSTACLES}{hitObstacles}";
        var visuals = new Notification.NotificationVisuals(message, SONGCOMPLETE, disableUI:false, autoTimeOutTime:1f);
        NotificationManager.RequestNotification(visuals,  _mainMenuAction);
    }

    public void DisplayEndLevelStats()
    {
        var totalScore = ScoringManager.Instance.CurrentScore;
        var goodHits = ScoringManager.Instance.GoodHits;
        var missedHits = ScoringManager.Instance.MissedTargets;
        var hitObstacles = ScoringManager.Instance.HitObstacles;
        var message = $"{TOTALSCORE}{totalScore}\n{GOODHITS}{goodHits}\n{MISSEDHITS}{missedHits}\n{HITOBSTACLES}{hitObstacles}";
        var visuals = new Notification.NotificationVisuals(message, WORKOUTCOMPLETE, FINISHBUTTON, disableUI:false);
        NotificationManager.RequestNotification(visuals,  _mainMenuAction);
    }

    public void DisplayEndLevel()
    {
        
    }
}