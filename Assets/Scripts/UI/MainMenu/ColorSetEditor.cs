using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ColorSetEditor : MonoBehaviour
    {
        [SerializeField]
        private Toggle[] _toggles;

        [SerializeField]
        private Slider _RGBSlider;

        [SerializeField]
        private TwoDSlider _HSVSlider;
        
        [SerializeField]
        private Image _RGBSliderHandle;

        [SerializeField]
        private Image _HSVSliderHandle;

        [SerializeField]
        private Image _HSVFill;
        
        [SerializeField]
        private ColorsManager.ColorSet _activeColorSet;

        [SerializeField]
        private CanvasGroup _parentCanvasGroup;
        private int _activeSetIndex;

        private int _activeColorIndex;

        public void CreateNewColorSet()
        {
            var colorSet = ColorsManager.ColorSet.Default;
            var index = ColorsManager.Instance.AddColorSet(colorSet);
            
            RequestShowEditor(colorSet, index);
        }

        public void DeleteColorSet()
        {
            ColorsManager.Instance.RemoveCurrentColorSet();
        }

        public void ResetCurrentColorSet()
        {
            ColorsManager.Instance.UpdateColorSet(ColorsManager.ColorSet.Default, ColorsManager.Instance.ActiveSetIndex);
        }

        public void RequestShowEditor(ColorsManager.ColorSet setToEdit, int index)
        {
            _activeColorSet = setToEdit;
            _activeSetIndex = index;
            _parentCanvasGroup.interactable = false;
            gameObject.SetActive(true);
            SetActiveColor(0);
            ResetDisplay();
        }

        private void ResetDisplay()
        {
            _toggles[0].isOn = true;
            _toggles[0].targetGraphic.color = _activeColorSet.LeftController;
            _toggles[1].targetGraphic.color = _activeColorSet.RightController;
            _toggles[2].targetGraphic.color = _activeColorSet.BlockColor;
            _toggles[3].targetGraphic.color = _activeColorSet.ObstacleColor;
            
            SetDisplayedColors(_activeColorSet.LeftController);
        }

        public void CloseEditor()
        {
            _parentCanvasGroup.interactable = true;
            SaveChanges();
            gameObject.SetActive(false);
        }

        public void SaveChanges()
        {
            ColorsManager.Instance.UpdateColorSet(_activeColorSet, _activeSetIndex);
        }

        public void SetDisplayedColors(Color color)
        {
            Color.RGBToHSV(color, out var hue, out var sat, out var val);
            _HSVFill.color = Color.HSVToRGB(hue, 1, 1);
            
            _RGBSlider.SetValueWithoutNotify(hue);
            _HSVSlider.SetValueWithoutNotify(new Vector2(sat, val));
            
            _RGBSliderHandle.color = color;
            _HSVSliderHandle.color = color;
        }

        public void SliderMoved()
        {
            var color = Color.HSVToRGB(_RGBSlider.value, _HSVSlider.normalizedValue.x, _HSVSlider.normalizedValue.y);
            _RGBSliderHandle.color = color;
            _HSVSliderHandle.color = color;
            _toggles[_activeColorIndex].targetGraphic.color = color;
            _HSVFill.color = Color.HSVToRGB(_RGBSlider.value, 1, 1);
            UpdateColorSet(color);
        }
        

        public void SetActiveColor(int colorIndex)
        {
            _activeColorIndex = colorIndex;
            SetDisplayedColors(_toggles[colorIndex].targetGraphic.color);
        }

        private void UpdateColorSet(Color color)
        {
            var set = _activeColorSet;
            switch (_activeColorIndex)
            {
                case 0:
                    _activeColorSet =
                        new ColorsManager.ColorSet(color, set.RightController, set.BlockColor, set.ObstacleColor);
                    break;
                case 1:
                    _activeColorSet =
                        new ColorsManager.ColorSet(set.LeftController, color, set.BlockColor, set.ObstacleColor);
                    break;
                case 2:
                    _activeColorSet =
                        new ColorsManager.ColorSet(set.LeftController, set.RightController,color, set.ObstacleColor);
                    break;
                case 3:
                    _activeColorSet =
                        new ColorsManager.ColorSet(set.LeftController, set.RightController, set.BlockColor, color);
                    break;
            }
        }
    }
}