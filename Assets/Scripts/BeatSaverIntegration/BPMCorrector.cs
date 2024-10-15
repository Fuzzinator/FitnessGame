using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ChoreographyEvent;

public static class BPMCorrector
{
    public static async UniTask<int> GetCorrectBPM(SongInfo info)
    {
        var song = await AssetManager.LoadCustomSong(info.fileLocation, info, new System.Threading.CancellationToken(), false);
        var bpm = await UniBpmAnalyzer.TryAnalyzeBpmWithJobs(song);
        return bpm;
    }

    public static async UniTask CorrectBPM(SongInfo info)
    {
        var bpm = await GetCorrectBPM(info);
        await CorrectBPM(info, bpm);
    }

    public static async UniTask CorrectBPM(SongInfo info, int correctBPM, bool roundToInt = true, bool skipNotes = false, bool skipObstacles = false, bool skipEvents = false)
    {
        var beatsTime = correctBPM / info.BeatsPerMinute;
        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];
            Array.Sort(set.DifficultyInfos, (x, y) => x.DifficultyRank.CompareTo(y.DifficultyRank));
            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];

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

                await WriteCustomSong(info.fileLocation, difInfo.FileName, choreography);
            }
        }
        info.SetBPM(correctBPM);
        await WriteSongInfo(info);
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
}
