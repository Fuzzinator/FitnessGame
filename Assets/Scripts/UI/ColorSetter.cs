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
            UpdateColors(ColorsManager.Instance.ActiveColorSet);
        }

        private void OnDisable()
        {
            RequestColorSelector(false);
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