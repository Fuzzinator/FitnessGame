using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static ChoreographyEvent;
using System.IO;

public class SongMerger : EditorWindow
{
    private static SongMerger s_SongMergerWindow;
    private List<SongInfo> _customSongs = new();
    private string[] _customSongNames = Array.Empty<string>();
    private int[] _mergeWithIndex = Array.Empty<int>();

    private Vector2 _foundSongsScrollPos;

    [MenuItem("Tools/Merge Songs")]
    public static void OpenSongMerger()
    {
        if (s_SongMergerWindow == null)
        {
            s_SongMergerWindow = GetWindow<SongMerger>();
        }
    }

    private void OnGUI()
    {
        if (s_SongMergerWindow == null)
        {
            s_SongMergerWindow = this;
        }


        if (GUILayout.Button("Refresh Custom Songs"))
        {
            _customSongs.Clear();
            AssetManager.GetCustomSongs((info) =>
            {
                _customSongs.Add(info);
            },
            new System.Threading.CancellationTokenSource(), true).ContinueWith(() =>
            {
                _customSongNames = _customSongs.Select((i) => i.SongName).ToArray();
                _mergeWithIndex = new int[_customSongs.Count];
            }).Forget();
        }
        EditorGUILayout.BeginVertical("box");
        {
            if (_customSongs.Count != _customSongNames.Length)
            {
                EditorGUILayout.LabelField("Loading...");
            }
            else
            {

                _foundSongsScrollPos = EditorGUILayout.BeginScrollView(_foundSongsScrollPos);
                {
                    for (var index = 0; index < _customSongs.Count; index++)
                    {
                        var song = _customSongs[index];
                        EditorGUILayout.BeginHorizontal("Box");
                        {
                            EditorGUILayout.LabelField(song.SongName, "Merge with:");

                            _mergeWithIndex[index] = EditorGUILayout.Popup(_mergeWithIndex[index], _customSongNames);

                            if (GUILayout.Button("Merge"))
                            {
                                MergeSongs(index).Forget();
                            }

                            if(GUILayout.Button("Make Additive"))
                            {
                                MakeAdditive(index).Forget();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private async UniTaskVoid MergeSongs(int index)
    {
        var original = _customSongs[index];
        var mergeWith = _customSongs[_mergeWithIndex[index]];

        for (var i = 0; i < original.DifficultySets.Length; i++)
        {
            var originalSet = original.DifficultySets[i];
            var mergeSet = mergeWith.DifficultySets[i];

            for (var j = originalSet.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var origDifInfo = originalSet.DifficultyInfos[j];
                var mergeDifInfo = mergeSet.DifficultyInfos[j];

                var origChoreography = Choreography.LoadFromSongInfo(original, origDifInfo);
                var mergeChoreography = Choreography.LoadFromSongInfo(mergeWith, mergeDifInfo);

                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                if (origChoreography != null && mergeChoreography != null)
                {
                    var allNotes = new List<ChoreographyNote>();
                    allNotes.AddRange(origChoreography.Notes);
                    var toAdd = new List<ChoreographyNote>();
                    foreach (var note in mergeChoreography.Notes)
                    {
                        if(allNotes.Exists((x) => x.Time == note.Time) || note.Time > allNotes[^1].Time)
                        {
                            continue;
                        }
                        toAdd.Add(note);
                    }
                    allNotes.AddRange(toAdd);
                    allNotes.Sort((x,y) => x.Time.CompareTo(y.Time));

                    Debug.Log($"{toAdd.Count} notes added.");
                    origChoreography.SetNotes(allNotes.ToArray());
                }
                await WriteCustomSong(original.fileLocation, origDifInfo.FileName, origChoreography);
                Debug.Log($"{original.SongName} Mode:{originalSet.MapGameMode} Difficulty:{origDifInfo.Difficulty} COMPLETE");
            }
        }
        Debug.Log($"{original.SongName} COMPLETE");
    }

    private async UniTaskVoid MakeAdditive(int index)
    {
        var current = _customSongs[index];

        for (var i = 0; i < current.DifficultySets.Length; i++)
        {
            var set = current.DifficultySets[i];

            for (var j = 1; j < set.DifficultyInfos.Length; j++)
            {
                var currentDifInfo = set.DifficultyInfos[j];
                var prevDifInfo = set.DifficultyInfos[j - 1];

                var currentChoreography = Choreography.LoadFromSongInfo(current, currentDifInfo);
                var prevChoreography = Choreography.LoadFromSongInfo(current, prevDifInfo);

                await UniTask.Delay(TimeSpan.FromSeconds(1f));

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

                    Debug.Log($"{toAdd.Count} notes added.");
                    currentChoreography.SetNotes(allNotes.ToArray());
                }
                await WriteCustomSong(current.fileLocation, currentDifInfo.FileName, currentChoreography);
                Debug.Log($"{current.SongName} Mode:{set.MapGameMode} Difficulty:{currentDifInfo.Difficulty} COMPLETE");
            }
        }
        Debug.Log($"{current.SongName} COMPLETE");
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
}
