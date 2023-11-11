using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine.SocialPlatforms.Impl;

public class AzureSqlManager : MonoBehaviour
{
    public static AzureSqlManager Instance;

    private const string RootURL = "https://updateleaderboards.azurewebsites.net/api/";
    private const string SetLeaderboardEnd = "?code=mwB8YMyIHWkFd0qnX7pTEz1vYa0mv989iYA4rkL5XDHoAzFuocSNyg==";
    private const string GetLeaderboardEnd = "?code=g5d2kGCy-SVPpBp0LlX0gNvvdQ17dFYU-nnaeB15hR4uAzFu2s9d6A==";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //PostLeaderboardScore("TestLeaderboard3", 10, 5).Forget();
        //GetWaiter().Forget();
    }

    private async UniTaskVoid GetWaiter()
    {
        var leaderboard = await GetTop10Leaderboard("TestLeaderboard3");
        if (leaderboard == null)
        {
            return;
        }
        foreach (var leaderboardObject in leaderboard)
        {
            Debug.Log($"{leaderboardObject.ProfileName} scored: {leaderboardObject.Score} with a streak of {leaderboardObject.Streak}");
        }
    }

    #region Posting Score
    public async UniTaskVoid PostLeaderboardScore(string leaderboardName, int score, int streak)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return;
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;
        var record = new LeaderboardObject()
        {
            GUID = activeProfile.GUID.Replace("-", string.Empty),
            ProfileName = activeProfile.ProfileName,
            Score = score,
            Streak = streak
        };

        var url = $"{RootURL}SetLeaderboard/{leaderboardName}-{record.GUID}{SetLeaderboardEnd}";

        var json = JsonConvert.SerializeObject(record);
        var bytes = Encoding.UTF8.GetBytes(json);
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        try
        {
            var result = await request.SendWebRequest();
            await UniTask.WaitWhile(() => result.result == UnityWebRequest.Result.InProgress);
            if (result.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(result.error);
            }
            else
            {
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
    }
    #endregion

    #region Retrieving Scores
    public void GetPlayerScore(string leaderboardName)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return;
        }
        var profileID = ProfileManager.Instance.ActiveProfile.GUID;
    }

    private async UniTask<LeaderboardObject[]> GetTop10Leaderboard(string leaderboardName)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return null;
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;
        //var url = $"https://updateleaderboards.azurewebsites.net/api/GetLeaderboard/{leaderboardName}/{0}-{10}?code=g5d2kGCy-SVPpBp0LlX0gNvvdQ17dFYU-nnaeB15hR4uAzFu2s9d6A==\r\n";
        var url = $"{RootURL}GetLeaderboard/{leaderboardName}/{0}-{10}{GetLeaderboardEnd}";
        var request = UnityWebRequest.Get(url);

        try
        {
            var result = await request.SendWebRequest();
            await UniTask.WaitWhile(() => result.result == UnityWebRequest.Result.InProgress);
            if (result.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(result.error);
            }
            else
            {
                var json = result.downloadHandler.text;
                var results = JsonConvert.DeserializeObject<LeaderboardObject[]>(json);
                return results;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
        return null;
    }
    #endregion

    public void UpdateDisplayName(string displayName)
    {

    }

    private void OnFailure()
    {
        Debug.LogError(System.Environment.StackTrace);
    }

    [System.Serializable]
    public struct LeaderboardObject
    {
        public string GUID;
        public string ProfileName;
        public int Score;
        public int Streak;

        public LeaderboardObject(SongRecord record)
        {
            ProfileName = record.ProfileName;
            GUID = record.GUID;
            Score = record.Score; 
            Streak = record.Streak;
        }
    }
}