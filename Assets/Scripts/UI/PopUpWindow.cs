using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopUpWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private CanvasGroup _parentCanvas;
    private bool _hovered;

    protected void OnEnable()
    {
        InputManager.Instance.MainInput[InputManager.SelectedRight].performed += TryHideFromDeselect;
        InputManager.Instance.MainInput[InputManager.SelectedLeft].performed += TryHideFromDeselect;
    }

    protected void OnDisable()
    {
        _hovered = false;
        InputManager.Instance.MainInput[InputManager.SelectedLeft].performed -= TryHideFromDeselect;
        InputManager.Instance.MainInput[InputManager.SelectedRight].performed -= TryHideFromDeselect;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
    }

    private void TryHideFromDeselect(InputAction.CallbackContext context)
    {
        if (_hovered)
        {
            return;
        }
        Hide();
    }

    public void Hide()
    {
        if(_parentCanvas != null)
        {
            _parentCanvas.interactable = true;
        }
        gameObject.SetActive(false);
    }
}
