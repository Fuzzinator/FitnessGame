using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YUR.Core
{
    public class YURBaseNode : MonoBehaviour
    {
        internal virtual Transform mainAnchor { get; set; }
        internal GameObject thisAnchor = null;

        protected virtual void Awake()
        {
            thisAnchor = new GameObject("YURNode");
            thisAnchor.transform.SetParent(transform, false);
        }

        private void OnEnable()
        {
            mainAnchor = thisAnchor.transform;
        }

        private void OnDisable()
        {
            if (mainAnchor == thisAnchor.transform)
            {
                mainAnchor = null;
            }
        }
    }
}