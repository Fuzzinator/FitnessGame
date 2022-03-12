using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SoundManager : BaseGameStateListener
{
    public static SoundManager Instance { get; private set; }

    [SerializeField]
    private SoundObject _basePoolObject;

    [SerializeField]
    private List<SoundObject> _activeSoundObjects = new List<SoundObject>(20);

    private PoolManager _poolManager;

    [SerializeField]
    private List<string> _assetReferenceNames = new List<string>();

    [SerializeField]
    private List<AssetReference> _assetReferences = new List<AssetReference>();


    private Dictionary<string, AsyncOperationHandle> _loadedAssets = new Dictionary<string, AsyncOperationHandle>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_assetReferences.Count != _assetReferenceNames.Count)
        {
            _assetReferenceNames.Clear();
            foreach (var reference in _assetReferences)
            {
                _assetReferenceNames.Add(reference?.editorAsset?.name);
            }
        }
    }
#endif

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
        _poolManager = new PoolManager(_basePoolObject, transform, 5);
    }

    public static void PlaySound(string soundName)
    {
        if (Instance._loadedAssets.ContainsKey(soundName))
        {
            if (Instance._loadedAssets[soundName].IsDone)
            {
                var sound = Instance._loadedAssets[soundName].Result as AudioClip;
                PlayClip(sound);
            }
            else
            {
                PlayLoadingAsset(soundName).Forget();
            }
        }
        else
        {
            PlaySoundAsync(soundName).Forget();
        }
    }

    private static async UniTaskVoid PlayLoadingAsset(string soundName)
    {
        await Instance._loadedAssets[soundName];
        PlayClip(Instance._loadedAssets[soundName].Result as AudioClip);
    }

    public static async UniTask<SoundObject> PlaySoundAsync(string soundName)
    {
        var index = Instance._assetReferenceNames.IndexOf(soundName);
        if (index < 0)
        {
            Debug.LogError($"{soundName} is not a valid sound.");
            return null;
        }


        var reference = Instance._assetReferences[index];
        if (reference == null)
        {
            Debug.LogError($"{soundName} is not a valid sound.");
            return null;
        }

        var assetHandle = Addressables.LoadAssetAsync<AudioClip>(reference);

        Instance._loadedAssets[soundName] = assetHandle;
        var audioClip = await assetHandle;
        return PlayClip(audioClip);
    }

    public static SoundObject PlayClip(AudioClip audioClip)
    {
        var sound = Instance._poolManager.GetNewPoolable() as SoundObject;
        sound.gameObject.SetActive(true);
        Instance._activeSoundObjects.Add(sound);
        sound.Play(audioClip);

        return sound;
    }

    public void ReturnToPool(SoundObject soundObject)
    {
        _activeSoundObjects.Remove(soundObject);
        _poolManager?.ReturnToPool(soundObject);
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.Paused && newState == GameState.Playing)
        {
            foreach (SoundObject soundObject in _activeSoundObjects)
            {
                soundObject.ToggleSound(true);
            }
        }
        else if (oldState == GameState.Playing && (newState == GameState.Paused || newState == GameState.Unfocused))
        {
            foreach (SoundObject soundObject in _activeSoundObjects)
            {
                soundObject.ToggleSound(false);
            }
        }
    }
}