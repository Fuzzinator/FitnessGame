using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class AudioInteractHook : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
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

            AudioController.Instance.Hovered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_registerPointerExit)
            {
                return;
            }

            AudioController.Instance.Unhovered();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_registerPointerClick)
            {
                return;
            }

            AudioController.Instance.Clicked();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_registerPointerDown)
            {
                return;
            }

            AudioController.Instance.PointerPressed();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_registerPointerUp)
            {
                return;
            }

            AudioController.Instance.PointerReleased();
        }
    }
}