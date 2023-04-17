using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURRightHand : YURBaseNode
    {
        public static YURRightHand Instance { get; private set; }
        internal override Transform mainAnchor => Instance?.transform;//{ get => YURWatch.RightHandAnchor; set => YURWatch.RightHandAnchor = value; }
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