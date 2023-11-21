using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Env Gloves Ref", menuName = "ScriptableObjects/Env Asset Refs/Env Gloves Ref", order = 3)]
public class EnvGlovesRef : ScriptableObject
{
    [field: SerializeField]
    public string GlovesName { get; private set; }

    [field: SerializeField]
    public Sprite Thumbnail { get; private set; }
    //public Texture2D GloveTexture { get; private set;}

    [field: SerializeField]
    public GloveController LeftGlove { get; private set; }

    [field: SerializeField]
    public GloveController RightGlove { get; private set;}
}
