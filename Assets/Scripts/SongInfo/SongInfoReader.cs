using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongInfoReader : MonoBehaviour
{
    public static SongInfoReader Instance { get; private set; }
    
    [TextArea]
    public string info;

    [SerializeField]
    public SongInfo songInfo;

    public float NoteSpeed => songInfo.NoteJumpMovementSpeed;
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
        UpdateSongInfo(info);
    }

    public void UpdateSongInfo(string json)
    {
        info = json;
        songInfo = JsonUtility.FromJson<SongInfo>(json);
    }
}
