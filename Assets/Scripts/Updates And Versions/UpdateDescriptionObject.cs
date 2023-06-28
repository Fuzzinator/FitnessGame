using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Update Description", menuName = "ScriptableObjects/Update Description", order = 3)]
public class UpdateDescriptionObject : ScriptableObject
{
    [field: SerializeField]
    public TargetPlatform TargetPlatform { get; private set; }

    [field: SerializeField]
    public string VersionNumber { get; private set; }
    [field: SerializeField, TextArea(2, 10)]
    public string ShortDescription { get; private set; }

    [field: SerializeField, TextArea(5, 20)]
    public string Description { get; private set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(VersionNumber))
        {
            VersionNumber = $"<style=Title>Version {UnityEditor.PlayerSettings.bundleVersion}</style>";
        }
    }
#endif
}
