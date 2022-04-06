using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SetPlayModeScene : MonoBehaviour
{
    private const string STARTFROMROOT = "Tools/Load From Root";
    private const string SETSTARTSCENE = "Assets/Set Start Scene";
    private const string SELECTEDSCENE = "SelectedPlayModeScene";
    private static string _selectedPath;
    private static SceneAsset _compositionRoot;
    private static SceneAsset Root => _compositionRoot ?? AssetDatabase.LoadAssetAtPath<SceneAsset>(_selectedPath);

    static SetPlayModeScene()
    {
        var isOn = EditorPrefs.GetBool(STARTFROMROOT, false);
        _selectedPath = EditorPrefs.GetString(SELECTEDSCENE);
        EditorSceneManager.playModeStartScene = isOn ? Root : null;
    }
    
    [MenuItem(STARTFROMROOT)]
    private static void SetStartFromRoot()
    {
        var isOn = EditorPrefs.GetBool(STARTFROMROOT, false);
        Menu.SetChecked(STARTFROMROOT, isOn);
        EditorSceneManager.playModeStartScene = isOn ? null : Root;
        EditorPrefs.SetBool(STARTFROMROOT, !isOn);
    }

    [MenuItem(STARTFROMROOT, true)]
    private static bool SetStartFromRootValidate()
    {
        var isOn = EditorPrefs.GetBool(STARTFROMROOT, false);
        Menu.SetChecked(STARTFROMROOT, isOn);
        return !Application.isPlaying;
    }


    [MenuItem(SETSTARTSCENE)]
    private static void SetScene()
    {
        var newPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (_selectedPath == newPath)
        {
            _selectedPath = string.Empty;
            EditorSceneManager.playModeStartScene = null;
            Menu.SetChecked(SETSTARTSCENE, false);
        }
        else
        {
            _selectedPath = newPath;
            EditorSceneManager.playModeStartScene = Selection.activeObject as SceneAsset;
            Menu.SetChecked(SETSTARTSCENE, true);
        }
        EditorPrefs.SetString(SELECTEDSCENE, _selectedPath);
    }


    [MenuItem(SETSTARTSCENE, true)]
    private static bool SetSceneValidator()
    {
        var isSelected = _selectedPath == AssetDatabase.GetAssetPath(Selection.activeObject);
        Menu.SetChecked(SETSTARTSCENE, isSelected);
        return Selection.activeObject is SceneAsset;
    }
}