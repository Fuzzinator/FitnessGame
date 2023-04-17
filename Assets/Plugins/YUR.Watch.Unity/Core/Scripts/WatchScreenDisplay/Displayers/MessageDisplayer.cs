using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YUR.Core;
namespace YUR.UI.Displayers
{
    public class MessageDisplayer : BaseDisplayer
    {
        public TextMeshProUGUI messageText;

        protected override void DisplayAction(object obj = null)
        {
            base.DisplayAction(obj);

            DefaultDisplayInfo info = (DefaultDisplayInfo)obj;

            if (info == null)
                return;

            messageText.text = info.message;
        }

    }
}