using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets // TODO: Finish setup with AR
{
    public class WidgetSquatsInWorkout : WidgetBase
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
            uiText.text = ((int)data.Squats).ToString(); // TODO: Get right value with workout 
        }

        protected override void UnSetup()
        {
            YURInterface.Instance.OnCResultsLoaded -= Setup;
        }
    }
}