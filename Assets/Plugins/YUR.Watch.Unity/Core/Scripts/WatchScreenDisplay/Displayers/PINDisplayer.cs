using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YUR.Core;

namespace YUR.UI.Displayers
{
    public class PINDisplayer : BaseDisplayer
    {
        public TextMeshProUGUI messageText;
        public TextMeshProUGUI pinText;

        protected override void DisplayAction(object obj = null)
        {
            base.DisplayAction(obj);

            PINInfo info = (PINInfo)obj;
            
            if (info == null)
                return;
            
            messageText.text = info.message;
            pinText.text = info.pin;
        }

    }

}