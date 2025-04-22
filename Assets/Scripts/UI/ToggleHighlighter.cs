using UnityEngine;
using UnityEngine.UI;

public class ToggleHighlighter : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private Color _onColor;
    [SerializeField]
    private Color _offColor;

    private void OnValidate()
    {
        if(_image != null)
        {
            return;
        }
        
        _image = GetComponent<Image>();
    }

    public void OnValueChanged(bool isOn)
    {
        _image.color = isOn ? _onColor : _offColor;
    }
}
