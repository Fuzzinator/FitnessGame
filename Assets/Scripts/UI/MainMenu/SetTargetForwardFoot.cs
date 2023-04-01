using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTargetForwardFoot : MonoBehaviour
{
    [SerializeField]
    private Toggle[] _forwardFootToggles;

    [SerializeField]
    private bool _referToPlaylist;

    [SerializeField]
    private bool _newPlaylist;

    public HitSideType TargetHitSideType { get; private set; }


    private const string LeftHanded = "LeftHanded";

    private void OnEnable()
    {
        if(_referToPlaylist)
        {
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateFromPlaylist);
            DelayAndUpdateFromPlaylist().Forget();
        }
        else
        {
            DelayUpdateDisplay().Forget();
        }
    }

    private void OnDisable()
    {

        if (_referToPlaylist)
        {
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateFromPlaylist);
        }
    }

    private async UniTaskVoid DelayAndUpdateFromPlaylist()
    {
        await UniTask.DelayFrame(1);
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        if (this == null || playlist == null)
        {
            return;
        }

        UpdateFromPlaylist(playlist);
    }

    private async UniTaskVoid DelayUpdateDisplay()
    {
        await UniTask.DelayFrame(1);
        if(this == null)
        {
            return;
        }

        var leftHanded = SettingsManager.GetSetting(LeftHanded, false);
        SetActiveToggle(leftHanded ? 0 : 1);
    }

    private void UpdateFromPlaylist(Playlist playlist)
    {
        SetActiveToggle((int)playlist.StartingSide);
    }

    private void SetActiveToggle(int activeToggle)
    {
        _forwardFootToggles[activeToggle].isOn = true;
    }

    public void OnToggleSelected(Toggle toggle)
    {

        var toggleID = _forwardFootToggles.GetToggleID(toggle);
        if (toggleID < 0)
        {
            Debug.LogError("Target forward foot toggles not set up correctly.");
            return;
        }
        TargetHitSideType = (HitSideType)toggleID;

        if(_newPlaylist)
        {
            PlaylistMaker.Instance.SetStartingType(TargetHitSideType);
        }
    }
}
