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
    public EnvAssetRef Gloves { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Targets { get; private set; }

    [field: SerializeField]
    public EnvAssetRef Obstacles { get; private set; }

    public string GlovesName => Gloves?.AssetName;

    public string TargetsName => Targets?.AssetName;

    public string ObstaclesName => Obstacles?.AssetName;

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

    public void SetGloves(EnvAssetRef asset)
    {
        Gloves = asset;
    }

    public void SetTargets(EnvAssetRef targets)
    {
        Targets = targets;
    }

    public void SetObstacles(EnvAssetRef obstacles)
    {
        Obstacles = obstacles;
    }

    public CustomEnvironment(string environmentName, string skyboxName = null, string skyboxPath = null,
        string skyboxDepthName = null, string skyboxDepthPath = null, float skyboxBrightness = 1,
        string meshPath = null, EnvAssetRef gloves = null, EnvAssetRef targets = null, EnvAssetRef obstacles = null)
    {
        EnvironmentName = environmentName;
        SkyboxName = skyboxName;
        SkyboxPath = skyboxPath;
        SkyboxDepthName = skyboxDepthName;
        SkyboxDepthPath = skyboxDepthPath;
        SkyboxBrightness = skyboxBrightness;
        MeshPath = meshPath;

        Gloves = gloves;
        Targets = targets;
        Obstacles = obstacles;
    }
}
