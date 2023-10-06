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

    [field: SerializeField]
    public HitSideType TargetHitSideType { get; private set; }


    private const string LeftHanded = "LeftHanded";

    private void OnEnable()
    {
        if (_referToPlaylist)
        {
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateFromPlaylist);

            var playlist = PlaylistManager.Instance.CurrentPlaylist;
            if (playlist != null)
            {
                DelayAndSetStartingType(playlist.StartingSide).Forget();
            }
        }
        else if (_newPlaylist)
        {
            DelayAndGetFromMaker().Forget();
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

    private async UniTaskVoid DelayAndSetStartingType(HitSideType startingSide)
    {
        await UniTask.DelayFrame(1);
        if (this == null)
        {
            return;
        }

        SetStartingSide(startingSide);
    }

    private async UniTaskVoid DelayAndGetFromMaker()
    {

        await UniTask.DelayFrame(1);
        if (this == null)
        {
            return;
        }
        var startingSide = PlaylistMaker.Instance.StartingSide;
        SetStartingSide(startingSide);
    }

    private async UniTaskVoid DelayUpdateDisplay()
    {
        await UniTask.DelayFrame(1);
        if (this == null)
        {
            return;
        }

        var leftHanded = SettingsManager.GetSetting(LeftHanded, false);
        SetActiveToggle(leftHanded ? 0 : 1);
    }
    private void UpdateFromPlaylist(Playlist playlist)
    {
        if(playlist == null)
        {
            return;
        }
        SetStartingSide(playlist.StartingSide);
    }

    private void SetStartingSide(HitSideType startingSide)
    {
        SetActiveToggle((int)startingSide);
    }

    private void SetActiveToggle(int activeToggle)
    {
        _forwardFootToggles[activeToggle].isOn = true;
    }

    public void OnToggleSelected(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }
        var toggleID = _forwardFootToggles.GetToggleID(toggle);
        if (toggleID < 0)
        {
            Debug.LogError("Target forward foot toggles not set up correctly.");
            return;
        }
        TargetHitSideType = (HitSideType)toggleID;

        if (_newPlaylist)
        {
            PlaylistMaker.Instance.SetStartingType(TargetHitSideType);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetForwardFoot(TargetHitSideType);
        }
    }
}
