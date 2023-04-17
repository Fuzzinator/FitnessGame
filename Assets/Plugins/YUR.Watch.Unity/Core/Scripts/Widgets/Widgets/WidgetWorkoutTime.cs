using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets // TODO: Finish setup with AR
{
    public class WidgetWorkoutTime : WidgetBase
    {

        protected void OnEnable()
        {
            Setup(WidgetManager.Instance.yurProfile);
        }

        protected void Start()
        {
            WidgetManager.Instance.OnProfileInitialize += Setup;
        }

        protected override void Setup(object data)
        {
            base.Setup(data);

            YURProfile profile = (YURProfile)data;

            if (profile == null)
                return;

            DateTime time = new DateTime();

            uiText.text = $"{time.Hour.ToString("00")}:{time.Minute.ToString("00")}";// TODO: Get right value with workout 
        }

        protected override void UnSetup()
        {
            WidgetManager.Instance.OnProfileInitialize -= Setup;
        }
    }
}