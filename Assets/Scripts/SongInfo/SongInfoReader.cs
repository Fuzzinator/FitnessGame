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

    public UnityEvent<PlaylistItem> finishedLoadingSongInfo = new UnityEvent<PlaylistItem>();

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
        if (item.IsCustomSong)
        {
            var streamReader = new StreamReader($"{Application.persistentDataPath}/Resources/{item.FileLocation}/Info.dat");
            var reading = streamReader.ReadToEndAsync();
            await reading;
            if (reading.IsCompleted)
            {
                UpdateSongInfo(reading.Result, item);
            }
            else
            {
                Debug.LogError("Failed to read song info");
                return;
            }
        }
        else
        {
            var request = Resources.LoadAsync<TextAsset>($"{item.FileLocation}/Info");
            await request;
            var json = request.asset as TextAsset;
            if (json == null)
            {
                Debug.LogError("Failed to load local resource file");
                return;
            }
            UpdateSongInfo(json.text, item);
        }
        item.SongInfo = songInfo;
        finishedLoadingSongInfo?.Invoke(item);
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