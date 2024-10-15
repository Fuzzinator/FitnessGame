using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class MaterialsManager : MonoBehaviour
{
    public static MaterialsManager Instance { get; private set; }

    [SerializeField]
    private int _targetInstances = 3;

    //[SerializeField]
    //private List<HitSideAndMaterials> _materials;
    [SerializeField]
    private HitSideAndMaterials[] _materialSets = new HitSideAndMaterials[4];

    private Dictionary<Renderer, MaterialAndInstance> _materialsBySource = new Dictionary<Renderer, MaterialAndInstance>();

    /*private readonly Vector3 LeftOffset = new Vector3(-2, 1, 0);
    private readonly Vector3 RightOffset = new Vector3(2, 1, 0);
    private readonly Vector3 TopOffset = new Vector3(0,1, 0);

    private const float LeftRotation = -45;
    private const float RightRotation = 45;*/

    private const string TextureIndex = "_TextureIndex";
    private const string BaseColor1 = "_Base_Color";
    private const string BaseColor2 = "_Base_Color_2";
    /*private const string PositionOffset = "_Position_Change";
    private const string RotationOffset = "_DistanceRotation";*/

    private int _texIndexID;
    private int _baseColor1ID;
    private int _baseColor2ID;
    /*private int _positionOffsetID;
    private int _rotationOffsetID;*/

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
        _texIndexID = Shader.PropertyToID(TextureIndex);
        _baseColor1ID = Shader.PropertyToID(BaseColor1);
        _baseColor2ID = Shader.PropertyToID(BaseColor2);
        /*_positionOffsetID = Shader.PropertyToID(PositionOffset);
        _rotationOffsetID = Shader.PropertyToID(RotationOffset);*/
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public async UniTask SetUpMaterialsAsync(Material targetMaterial, Material obstacleMaterial, Material superMaterial)
    {
        var stopWatch = Stopwatch.StartNew();
        for (var i = 0; i <= _targetInstances; i++)//currently one for each HitSideType
        {
            var type = (HitSideType)i;

            _materialSets[i] = new HitSideAndMaterials();
            _materialSets[i].hitSideType = type;
            var indexOffset = 0;
            switch (type)
            {
                case HitSideType.Left:
                    _materialSets[i].mats = new MatAndIsNote[6];
                    for (var j = 0; j < 3; j++)
                    {
                        var texIndex = Mathf.Clamp(j, 0, 1);
                        var normalMat = new Material(targetMaterial);
                        /*normalMat.SetVector(_positionOffsetID, LeftOffset);
                        normalMat.SetFloat(_rotationOffsetID, LeftRotation);*/
                        normalMat.SetInt(_texIndexID, texIndex);

                        var superMat = new Material(superMaterial);
                        /*superMat.SetVector(_positionOffsetID, LeftOffset);
                        superMat.SetFloat(_rotationOffsetID, LeftRotation);*/
                        superMat.SetInt(_texIndexID, texIndex);

                        _materialSets[i].mats[j + indexOffset] = new MatAndIsNote(normalMat, true, false);
                        indexOffset++;
                        _materialSets[i].mats[j + indexOffset] = new MatAndIsNote(superMat, true, true);
                    }
                    break;
                case HitSideType.Right:
                    _materialSets[i].mats = new MatAndIsNote[6];

                    for (var j = 0; j < 3; j++)
                    {
                        var texIndex = Mathf.Clamp(j, 0, 1);
                        var normalMat = new Material(targetMaterial);
                        /*normalMat.SetVector(_positionOffsetID, RightOffset);
                        normalMat.SetFloat(_rotationOffsetID, RightRotation);*/
                        normalMat.SetInt(_texIndexID, texIndex);

                        var superMat = new Material(superMaterial);
                        /*superMat.SetVector(_positionOffsetID, RightOffset);
                        superMat.SetFloat(_rotationOffsetID, RightRotation);*/
                        superMat.SetInt(_texIndexID, texIndex);

                        _materialSets[i].mats[j + indexOffset] = new MatAndIsNote(normalMat, true, false);
                        indexOffset++;
                        _materialSets[i].mats[j + indexOffset] = new MatAndIsNote(superMat, true, true);
                    }
                    break;
                case HitSideType.Unused://Used for obstacles
                    _materialSets[i].mats = new MatAndIsNote[2];

                    for (var j = 0; j < 2; j++)
                    {
                        var normalMat = new Material(obstacleMaterial);
                        normalMat.SetInt(_texIndexID, j);

                        _materialSets[i].mats[j] = new MatAndIsNote(normalMat, false, false);
                    }
                    break;
                case HitSideType.Block:
                    {
                        var normalMat = new Material(targetMaterial);
                        /*normalMat.SetVector(_positionOffsetID, TopOffset);
                        normalMat.SetFloat(_rotationOffsetID, 0);*/
                        normalMat.SetInt(_texIndexID, 2);
                        _materialSets[i].mats = new[] { new MatAndIsNote(normalMat, true, false) };
                    }
                    break;
            }

            for (var j = 0; j < _materialSets[i].mats.Length; j++)
            {
                var material = _materialSets[i].mats[j];
                var color = ColorsManager.Instance.GetAppropriateColor(_materialSets[i].hitSideType, material.isNote);
                material.material.color = color;
                if (material.superNote)
                {
                    var complementary = new Color(1 - color.r, 1 - color.g, 1 - color.b) * 10;
                    material.material.SetColor(_baseColor1ID, complementary);
                }
            }

            if (stopWatch.ElapsedMilliseconds < 5f)
            {
                continue;
            }
            await UniTask.NextFrame();
            stopWatch.Restart();
        }
        stopWatch.Stop();
    }

    public Material GetMaterial(int index, int subIndex)
    {
        return _materialSets[index].mats[subIndex].material;
    }

    public Material GetInstancedMaterial(Renderer sourceRenderer)
    {
        if (!_materialsBySource.TryGetValue(sourceRenderer, out var materials))
        {
            materials = new MaterialAndInstance(sourceRenderer.sharedMaterial, sourceRenderer.material);
            _materialsBySource[sourceRenderer] = materials;
        }
        return materials.Instanced;
    }


    public bool TryGetOriginalMaterial(Renderer sourceRenderer, out Material original)
    {
        var found = _materialsBySource.TryGetValue(sourceRenderer, out var materials);
        original = materials.Original;
        return found;
    }

    /*private void InstanceMaterials()
    {
        var hex = Shader.PropertyToID("_TextureIndex");
        foreach (var set in _materialSets)
        {
            *//*if (!set.shouldInstance || set.materials.Count <= 0)
            {
                continue;
            }*//*
            set.materials[0].material.SetInt(hex, 0);

            //var keyword = new LocalKeyword(set.materials[0].material.shader, DISSOLVEKEYWORD);
            for (var i = 0; i < _targetInstances; i++)
            {
                var newMat = new Material(set.materials[0].material);
                newMat.name = $"{newMat.name} TexIndex({i + 1})";
                newMat.SetInt(hex, i + 1);
                //newMat.SetKeyword(keyword, false);
                set.materials.Add(new MatAndIsNote(newMat, set.materials[0].isNote));
            }
        }
    }*/

    [Serializable]
    public struct HitSideAndMaterials
    {
        //public bool shouldInstance;
        public HitSideType hitSideType;
        //public List<MatAndIsNote> materials;
        public MatAndIsNote[] mats;
    }

    [Serializable]
    public struct MatAndIsNote
    {
        public Material material;
        public bool isNote;
        public bool superNote;

        public MatAndIsNote(Material material, bool isNote, bool superNote)
        {
            this.material = material;
            this.isNote = isNote;
            this.superNote = superNote;
        }
    }

    private struct MaterialAndInstance
    {
        public Material Original { get; private set; }
        public Material Instanced { get; private set; }

        public MaterialAndInstance(Material original, Material instanced)
        {
            Original = original;
            Instanced = instanced;
        }
    }
}
