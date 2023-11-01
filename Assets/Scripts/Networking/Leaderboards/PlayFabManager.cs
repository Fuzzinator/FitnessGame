using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using UnityEngine.SocialPlatforms.Impl;





using Newtonsoft.Json;



public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    public UnityEvent<bool> IsLoggedIn = new();

    private string _developerClientToken = null;

    //private List<>

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

    #region Logging in
    public void SignInWithProfile(Profile profile)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_developerClientToken))
        {
            _developerClientToken = null;
            IsLoggedIn.Invoke(false);
        }
        var request = new LoginWithCustomIDRequest
        {
            CustomId = profile.GUID,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            DisplayNameUpdateCheck(result, profile);
            OnLoginSuccess(result);
        }, OnFailure);
    }

    private void DisplayNameUpdateCheck(LoginResult result, Profile profile)
    {
        var displayName = result?.InfoResultPayload?.AccountInfo?.TitleInfo?.DisplayName;
        var hasDisplayName = !string.IsNullOrWhiteSpace(displayName);
        if (!hasDisplayName || !string.Equals(displayName, profile.ProfileName))
        {
            UpdateDisplayName(profile.ProfileName);
        }
    }

    public void UpdateDisplayName(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, UpdateDisplayNameSuccess, OnFailure);
    }

    private void UpdateDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
    {

    }

    private void OnLoginSuccess(LoginResult result)
    {
        _developerClientToken = result.SessionTicket;

        var stats = result.InfoResultPayload;

        IsLoggedIn.Invoke(true);
    }
    #endregion

    #region Posting Score
    public void PostScore(string leaderboardName, int score)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate()
                {
                    StatisticName = leaderboardName,
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, result => PostScoreSuccessful(result), OnFailure);
    }

    private void PostScoreSuccessful(UpdatePlayerStatisticsResult result)
    {
        if (result != null && result.Request is UpdatePlayerStatisticsRequest update)
        {
            if (update.Statistics.Count == 1)
            {
                PlayFabAdminAPI.UpdatePlayerStatisticDefinition(new PlayFab.AdminModels.UpdatePlayerStatisticDefinitionRequest
                {
                    StatisticName = update.Statistics[0].StatisticName,
                    AggregationMethod = PlayFab.AdminModels.StatisticAggregationMethod.Max,
                    VersionChangeInterval = PlayFab.AdminModels.StatisticResetIntervalOption.Never
                },
                result => DefinitionUpdated(result), OnFailure);
            }
        }
    }
    private void DefinitionUpdated(PlayFab.AdminModels.UpdatePlayerStatisticDefinitionResult result)
    {

    }
    #endregion

    #region Retrieving Scores
    public void GetPlayerScore(string leaderboardName)
    {
        if (!NetworkConnectionManager.Instance.NetworkConnected)
        {
            return;
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = leaderboardName,

        };
        PlayFabClientAPI.GetLeaderboard(request, result => GetResults(result), OnFailure);
    }

    private void GetResults(GetLeaderboardResult result)
    {

    }
    #endregion

    private void OnFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
        Debug.LogError(System.Environment.StackTrace);
    }
}







public class Temp
{
    public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        var leaderboard = new Leaderboard
        {
            leaderboardObjects = new List<LeaderboardObject>
            {
                new LeaderboardObject
                {
                    Name = "TestAccount1",
                    Score = 100
                },
                new LeaderboardObject
                {
                    Name = "TestAccount2",
                    Score = 20
                }
            }
        };
        var json = JsonConvert.SerializeObject(leaderboard);

        return new OkObjectResult(json);
    }
}


public class Leaderboard
{
    public List<LeaderboardObject> leaderboardObjects;
}

public class LeaderboardObject
{
    public string Name { get; set; }
    public ulong Score { get; set; }
}