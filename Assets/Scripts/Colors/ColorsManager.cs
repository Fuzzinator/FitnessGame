using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ColorsManager : MonoBehaviour
{
    public static ColorsManager Instance { get; private set; }

    [SerializeField]
    private ColorSet _currentColorSet = ColorSet.Default;
    
    [SerializeField]
    private Texture2DArray _targetTexturesArray;

    [SerializeField]
    private Texture2DArray _obstacleTexturesArray;

    private List<ColorSet> _colorSets = new List<ColorSet>();
    private int _customColorCount;

    public UnityEvent<ColorSet> activeColorSetUpdated = new UnityEvent<ColorSet>();
    public UnityEvent availableColorSetsUpdated = new UnityEvent();
    
    public ColorSet ActiveColorSet
    {
        get => _currentColorSet;
        private set
        {
            _currentColorSet = value;
            activeColorSetUpdated?.Invoke(value);
        }
    }
    public List<ColorSet> AvailableColorSets => _colorSets;
    public int ActiveSetIndex { get; private set; }
    
    #region Const Vars

    private const string CUSTOMCOLORSETCOUNT = "CustomColorSetCount";
    private const string CUSTOMCOLORSETNUMBERX = "CustomColorSetNumber:";
    private const string ACTIVECOLORSETINDEX = "ActiveColorSetIndex";
    
    #endregion
    
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
        GetColorSets();
    }

    private void OnValidate()
    {
        UpdateTextureSets();
    }

    public void SetAndUpdateTextureSets(Texture2DArray targetTextures, Texture2DArray obstacleTextures)
    {
        _targetTexturesArray = targetTextures;
        _obstacleTexturesArray = obstacleTextures;
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
            HitSideType.Left => isNote?_currentColorSet.LeftController:_currentColorSet.LeftEnvironment,
            HitSideType.Right => isNote?_currentColorSet.RightController:_currentColorSet.RightEnvironment,
            HitSideType.Block =>  isNote?_currentColorSet.BlockColor:_currentColorSet.CenterEnvironment,
            HitSideType.Unused => _currentColorSet.ObstacleColor,
            _ => Color.white
        };
    }
    
    private void GetColorSets()
    {
        _customColorCount = SettingsManager.GetSetting(CUSTOMCOLORSETCOUNT, 0);
        _colorSets.Clear();
        _colorSets.Add(ColorSet.Default);
        for (var i = 0; i < _customColorCount; i++)
        {
            _colorSets.Add(SettingsManager.GetSetting($"{CUSTOMCOLORSETNUMBERX}{i}", ColorSet.Default));
        }

        ActiveSetIndex = SettingsManager.GetSetting(ACTIVECOLORSETINDEX, 0);
        SetActiveColorSet(ActiveSetIndex);
        availableColorSetsUpdated?.Invoke();
    }

    public int AddColorSet(ColorSet colorSet)
    {
        _colorSets.Add(colorSet);
        availableColorSetsUpdated?.Invoke();
        SaveColorSet(colorSet, AvailableColorSets.Count - 2);
        SettingsManager.SetSetting(CUSTOMCOLORSETCOUNT, AvailableColorSets.Count-1);
        return _colorSets.Count - 2;
    }

    public void UpdateColorSet(ColorSet colorSet, int index)
    {
        _colorSets[index+1] = colorSet;
        availableColorSetsUpdated?.Invoke();
        SaveColorSet(colorSet, index);
    }

    public bool IsActiveColorSet(ColorSet colorSet)
    {
        return _currentColorSet == colorSet;
    }

    public void SetActiveColorSet(ColorSet colorSet, int index)
    {
        ActiveSetIndex = index;
        ActiveColorSet = colorSet;
        
        SettingsManager.SetSetting(ACTIVECOLORSETINDEX, index);
    }

    private void SetActiveColorSet(int index)
    {
        ActiveSetIndex = index;
        ActiveColorSet = _colorSets[index];
    }

    private void SaveColorSet(ColorSet colorSet, int index)
    {
        SettingsManager.SetSetting($"{CUSTOMCOLORSETNUMBERX}{index}", colorSet);
    }

    [Serializable]
    public struct ColorSet
    {
        [Header("Controllers & Notes Colors")]
        [SerializeField]
        private Color _leftController;

        [SerializeField]
        private Color _rightController;

        [SerializeField]
        private Color _blockColor;

        [SerializeField]
        private Color _obstacleColor;

        [Space]
        [Header("Environment Colors: Currently Unsed")]
        [SerializeField]
        private Color _leftEnvironment;

        [SerializeField]
        private Color _rightEnvironment;

        [SerializeField]
        private Color _centerEnvironment;

        public Color LeftController => _leftController;
        public Color RightController => _rightController;
        public Color BlockColor => _blockColor;
        public Color ObstacleColor => _obstacleColor;

        public Color LeftEnvironment => _leftEnvironment;
        public Color RightEnvironment => _rightEnvironment;
        public Color CenterEnvironment => _centerEnvironment;

        public ColorSet(Color leftController, Color rightController, Color blockColor, Color obstacleColor,
            Color leftEnv, Color rightEnv, Color centerEnv)
        {
            _leftController = leftController;
            _rightController = rightController;
            _blockColor = blockColor;
            _obstacleColor = obstacleColor;
            _leftEnvironment = leftEnv;
            _rightEnvironment = rightEnv;
            _centerEnvironment = centerEnv;
        }
        
        public ColorSet(Color leftController, Color rightController, Color blockColor, Color obstacleColor)
        {
            _leftController = leftController;
            _rightController = rightController;
            _blockColor = blockColor;
            _obstacleColor = obstacleColor;
            _leftEnvironment = Color.black;
            _rightEnvironment = Color.black;
            _centerEnvironment = Color.black;
        }

        public static bool operator ==(ColorSet a, ColorSet b)
        {
            return a._leftController == b._leftController &&
                   a._rightController == b._rightController &&
                   a._blockColor == b._blockColor &&
                   a._obstacleColor == b._obstacleColor &&
                   a._leftEnvironment == b._leftEnvironment &&
                   a._rightEnvironment == b._rightEnvironment &&
                   a._centerEnvironment == b._centerEnvironment;
        }

        public static bool operator !=(ColorSet a, ColorSet b)
        {
            return a._leftController != b._leftController ||
                a._rightController != b._rightController ||
                a._blockColor != b._blockColor ||
                a._obstacleColor != b._obstacleColor ||
                a._leftEnvironment != b._leftEnvironment ||
                a._rightEnvironment != b._rightEnvironment ||
                a._centerEnvironment != b._centerEnvironment;
        }

        public static bool Equal(ColorSet a, ColorSet b)
        {
            return a == b;
        }
        
        public static readonly ColorSet Default = new (
            new Color(.1125f, .5374f, .75f),
            new Color(.75f, .1125f, .1125f),
            new Color(.1921f, .749f, .1137f),
            new Color(0f, .949f, 1f),
            new Color(),
            new Color(),
            new Color());
    }
}
