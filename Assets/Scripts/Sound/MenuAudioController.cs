using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class MenuAudioController : MonoBehaviour
{
    [SerializeField]
    private List<string> _menuMusic = new List<string>();
    
    [SerializeField]
    private AudioMixerGroup _menuMusicGroup;

    [SerializeField]
    private AudioMixerGroup _menuSFXGroup;

    private SoundObject _prevSoundObject;

    private CancellationTokenSource _cancellationTokenSource;
    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        PlayMenuMusic().Forget();
    }

    private async UniTaskVoid PlayMenuMusic()
    {
        var settings = new SoundManager.AudioSourceSettings(true, _menuMusicGroup, 0);
        
        var music = _menuMusic[Random.Range(0, _menuMusic.Count - 1)];
        var soundObject = await SoundManager.PlaySoundAsnyc(music, settings);
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            CleanUp(soundObject);
            return;
        }
        
        for (var f = 0f; f < 1; f+=.1f * Time.deltaTime)
        {
            soundObject.SetVolume(f);
            if (_prevSoundObject != null)
            {
                _prevSoundObject.SetVolume(1-f);
            }
                
            await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                CleanUp(soundObject);
                return;
            }
        }
        
        soundObject.SetVolume(1);
        if (_prevSoundObject != null)
        {
            _prevSoundObject.ReturnToPool();
        }
        _prevSoundObject = soundObject;

        await UniTask.Delay(TimeSpan.FromMinutes(Random.Range(.5f, 1)), cancellationToken: _cancellationTokenSource.Token);
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            CleanUp(soundObject);
            return;
        }
        PlayMenuMusic().Forget();
    }

    private void CleanUp(SoundObject soundObject = null)
    {
        if (soundObject != null)
        {
            soundObject.ReturnToPool();
        }

        if (_prevSoundObject != null)
        {
            _prevSoundObject.ReturnToPool();
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        CleanUp();
    }
}
