using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistOverridesSetter : MonoBehaviour
{
    [SerializeField]
    private Toggle _noObstaclesToggle;
    [SerializeField]
    private Toggle _oneHandedToggle;
    [SerializeField]
    private Toggle _jabsOnlyToggle;
    [SerializeField]
    private TextMeshProUGUI _noObstaclesText;
    [SerializeField]
    private TextMeshProUGUI _oneHandedText;
    [SerializeField]
    private TextMeshProUGUI _jabsOnlyText;

    [SerializeField]
    private bool _referToPlaylist;

    [SerializeField]
    private bool _newPlaylist;

    [SerializeField]
    private bool _singleSong;

    private const string ON = "On";
    private const string OFF = "Off";

    private void OnEnable()
    {
        if (_referToPlaylist || _singleSong)
        {
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateFromPlaylist);

            var playlist = PlaylistManager.Instance.CurrentPlaylist;
            if (playlist != null)
            {
                DelayAndSet(playlist).Forget();
            }
        }
        else if (_newPlaylist)
        {
            DelayAndGetFromMaker().Forget();
        }
    }

    private void OnDisable()
    {

        if (_referToPlaylist)
        {
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateFromPlaylist);
        }
    }

    private async UniTaskVoid DelayAndSet(Playlist playlist)
    {
        await UniTask.DelayFrame(1);
        UpdateFromPlaylist(playlist);
    }

    private async UniTaskVoid DelayAndGetFromMaker()
    {

        await UniTask.DelayFrame(1);
        if (this == null)
        {
            return;
        }
        var noObstacles = PlaylistMaker.Instance.ForceNoObstacles;
        var oneHanded = PlaylistMaker.Instance.ForceOneHanded;
        var jabsOnly = PlaylistMaker.Instance.ForceJabsOnly;
        NoObstaclesToggleSet(noObstacles);
        OneHandedToggleSet(oneHanded);
        JabsOnlyToggleSet(jabsOnly);
    }

    private void UpdateFromPlaylist(Playlist playlist)
    {
        if (playlist == null)
        {
            return;
        }
        NoObstaclesToggleSet(playlist.ForceNoObstacles);
        OneHandedToggleSet(playlist.ForceOneHanded);
        JabsOnlyToggleSet(playlist.ForceJabsOnly);

        _noObstaclesToggle.SetIsOnWithoutNotify(playlist.ForceNoObstacles);
        _oneHandedToggle.SetIsOnWithoutNotify(playlist.ForceOneHanded);
        _jabsOnlyToggle.SetIsOnWithoutNotify(playlist.ForceJabsOnly);
    }

    public void NoObstaclesToggleSet(bool isOn)
    {
        _noObstaclesText.SetText(isOn ? ON : OFF);
        if (_newPlaylist || _singleSong)
        {
            PlaylistMaker.Instance.SetForceNoObstacles(isOn);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetForceNoObstacles(isOn);
        }
    }

    public void OneHandedToggleSet(bool isOn)
    {
        _oneHandedText.SetText(isOn ? ON : OFF);
        if (_newPlaylist || _singleSong)
        {
            PlaylistMaker.Instance.SetForceOneHanded(isOn);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetForceOneHanded(isOn);
        }
    }

    public void JabsOnlyToggleSet(bool isOn)
    {
        _jabsOnlyText.SetText(isOn ? ON : OFF);
        if (_newPlaylist || _singleSong)
        {
            PlaylistMaker.Instance.SetForceJabsOnly(isOn);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetForceJabsOnly(isOn);
        }
    }
}
