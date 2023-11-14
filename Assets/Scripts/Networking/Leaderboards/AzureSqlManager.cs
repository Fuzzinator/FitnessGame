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
using GameModeManagement;
using System.Threading;

public class AzureSqlManager : MonoBehaviour
{
    public static AzureSqlManager Instance;

    private const string RootURL = "https://updateleaderboards.azurewebsites.net/api/";
    private const string SetLeaderboardEnd = "?code=mwB8YMyIHWkFd0qnX7pTEz1vYa0mv989iYA4rkL5XDHoAzFuocSNyg==";
    private const string GetLeaderboardEnd = "?code=g5d2kGCy-SVPpBp0LlX0gNvvdQ17dFYU-nnaeB15hR4uAzFu2s9d6A==";

    private bool _serverRunning = false;
    private bool _warmingServer = false;

    public bool ServerIsRunning => _serverRunning;

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
        WarmServer().Forget();
    }

    private async UniTaskVoid WarmServer()
    {
        while (!_serverRunning)
        {
            _warmingServer = true;
            if (!NetworkConnectionManager.Instance.NetworkConnected)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                continue;
            }

            var leaderboard = await GetTop10Leaderboard("WarmUpTheDatabase");
            if (leaderboard != null)
            {
                _serverRunning = true;
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(30));
            }
        }
        _warmingServer = false;
        KeepServerWarm().Forget();
    }

    private async UniTaskVoid KeepServerWarm()
    {
        await UniTask.Delay(TimeSpan.FromMinutes(30));
        WarmServer().Forget();
    }

    #region Posting Score
    public async UniTaskVoid PostLeaderboardScore(string leaderboardName, int score, int streak, CancellationToken token)
    {
        try
        {
            while (!_serverRunning && !token.IsCancellationRequested)
            {
                if (!_warmingServer)
                {
                    WarmServer().Forget();
                }
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
            if (token.IsCancellationRequested)
            {
                return;
            }

        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                return;
            }
            Debug.LogError(e.Message);
        }
        PostLeaderboardScore(leaderboardName, score, streak).Forget();
    }

    private async UniTaskVoid PostLeaderboardScore(string leaderboardName, int score, int streak)
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
                Debug.LogWarning($"Failed to post score. Error returned was:{result.error}");
            }
            else
            {
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to post score. Error returned was:{e.Message}");
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
    public async UniTask<LeaderboardObject[]> GetTopLeaderboard(SongInfo song, DifficultyInfo.DifficultyEnum difficulty, GameMode mode, CancellationToken token)
    {
        try
        {
            while (!_serverRunning && !token.IsCancellationRequested)
            {
                if (!_warmingServer)
                {
                    WarmServer().Forget();
                }
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
            if (token.IsCancellationRequested)
            {
                return null;
            }

        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                return null;
            }
            Debug.LogError(e.Message);
        }
        return await GetTopLeaderboard(song, difficulty, mode);
    }

    private async UniTask<LeaderboardObject[]> GetTopLeaderboard(SongInfo song, DifficultyInfo.DifficultyEnum difficulty, GameMode mode)
    {
        var songID = $"Song_{song.RecordableName}{difficulty}{mode}";
        return await GetTop10Leaderboard(songID);
    }

    private async UniTask<LeaderboardObject[]> GetTop10Leaderboard(string leaderboardName)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return null;
        }

        var activeProfile = ProfileManager.Instance.ActiveProfile;
        var url = $"{RootURL}GetLeaderboard/{leaderboardName}/{0}-{10}{GetLeaderboardEnd}";
        var request = UnityWebRequest.Get(url);

        try
        {
            var result = await request.SendWebRequest();
            await UniTask.WaitWhile(() => result.result == UnityWebRequest.Result.InProgress);
            if (result.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to retrieve leaderboard. Server returned error: {result.error}");
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
            Debug.LogWarning($"Failed to retrieve leaderboard. Server returned error: {e.Message}");
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
}

[Serializable]
public class LeaderboardObject
{
    public string GUID;
    public string ProfileName;
    public int Score;
    public int Streak;

    public LeaderboardObject()
    {

    }

    public LeaderboardObject(SongRecord record)
    {
        ProfileName = record.ProfileName;
        GUID = record.GUID;
        Score = record.Score;
        Streak = record.Streak;
    }
}