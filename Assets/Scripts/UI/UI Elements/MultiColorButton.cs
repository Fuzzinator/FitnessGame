using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
public class MultiColorButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField]
        private GraphicAndColorSet[] _graphicAndColors = Array.Empty<GraphicAndColorSet>();
        [System.Serializable]
        public class ButtonClickedEvent : UnityEvent
        {
        }

        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_graphicAndColors == null || _graphicAndColors.Length == 0)
            {
                var graphics = GetComponentsInChildren<Graphic>();
                _graphicAndColors = new GraphicAndColorSet[graphics.Length];
                for (var i = 0; i < _graphicAndColors.Length; i++)
                {
                    _graphicAndColors[i] = new GraphicAndColorSet(graphics[i]);
                }
            }
            base.OnValidate();
            if (isActiveAndEnabled)
            {
                // If the transition mode got changed, we need to clear all the transitions, since we don't know what the old transition mode was.
                foreach (var set in _graphicAndColors)
                {
                    StartColorTween(set.Image, Color.white, 0f,true);
                }
                DoStateTransition(currentSelectionState, true);
            }
        }
        #endif
        protected MultiColorButton()
        {
        }

        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            m_OnClick.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (!gameObject.activeInHierarchy)
                return;

            switch (state)
            {
                case SelectionState.Normal:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        var tintColor = colorBlock.normalColor * colorBlock.colorMultiplier;
                        StartColorTween(graphic.Image, tintColor, colorBlock.fadeDuration, false);
                    }
                    break;
                case SelectionState.Highlighted:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        var tintColor = colorBlock.highlightedColor * colorBlock.colorMultiplier;
                        StartColorTween(graphic.Image, tintColor, colorBlock.fadeDuration, false);
                    }
                    break;
                case SelectionState.Pressed:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        var tintColor = colorBlock.pressedColor * colorBlock.colorMultiplier;
                        StartColorTween(graphic.Image, tintColor, colorBlock.fadeDuration, false);
                    }
                    break;
                case SelectionState.Selected:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        var tintColor = colorBlock.selectedColor * colorBlock.colorMultiplier;
                        StartColorTween(graphic.Image, tintColor, colorBlock.fadeDuration, false);
                    }
                    break;
                case SelectionState.Disabled:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        var tintColor = colorBlock.disabledColor * colorBlock.colorMultiplier;
                        StartColorTween(graphic.Image, tintColor, colorBlock.fadeDuration, false);
                    }
                    break;
                default:
                    for (var i = 0; i < _graphicAndColors.Length; i++)
                    {
                        var graphic = _graphicAndColors[i];
                        if (graphic.Image == null)
                            continue;
                        
                        var colorBlock = _graphicAndColors[i].Colors;
                        StartColorTween(graphic.Image, Color.black, colorBlock.fadeDuration, false);
                    }
                    break;
            }

        }

        private void StartColorTween(Graphic graphic, Color tintColor, float fadeDuration, bool instant)
        {
            graphic.CrossFadeColor(tintColor, instant ? 0f : fadeDuration, true, true);
        }

        [Serializable]
        private struct GraphicAndColorSet
        {
            [SerializeField]
            private Graphic _graphic;
            [SerializeField]
            private ColorBlock _overrideColors;
            
            public Graphic Image => _graphic;
            public ColorBlock Colors => _overrideColors;

            public GraphicAndColorSet(Graphic graphic)
            {
                _graphic = graphic;
                _overrideColors = ColorBlock.defaultColorBlock;
            }
        }
    }
}