using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
//using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ActiveSceneManager : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    private bool _trailer;
    public static ActiveSceneManager Instance { get; private set; }
    public UnityEvent newSceneLoaded = new UnityEvent();

    private UnityEngine.AsyncOperation _gameSceneLoader;

    private const string MAINMENUNAME = "Main Menu";
    private const string NonVRMAINMENUNAME = "Non VR Tool";
    private const string BASELEVELNAME = "Base Level";
    private const string BASELEVELTRAILERNAME = "Base Level (For Trailer)";

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
        if (GameManager.Instance.VRMode)
        {
        }
        else
        {
            //LoadNonVRMainMenu();
        }
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
            Addressables.Release(EnvironmentController.Instance.SceneLoadHandle);
        }
    }

    public async void LoadNonVRMainMenu()
    {
        await LoadSceneAsync(NonVRMAINMENUNAME);
        if (EnvironmentController.Instance.SceneLoadHandle.IsValid())
        {
            await Addressables.UnloadSceneAsync(EnvironmentController.Instance.SceneLoadHandle);
            Addressables.Release(EnvironmentController.Instance.SceneLoadHandle);
        }
    }

    public async UniTaskVoid LoadBaseLevel()
    {
        await SceneManager.UnloadSceneAsync(MAINMENUNAME);
        await LoadSceneAsync(_trailer ? BASELEVELTRAILERNAME : BASELEVELNAME, true);
    }

    private async UniTask LoadSceneAsync(string newSceneName, bool additive = false)
    {
        _gameSceneLoader = SceneManager.LoadSceneAsync(newSceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        await _gameSceneLoader;
        newSceneLoaded?.Invoke();
        await UnityEngine.Resources.UnloadUnusedAssets();
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