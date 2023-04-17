using System.Collections;
using UnityEngine;
using YUR.Core;

namespace YUR.Watch.Widgets
{
    public class WidgetColories : WidgetBase
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
            uiText.text = ((int)data.Calories).ToString();
        }

        protected override void UnSetup()
        {
            YURInterface.Instance.OnCResultsLoaded -= Setup;
        }
    }
}