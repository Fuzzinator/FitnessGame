using Cysharp.Text;
using Cysharp.Threading.Tasks;
using OVRSimpleJSON;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlaylistOverridesSetter : MonoBehaviour
{
    [SerializeField]
    private Toggle _noObstaclesToggle;
    [SerializeField]
    private Toggle _oneHandedToggle;
    [SerializeField]
    private Toggle _jabsOnlyToggle;
    [SerializeField, FormerlySerializedAs("_speedModSlider")]
    private Slider _targetSpeedModSlider;
    [SerializeField]
    private Slider _songSpeedModSlider;
    [SerializeField]
    private TextMeshProUGUI _noObstaclesText;
    [SerializeField]
    private TextMeshProUGUI _oneHandedText;
    [SerializeField]
    private TextMeshProUGUI _jabsOnlyText;
    [SerializeField, FormerlySerializedAs("_speedModText")]
    private TextMeshProUGUI _targetSpeedModText;

    [SerializeField]
    private bool _referToPlaylist;

    [SerializeField]
    private bool _newPlaylist;

    [SerializeField]
    private bool _singleSong;

    private const string ON = "On";
    private const string OFF = "Off";

    /*if (sliderValue <= 5)
        {
            return 0.5f + 0.1f * sliderValue;
        }
        else
        {
            return 1.2f + 0.2f * (sliderValue - 6f);
        }*/

    private readonly float[] TargetSpeeds = { .5f, .6f, .7f, .8f, .9f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f};

    private void OnEnable()
    {
        if (_singleSong)
        {
            var noObstacles = PlaylistManager.GetDefaultForceNoObstacles();
            var oneHanded = PlaylistManager.GetDefaultForceOneHanded();
            var jabsOnly = PlaylistManager.GetDefaultForceJabsOnly();
            var targetSpeedMod = PlaylistManager.GetDefaultTargetSpeedMod();
            var songSpeedMod = PlaylistManager.GetDefaultSongSpeedMod();
            NoObstaclesSet(noObstacles);
            OneHandedSet(oneHanded);
            JabsOnlySet(jabsOnly);
            TargetSpeedModSet(targetSpeedMod);
            SongSpeedModSet(songSpeedMod);

            SetToggles(noObstacles, oneHanded, jabsOnly);
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
        var targetSpeedMod = PlaylistMaker.Instance.TargetSpeedMod;
        var songSpeedMod = PlaylistMaker.Instance.SongSpeedMod;

        NoObstaclesSet(noObstacles);
        OneHandedSet(oneHanded);
        JabsOnlySet(jabsOnly);
        TargetSpeedModSet(targetSpeedMod);
        SongSpeedModSet(songSpeedMod);
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
        TargetSpeedModSet(playlist.TargetSpeedMod);
        SongSpeedModSet(playlist.SongSpeedMod);

        SetToggles(playlist.ForceNoObstacles, playlist.ForceOneHanded, playlist.ForceJabsOnly);
    }

    private void SetToggles(bool noObstacles, bool oneHanded, bool jabsOnly)
    {
        _noObstaclesToggle.SetIsOnWithoutNotify(noObstacles);
        _oneHandedToggle.SetIsOnWithoutNotify(oneHanded);
        _jabsOnlyToggle.SetIsOnWithoutNotify(jabsOnly);
    }

    private void SetTargetSpeedModSlider(float targetSpeedMod)
    {
        var speed = PlaylistToTargetSliderSpeedMod(targetSpeedMod);
        _targetSpeedModSlider.SetValueWithoutNotify(speed);
    }

    private void SetSongSpeedModSlider(float songSpeedMod)
    {
        var speed = PlaylistToSongSliderSpeedMod(songSpeedMod);
        _songSpeedModSlider.value = speed;
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

    public void TargetSpeedModSliderSet()
    {
        var value = _targetSpeedModSlider.value;
        var playlistValue = TargetSliderToPlaylistSpeedMod(value);

        PlaylistManager.SetDefaultTargetSpeedMod(TargetSliderToPlaylistSpeedMod(value));
        TargetSpeedModSet(playlistValue);
    }

    public void SongSpeedModSliderSet()
    {
        var value = _songSpeedModSlider.value;
        var playlistValue = SongSliderToPlaylistSpeedMod(value);

        PlaylistManager.SetDefaultSongSpeedMod(playlistValue);
        SongSpeedModSet(playlistValue);
    }

    public void TargetSpeedModValueChanged(float value)
    {
        var playlistValue = TargetSliderToPlaylistSpeedMod(value);
        SetTargetSpeedModText(playlistValue);
    }

    public void SongSpeedModValueChanged(float value)
    {
        //var playlistValue = SongSliderToPlaylistSpeedMod(value);
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

    private float PlaylistToTargetSliderSpeedMod(float speed)
    {
        for (int i = 0; i < TargetSpeeds.Length; i++)
        {
            var thisSpeed = TargetSpeeds[i];
            if(Mathf.Approximately(speed, thisSpeed))
            {
                return i;
            }
        }
        return 1f;
    }

    private float PlaylistToSongSliderSpeedMod(float speed)
    {
        if (Mathf.Approximately(speed, .75f))
        {
            return 0;
        }
        else if (Mathf.Approximately(speed, .875f))
        {
            return 1;
        }
        else if (Mathf.Approximately(speed, 1f))
        {
            return 2;
        }
        else if (Mathf.Approximately(speed, 1.125f))
        {
            return 3;
        }
        else if (Mathf.Approximately(speed, 1.25f))
        {
            return 4;
        }
        else return 2;
    }

    private float TargetSliderToPlaylistSpeedMod(float sliderValue)
    {
        var index = Mathf.RoundToInt(sliderValue);
        return TargetSpeeds[index];
    }

    private float SongSliderToPlaylistSpeedMod(float sliderValue)
    {
        switch ((int)sliderValue)
        {
            case 0:
                return .75f;
            case 1:
                return .875f;
            case 2:
                return 1;
            case 3:
                return 1.125f;
            case 4:
                return 1.25f;
            default:
                return 1;
        }
    }

    private void SetTargetSpeedModText(float speed)
    {
        var asInt = Mathf.RoundToInt(speed * 10);
        speed = asInt * .1f;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            if (Mathf.Approximately(speed, 1f) || Mathf.Approximately(speed, 2f))
            {
                sb.AppendFormat("{0:0}x", speed);
            }
            else
            {
                if (speed < 1)
                {
                    sb.AppendFormat("{0:.0}x", speed);
                }
                else
                {
                    sb.AppendFormat("{0:0.0}x", speed);
                }
            }
            _targetSpeedModText.SetText(sb);
        }
    }

    private void TargetSpeedModSet(float speed)
    {
        SetTargetSpeedModText(speed);
        SetTargetSpeedModSlider(speed);
        if (_newPlaylist || _singleSong)
        {
            PlaylistMaker.Instance.SetTargetSpeedMod(speed);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetTargetSpeedMod(speed);
        }
    }

    private void SongSpeedModSet(float speed)
    {
        SetSongSpeedModSlider(speed);
        if (_newPlaylist || _singleSong)
        {
            PlaylistMaker.Instance.SetSongSpeedMod(speed);
        }
        else if (_referToPlaylist)
        {
            PlaylistManager.Instance.CurrentPlaylist.SetSongSpeedMod(speed);
        }
    }
}
