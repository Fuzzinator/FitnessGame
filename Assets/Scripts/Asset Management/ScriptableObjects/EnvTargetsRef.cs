using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Env Targets Ref", menuName = "ScriptableObjects/Env Asset Refs/Env Targets Ref", order = 1)]
public class EnvTargetsRef : ScriptableObject
{
    [field: SerializeField]
    public string TargetsName { get; private set; }

    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }

    [field: SerializeField]
    public Texture2DArray TargetsTexture { get; private set; }

    [field: SerializeField]
    public BaseTarget JabTarget { get; private set; }

    [field: SerializeField]
    public BaseTarget HookLeftTarget { get; private set; }

    [field: SerializeField]
    public BaseTarget HookRightTarget { get; private set; }

    [field: SerializeField]
    public BaseTarget UppercutTarget { get; private set; }

    [field: SerializeField]
    public BlockTarget BlockTarget { get; private set; }

    [field: SerializeField]
    public BaseHitVFX HitVFX { get; private set; }

    [field: SerializeField]
    public Material TargetsMaterial { get; private set; }

    [field: SerializeField]
    public Material SuperTargetMaterial { get; private set; }
}
