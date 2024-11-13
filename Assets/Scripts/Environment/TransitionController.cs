using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance {  get; private set; }

    [SerializeField]
    private float _transitionSpeed = 1;

    [SerializeField]
    private TransitionData[] _transitionDatas;

    [SerializeField]
    private UnityEvent _transitionStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent _transitionCompleted = new UnityEvent();

    [SerializeField]
    private GameState _resumedState;

    [SerializeField, ReadOnly]
    private float _longestClipTime;

    private int _propertyID;
    private CancellationToken _cancellationToken;
    private Func<bool> _reset;

    public float TransitionSpeed => _transitionSpeed;

    private const string AnimatorChange = "Change";
    private const string AnimatorPassthrough = "Passthrough";
    
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

    /*private void OnValidate()
    {
        foreach (var clip in _transitionDatas)
        {
            var length = clip.Length;
            if (length > _longestClipTime)
            {
                _longestClipTime = length;
            }
        }
    }*/

    public void RequestTransition()
    {
        RunTransition().Forget();
    }

    private async UniTaskVoid RunTransition()
    {
        await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);

        foreach (var data in _transitionDatas)
        {
            data.AnimController.SetBool(AnimatorPassthrough, false);
        }

        //var startingValue = _sourceMaterial.GetFloat(_propertyID);
        _transitionStarted?.Invoke();
        if (EnvironmentController.Instance != null)
        {
            await EnvironmentController.Instance.LoadEnvironmentAsync();
        }
        if (!SettingsManager.GetSetting(SettingsManager.REDUCEMOTION, false))
        {
            foreach (var data in _transitionDatas)
            {
                data.AnimController.SetTrigger(AnimatorChange);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_longestClipTime));
        }

        GameStateManager.Instance.SetState(_resumedState);
        _transitionCompleted?.Invoke();
    }

    public void TryLoadBaseLevel()
    {
        var playlist = PlaylistManager.Instance.CurrentPlaylist;
        
        if (playlist is not {isValid: true}) // this means if playlist == null || !playlist.isValid
        {
            NotificationManager.RequestNotification(
                new Notification.NotificationVisuals(
                    $"A song in {playlist.PlaylistName} is missing from this device. Cannot play {playlist.PlaylistName}. Please remove the missing song from the playlist or add it to this device.",
                    "Playlist Invalid",
                    autoTimeOutTime: 1.5f,
                    popUp: true));
        }
        else
        {
            ActiveSceneManager.Instance.LoadBaseLevel();
        }
    }

    [Serializable]
    private struct TransitionData
    {
        [SerializeField]
        private GameObject _gameObject;

        [SerializeField]
        private Animator _animator;

        public GameObject GameObj => _gameObject;
        public Animator AnimController => _animator;
        public float Length => _animator != null ? _animator.GetCurrentAnimatorClipInfo(0).Length : 0;
    }
    /*    
        [SerializeField]
        public Material _sourceMaterial;

        [SerializeField]
        public string _propertyName;

        [SerializeField]
        public float _transitionSpeed;
        [SerializeField]
        public float _targetValue;
        [SerializeField]
        public float _defaultValue;

        [SerializeField]
        public AnimationCurve _transitionCurve;
    
        [SerializeField]
        public UnityEvent _transitionStarted;
        [SerializeField]
        public UnityEvent _transitionCompleted;
    }*/
}