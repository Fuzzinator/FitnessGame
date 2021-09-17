using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class SongInfoReader : MonoBehaviour
{
    public static SongInfoReader Instance { get; private set; }
    
    [TextArea]
    public string info;

    [SerializeField]
    public SongInfo songInfo;

    [SerializeField]
    private DifficultyInfo _difficultyInfo;
    
    public UnityEvent finishedLoadingSongInfo = new UnityEvent();
    
    public float NoteSpeed => _difficultyInfo.MovementSpeed;
    public float BeatsPerMinute => songInfo.BeatsPerMinute;
    
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
        //UpdateSongInfo(info);
    }
    
    public void LoadJson(PlaylistItem item)
    {
#pragma warning disable 4014
        AsyncLoadJson(item);
#pragma warning restore 4014
    }

    private async UniTaskVoid AsyncLoadJson(PlaylistItem item)
    {
        using (var streamReader = new StreamReader($"{Application.dataPath}\\Resources\\{item.FileLocation}\\Info.dat"))
        {
            var reading = streamReader.ReadToEndAsync();
            await reading;
            UpdateSongInfo(reading.Result, item);
            finishedLoadingSongInfo?.Invoke();
        }
    }

    public void UpdateSongInfo(string json, PlaylistItem item)
    {
        info = json;
        songInfo = JsonUtility.FromJson<SongInfo>(json);
        _difficultyInfo = songInfo.TryGetActiveDifficultySet(item.Difficulty);
    }

    public AudioClip GetCurrentSong()
    {
        return null;
    }
}
