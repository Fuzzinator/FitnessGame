using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentController : MonoBehaviour
{
    public static EnvironmentController Instance { get; private set; }

    [SerializeField]
    private string _targetSceneName = SCIFILEVEL;

    private AsyncOperation _sceneLoadOperation;

    private CancellationToken _cancellationToken;

    private const string SCIFILEVEL = "Sci-Fi Arena";

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
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public void SetTargetEnvironment(Environments environment)
    {
        _targetSceneName = environment switch
        {
            Environments.None => string.Empty,
            Environments.SciFi => SCIFILEVEL,
            _ => _targetSceneName
        };
    }

    public void RequestEnvLoad()
    {
        LoadEnvironmentAsync().Forget();
    }
    
    public async UniTask LoadEnvironmentAsync()
    {
        if (string.IsNullOrWhiteSpace(_targetSceneName))
        {
            _sceneLoadOperation = null;
            return;
        }

        _sceneLoadOperation = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
        //_sceneLoadOperation.allowSceneActivation = false;
        await _sceneLoadOperation;
    }

    public void FinishSceneLoad()
    {
        if (_sceneLoadOperation == null)
        {
            return;
        }

        _sceneLoadOperation.allowSceneActivation = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public enum Environments
    {
        None = 0,
        SciFi = 1,
    }
}