using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURLeftHand : YURBaseNode
    {
        public static YURLeftHand Instance { get; private set; }
        public static bool IsNull { get; private set; } = true;
        internal override Transform mainAnchor => IsNull ? null : Instance.transform;//{ get => YURWatch.LeftHandAnchor; set => YURWatch.LeftHandAnchor = value; }

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