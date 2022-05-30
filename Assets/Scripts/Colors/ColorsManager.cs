using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ColorsManager : MonoBehaviour
{
    public static ColorsManager Instance { get; private set; }

    [Header("Controllers & Notes")]
    [SerializeField]
    private Color _leftController;

    [SerializeField]
    private Color _rightController;

    [SerializeField]
    private Color _blockColor;

    [SerializeField]
    private Color _obstacleColor;

    [Space]
    [Header("Environment")]
    [SerializeField]
    private Color _leftEnvironment;

    [SerializeField]
    private Color _rightEnvironment;

    [SerializeField]
    private Color _centerEnvironment;

    [FormerlySerializedAs("_texture2DArray")] [SerializeField]
    private Texture2DArray _targetTexturesArray;

    [SerializeField]
    private Texture2DArray _obstacleTexturesArray;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        UpdateTextureSets();
    }

    private void OnValidate()
    {
        UpdateTextureSets();
    }

    public void UpdateTextureSets()
    {
        Shader.SetGlobalTexture("_TargetTextures", _targetTexturesArray);
        Shader.SetGlobalTexture("_ObstacleTextures", _obstacleTexturesArray);
    }

    public Color GetAppropriateColor(HitSideType hitSide, bool isNote = true)
    {
        return hitSide switch
        {
            HitSideType.Left => isNote?_leftController:_leftEnvironment,
            HitSideType.Right => isNote?_rightController:_rightEnvironment,
            HitSideType.Block =>  isNote?_blockColor:_centerEnvironment,
            _ => Color.white
        };
    }
}
