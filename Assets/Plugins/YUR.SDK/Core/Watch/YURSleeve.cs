using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.SDK.Core.Enums;

namespace YUR.SDK.Core.Watch
{
    public class YURSleeve : MonoBehaviour
    {
        [field : SerializeField]
        public SleeveState State {get; private set;}

        [field: SerializeField]
        public TriggerContent TriggerContent { get; private set; }

        [field: SerializeField]
        public MeshRenderer MeshRenderer { get; private set; }

        [field: SerializeField]
        public bool HasMeshRenderer { get; private set; }

        private void OnValidate()
        {
            if(TriggerContent == null)
            {
                TryGetComponent(out TriggerContent content);
                TriggerContent = content;
            }

            if(MeshRenderer == null)
            {
                TryGetComponent(out MeshRenderer meshRenderer);
                MeshRenderer = meshRenderer;
            }

            HasMeshRenderer = MeshRenderer != null;
        }
    }
}