using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ActiveSceneManager : MonoBehaviour
{
    public static ActiveSceneManager Instance { get; private set; }
    public UnityEvent newSceneLoaded = new UnityEvent();

    private AsyncOperation _gameSceneLoader;

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
        if (EnvironmentController.Instance.SceneLoadHandle.IsValid())
        {
            await Addressables.UnloadSceneAsync(EnvironmentController.Instance.SceneLoadHandle);
        }
    }
    
    public async void LoadBaseLevel()
    {
        await SceneManager.UnloadSceneAsync(MAINMENUNAME);
        await LoadSceneAsync(BASELEVELNAME, true);
    }
    
    private async UniTask LoadSceneAsync(string newSceneName, bool additive = false)
    {
        _gameSceneLoader = SceneManager.LoadSceneAsync(newSceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        await _gameSceneLoader;
        newSceneLoaded?.Invoke();
        await Resources.UnloadUnusedAssets();
    }

    /*public void CompleteSceneLoad()
    {
        if (_gameSceneLoader != null)
        {
            _gameSceneLoader.allowSceneActivation = true;
            _gameSceneLoader = null;
        }
    }*/
}