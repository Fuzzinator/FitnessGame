using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnvironmentController : MonoBehaviour
{
    public static EnvironmentController Instance { get; private set; }

    [SerializeField]
    private string _targetSceneName = SCIFILEVEL;

    //[SerializeField]
    //private Slider _slider;

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
        //LoadTracker().Forget();
        //_sceneLoadOperation.allowSceneActivation = false;
        await _sceneLoadOperation;
        _sceneLoadOperation = null;
    }

    /*private async UniTaskVoid LoadTracker()
    {
        while (_sceneLoadOperation != null && !_cancellationToken.IsCancellationRequested)
        {
            _slider.value = _sceneLoadOperation.progress;
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }
    }*/
    
    public void FinishSceneLoad()
    {
        if (_sceneLoadOperation == null)
        {
            return;
        }

        _sceneLoadOperation.allowSceneActivation = true;
    }

    public enum Environments
    {
        None = 0,
        SciFi = 1,
    }
}