using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Asset Container", menuName = "ScriptableObjects/Environment Asset Container", order = 1)]
public class EnvironmentAssetContainer : ScriptableObject
{
    [SerializeField]
    private string _environmentName;

    [SerializeField]
    private Texture2DArray _targetTextures;

    [SerializeField]
    private Texture2DArray _obstacleTextures;

    [SerializeField]
    private TextureSet[] _globalTextureSets;
    
    [SerializeField]
    private TextureArraySet[] _globalTextureArraySets;

    [SerializeField]
    private Collider _leftGlove;
    
    [SerializeField]
    private Collider _rightGlove;
    
    [SerializeField]
    private BaseTarget _jabTarget;

    [SerializeField]
    private BaseTarget _hookLeftTarget;

    [SerializeField]
    private BaseTarget _hookRightTarget;

    [SerializeField]
    private BaseTarget _uppercutTarget;

    [SerializeField]
    private BlockTarget _blockTarget;

    [SerializeField]
    private BaseObstacle _duckObstacle;

    [SerializeField]
    private BaseObstacle _dodgeLeftObstacle;

    [SerializeField]
    private BaseObstacle _dodgeRightObstacle;

    [SerializeField]
    private BaseHitVFX _baseHitVFX;

    public string EnvironmentName => _environmentName;
    public Texture2DArray TargetTextures => _targetTextures;
    public Texture2DArray ObstacleTextures => _obstacleTextures;
    public TextureSet[] GlobalTextureSets => _globalTextureSets;
    public TextureArraySet[] GlobalTextureArraySets => _globalTextureArraySets;

    public Collider LeftGlove => _leftGlove;
    public Collider RightGlove => _rightGlove;
    public BaseTarget JabTarget => _jabTarget;
    public BaseTarget HookLeftTarget => _hookLeftTarget;
    public BaseTarget HookRightTarget => _hookRightTarget;
    public BaseTarget UppercutTarget => _uppercutTarget;
    public BlockTarget BlockTarget => _blockTarget;
    public BaseObstacle DuckObstacle => _duckObstacle;
    public BaseObstacle DodgeLeftObstacle => _dodgeLeftObstacle;
    public BaseObstacle DodgeRightObstacle => _dodgeRightObstacle;
    public BaseHitVFX BaseHitVFX => _baseHitVFX;
}
