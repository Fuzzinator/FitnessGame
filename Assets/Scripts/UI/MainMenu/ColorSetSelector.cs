using System;
using System.Collections;
using System.Collections.Generic;
using UI.Scrollers;
using UnityEngine;
using ColorSet = ColorsManager.ColorSet;

namespace UI
{
    public class ColorSetSelector : MonoBehaviour
    {
        [SerializeField]
        private AvailableColorSetsScrollerController _controller;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private ColorSetEditor _colorSetEditor;
        private void OnEnable()
        {
            _controller.Refresh();
        }

        private void OnDisable()
        {
            RequestCloseSetEditor();
        }

        public void SetActiveColorSet(ColorSet set)
        {
            ColorsManager.Instance.SetActiveColorSet(set);
        }

        public void RequestOpenSetEditor(ColorSet set)
        {
            
        }

        public void RequestCloseSetEditor()
        {
            
        }
    }
}