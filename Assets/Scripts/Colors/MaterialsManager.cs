using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialsManager : MonoBehaviour
{
    public static MaterialsManager Instance { get; private set; }

    [SerializeField]
    private int _targetInstances = 3;
    
    [SerializeField]
    private List<HitSideAndMaterials> _materials;
    
    //private const string DISSOLVEKEYWORD = "_USEDISTANCEDISSOLVE";
    
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
        InstanceMaterials();
        UpdateMaterialColors();
    }
    
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    

    public void UpdateMaterialColors()
    {
        foreach (var set in _materials)
        {
            foreach (var material in set.materials)
            {
                var color = ColorsManager.Instance.GetAppropriateColor(set.hitSideType, material.isNote);
                material.material.color = color;
            }
        }
    }

    public Material GetMaterial(int index, int subIndex)
    {
        return _materials[index].materials[subIndex].material;
    }
    
    private void InstanceMaterials()
    {
        var hex = Shader.PropertyToID("_TextureIndex");
        foreach (var set in _materials)
        {
            if (!set.shouldInstance || set.materials.Count <= 0)
            {
                continue;
            }
            set.materials[0].material.SetInt(hex, 0);
            
            //var keyword = new LocalKeyword(set.materials[0].material.shader, DISSOLVEKEYWORD);
            for (var i = 0; i < _targetInstances; i++)
            {
                var newMat = new Material(set.materials[0].material);
                newMat.name = $"{newMat.name} TexIndex({i + 1})";
                newMat.SetInt(hex, i+1);
                //newMat.SetKeyword(keyword, false);
                set.materials.Add(new MatAndIsNote(newMat, set.materials[0].isNote));
            }

            /*for (var i = 0; i < _targetInstances+1; i++)
            {
                var newMat = new Material(set.materials[i].material);
                newMat.name = $"{newMat.name} TexIndex({i}) Dissolve";
                newMat.SetInt(hex, i);
                newMat.SetKeyword(keyword, true);
                set.materials.Add(new MatAndIsNote(newMat, set.materials[0].isNote));
            }*/
        }
    }

    [Serializable]
    public struct HitSideAndMaterials
    {
        public bool shouldInstance;
        public HitSideType hitSideType;
        public List<MatAndIsNote> materials;
    }

    [Serializable]
    public struct MatAndIsNote
    {
        public Material material;
        public bool isNote;

        public MatAndIsNote(Material material, bool isNote)
        {
            this.material = material;
            this.isNote = isNote;
        }
    }
}
