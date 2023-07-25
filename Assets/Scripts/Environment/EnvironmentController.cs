using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnvironmentController : MonoBehaviour
{
    public static EnvironmentController Instance { get; private set; }

    [SerializeField]
    private string _targetSceneName = SCIFILEVEL;

    [SerializeField]
    private AssetReference _sceneReference;

    private AsyncOperationHandle<SceneInstance> _sceneInstanceHandle;
    public AsyncOperationHandle<SceneInstance> SceneLoadHandle => _sceneInstanceHandle;

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
        if (EnvironmentControlManager.Instance != null)
        {
            await UniTask.WaitWhile(() => EnvironmentControlManager.Instance.LoadingEnvironmentContainer,
                cancellationToken: _cancellationToken);
            _sceneReference = EnvironmentControlManager.Instance.ActiveEnvironmentContainer.SceneAsset;
            _targetSceneName = EnvironmentControlManager.Instance.ActiveEnvironmentContainer.EnvironmentName;
        }

        if (string.IsNullOrWhiteSpace(_targetSceneName) || _sceneReference == null)
        {
            _sceneInstanceHandle = new AsyncOperationHandle<SceneInstance>();
            return;
        }

        if (_sceneInstanceHandle.IsValid())
        {
            await Addressables.UnloadSceneAsync(_sceneInstanceHandle);
        }
        
        var sceneInstance =  await Addressables.LoadSceneAsync(_sceneReference, LoadSceneMode.Additive, true);

        SceneManager.SetActiveScene(sceneInstance.Scene);

        _sceneInstanceHandle = new AsyncOperationHandle<SceneInstance>();
        //_sceneLoadOperation = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
        //LoadTracker().Forget();
        //_sceneLoadOperation.allowSceneActivation = false;
        //await _sceneLoadOperation;
        //_sceneLoadOperation = null;
    }

    /*private async UniTaskVoid LoadTracker()
    {
        while (_sceneLoadOperation != null && !_cancellationToken.IsCancellationRequested)
        {
            _slider.value = _sceneLoadOperation.progress;
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
        }
    }*/

    public void UpdateObstacleTargetTextures()
    {
        EnvironmentControlManager.Instance.UpdateObstacleTargetTextures();
    }

    public void FinishSceneLoad()
    {
        if (!_sceneInstanceHandle.IsValid())//_sceneLoadOperation == null)
        {
            return;
        }

        _sceneInstanceHandle.Result.ActivateAsync();
    }

    public void UnloadScene()
    {
        if (!_sceneInstanceHandle.IsValid())
        {
            return;
        }
        Addressables.UnloadSceneAsync(_sceneInstanceHandle);
    }

    public enum Environments
    {
        None = 0,
        SciFi = 1,
    }
}