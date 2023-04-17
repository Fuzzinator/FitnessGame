using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURLeftHand : YURBaseNode
    {
        public static YURLeftHand Instance { get; private set; }
        internal override Transform mainAnchor => Instance?.transform;//{ get => YURWatch.LeftHandAnchor; set => YURWatch.LeftHandAnchor = value; }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            base.Awake();
        }
    }
}