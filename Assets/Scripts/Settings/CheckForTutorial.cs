using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForTutorial : MonoBehaviour
{
    private const string PLAYEDBEFORE = "HasPlayerPlayedBefore";
    private bool hasKey;
    private void Start()
    {
        hasKey = PlayerPrefs.HasKey(PLAYEDBEFORE);
        if (hasKey)
        {
            return;
        }

        PlayerPrefs.SetInt(PLAYEDBEFORE, 1);
        PlayerPrefs.Save();
        ShowTutorialRequest();
    }

    private void ShowTutorialRequest()
    {
        if (MainMenuUIController.Instance == null)
        {
            return;
        }

        var mainMenu = MainMenuUIController.Instance;
        mainMenu.SetActivePage(mainMenu.MenuPageCount-1);
    }
    
    
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        hasKey = PlayerPrefs.HasKey(PLAYEDBEFORE);
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey(PLAYEDBEFORE);
        hasKey = PlayerPrefs.HasKey(PLAYEDBEFORE);
    }
#endif
}





#if UNITY_EDITOR
namespace EditorStuff
{
    using UnityEditor;

    [CustomEditor(typeof(CheckForTutorial), true)]
    [CanEditMultipleObjects]
    internal class CheckForTutorialEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Reset"))
            {
                var script = target as CheckForTutorial;
                script.Reset();
            }
        }
    }
}
#endif
