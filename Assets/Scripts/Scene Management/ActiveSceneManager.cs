using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ActiveSceneManager : MonoBehaviour
{
    public ActiveSceneManager Instance { get; private set; }
    public UnityEvent newSceneLoaded = new UnityEvent();

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

    public void SetScene(string newSceneName, bool additive = false)
    {
        
    }

    public void LoadBaseLevel()
    {
        LoadSceneAsync(BASELEVELNAME);
    }
    
    private async UniTask LoadSceneAsync(string newSceneName, bool additive = false)
    {
        await SceneManager.LoadSceneAsync(newSceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        newSceneLoaded?.Invoke();
    }
}