using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RenameTool : EditorWindow
{
    private static RenameTool s_RenameWindow;
    private string _prefix;
    private string _suffix;
    private string _newName;

    private string _searchFor;
    private string _replaceWith;

    [MenuItem("Tools/Rename Tool")]
    public static void OpenRenameTool()
    {
        if (s_RenameWindow == null)
        {
            s_RenameWindow = GetWindow<RenameTool>();
        }
    }

    private void OnGUI()
    {
        if (s_RenameWindow == null)
        {
            s_RenameWindow = this;
        }

        _prefix = EditorGUILayout.TextField("Prefix: ", _prefix);
        _newName = EditorGUILayout.TextField("New Name: ", _newName);
        _suffix = EditorGUILayout.TextField("Suffix: ", _suffix);

        if(GUILayout.Button("Rename Selection"))
        {
            var selection = Selection.objects;

            foreach (var selectedObj in selection)
            {
                if (!AssetDatabase.Contains(selectedObj))
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(selectedObj);
                var newName = string.IsNullOrWhiteSpace(_newName) ? selectedObj.name : _newName;
                newName = $"{_prefix}{newName}{_suffix}";

                AssetDatabase.RenameAsset(path, newName);
            }
        }

        EditorGUILayout.Space(20);

        _searchFor = EditorGUILayout.TextField("Search For: ", _searchFor);
        _replaceWith = EditorGUILayout.TextField("Replace With: ", _replaceWith);

        if (GUILayout.Button("Rename Selection"))
        {
            var selection = Selection.objects;

            foreach (var selectedObj in selection)
            {
                if (!AssetDatabase.Contains(selectedObj) || !selectedObj.name.Contains(_searchFor, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(selectedObj);
                var newName = selectedObj.name.Replace(_searchFor, _replaceWith);
                AssetDatabase.RenameAsset(path, newName);
            }
        }
    }
}