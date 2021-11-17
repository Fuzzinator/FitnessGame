using System;
using System.Collections;
using System.Collections.Generic;
using BeatSaverSharp;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebSongRetriever : MonoBehaviour
{
    [SerializeField]
    private MapSortType _sortType;
    
    private const string BEATSAVERURL = "https://beatsaver.com";

    private const string MAPS = "/maps/";

    private string[] _sortingBy = {"top", "latest"};

    private BeatSaver _beatSaver;
    // Start is called before the first frame update
    private async void Start()
    {
        _beatSaver = new BeatSaver("Fitness Game", Version.Parse("0.0.1"));
        var request = await _beatSaver.LatestBeatmaps();
        if (request == null)
        {
            return;
        }
        foreach (var beatmap in request.Beatmaps)
        {
            Debug.Log(beatmap.Name);
        }
    }

    private enum MapSortType
    {
        TOP = 0,
        LATEST = 1,
    }
}
