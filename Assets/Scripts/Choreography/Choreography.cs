using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

[Serializable]
public class Choreography
{
    public string Version => _version;
    public ChoreographyEvent[] Events => _events;
    public ChoreographyNote[] Notes => _notes;
    public ChoreographyObstacle[] Obstacles => _obstacles;
    public ChoreographyCustomData CustomData => _customData;


    [SerializeField]
    private string _version;

    [SerializeField]
    private ChoreographyEvent[] _events;

    [SerializeField]
    private ChoreographyNote[] _notes;

    [SerializeField]
    private ChoreographyObstacle[] _obstacles;

    [SerializeField]
    private ChoreographyCustomData _customData;

    #region Const Strings

#if UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string DAT = ".dat";
    private const string TXT = ".txt";

    #endregion

    public static async UniTask<Choreography> AsyncLoadFromSongInfo(SongInfo info, DifficultyInfo difficultyInfo,
        CancellationToken token)
    {
        return await AsyncLoadCustomSong(info.fileLocation, difficultyInfo.FileName, info.SongName, token);
    }

    private static async UniTask<Choreography> AsyncLoadCustomSong(string fileLocation, string fileName,
        string songName, CancellationToken token)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path =
            $"{Application.persistentDataPath}{SONGSFOLDER}{fileLocation}/{fileName}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{UNITYEDITORLOCATION}{fileLocation}/{fileName}";
#endif
        try
        {
            var streamReader = new StreamReader(path);

            var json = await streamReader.ReadToEndAsync().AsUniTask().AttachExternalCancellation(token);
            Choreography choreography = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                choreography = JsonUtility.FromJson<Choreography>(json);
            }

            if (choreography == null || choreography.Notes == null)
            {
                LevelManager.Instance.LoadFailed();
                NotificationManager.ReportFailedToLoadInGame($"{songName}'s choreography failed to load.");
            }

            return choreography;
        }
        catch (Exception e)when (e is OperationCanceledException)
        {
        }

        return null;
    }

    public static async UniTask<Choreography> AsyncLoadFromPlaylist(PlaylistItem item, DifficultyInfo difficultyInfo,
        CancellationToken token)
    {
        if (item.IsCustomSong)
        {
            return await AsyncLoadCustomSong(item.FileLocation, difficultyInfo.FileName, item.SongName, token);
        }

        try
        {
            var txtVersion = difficultyInfo.FileName;
            if (txtVersion.EndsWith(DAT))
            {
                txtVersion = txtVersion.Replace(DAT, TXT);
            }

            var request =
                Addressables.LoadAssetAsync<TextAsset>($"{LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}")
                    .WithCancellation(token);

            var json = await request;
            if (json == null)
            {
                NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s choreography failed to load.");
            }

            return JsonUtility.FromJson<Choreography>((json).text);
        }
        catch (Exception e)when (e is OperationCanceledException)
        {
        }

        return null;
    }

    public static async UniTask<bool> AsyncSave(Choreography choreography, string fileLocation, string fileName,
        string songName, CancellationToken token)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var path =
            $"{Application.persistentDataPath}{SONGSFOLDER}{fileLocation}/{fileName}";
#elif UNITY_EDITOR
        var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        var path = $"{dataPath}{UNITYEDITORLOCATION}{fileLocation}/{fileName}";
#endif
        try
        {
            using (var streamWriter = new StreamWriter(path))
            {
                var json = JsonUtility.ToJson(choreography);
                await streamWriter.WriteAsync(json).AsUniTask().AttachExternalCancellation(token);
            }

            return true;
        }
        catch (Exception e)when (e is OperationCanceledException)
        {
        }

        return false;
    }

    public async UniTask AddRotationEventsAsync()
    {
        var events = new NativeArray<ChoreographyEvent>(_events, Allocator.TempJob);
        var jobHandle = new AddRotationEventsJob(events, (uint) (Time.time * Random.Range(1, 10)));
        await jobHandle.Schedule(events.Length, 8);
        _events = jobHandle.Events.ToArray();
        events.Dispose();
    }

    [BurstCompile]
    private struct UnManagedChoreography
    {
        private NativeArray<ChoreographyEvent> _events;
        private NativeArray<ChoreographyNote> _notes;
        private NativeArray<ChoreographyObstacle> _obstacles;
        private ChoreographyCustomData _customData;
    }

    //[BurstCompile]
    private struct AddRotationEventsJob : IJobParallelFor
    {
        public NativeArray<ChoreographyEvent> Events => _events;
        private NativeArray<ChoreographyEvent> _events;
        private Unity.Mathematics.Random _random;
        private const int INTERVAL = 10;

        public AddRotationEventsJob(NativeArray<ChoreographyEvent> events, uint seed)
        {
            _events = events;
            _random = new Unity.Mathematics.Random(seed);
        }

        public void Execute(int index)
        {
            if (index % INTERVAL == 0)
            {
                _events[index] = new ChoreographyEvent(_events[index].Time,
                    (ChoreographyEvent.EventType) Math.Clamp((int) _events[index].Type * 2,
                        (int) ChoreographyEvent.EventType.EarlyRotation,
                        (int) ChoreographyEvent.EventType.LateRotation), _random);
            }
        }
    }
}