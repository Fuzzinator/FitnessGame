using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsManager : MonoBehaviour
{
    public static MaterialsManager Instance { get; private set; }

    [SerializeField]
    private List<HitSideAndMaterials> _materials;

    [SerializeField]
    private Material _leftController;
    public Material LeftController => _leftController;
    [SerializeField]
    private Material _leftTarget;
    public Material LeftTarget => _leftTarget;
    
    [SerializeField]
    private Material _rightController;
    public Material RightController => _rightController;
    [SerializeField]
    private Material _rightTarget;
    public Material RightTarget => _rightTarget;
    
    [SerializeField]
    private Material _centerTarget;
    public Material CenterTarget => _centerTarget;
    
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
        UpdateMaterialColors();
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

    [Serializable]
    public struct HitSideAndMaterials
    {
        public HitSideType hitSideType;
        public List<MatAndIsNote> materials;
    }

    [Serializable]
    public struct MatAndIsNote
    {
        public Material material;
        public bool isNote;
    }
}
