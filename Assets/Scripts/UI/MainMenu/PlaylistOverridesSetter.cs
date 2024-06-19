using Cysharp.Threading.Tasks;
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
        if(_singleSong)
        {
            var noObstacles = PlaylistManager.GetDefaultForceNoObstacles();
            var oneHanded = PlaylistManager.GetDefaultForceOneHanded();
            var jabsOnly = PlaylistManager.GetDefaultForceJabsOnly();
            NoObstaclesSet(noObstacles);
            OneHandedSet(oneHanded);
            JabsOnlySet(jabsOnly);

            SetToggles(noObstacles, oneHanded,jabsOnly);
        }
        else if (_referToPlaylist)
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
        PlaylistMaker.Instance.RefreshOverrides();
        var noObstacles = PlaylistMaker.Instance.ForceNoObstacles;
        var oneHanded = PlaylistMaker.Instance.ForceOneHanded;
        var jabsOnly = PlaylistMaker.Instance.ForceJabsOnly;

        NoObstaclesSet(noObstacles);
        OneHandedSet(oneHanded);
        JabsOnlySet(jabsOnly);
    }

    private void UpdateFromPlaylist(Playlist playlist)
    {
        if (playlist == null)
        {
            return;
        }

        NoObstaclesSet(playlist.ForceNoObstacles);
        OneHandedSet(playlist.ForceOneHanded);
        JabsOnlySet(playlist.ForceJabsOnly);

        SetToggles(playlist.ForceNoObstacles, playlist.ForceOneHanded, playlist.ForceJabsOnly);
    }

    private void SetToggles(bool noObstacles, bool oneHanded, bool jabsOnly)
    {
        _noObstaclesToggle.SetIsOnWithoutNotify(noObstacles);
        _oneHandedToggle.SetIsOnWithoutNotify(oneHanded);
        _jabsOnlyToggle.SetIsOnWithoutNotify(jabsOnly);
    }

    public void NoObstaclesToggleSet(bool isOn)
    {
        PlaylistManager.SetDefaultForceNoObstacles(isOn);

        NoObstaclesSet(isOn);
    }

    public void OneHandedToggleSet(bool isOn)
    {
        PlaylistManager.SetDefaultForceOneHanded(isOn);

        OneHandedSet(isOn);
    }

    public void JabsOnlyToggleSet(bool isOn)
    {
        PlaylistManager.SetDefaultForceJabsOnly(isOn);

        JabsOnlySet(isOn);
    }
    
    public void NoObstaclesSet(bool isOn)
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

    public void OneHandedSet(bool isOn)
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

    public void JabsOnlySet(bool isOn)
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
