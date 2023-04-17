using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURHMD : YURBaseNode
    {
        public static YURHMD Instance { get; private set; }
        internal override Transform mainAnchor => Instance?.transform;//{ get => YURWatch.HMDAnchor; set => YURWatch.HMDAnchor = value; }

        protected override void Awake()
        {
            if(Instance == null)
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
