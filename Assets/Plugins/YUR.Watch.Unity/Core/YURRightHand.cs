using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURRightHand : YURBaseNode
    {
        public static YURRightHand Instance { get; private set; }
        public static bool IsNull { get; private set; } = true;
        internal override Transform mainAnchor => IsNull ? null : Instance.transform;//{ get => YURWatch.RightHandAnchor; set => YURWatch.RightHandAnchor = value; }
        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                IsNull = false;
            }
            else
            {
                Destroy(this);
            }
            base.Awake();
        }

        protected void OnDestroy()
        {
            if (this == Instance)
            {
                IsNull = true;
            }
        }
    }
}