using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YUR.Watch;
using YUR.Core;

namespace YUR.UI.Displayers
{
    public class ConfirmDisplayer : BaseDisplayer
    {
        public TextMeshProUGUI messageText;

        private Action method;

        private ScreenType _screenCaller;

        protected override void DisplayAction(object obj = null)
        {
            base.DisplayAction(obj);

            ConfirmActionInfo info = (ConfirmActionInfo)obj;

            if (info == null)
                return;

            _screenCaller = WatchManager.Instance.LastScreen;

            messageText.text = info.message;
            method = info.method;
        }

        public void ExecuteConfirmAction()
        {
            WatchManager.Instance.SetLastAsDefaultScreen();
            method?.Invoke();
        }

        public void ExecuteCancelAction()
        {
            WatchManager.Instance.ShowByType(_screenCaller);
        }
    }
}