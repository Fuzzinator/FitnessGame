using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
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
    private const string ENDSONGSTATSFORMAT = "{0}{1}\n{2}{3}\n{4}{5}\n{6}{7}";
    private const string ENDLEVELSTATSFORMAT = "{0}{1}\n{2}{3}\n{4}{5}\n{6}{7}";

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
        var goodHits = (int)ScoringManager.Instance.GoodHitsThisSong;
        var missedHits = (int)ScoringManager.Instance.MissedTargetsThisSong;
        var hitObstacles = (int)ScoringManager.Instance.HitObstaclesThisSong;
        string message;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(ENDSONGSTATSFORMAT, SONGSCORE,score,GOODHITS,goodHits,MISSEDHITS,missedHits,HITOBSTACLES,hitObstacles);

            //var buffer = sb.AsArraySegment();
            message = sb.ToString(); //(buffer.Array, buffer.Offset, buffer.Count);
        }
        
        var visuals = new Notification.NotificationVisuals(message, SONGCOMPLETE, disableUI:false, autoTimeOutTime:4f, popUp:true);
        NotificationManager.RequestNotification(visuals,  _mainMenuAction);
    }

    public void DisplayEndLevelStats()
    {
        var totalScore = ScoringManager.Instance.CurrentScore;
        var goodHits = (int)ScoringManager.Instance.GoodHits;
        var missedHits = (int)ScoringManager.Instance.MissedTargets;
        var hitObstacles = (int)ScoringManager.Instance.HitObstacles;

        string message;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(ENDLEVELSTATSFORMAT, TOTALSCORE, totalScore,GOODHITS,goodHits,MISSEDHITS,missedHits,HITOBSTACLES,hitObstacles);

            //var buffer = sb.AsArraySegment();
            message = sb.ToString(); //(buffer.Array, buffer.Offset, buffer.Count);
        }
        
        var visuals = new Notification.NotificationVisuals(message, WORKOUTCOMPLETE, FINISHBUTTON, disableUI:false);
        NotificationManager.RequestNotification(visuals,  _mainMenuAction);
    }

    public void DisplayEndLevel()
    {
        
    }
}