using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

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

    private static readonly int[] TypeOptionsArray = new[]
    {
        0, 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 2, 3, 4, 5, 2, 3, 4, 5, 3, 4, 5, 3, 4, 3, 4
    };

    #region Const Strings

#if UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string DAT = ".dat";
    private const string TXT = ".txt";

    private const string V1V1 = "version\":\"1.";
    private const string V2V1 = "version\":\"2.";

    private const string V3V1 = "\"version\":\"3.";

    #endregion

    public static async UniTask<Choreography> AsyncLoadFromSongInfo(SongInfo info, DifficultyInfo difficultyInfo,
        CancellationToken token)
    {
        return await AsyncLoadCustomSong(info.fileLocation, difficultyInfo.FileName, info.SongName, token);
    }

    public static Choreography LoadFromSongInfo(SongInfo info, DifficultyInfo difficultyInfo)
    {
        return LoadCustomSong(info.fileLocation, difficultyInfo.FileName, info.SongName);
    }

    private static async UniTask<Choreography> AsyncLoadCustomSong(string fileLocation, string fileName,
        string songName, CancellationToken token)
    {
        var path = $"{AssetManager.SongsPath}/{fileLocation}/{fileName}";
        try
        {
            var streamReader = new StreamReader(path);

            var json = await streamReader.ReadToEndAsync().AsUniTask().AttachExternalCancellation(token);
            return ReadJsonToChoreography(json, songName);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
        }

        return null;
    }

    private static Choreography LoadCustomSong(string fileLocation, string fileName, string songName)
    {
        var path = $"{AssetManager.SongsPath}/{fileLocation}/{fileName}";
        try
        {
            var streamReader = new StreamReader(path);

            var json = streamReader.ReadToEnd();
            return ReadJsonToChoreography(json, songName);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
        }

        return null;
    }

    private static Choreography ReadJsonToChoreography(string json, string songName)
    {
        Choreography choreography = null;

        if (!string.IsNullOrWhiteSpace(json))
        {
            var max = json.Length < 150 ? json.Length -1 : 150;
            var first150 = json[..max].Replace(" ", "");
            if (first150.Contains(V3V1))
            {
                var v3Choreography = JsonUtility.FromJson<BeatsaberV3Choreography>(json);
                if (v3Choreography.colorNotes != null)
                {
                    choreography = new Choreography(v3Choreography);
                }
            }
            else// if (first150.Contains(V1V1) || first150.Contains(V2V1))
            {
                try
                {
                    choreography = JsonUtility.FromJson<Choreography>(json);
                }
                catch (Exception e)
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.LoadFailed();
                        NotificationManager.ReportFailedToLoadInGame($"{songName}'s choreography failed to load.");
                    }

                    Debug.LogError(e);
                    return choreography;
                }
            }
        }

        if (LevelManager.Instance != null && choreography?.Notes == null)
        {
            LevelManager.Instance.LoadFailed();
            NotificationManager.ReportFailedToLoadInGame($"{songName}'s choreography failed to load.");
        }

        return choreography;
    }

    public static async UniTask<Choreography> AsyncLoadFromPlaylist(PlaylistItem item, DifficultyInfo difficultyInfo,
        CancellationToken token)
    {
        if (item.IsCustomSong)
        {
            var result = await AsyncLoadCustomSong(item.FileLocation, difficultyInfo.FileName, item.SongName, token);
            return result;
        }
        AsyncOperationHandle requestHandle = new AsyncOperationHandle();
        try
        {
            var txtVersion = difficultyInfo.FileName;
            if (txtVersion.EndsWith(DAT))
            {
                txtVersion = txtVersion.Replace(DAT, TXT);
            }

            var request = Addressables.LoadAssetAsync<TextAsset>($"{LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}");
            requestHandle = request;
            var json = await request.WithCancellation(token);
            if (json == null)
            {
                NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s choreography failed to load.");
            }
            var choreography = JsonUtility.FromJson<Choreography>((json).text);
            Addressables.Release(requestHandle);
            return choreography;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            if (requestHandle.IsValid())
            {
                Addressables.Release(requestHandle);
            }
        }

        return null;
    }

    public static async UniTask<bool> AsyncSave(Choreography choreography, string fileLocation, string fileName,
        string songName, CancellationToken token)
    {
        fileName = fileName.Replace("/", "");
        var path = $"{AssetManager.SongsPath}/{fileLocation}/{fileName}";
        try
        {
            using (var streamWriter = new StreamWriter(path))
            {
                var json = JsonUtility.ToJson(choreography);
                await streamWriter.WriteAsync(json).AsUniTask().AttachExternalCancellation(token);
            }

            return true;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
        }

        return false;
    }

    public async UniTask<Choreography> AddRotationEventsAsync()
    {
        var eventTypes = GetOptionTypes();
        if (_events == null || _events.Length == 0)
        {
            var targetLength = _notes.Length / 20;
            var events = new NativeArray<ChoreographyEvent>(targetLength, Allocator.TempJob);
            var notes = new NativeArray<ChoreographyNote>(_notes, Allocator.TempJob);
            var jobHandle = new CreateNewRotationEventsJob(ref events, notes.AsReadOnly(), eventTypes.AsReadOnly());
            try
            {
                await jobHandle.Schedule(events.Length, 8);
            }
            catch (Exception e)
            {
                eventTypes.Dispose();
                events.Dispose();
                notes.Dispose();
                if (e is not OperationCanceledException)
                {
                    Debug.LogError(e);
                }

                return this;
            }

            _events = jobHandle.Events.ToArray();
            eventTypes.Dispose();
            events.Dispose();
            notes.Dispose();
        }
        else
        {
            var events = new NativeArray<ChoreographyEvent>(_events, Allocator.TempJob);
            var jobHandle = new ReplaceRotationEventsJob(ref events, eventTypes);
            try
            {
                await jobHandle.Schedule(events.Length, 8);
            }
            catch (Exception e)
            {
                eventTypes.Dispose();
                events.Dispose();
                if (e is not OperationCanceledException)
                {
                    Debug.LogError(e);
                }

                return this;
            }

            _events = jobHandle.Events.ToArray();

            eventTypes.Dispose();
            events.Dispose();
        }

        return this;
    }

    public async UniTask<Choreography> AddObstaclesAsync(SongInfo info)
    {
        var bps = info.BeatsPerMinute / 60;
        var modifiedBPS = (bps / Mathf.Ceil(bps)) * 3;

        var beatCount = Mathf.FloorToInt(info.SongLength * modifiedBPS);

        var obstacles = new NativeArray<ChoreographyObstacle>((_notes.Length / 5) + beatCount, Allocator.TempJob);
        var notes = new NativeArray<ChoreographyNote>(_notes, Allocator.TempJob);
        var jobHandle = new AddObstaclesJob(ref obstacles, notes.AsReadOnly(), modifiedBPS);
        try
        {
            await jobHandle.Schedule(obstacles.Length, 8);
        }
        catch (Exception e)
        {
            obstacles.Dispose();
            notes.Dispose();
            if (e is not OperationCanceledException)
            {
                Debug.LogError(e);
            }
        }

        jobHandle.Obstacles.Sort(new SortISequenceable());
        _obstacles = jobHandle.Obstacles.ToArray();

        obstacles.Dispose();
        notes.Dispose();
        return this;
    }

    private struct SortISequenceable : IComparer<ChoreographyObstacle>
    {
        public int Compare(ChoreographyObstacle x, ChoreographyObstacle y)
        {
            return x.Time.CompareTo(y.Time);
        }
    }

    public static ChoreographyNote[] SetNotesToType(ChoreographyNote[] notes,
        ChoreographyNote.CutDirection cutDirection)
    {
        var nativeArray = new NativeArray<ChoreographyNote>(notes.Length, Allocator.TempJob);
        for (var i = 0; i < notes.Length; i++)
        {
            nativeArray[i] = notes[i];
        }

        var job = new SetNotesCutDirection(ref nativeArray, cutDirection);
        var jobHandler = job.Schedule(nativeArray.Length, 8);
        jobHandler.Complete();
        var tweakedNotes = job.Notes.ToArray();
        nativeArray.Dispose();
        return tweakedNotes;
    }

    public static ChoreographyNote[] SetNotesToSide(ChoreographyNote[] notes, HitSideType hitSideType)
    {
        var nativeArray = new NativeArray<ChoreographyNote>(notes.Length, Allocator.TempJob);
        for (var i = 0; i < notes.Length; i++)
        {
            nativeArray[i] = notes[i];
        }

        var job = new SetNotesCutSideTypeJob(ref nativeArray, hitSideType);
        var jobHandler = job.Schedule(nativeArray.Length, 8);
        jobHandler.Complete();
        var tweakedNotes = job.Notes.ToArray();

        nativeArray.Dispose();
        return tweakedNotes;
    }

    public static ChoreographyNote[] SwapNotesSide(ChoreographyNote[] notes)
    {
        for (var i = 0; i < notes.Length; i++)
        {
            notes[i] = notes[i].SwapSides();
        }
        return notes;
    }

    private NativeArray<int> GetOptionTypes()
    {
        return new NativeArray<int>(TypeOptionsArray, Allocator.Persistent);
    }

    [BurstCompile]
    private struct UnManagedChoreography
    {
        private NativeArray<ChoreographyEvent> _events;
        private NativeArray<ChoreographyNote> _notes;
        private NativeArray<ChoreographyObstacle> _obstacles;
        private ChoreographyCustomData _customData;
    }

    public Choreography()
    {
    }

    public Choreography(BeatsaberV3Choreography v3Choreography)
    {
        _version = "2.0.0";
        _events = new ChoreographyEvent[v3Choreography.EventCount];
        _notes = new ChoreographyNote[v3Choreography.NoteCount];
        _obstacles = new ChoreographyObstacle[v3Choreography.ObstacleCount];

        for (var i = 0; i < _events.Length; i++)
        {
            var rotEvent = v3Choreography.rotationEvents[i];
            var eventType = rotEvent.e == 0
                ? ChoreographyEvent.EventType.EarlyRotation
                : ChoreographyEvent.EventType.LateRotation;
            var rotValue = ChoreographyEvent.FloatToValue(rotEvent.r);
            _events[i] = new ChoreographyEvent(rotEvent.b, eventType, rotValue);
        }

        var targetIndex = 0;
        var blockIndex = 0;
        var completedTargets = false;
        var completedBlocks = false;
        for (var i = 0; i < _notes.Length; i++)
        {
            var targetNote = v3Choreography.colorNotes[targetIndex];
            var time = targetNote.b;
            var lineIndex = targetNote.x;
            var lineLayer = targetNote.y;
            var type = targetNote.c;
            var cutDir = targetNote.d;

            if (!completedBlocks && v3Choreography.bombNotes.Length > 0 &&
                targetNote.b > v3Choreography.bombNotes[blockIndex].b)
            {
                if (blockIndex + 1 < v3Choreography.bombNotes.Length)
                {
                    blockIndex++;
                }
                else
                {
                    completedBlocks = true;
                }

                var blockNote = v3Choreography.bombNotes[blockIndex];
                time = blockNote.b;
                lineIndex = blockNote.x;
                lineLayer = blockNote.y;
                type = blockNote.C;
            }
            else if (!completedTargets)
            {
                if (targetIndex + 1 < v3Choreography.colorNotes.Length)
                {
                    targetIndex++;
                }
                else
                {
                    completedTargets = true;
                }
            }

            _notes[i] = new ChoreographyNote(time, lineIndex, lineLayer, type, cutDir, false);
        }

        for (var i = 0; i < _obstacles.Length; i++)
        {
            var obstacle = v3Choreography.obstacles[i];
            var obstacleType = (int)obstacle.y < 1
                ? ChoreographyObstacle.ObstacleType.Dodge
                : ChoreographyObstacle.ObstacleType.Crouch;

            _obstacles[i] = new ChoreographyObstacle(obstacle.b, obstacle.d, obstacleType, obstacle.x, obstacle.w);
        }
    }
}

[BurstCompile]
public struct ReplaceRotationEventsJob : IJobParallelFor
{
    public readonly NativeArray<ChoreographyEvent> Events => _events;
    private NativeArray<ChoreographyEvent> _events;
    private uint _seed;
    private const int INTERVAL = 10;

    private const int EARLYROTATION = 14;
    private const int LATEROTATION = 15;

    private readonly NativeArray<int> _rotateEventValues;

    public ReplaceRotationEventsJob(ref NativeArray<ChoreographyEvent> events, NativeArray<int> rotateEventValues)
    {
        _events = events;
        _rotateEventValues = rotateEventValues;
        _seed = 0 + 118 + 999 + 881 + 999 + 119 + 725;
    }

    public void Execute(int index)
    {
        if (index % INTERVAL == 0)
        {
            var newEvent = new ChoreographyEvent(_events[index].Time, GetEventType(index), GetRotationEvent(index));
            _events[index] = newEvent;
        }
    }

    private ChoreographyEvent.EventType GetEventType(int index)
    {
        if (index + _seed > uint.MaxValue)
        {
            index = (int)(index * .5);
        }

        var random = new Unity.Mathematics.Random((uint)(_seed + index));
        var randValue = random.NextInt(EARLYROTATION, LATEROTATION);
        for (var i = 0; i < INTERVAL; i++)
        {
            randValue = random.NextInt(EARLYROTATION, LATEROTATION);
        }

        return (ChoreographyEvent.EventType)randValue;
    }

    private ChoreographyEvent.RotateEventValue GetRotationEvent(int index)
    {
        if (index + _seed > uint.MaxValue)
        {
            index = (int)(index * .5);
        }

        var random = new Unity.Mathematics.Random((uint)(_seed + index));
        var randValue = random.NextInt(0, _rotateEventValues.Length - 1);
        for (var i = 0; i < INTERVAL; i++)
        {
            randValue = random.NextInt(0, _rotateEventValues.Length - 1);
        }

        var value = (ChoreographyEvent.RotateEventValue)_rotateEventValues.AsReadOnly()[randValue];
        return value;
    }
}


[BurstCompile]
public struct CreateNewRotationEventsJob : IJobParallelFor
{
    public readonly NativeArray<ChoreographyEvent> Events => _events;
    private NativeArray<ChoreographyEvent> _events;
    private uint _seed;
    private const int INTERVAL = 20;

    private const int EARLYROTATION = 14;
    private const int LATEROTATION = 15;

    private readonly NativeArray<int>.ReadOnly _rotateEventValues;
    private readonly NativeArray<ChoreographyNote>.ReadOnly _notes;

    public CreateNewRotationEventsJob(ref NativeArray<ChoreographyEvent> events, NativeArray<ChoreographyNote>.ReadOnly notes,
        NativeArray<int>.ReadOnly rotateEventValues)
    {
        _events = events;
        _notes = notes;
        _rotateEventValues = rotateEventValues;
        _seed = 0 + 118 + 999 + 881 + 999 + 119 + 725;
    }

    public void Execute(int index)
    {
        var targetIndex = index * INTERVAL;
        if (targetIndex >= _notes.Length)
        {
            return;
        }

        var newEvent =
            new ChoreographyEvent(_notes[index * INTERVAL].Time, GetEventType(index), GetRotationEvent(index));
        _events[index] = newEvent;
    }

    private ChoreographyEvent.EventType GetEventType(int index)
    {
        if (index + _seed > uint.MaxValue)
        {
            index = (int)(index * .5);
        }

        var random = new Unity.Mathematics.Random((uint)(_seed + index));
        var randValue = random.NextInt(EARLYROTATION, LATEROTATION);
        for (var i = 0; i < INTERVAL; i++)
        {
            randValue = random.NextInt(EARLYROTATION, LATEROTATION);
        }

        return (ChoreographyEvent.EventType)randValue;
    }

    private ChoreographyEvent.RotateEventValue GetRotationEvent(int index)
    {
        if (index + _seed > uint.MaxValue)
        {
            index = (int)(index * .5);
        }

        var random = new Unity.Mathematics.Random((uint)(_seed + index));
        var randValue = random.NextInt(0, _rotateEventValues.Length - 1);
        for (var i = 0; i < INTERVAL; i++)
        {
            randValue = random.NextInt(0, _rotateEventValues.Length - 1);
        }

        var value = (ChoreographyEvent.RotateEventValue)_rotateEventValues[randValue];
        return value;
    }
}

[BurstCompile]
public struct AddObstaclesJob : IJobParallelFor
{
    public readonly NativeArray<ChoreographyObstacle> Obstacles => _obstacles;
    private NativeArray<ChoreographyObstacle> _obstacles;
    private readonly NativeArray<ChoreographyNote>.ReadOnly _notes;

    private uint _seed;
    private readonly float _bps;

    private const int INTERVAL = 15;

    [DeallocateOnJobCompletion]
    private readonly NativeArray<int> _obstacleOptions;
    [DeallocateOnJobCompletion]
    private readonly NativeArray<int> _obstacleSideOptions;

    public AddObstaclesJob(ref NativeArray<ChoreographyObstacle> obstacles, NativeArray<ChoreographyNote>.ReadOnly sourceNotes,
        float bps)
    {
        _obstacles = obstacles;
        _notes = sourceNotes;
        _seed = 0 + 118 + 999 + 881 + 999 + 119 + 725;
        _bps = bps;
        _obstacleOptions = new NativeArray<int>(15, Allocator.TempJob);
        for (var i = 0; i < _obstacleOptions.Length; i++)
        {
            _obstacleOptions[i] = 5 > i ? 0 : 1;
        }
        _obstacleSideOptions = new NativeArray<int>(15, Allocator.TempJob);
        for (var i = 0; i < _obstacleSideOptions.Length; i++)
        {
            _obstacleSideOptions[i] = i % 2 == 0 ? 0 : 2;
        }
    }

    public void Execute(int index)
    {
        var noteIndex = index * INTERVAL;
        ChoreographyObstacle newObstacle;
        if (noteIndex < _notes.Length)
        {
            newObstacle = new ChoreographyObstacle(_notes[noteIndex].Time, 1, GetObstacleType(index, out var lineIndex),
                _notes[noteIndex].LineIndex, 1);
        }
        else
        {
            var newIndex = index - (_notes.Length / INTERVAL);
            var type = GetObstacleType(index, out var lineIndex);
            newObstacle = new ChoreographyObstacle(_bps * newIndex, 1, type, lineIndex, 1);
        }

        _obstacles[index] = newObstacle;
    }

    private ChoreographyObstacle.ObstacleType GetObstacleType(int index, out int lineIndex)
    {
        if (index + _seed > uint.MaxValue)
        {
            index = (int)(index * .5);
        }

        var random = new Unity.Mathematics.Random((uint)(_seed + index));
        var length = _obstacleOptions.Length - 1;
        var randValue = random.NextInt(0, length);
        for (var i = 0; i < INTERVAL; i++)
        {
            randValue = random.NextInt(0, length);
        }

        var value = (ChoreographyObstacle.ObstacleType)_obstacleOptions.AsReadOnly()[randValue];
        lineIndex = _obstacleSideOptions.AsReadOnly()[randValue] - 1;
        return value;
    }
}

[BurstCompile]
public struct SetNotesCutDirection : IJobParallelFor
{
    public NativeArray<ChoreographyNote> Notes => _notes;
    private NativeArray<ChoreographyNote> _notes;

    private ChoreographyNote.CutDirection _cutDirection;

    public SetNotesCutDirection(ref NativeArray<ChoreographyNote> notes, ChoreographyNote.CutDirection cutDirection)
    {
        _notes = notes;
        _cutDirection = cutDirection;
    }

    public void Execute(int index)
    {
        _notes[index] = _notes[index].SetCutDirection(_cutDirection);
        if (_cutDirection != ChoreographyNote.CutDirection.Jab)
        {
            return;
        }

        if (_notes[index].HitSideType == HitSideType.Block)
        {
            var random = new Unity.Mathematics.Random((uint)(index));
            var randValue = random.NextInt(0, 1);
            for (var i = 0; i < 5; i++)
            {
                randValue = random.NextInt(0, 1);
            }

            _notes[index] = _notes[index].SetType((HitSideType)randValue);
        }
    }
}

[BurstCompile]
public struct SetNotesCutSideTypeJob : IJobParallelFor
{
    public NativeArray<ChoreographyNote> Notes => _notes;
    private NativeArray<ChoreographyNote> _notes;

    private HitSideType _hitSideType;

    public SetNotesCutSideTypeJob(ref NativeArray<ChoreographyNote> notes, HitSideType hitSideType)
    {
        _notes = notes;
        _hitSideType = hitSideType;
    }

    public void Execute(int index)
    {
        _notes[index] = _notes[index].SetType(_hitSideType);
    }
}
