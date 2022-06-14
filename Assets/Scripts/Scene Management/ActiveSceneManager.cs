using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ActiveSceneManager : MonoBehaviour
{
    public static ActiveSceneManager Instance { get; private set; }
    public UnityEvent newSceneLoaded = new UnityEvent();

    private const string MAINMENUNAME = "Main Menu";
    private const string BASELEVELNAME = "Base Level";

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
        LoadMainMenu();
    }

    public void SetScene(string newSceneName, bool additive = false)
    {
        
    }

    public async void LoadMainMenu()
    {
        await LoadSceneAsync(MAINMENUNAME);
    }
    
    public async void LoadBaseLevel()
    {
        await SceneManager.UnloadSceneAsync(MAINMENUNAME);
        await LoadSceneAsync(BASELEVELNAME, true);
    }
    
    private async UniTask LoadSceneAsync(string newSceneName, bool additive = false)
    {
        await SceneManager.LoadSceneAsync(newSceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        newSceneLoaded?.Invoke();
    }
}