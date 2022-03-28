using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAudioInteractHook : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private bool _registerPointerEnter = true;
    [SerializeField]
    private bool _registerPointerExit = true;
    [SerializeField]
    private bool _registerPointerClick = true;
    [SerializeField]
    private bool _registerPointerDown = true;
    [SerializeField]
    private bool _registerPointerUp = true;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_registerPointerEnter)
        {
            return;
        }
        UIAudioController.Instance.Hovered();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_registerPointerExit)
        {
            return;
        }
        UIAudioController.Instance.Unhovered();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_registerPointerClick)
        {
            return;
        }
        UIAudioController.Instance.Clicked();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_registerPointerDown)
        {
            return;
        }
        UIAudioController.Instance.PointerPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_registerPointerUp)
        {
            return;
        }
        UIAudioController.Instance.PointerReleased();
    }
}
