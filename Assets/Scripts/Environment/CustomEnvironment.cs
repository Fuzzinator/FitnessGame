using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomEnvironment
{
    [field: SerializeField]
    public string EnvironmentName { get; private set; }

    [field: SerializeField]
    public string SkyboxName { get; private set; }

    [field: SerializeField]
    public string SkyboxPath { get; private set; }

    [field: SerializeField]
    public string SkyboxDepthName { get; private set; }

    [field: SerializeField]
    public string SkyboxDepthPath { get; private set; }
    [field: SerializeField]
    public float SkyboxBrightness { get; private set; }

    [field: SerializeField]
    public string MeshPath { get; private set; }

    [field: SerializeField]
    public string ObjectsPath { get; private set; }

    [field: SerializeField]
    public string VFXPath { get; private set; }

    public Sprite SkyboxSprite { get; private set; }
    public Sprite DepthSprite { get; private set; }

    [NonSerialized]
    public bool isValid;

    public void SetSkyboxSprite(Sprite sprite)
    {
        SkyboxSprite = sprite;
    }

    public void ClearSkyboxSprite()
    {
        SkyboxSprite = null;
    }

    public void SetDepthSprite(Sprite sprite)
    {
        DepthSprite = sprite;
    }

    public void ClearDepthSprite()
    {
        DepthSprite = null;
    }

    public void SetName(string name)
    {
        EnvironmentName = name;
    }

    public void SetSkyboxName(string skyboxName)
    {
        SkyboxName = skyboxName;
    }

    public void SetSkyboxPath(string texture)
    {
        SkyboxPath = texture;
    }

    public void SetSkyboxBrightness(float brightness)
    {
        SkyboxBrightness = brightness;
    }

    public CustomEnvironment(string environmentName, string skyboxName = null, string skyboxPath = null,
        string skyboxDepthName = null, string skyboxDepthPath = null, float skyboxBrightness = 1,
        string meshPath = null, string objectsPath = null, string vFXPath = null)
    {
        EnvironmentName = environmentName;
        SkyboxName = skyboxName;
        SkyboxPath = skyboxPath;
        SkyboxDepthName = skyboxDepthName;
        SkyboxDepthPath = skyboxDepthPath;
        SkyboxBrightness = skyboxBrightness;
        MeshPath = meshPath;
        ObjectsPath = objectsPath;
        VFXPath = vFXPath;
    }
}
