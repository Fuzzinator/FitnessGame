using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ColorSetter : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private ColorSetSelector _selector;
        
        [SerializeField]
        private Image _leftGloveColor;

        [SerializeField]
        private Image _rightGloveColor;

        [SerializeField]
        private Image _blockNoteColor;

        [SerializeField]
        private Image _obstacleColor;


        private void Awake()
        {
            ColorsManager.Instance.activeColorSetUpdated.AddListener(UpdateColors);
        }

        private void OnEnable()
        {
            UpdateFromPlaylist(PlaylistManager.Instance.CurrentPlaylist);
            PlaylistManager.Instance.currentPlaylistUpdated.AddListener(UpdateFromPlaylist);
        }

        private void OnDisable()
        {
            RequestColorSelector(false);
            PlaylistManager.Instance.currentPlaylistUpdated.RemoveListener(UpdateFromPlaylist);
        }

        private void UpdateFromPlaylist(Playlist playlist)
        {
            var colorSet = new ColorsManager.ColorSet();
            if (playlist != null && playlist.TargetColors.IsValid)
            {
                colorSet = playlist.TargetColors;
                ColorsManager.Instance.SetColorSetOverride(colorSet);
            }
            else
            {
                colorSet = ColorsManager.Instance.ActiveColorSet;
            }
            
            UpdateColors(colorSet);
        }
        
        private void UpdateColors(ColorsManager.ColorSet colorSet)
        {
            _leftGloveColor.color = colorSet.LeftController;
            _rightGloveColor.color = colorSet.RightController;
            _blockNoteColor.color = colorSet.BlockColor;
            _obstacleColor.color = colorSet.ObstacleColor;
        }


        public void RequestColorSelector(bool enabled)
        {
            _canvasGroup.interactable = !enabled;
            _selector.gameObject.SetActive(enabled);
        }
    }
}