using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets
{
    public class WidgetHeartRate : WidgetBase
    {
        protected void OnEnable()
        {
            Setup(YURInterface.Instance.cResults);
        }
        protected void Start()
        {
            YURInterface.Instance.OnCResultsLoaded += Setup;
        }
        protected void Setup(YUR_SDK.CResults data)
        {
            uiText.text = data.EstHeartRate.ToString("0");
        }

        protected override void UnSetup()
        {
            YURInterface.Instance.OnCResultsLoaded -= Setup;
        }
    }
}