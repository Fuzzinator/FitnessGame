using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Env Obstacles Ref", menuName = "ScriptableObjects/Env Asset Refs/Env Obstacles Ref", order = 2)]
public class EnvObstaclesRef : ScriptableObject
{
    [field: SerializeField]
    public string ObstaclesName { get; private set; }
    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }

    [field: SerializeField]
    public Texture2DArray ObstacleTexture { get; private set; }

    [field: SerializeField]
    public BaseObstacle DuckObstacle { get; private set; }

    [field: SerializeField]
    public BaseObstacle DodgeLeftObstacle { get; private set; }

    [field: SerializeField]
    public BaseObstacle DodgeRightObstacle { get; private set; }
    [field: SerializeField]
    public Material ObstacleMaterial { get; private set; }
}
