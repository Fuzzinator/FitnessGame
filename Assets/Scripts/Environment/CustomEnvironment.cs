using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomEnvironment
{
    [field: SerializeField]
    public string EnvironmentName { get; private set; }

    [field: SerializeField]
    public bool CustomSkybox { get; private set; }

    [field: SerializeField]
    public string SkyboxPath { get; private set; }

    [field: SerializeField]
    public string SkyboxDepthPath { get; private set; }
    [field: SerializeField]
    public float SkyboxBrightness { get; private set; }


    [field: SerializeField]
    public bool CustomMesh { get; private set; }

    [field: SerializeField]
    public string MeshPath { get; private set; }


    [field: SerializeField]
    public bool CustomObjects { get; private set; }

    [field: SerializeField]
    public string ObjectsPath { get; private set; }

    [field: SerializeField]
    public bool CustomVFX  { get; private set; }

    [field: SerializeField]
    public string VFXPath { get; private set; }
}
