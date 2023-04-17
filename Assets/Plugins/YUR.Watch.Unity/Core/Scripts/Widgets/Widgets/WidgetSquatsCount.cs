using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;
namespace YUR.Watch.Widgets
{
    public class WidgetSquatsCount : WidgetBase
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
            uiText.text = ((int)data.Squats).ToString();
        }

        protected override void UnSetup()
        {
            YURInterface.Instance.OnCResultsLoaded -= Setup;
        }
    }
}