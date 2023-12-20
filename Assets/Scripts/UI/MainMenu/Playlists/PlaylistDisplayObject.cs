using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using TMPro;
using UI.Scrollers;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistDisplayObject : MonoBehaviour
{
    [SerializeField]
    private Image _combinedImage;
    [SerializeField]
    private Image[] _images = new Image[4];

    [SerializeField]
    private TextMeshProUGUI _playlistTitle;

    [SerializeField]
    private TextMeshProUGUI _playlistDetails;
    
    [SerializeField]
    private DualPlaylistDisplayCellView _cellView;

    private Playlist _playlist;
    private bool _initialized = false;
    private CancellationToken _cancellationToken;
    
    private const string TITLE = "<line-height=125%>{0}";
    private const string DETAILS = "{0:00}:{1:00}\n{2}\n{3}";

    private void Initialize()
    {
        _initialized = true;
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void SetData(Playlist playlist)
    {
        if (!_initialized)
        {
            Initialize();
        }

        if (playlist == null || !playlist.isValid)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        _playlist = playlist;
        
        SetImages().Forget();
        SetTitle(_playlist.PlaylistName);
        SetDetails();
    }

    private async UniTaskVoid SetImages()
    {
        if (_playlist?.PlaylistImage != null)
        {
            _combinedImage.gameObject.SetActive(true);
            _combinedImage.sprite = _playlist.PlaylistImage;
            foreach (var img in _images)
            {
                img.gameObject.SetActive(false);
            }
            return;
        }

        _combinedImage.gameObject.SetActive(false);
        for (var i = 0; i < _images.Length; i++)
        {
            Sprite image = null;
            if (_playlist.Items.Length > i)
            {
                image = await _playlist.Items[i].SongInfo.LoadImage(_cancellationToken);
            }

            _images[i].gameObject.SetActive(true);
            _images[i].sprite = image;
            _images[i].enabled = image != null;
        }
    }
    
    private void SetTitle(string title)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(TITLE, title);

            var buffer = sb.AsArraySegment();
            _playlistTitle.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    private void SetDetails()
    {
        var minutes = (int)Mathf.Floor(_playlist.Length / 60);
        var seconds = (int)Mathf.Floor(_playlist.Length % 60);
        var difficulty = _playlist.DifficultyEnum.Readable();
        var gameMode = _playlist.TargetGameMode.Readable();
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.AppendFormat(DETAILS, minutes, seconds, difficulty, gameMode);

            var buffer = sb.AsArraySegment();
            _playlistDetails.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public void PlayPlaylist()
    {
        if (PlaylistManager.Instance != null)
        {
            PlaylistManager.Instance.CurrentPlaylist = _playlist;
            
            if (EnvironmentControlManager.Instance != null)
            {
                EnvironmentControlManager.Instance.LoadFromPlaylist(_playlist);
            }
            
            _cellView.PlayPlaylist();
        }
    }

    public void ViewPlaylist()
    {
        if (PlaylistManager.Instance != null)
        {
            PlaylistManager.Instance.CurrentPlaylist = _playlist;
            _cellView.ViewPlaylist();
        }
    }
}