using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetGraphicsInteractable : Graphic
{
    [SerializeField]
    private bool _interactable = true;

    [SerializeField]
    private Color _interactableColor = Color.white;

    [SerializeField]
    private Color _disabledColor = new Color(.1f,.1f,.1f,.9f);

    [SerializeField]
    private float _fadeDuration = .1f;

    [SerializeField]
    private CanvasGroup _parentGroup;
    
    [SerializeField]
    private Graphic[] _graphics;
    
    private bool _groupsEnabled = false;

    private bool EnabledByCanvasGroup => !_parentGroup || _parentGroup.interactable;
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
        if (isActiveAndEnabled)
        {
            DoStateTransition(Interactable, true);
        }

        if (_parentGroup == null)
        {
            TryGetComponent(out _parentGroup);
        }
        if (_graphics.Length == 0 && _parentGroup != null)
        {
            _graphics = _parentGroup.GetComponentsInChildren<Graphic>();
        }
    }
#endif
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
        foreach (var graphic in _graphics)
        {
            graphic.CrossFadeColor(targetColor, instant ? 0f : _fadeDuration, true, true);
        }
    }
}
#if UNITY_EDITOR
namespace CustomEditors
{
    using UnityEditor;

    [CustomEditor(typeof(SetGraphicsInteractable))]
    public class SetGraphicsInteractableEditor : Editor
    {
        private SerializedProperty _interactableProperty;
        private SerializedProperty _interactableColorProperty;
        private SerializedProperty _disabledColorProperty;
        private SerializedProperty _fadeDurationProperty;
        private SerializedProperty _parentGroupProperty;
        private SerializedProperty _graphicsProperty;
        
        private const string INTERACTABLE = "_interactable";
        private const string INTERACTABLECOLOR = "_interactableColor";
        private const string DISABLEDCOLOR = "_disabledColor";
        private const string FADEDURATION = "_fadeDuration";
        private const string PARENTGROUP = "_parentGroup";
        private const string GRAPHICS = "_graphics";

        private void Awake()
        {
            _interactableProperty = serializedObject.FindProperty(INTERACTABLE);
            _interactableColorProperty = serializedObject.FindProperty(INTERACTABLECOLOR);
            _disabledColorProperty = serializedObject.FindProperty(DISABLEDCOLOR);
            _fadeDurationProperty = serializedObject.FindProperty(FADEDURATION);
            _parentGroupProperty = serializedObject.FindProperty(PARENTGROUP);
            _graphicsProperty = serializedObject.FindProperty(GRAPHICS);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_interactableProperty);
            EditorGUILayout.PropertyField(_interactableColorProperty);
            EditorGUILayout.PropertyField(_disabledColorProperty);
            EditorGUILayout.PropertyField(_fadeDurationProperty);
            EditorGUILayout.PropertyField(_parentGroupProperty);
            EditorGUILayout.PropertyField(_graphicsProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif