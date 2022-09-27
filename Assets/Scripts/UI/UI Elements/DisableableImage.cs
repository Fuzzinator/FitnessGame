using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableableImage : Image
{
    [SerializeField]
    private bool _interactable = true;

    [SerializeField]
    private Color _interactableColor = Color.white;

    [SerializeField]
    private Color _disabledColor = new Color(.1f,.1f,.1f,.9f);

    [SerializeField]
    private float _fadeDuration = .1f;

    private bool _groupsEnabled = false;
    private List<CanvasGroup> _canvasGroups = new List<CanvasGroup>(10);


    private bool EnabledByCanvasGroup
    {
        get
        {
            foreach (var canvasGroup in _canvasGroups)
            {
                if (canvasGroup.enabled && !canvasGroup.interactable)
                {
                    return false;
                }

                if (canvasGroup.ignoreParentGroups)
                {
                    return true;
                }
            }

            return true;
        }
    }
#if UNITY_EDITOR

    public bool Interactable => Application.isPlaying
        ? (_interactable && _groupsEnabled)
        : (_interactable && (EnabledByCanvasGroup));
#else
    public bool Interactable => _interactable && _groupsEnabled;
#endif

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateGroups();
        }
        base.OnValidate();
        if (isActiveAndEnabled)
        {
            DoStateTransition(Interactable, true);
        }
    }
#endif
    protected override void Awake()
    {
        UpdateGroups();
    }

    private void UpdateGroups()
    {
        _canvasGroups.Clear();
        var t = transform;
        while (TryGetCanvasGroup(t, out CanvasGroup group))
        {
            _canvasGroups.Add(group);
            t = group.transform.parent;
        }
    }

    protected override void OnCanvasGroupChanged()
    {
        if (!_interactable)
        {
            return;
        }

        var isEnabled = EnabledByCanvasGroup;
        if (isEnabled == _groupsEnabled)
        {
            return;
        }

        _groupsEnabled = isEnabled;
        UpdateState();
    }

    private bool TryGetCanvasGroup(Transform t, out CanvasGroup group)
    {
        while (t != null)
        {
            if (t.TryGetComponent(out group))
            {
                return true;
            }

            t = t.parent;
        }

        group = null;
        return false;
    }

    private void UpdateState()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DoStateTransition(Interactable, true);
        else
#endif
            DoStateTransition(Interactable, false);
    }

    private void DoStateTransition(bool enabled, bool instant)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        var targetColor = enabled ? _interactableColor : _disabledColor;
        CrossFadeColor(targetColor, instant ? 0f : _fadeDuration, true, true);
    }
}

#if UNITY_EDITOR
namespace CustomEditors
{
    using UnityEditor;
    
    [CustomEditor(typeof(DisableableImage))]
    public class DisabeableImageEditor : Editor
    {
        private SerializedProperty _sourceImageProperty;
        private SerializedProperty _colorProperty;
        private SerializedProperty _materialProperty;
        private SerializedProperty _raycastTargetProperty;
        private SerializedProperty _raycastPaddingProperty;
        private SerializedProperty _maskableProperty;
        private SerializedProperty _imageTypeProperty;
        private SerializedProperty _useSpriteMeshProperty;
        private SerializedProperty _preseveAspectProperty;
        private SerializedProperty _fillCenterProperty;
        private SerializedProperty _pixelsPerUnitProperty;
        private SerializedProperty _fillMethodProperty;
        private SerializedProperty _fillAmountProperty;
        private SerializedProperty _clockwiseProperty;
        private SerializedProperty _interactableProperty;
        private SerializedProperty _interactableColorProperty;
        private SerializedProperty _disabledColorProperty;
        private SerializedProperty _fadeDurationProperty;

        #region Const Strings
        private const string SPRITE = "m_Sprite";
        private const string COLOR = "m_Color";
        private const string MATERIAL = "m_Material";
        private const string RAYCASTTARGET = "m_RaycastTarget";
        private const string RAYCASTPADDING = "m_RaycastPadding";
        private const string MASKABLE = "m_Maskable";
        private const string IMAGETYPE = "m_Type";
        private const string PRESERVEASPECT = "m_PreserveAspect";
        private const string FILLCENTER = "m_FillCenter";
        private const string FILLAMOUNT = "m_FillAmount";
        private const string FILLMETHOD = "m_FillMethod";
        private const string FILLCLOCKWISE = "m_FillClockwise";
        private const string USESPRITEMESH = "m_UseSpriteMesh";
        private const string PIXELMULTIPLIER = "m_PixelsPerUnitMultiplier";
        private const string INTERACTABLE = "_interactable";
        private const string INTERACTABLECOLOR = "_interactableColor";
        private const string DISABLEDCOLOR = "_disabledColor";
        private const string FADEDURATION = "_fadeDuration";
        #endregion

        private readonly string[] _fillOriginOptions = new[] {"Bottom", "Right", "Top", "Left"};

        private void Awake()
        {
            _sourceImageProperty = serializedObject.FindProperty(SPRITE);
            _colorProperty = serializedObject.FindProperty(COLOR);
            _materialProperty = serializedObject.FindProperty(MATERIAL);
            _raycastTargetProperty = serializedObject.FindProperty(RAYCASTTARGET);
            _raycastPaddingProperty = serializedObject.FindProperty(RAYCASTPADDING);
            _maskableProperty = serializedObject.FindProperty(MASKABLE);
            _imageTypeProperty = serializedObject.FindProperty(IMAGETYPE);
            _useSpriteMeshProperty = serializedObject.FindProperty(USESPRITEMESH);
            _preseveAspectProperty = serializedObject.FindProperty(PRESERVEASPECT);
            _fillCenterProperty = serializedObject.FindProperty(FILLCENTER);
            _pixelsPerUnitProperty = serializedObject.FindProperty(PIXELMULTIPLIER);
            _fillMethodProperty = serializedObject.FindProperty(FILLMETHOD);
            _fillAmountProperty = serializedObject.FindProperty(FILLAMOUNT);
            
            _clockwiseProperty = serializedObject.FindProperty(FILLCLOCKWISE);
            _interactableProperty = serializedObject.FindProperty(INTERACTABLE);
            _interactableColorProperty = serializedObject.FindProperty(INTERACTABLECOLOR);
            _disabledColorProperty = serializedObject.FindProperty(DISABLEDCOLOR);
            _fadeDurationProperty = serializedObject.FindProperty(FADEDURATION);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_sourceImageProperty);
            EditorGUILayout.PropertyField(_colorProperty);
            EditorGUILayout.PropertyField(_materialProperty);
            EditorGUILayout.PropertyField(_raycastTargetProperty);
            EditorGUILayout.PropertyField(_raycastPaddingProperty);
            EditorGUILayout.PropertyField(_maskableProperty);

            EditorGUILayout.PropertyField(_interactableProperty);
            EditorGUILayout.PropertyField(_interactableColorProperty);
            EditorGUILayout.PropertyField(_disabledColorProperty);
            EditorGUILayout.PropertyField(_fadeDurationProperty);
            
            EditorGUILayout.PropertyField(_imageTypeProperty);
            EditorGUI.indentLevel++;
            var script = target as DisableableImage;
            var type = script.type;
            switch (type)
            {
                case Image.Type.Simple:
                    EditorGUILayout.PropertyField(_useSpriteMeshProperty);
                    EditorGUILayout.PropertyField(_preseveAspectProperty);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Empty);
                    if (GUILayout.Button("Set Native Size"))
                    {
                        script.SetNativeSize();
                    }
                    GUILayout.EndHorizontal();
                    break;
                case Image.Type.Tiled:
                    EditorGUILayout.PropertyField(_fillCenterProperty);
                    EditorGUILayout.PropertyField(_pixelsPerUnitProperty);
                    break;
                case Image.Type.Sliced:
                    EditorGUILayout.PropertyField(_fillCenterProperty);
                    EditorGUILayout.PropertyField(_pixelsPerUnitProperty);
                    break;
                case Image.Type.Filled:
                    EditorGUILayout.PropertyField(_fillMethodProperty);
                    script.fillOrigin = EditorGUILayout.Popup("Fill Origin", script.fillOrigin, _fillOriginOptions);
                    EditorGUILayout.PropertyField(_fillAmountProperty);
                    EditorGUILayout.PropertyField(_clockwiseProperty);
                    EditorGUILayout.PropertyField(_preseveAspectProperty);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Empty);
                    if (GUILayout.Button("Set Native Size"))
                    {
                        script.SetNativeSize();
                    }
                    GUILayout.EndHorizontal();
                    break;
            }

            
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif