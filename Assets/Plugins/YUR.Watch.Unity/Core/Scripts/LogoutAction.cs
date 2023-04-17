using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR;
using YUR.Core;
using YUR.Watch;

namespace YUR.Core.YURActions
{
    public class LogoutAction : MonoBehaviour
    {
        public void Logout()
        {
            ConfirmActionInfo confirmSettings = new ConfirmActionInfo("Do you want to log out?", () =>
            {
                YURInterface.Instance.Logout();
                WatchManager.Instance.ShowByType(ScreenType.MainScreen);
            });
            WatchManager.Instance.ShowByType(ScreenType.ConfirmScreen, confirmSettings);
        }
    }
}