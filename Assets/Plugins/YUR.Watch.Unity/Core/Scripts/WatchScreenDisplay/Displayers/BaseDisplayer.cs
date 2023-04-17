using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Watch;

namespace YUR.UI.Displayers
{
    public class BaseDisplayer : MonoBehaviour
    {
        [HideInInspector] public bool debug = false;

        public ScreenType screenType;
        public GameObject container;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Show()
        {
            Show(true);
        }

        public void Hide()
        {
            Hide(true);
        }

        public virtual void Show(bool useAnimation, object obj = null)
        {
            if (_animator != null && useAnimation)
            {
                container.SetActive(true);
                _animator.SetTrigger("Show");
            }
            else
                container.SetActive(true);

            DisplayAction(obj);
        }

        public virtual void Hide(bool useAnimation, object obj = null)
        {
            if (_animator != null && useAnimation)
                _animator.SetTrigger("Hide");
            else
                container.SetActive(false);

            UnDisplayAction(obj);
        }

        protected virtual void DisplayAction(object obj = null)
        {
            if (obj == null)
                return;
        }

        protected virtual void UnDisplayAction(object obj = null)
        {
            if (obj == null)
                return;
        }

    }
}