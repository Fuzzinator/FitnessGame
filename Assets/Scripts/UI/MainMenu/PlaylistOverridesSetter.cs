using Cysharp.Text;
using Cysharp.Threading.Tasks;
using OVRSimpleJSON;
using System;
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
    private Slider _speedModSlider;
    [SerializeField]
    private TextMeshProUGUI _noObstaclesText;
    [SerializeField]
    private TextMeshProUGUI _oneHandedText;
    [SerializeField]
    private TextMeshProUGUI _jabsOnlyText;
    [SerializeField]
    private TextMeshProUGUI _speedModText;

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
        if (_singleSong)
        {
            var noObstacles = PlaylistManager.GetDefaultForceNoObstacles();
            var oneHanded = PlaylistManager.GetDefaultForceOneHanded();
            var jabsOnly = PlaylistManager.GetDefaultForceJabsOnly();
            var targetSpeedMod = PlaylistManager.GetDefaultTargetSpeedMod();
            NoObstaclesSet(noObstacles);
            OneHandedSet(oneHanded);
            JabsOnlySet(jabsOnly);
            TargetSpeedModSet(targetSpeedMod);

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

        NoObstaclesSet(noObstacles);
        OneHandedSet(oneHanded);
        JabsOnlySet(jabsOnly);
        TargetSpeedModSet(targetSpeedMod);
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
        var speed = PlaylistToSliderSpeedMod(targetSpeedMod);
        _speedModSlider.SetValueWithoutNotify(speed);
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
        var value = _speedModSlider.value;
        var playlistValue = SliderToPlaylistSpeedMod(value);

        PlaylistManager.SetDefaultTargetSpeedMod(SliderToPlaylistSpeedMod(value));
        TargetSpeedModSet(playlistValue);
    }

    public void TargetSpeedModValueChanged(float value)
    {
        var playlistValue = SliderToPlaylistSpeedMod(value);
        SetTargetSpeedModText(playlistValue);
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

    private float PlaylistToSliderSpeedMod(float speed)
    {
        if (speed <= 1.0)
        {
            return ((speed - 0.5f) / 0.1f);
        }
        else
        {
            return 6 + ((speed - 1.2f) / 0.2f);
        }
    }

    private float SliderToPlaylistSpeedMod(float sliderValue)
    {
        if (sliderValue <= 5)
        {
            return 0.5f + 0.1f * sliderValue;
        }
        else
        {
            return 1.2f + 0.2f * (sliderValue - 6f);
        }
    }

    private float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        var normalized = (value - oldMin) / (oldMax - oldMin);
        var curved = Mathf.Pow(normalized, 2);
        return newMin + curved * (newMax - newMin);
    }

    private void SetTargetSpeedModText(float speed)
    {
        var asInt = Mathf.RoundToInt(speed * 10);
        speed = asInt * .1f;
        using (var sb = ZString.CreateStringBuilder(true))
        {
            if (Mathf.Approximately(speed, 1f) || Mathf.Approximately(speed, 2f))
            {
                sb.AppendFormat("{0:0}x",speed);
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
            _speedModText.SetText(sb);
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
}
