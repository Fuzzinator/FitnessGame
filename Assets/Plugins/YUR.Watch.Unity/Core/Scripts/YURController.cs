using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using YUR.Watch;
using YUR.UI.Displayers;
using System;

namespace YUR.Core
{
    public class YURController : MonoBehaviour
    {

        public bool debug;

        private void Start()
        {
            OnLoadingMessage();

            YURInterface.Instance.OnSDKCodeGenerated += OnSDKCodeGenerated;
            YURInterface.Instance.OnStartLoadData += OnLoadingMessage;
            YURInterface.Instance.OnLoadedDataInitialize += OnSDKInitialized;
        }

        private void OnLoadingMessage()
        {
            DefaultDisplayInfo message = new DefaultDisplayInfo("Loading your profile...");
            WatchManager.Instance.ShowByType(ScreenType.MessageScreen, message);

            if (debug)
                Debug.Log($"Loading your profile...");
        }

        private void OnSDKCodeGenerated(string code,string url)
        {
            string sText = "To LOGIN visit \n" + url + "\nand enter your PIN:";
            PINInfo pin = new PINInfo(sText, code);
            WatchManager.Instance.ShowByType(ScreenType.PINScreen, pin);

            if (debug)
                Debug.Log($"To LOGIN visit \nhttp://yur.watch \nand enter your PIN...");
        }

        private void OnSDKInitialized()
        {
            WatchManager.Instance.ShowByType(ScreenType.MainScreen);
            WatchManager.Instance.SetLastAsDefaultScreen(true);

            if (debug)
                Debug.Log($"Set default screen...");
        }
    }


    public class DefaultDisplayInfo
    {
        public string message;

        public DefaultDisplayInfo(string _message)
        {
            message = _message;
        }
    }

    public class PINInfo : DefaultDisplayInfo
    {
        public string pin;

        public PINInfo(string _message, string _pin) : base(_message)
        {
            message = _message;
            pin = _pin;
        }
    }

    public class ConfirmActionInfo : DefaultDisplayInfo
    {
        public Action method;

        public ConfirmActionInfo(string _message, Action _method) : base(_message)
        {
            message = _message;
            method = _method;
        }
    }
}