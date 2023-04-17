using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Watch
{
    public class WatchScreenCaller : MonoBehaviour
    {
        public ScreenType screen;

        public bool callLastScreen;

        public void Call()
        {
            Call(null);
        }

        public void SwitchToMain()
        {
            var current = WatchManager.Instance.CurrentScreen;

            ScreenType screenToCall = current == ScreenType.MainScreen ? WatchManager.Instance.LastScreen : ScreenType.MainScreen;

            WatchManager.Instance.ShowByType(screenToCall, null);
        }

        public void Call(object obj = null)
        {
            ScreenType screenToCall = callLastScreen ? WatchManager.Instance.LastScreen : screen;

            WatchManager.Instance.ShowByType(screenToCall, obj);
        }
    }
}
