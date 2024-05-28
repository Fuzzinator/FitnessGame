using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetMaterialByPlatform : MonoBehaviour
{
    [SerializeField]
    private PlatformColor[] _platformColors;
    [SerializeField]
    private PlatformFloat[] _platformFloats;
    [SerializeField]
    private Renderer _renderer;

    private void Start()
    {

        foreach (var platform in _platformColors)
        {
#if UNITY_STANDALONE_WIN
            if (platform.TargetPlatform == TargetPlatform.PCVR || platform.TargetPlatform == TargetPlatform.All)
#elif UNITY_ANDROID
            if (platform.TargetPlatform == TargetPlatform.Android || platform.TargetPlatform == TargetPlatform.All)
#endif
            {
                _renderer.sharedMaterial.SetColor(platform.PropertyName, platform.TargetColor);
            }

        }

        foreach (var platform in _platformFloats)
        {
#if UNITY_STANDALONE_WIN
            if (platform.TargetPlatform == TargetPlatform.PCVR || platform.TargetPlatform == TargetPlatform.All)
#elif UNITY_ANDROID
            if (platform.TargetPlatform == TargetPlatform.Android || platform.TargetPlatform == TargetPlatform.All)
#endif
            {
                _renderer.sharedMaterial.SetFloat(platform.PropertyName, platform.TargetFloat);
            }

        }
    }


    [System.Serializable]
    private struct PlatformColor
    {
        [field: SerializeField]
        public TargetPlatform TargetPlatform { get; private set; }
        [field: SerializeField]
        public string PropertyName {get; private set;}
        [field: SerializeField]
        [field: ColorUsage(true, true)]
        public Color TargetColor { get;private set; }
    }
    
    [System.Serializable]
    private struct PlatformFloat
    {
        [field: SerializeField]
        public TargetPlatform TargetPlatform { get; private set; }
        [field: SerializeField]
        public string PropertyName { get; private set; }
        [field: SerializeField]
        public float TargetFloat { get; private set; }
    }
}
