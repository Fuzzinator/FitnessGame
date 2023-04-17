using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets
{
    public class WidgetYURLevel : WidgetBase
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

            uiText.text = profile.level.level.ToString();
        }

        protected override void UnSetup()
        {
            WidgetManager.Instance.OnProfileInitialize -= Setup;
        }

    }
}