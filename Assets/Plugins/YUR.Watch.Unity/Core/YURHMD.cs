using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURHMD : YURBaseNode
    {
        public static YURHMD Instance { get; private set; }
        public static bool IsNull { get; private set; }
        internal override Transform mainAnchor => IsNull ? null : Instance.transform;//{ get => YURWatch.HMDAnchor; set => YURWatch.HMDAnchor = value; }

        protected override void Awake()
        {
            if(Instance == null)
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
