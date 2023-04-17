using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YUR.Watch;
using YUR.UI.Displayers;

/*
[CustomEditor(typeof(BaseDisplayer), true)]
public class ManagerBaseEditor : Editor
{
    SerializedProperty debugProperty;
    BaseDisplayer baseManager2;

    void OnEnable()
    {
        debugProperty = serializedObject.FindProperty("debug");
        baseManager2 = (BaseDisplayer)target;
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        debugProperty.boolValue = EditorGUILayout.Toggle("Debug", debugProperty.boolValue);

        serializedObject.ApplyModifiedProperties();

        if (!debugProperty.boolValue) return;

        if (GUILayout.Button("Show"))
        {
            baseManager2.Show();
        }
        if (GUILayout.Button("Hide"))
        {
            baseManager2.Hide();
        }
    }
}
*/
