using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
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


    private Dictionary<string, AudioClip> _loadedAssets = new Dictionary<string, AudioClip>();

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

    public static async UniTask<SoundObject> PlaySoundAsnyc(string soundName, AudioSourceSettings settings)
    {
        SoundObject playingObject;
        if (Instance._loadedAssets.ContainsKey(soundName))
        {
            if (Instance._loadedAssets[soundName] != null)
            {
                var sound = Instance._loadedAssets[soundName];
                playingObject = PlayClip(sound, settings);
            }
            else
            {
                playingObject = await PlayLoadingAsset(soundName, settings);
            }
        }
        else
        {
            playingObject = await LoadAndPlayAsync(soundName, settings);
        }

        return playingObject;
    }

    public static void PlaySound(string soundName, AudioSourceSettings settings,
        Action<SoundObject> loadCompleted = null)
    {
        SoundObject playingObject;
        if (Instance._loadedAssets.ContainsKey(soundName))
        {
            if (Instance._loadedAssets[soundName] != null)
            {
                var sound = Instance._loadedAssets[soundName];
                playingObject = PlayClip(sound, settings);
                loadCompleted?.Invoke(playingObject);
            }
            else
            {
                PlayLoadingAsset(soundName, settings, loadCompleted).Forget();
            }
        }
        else
        {
            LoadAndPlayAsync(soundName, settings, loadCompleted).Forget();
        }
    }

    private static async UniTask<SoundObject> PlayLoadingAsset(string soundName, AudioSourceSettings settings,
        Action<SoundObject> loadCompleted = null)
    {
        await UniTask.WaitWhile(() => Instance._loadedAssets[soundName] == null);
        var playingObject = PlayClip(Instance._loadedAssets[soundName], settings);

        loadCompleted?.Invoke(playingObject);
        return playingObject;
    }

    public static async UniTask<SoundObject> LoadAndPlayAsync(string soundName, AudioSourceSettings settings,
        Action<SoundObject> loadCompleted = null)
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
        Instance._loadedAssets[soundName] = null;
        var audioClip = await assetHandle;
        
        Instance._loadedAssets[soundName] = assetHandle.Result;
        var playingObject = PlayClip(audioClip, settings);

        loadCompleted?.Invoke(playingObject);
        return playingObject;
    }

    public static SoundObject PlayClip(AudioClip audioClip, AudioSourceSettings settings)
    {
        var sound = Instance._poolManager.GetNewPoolable() as SoundObject;
        if (sound == null)
        {
            Debug.LogError("Sound Object is null. Game may be ending but this should not be null.");
        }

        Instance._activeSoundObjects.Add(sound);
        sound.Play(audioClip, settings);

        return sound;
    }

    public void ReturnToPool(SoundObject soundObject)
    {
        _activeSoundObjects.Remove(soundObject);
        _poolManager?.ReturnToPool(soundObject);
    }

    protected override void GameStateListener(GameState oldState, GameState newState)
    {
        if (oldState == GameState.PreparingToPlay && newState == GameState.Playing)
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

    public struct AudioSourceSettings
    {
        public bool Looping { get; private set; }
        public AudioMixerGroup MixerGroup { get; private set; }

        public float InitialVolume { get; private set; }

        public AudioSourceSettings(bool looping = false, AudioMixerGroup mixerGroup = null, float initialVolume = 1)
        {
            Looping = looping;
            MixerGroup = mixerGroup;
            InitialVolume = initialVolume;
        }
    }
}