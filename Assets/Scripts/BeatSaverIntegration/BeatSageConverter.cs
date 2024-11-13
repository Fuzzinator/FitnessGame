using Cysharp.Threading.Tasks;
using GameModeManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using static ChoreographyEvent;

public static class BeatSageConverter
{
    private static List<string> s_fileNames = new();
    private static List<FileInfo> s_easyFiles = new();
    private static List<FileInfo> s_normalFiles = new();
    private static List<FileInfo> s_hardFiles = new();
    private static List<FileInfo> s_expertFiles = new();
    private static List<FileInfo> s_expertPlusFiles = new();

    private static FileInfo s_infoFile = null;

    public static async UniTask<FileInfo> ConvertSong(SongInfo songInfo, CancellationToken token)
    {
        s_infoFile = await AssetManager.ConvertFromBeatSage(songInfo, songInfo.fileLocation, false, s_easyFiles, s_normalFiles, s_hardFiles, s_expertFiles, s_expertPlusFiles, token);
        var choreographies = new Dictionary<string, SimpleTuple<GameMode, Choreography>>();
        await CorrectBPM(songInfo, choreographies);
        await AddRotationsAndObstacles(songInfo, token, choreographies);
        await SaveChoreographies(songInfo, choreographies);
        return s_infoFile;
    }

    public static async UniTask CorrectBPM(SongInfo info, Dictionary<string, SimpleTuple<GameMode, Choreography>> choreographies)
    {
        var bpm = await GetCorrectBPM(info);
        await CorrectBPM(info, bpm, choreographies);
    }

    public static async UniTask<int> GetCorrectBPM(SongInfo info)
    {
        var song = await AssetManager.LoadCustomSong(info.fileLocation, info, new System.Threading.CancellationToken(), false);
        var bpm = await UniBpmAnalyzer.TryAnalyzeBpmWithJobs(song);
        return bpm;
    }

    public static async UniTask CorrectBPM(SongInfo info, int correctBPM, Dictionary<string, SimpleTuple<GameMode, Choreography>> choreographies, bool roundToInt = true, bool skipNotes = false, bool skipObstacles = false, bool skipEvents = false)
    {
        var beatsTime = correctBPM / info.BeatsPerMinute;
        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];
            Array.Sort(set.DifficultyInfos, (x, y) => x.DifficultyRank.CompareTo(y.DifficultyRank));
            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];
                if(choreographies.ContainsKey(difInfo.FileName))
                {
                    continue;
                }

                var choreography = Choreography.LoadFromSongInfo(info, difInfo);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
                if (choreography != null)
                {
                    if (!skipNotes)
                    {
                        for (var k = 0; k < choreography.Notes.Length; k++)
                        {
                            var note = choreography.Notes[k];
                            var time = note.Time * beatsTime;
                            if (roundToInt)
                            {
                                var previousTime = k > 0 ? choreography.Notes[k - 1].Time : 0;
                                time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                            }

                            choreography.Notes[k] = new ChoreographyNote(time, note.LineIndex, note.LineLayer, note.HitSideType, note.CutDir, note.IsSuperNote);
                        }
                    }

                    if (!skipObstacles)
                    {

                        for (var k = 0; k < choreography.Obstacles.Length; k++)
                        {
                            var obst = choreography.Obstacles[k];
                            var time = obst.Time * beatsTime;
                            if (roundToInt)
                            {
                                var previousTime = k > 0 ? choreography.Obstacles[k - 1].Time : 0;
                                time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                            }
                            choreography.Obstacles[k] = new ChoreographyObstacle(time, obst.Duration, obst.Type, obst.LineIndex, obst.Width);
                        }
                    }

                    if (!skipEvents)
                    {
                        for (var k = 0; k < choreography.Events.Length; k++)
                        {
                            var even = choreography.Events[k];
                            var time = even.Time * beatsTime;
                            if (roundToInt)
                            {
                                var previousTime = k > 0 ? choreography.Events[k - 1].Time : 0;
                                time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                            }
                            choreography.Events[k] = new ChoreographyEvent(time, even.Type, (RotateEventValue)even.Value);
                        }
                    }
                }

                choreographies[difInfo.FileName] = new(set.MapGameMode, choreography);
            }
        }
        info.SetBPM(correctBPM);
    }

    public static async UniTask MakeAdditive(SongInfo songInfo)
    {
        for (var i = 0; i < songInfo.DifficultySets.Length; i++)
        {
            var set = songInfo.DifficultySets[i];

            for (var j = 1; j < set.DifficultyInfos.Length; j++)
            {
                var currentDifInfo = set.DifficultyInfos[j];
                var prevDifInfo = set.DifficultyInfos[j - 1];

                var currentChoreography = Choreography.LoadFromSongInfo(songInfo, currentDifInfo);
                var prevChoreography = Choreography.LoadFromSongInfo(songInfo, prevDifInfo);

                await UniTask.Delay(TimeSpan.FromSeconds(.1f));

                if (currentChoreography != null)
                {
                    var allNotes = new List<ChoreographyNote>();
                    allNotes.AddRange(currentChoreography.Notes);
                    var toAdd = new List<ChoreographyNote>();
                    foreach (var note in prevChoreography.Notes)
                    {
                        if (allNotes.Exists((x) => x.Time == note.Time) || note.Time > allNotes[^1].Time)
                        {
                            continue;
                        }
                        toAdd.Add(note);
                    }
                    allNotes.AddRange(toAdd);
                    allNotes.Sort((x, y) => x.Time.CompareTo(y.Time));

                    currentChoreography.SetNotes(allNotes.ToArray());
                }
                await WriteCustomSong(songInfo.fileLocation, currentDifInfo.FileName, currentChoreography);
            }
        }
    }


    private static async UniTask AddRotationsAndObstacles(SongInfo info, CancellationToken token, Dictionary<string, SimpleTuple<GameMode, Choreography>> choreographies)
    {
        foreach (var set in choreographies)
        {
            var gameMode = set.Value.Item1;
            var choreography = set.Value.Item2;
            await info.AsyncAddRotations(choreography, set.Key, gameMode, false, token);
            await info.AsyncAddObstacles(choreography, set.Key, gameMode, false, token);
        }
    }

    private static async UniTask SaveChoreographies(SongInfo info, Dictionary<string, SimpleTuple<GameMode, Choreography>> choreographies)
    {
        foreach (var keyPair in choreographies)
        {
            await WriteCustomSong(info.fileLocation, keyPair.Key, keyPair.Value.Item2);
        }
    }

    public static async UniTask CleanMaps(SongInfo info)
    {
        s_fileNames.Clear();

        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];
            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];
                if (s_fileNames.Contains(difInfo.FileName))
                {
                    continue;
                }
                s_fileNames.Add(difInfo.FileName);
                var choreography = Choreography.LoadFromSongInfo(info, difInfo);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
                if (choreography != null)
                {
                    var distinctNotes = new List<ChoreographyNote>();
                    foreach (var note in choreography.Notes)
                    {
                        if (!distinctNotes.Exists((i) => i.Time == note.Time))
                        {
                            distinctNotes.Add(note);
                        }
                    }

                    choreography.SetNotes(distinctNotes.ToArray());

                    var distinctObstacles = new List<ChoreographyObstacle>();
                    foreach (var obstacle in choreography.Obstacles)
                    {
                        if (!distinctObstacles.Exists((i) => i.Time == obstacle.Time))
                        {
                            distinctObstacles.Add(obstacle);
                        }
                    }

                    choreography.SetObstacles(distinctObstacles.ToArray());

                    var distinctEvents = new List<ChoreographyEvent>();
                    foreach (var e in choreography.Events)
                    {
                        if (!distinctEvents.Exists((i) => i.Time == e.Time))
                        {
                            distinctEvents.Add(e);
                        }
                    }

                    choreography.SetEvents(distinctEvents.ToArray());
                }
                await WriteCustomSong(info.fileLocation, difInfo.FileName, choreography);
            }
        }
    }

    private static float RoundTime(float value, DifficultyInfo.DifficultyEnum difficulty, float previousTime)
    {
        switch (difficulty)
        {
            case DifficultyInfo.DifficultyEnum.Easy:
                value = Mathf.RoundToInt(value);
                break;
            case DifficultyInfo.DifficultyEnum.Normal:
            case DifficultyInfo.DifficultyEnum.Hard:
                _:
                {
                    var time = 0.5f * Mathf.Round(value / 0.5f);
                    if (time - previousTime > .6)
                    {
                        value = Mathf.RoundToInt(value);
                    }
                    else
                    {
                        value = time;
                    }
                    break;
                }
            case DifficultyInfo.DifficultyEnum.Expert:
                {
                    var time = 0.25f * Mathf.Round(value / 0.25f);
                    if (time - previousTime > .3)
                    {
                        if (time - previousTime > .6)
                        {
                            value = Mathf.RoundToInt(value);
                        }
                        else
                        {
                            value = Mathf.RoundToInt(value);
                        }
                    }
                    else
                    {
                        value = time;
                    }
                }
                break;

        }
        return value;
    }

    private static async UniTask WriteCustomSong(string fileLocation, string fileName, Choreography choreography)
    {
        var path = $"{AssetManager.SongsPath}/{fileLocation}/{fileName}";

        using (var streamWriter = new StreamWriter(path))
        {
            await streamWriter.WriteAsync(JsonUtility.ToJson(choreography));
            streamWriter.Close();
        }
    }

    private static async UniTask WriteSongInfo(SongInfo songInfo)
    {
        var path = $"{AssetManager.SongsPath}{songInfo.fileLocation}/Info.dat";

        using (var streamWriter = new StreamWriter(path))
        {
            await streamWriter.WriteAsync(JsonUtility.ToJson(songInfo));
            streamWriter.Close();
        }
    }

    public struct SimpleTuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        public SimpleTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
