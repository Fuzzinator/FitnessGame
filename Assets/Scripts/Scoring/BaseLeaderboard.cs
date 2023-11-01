using UnityEngine;
using UnityEngine.Events;

public class BaseLeaderboard : MonoBehaviour
{
    public UnityEvent leaderboardsUpdated = new UnityEvent();
    public virtual void GetLeaderboard()
    {

    }

}
