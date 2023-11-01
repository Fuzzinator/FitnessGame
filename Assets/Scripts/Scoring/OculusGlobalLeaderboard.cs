using Oculus.Platform;
using Models = Oculus.Platform.Models;

public class OculusGlobalLeaderboard : BaseLeaderboard
{
#if UNITY_ANDROID
    private Models.LeaderboardList Leaderboards { get; set; }

    private const string GlobalLeaderboardName = "Global Leaderboard";
    public override void GetLeaderboard()
    {
        var request = Oculus.Platform.Leaderboards.Get(GlobalLeaderboardName);
        request.OnComplete(HandleRequest);
        base.GetLeaderboard();
    }

    private void HandleRequest(Message<Models.LeaderboardList> message)
    {
        if(message == null)
        {
            return;
        }
        Leaderboards = message.Data;

    }


#endif
}
