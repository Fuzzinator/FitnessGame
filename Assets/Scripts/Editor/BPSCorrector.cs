#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ChoreographyEvent;

public class BPSCorrector : EditorWindow
{
    private static BPSCorrector s_BPSCorrectorWindow;

    private bool _roundToInt;
    private bool _skipObstacles;
    private bool _skipAddingMaps;
    private Dictionary<string, int> _correctBPMs = new Dictionary<string, int>();
    private Dictionary<string, SongInfo> _customSongs = new Dictionary<string, SongInfo>();
    private List<string> _fileNames = new List<string>();

    private string _easy90RotJson;
    private string _medium90RotJson;
    private string _hard90RotJson;
    private string _expert90RotJson;

    private string _normalObstaclesJson;
    private string _expertObstaclesJson;

    private bool _showEasy90RotJson;
    private bool _showMedium90RotJson;
    private bool _showHard90RotJson;
    private bool _showExpert90RotJson;

    private bool _showNormalObstacleJson;
    private bool _showExpertObstacleJson;

    private Vector2 _easy90RotJsonPos;
    private Vector2 _medium90RotJsonPos;
    private Vector2 _hard90RotJsonPos;
    private Vector2 _expert90RotJsonPos;

    private Vector2 _normalObsJsonScrollPos;
    private Vector2 _expertObsJsonScrollPos;

    private Vector2 _foundSongsScrollPos;


    [MenuItem("Tools/BPS Corrector Tool")]
    public static void OpenRenameTool()
    {
        if (s_BPSCorrectorWindow == null)
        {
            s_BPSCorrectorWindow = GetWindow<BPSCorrector>();
        }
    }

    private void OnGUI()
    {
        if (s_BPSCorrectorWindow == null)
        {
            s_BPSCorrectorWindow = this;
        }
        //_correctBPS = EditorGUILayout.IntField("Correct BPS", _correctBPS);
        _roundToInt = EditorGUILayout.Toggle("Round to Int", _roundToInt);
        _skipObstacles = EditorGUILayout.Toggle("Skip Correcting Obstacles", _skipObstacles);
        _skipAddingMaps = EditorGUILayout.Toggle("Skip adding Maps", _skipAddingMaps);

        DisplayRotationEventStuff();


        EditorGUILayout.BeginHorizontal();
        {
            _showNormalObstacleJson = EditorGUILayout.Foldout(_showNormalObstacleJson, "Normal Obstacles Json");
            if (GUILayout.Button("Test Json"))
            {
                Debug.Log(JsonUtility.FromJson(_normalObstaclesJson, typeof(Obs)) != null ? "Success" : "Failure");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (_showNormalObstacleJson)
        {
            _normalObsJsonScrollPos = EditorGUILayout.BeginScrollView(_normalObsJsonScrollPos);
            _normalObstaclesJson = EditorGUILayout.TextArea(_normalObstaclesJson);
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.BeginHorizontal();
        {
            _showExpertObstacleJson = EditorGUILayout.Foldout(_showExpertObstacleJson, "Expert Obstacles Json");
            if (GUILayout.Button("Test Json"))
            {
                Debug.Log(JsonUtility.FromJson(_normalObstaclesJson, typeof(Obs)) != null ? "Success" : "Failure");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (_showExpertObstacleJson)
        {
            _expertObsJsonScrollPos = EditorGUILayout.BeginScrollView(_expertObsJsonScrollPos);
            _expertObstaclesJson = EditorGUILayout.TextArea(_expertObstaclesJson);
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("Refresh Custom Songs"))
        {
            AssetManager.GetCustomSongs((info) => _customSongs[info.SongName] = info, new System.Threading.CancellationTokenSource(), _skipAddingMaps).Forget();
        }
        /*var file = (TextAsset)EditorGUILayout.ObjectField("info.txt file:", _infoFile, typeof(TextAsset), false);
        if(file != _infoFile)
        {
            _infoFile = file;
            if(_infoFile == null)
            {
                _fileName = string.Empty;
            }
            else
            {
                var item = JsonUtility.FromJson<SongInfo>(_infoFile.text);
                _fileName = item.SongName;
            }
        }*/

        /*var count = EditorGUILayout.IntField("filesToFix:", _toCorrect.Count);
        while(count > _toCorrect.Count)
        {
            _toCorrect.Add(null);
        }
        while(count < _toCorrect.Count)
        {
            _toCorrect.RemoveAt(_toCorrect.Count - 1);
        }*/

        EditorGUILayout.BeginVertical("box");
        _foundSongsScrollPos = EditorGUILayout.BeginScrollView(_foundSongsScrollPos);
        foreach (var song in _customSongs)
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(song.Key, song.Value.BeatsPerMinute.ToString());
            if (!_correctBPMs.ContainsKey(song.Key))
            {
                _correctBPMs[song.Key] = 0;
            }
            _correctBPMs[song.Key] = EditorGUILayout.IntField("Target BPS", _correctBPMs[song.Key]);

            /*if (GUILayout.Button("Get Correct BPS"))
            {
                GetCorrectBPS(song.Value).Forget();
            }*/
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto Generate Maps"))
            {
                GenerateMaps(song).Forget();
            }
            if(GUILayout.Button("Convert From Beat Sage"))
            {
                RenameBeatmaps(song.Value).Forget();
            }
            if (GUILayout.Button("Set Correct BPS"))
            {
                CorrectBPS(song.Value).Forget();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Rotation Events"))
            {
                SetRotationEvents(song.Value).Forget();
            }
            if (GUILayout.Button("Add Obstacles"))
            {
                SetObstacles(song.Value).Forget();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Convert All To Local Maps"))
        {
            ConvertToLocalMaps().Forget();
        }
    }

    private void DisplayRotationEventStuff()
    {
        RotationEventDisplay(ref _showEasy90RotJson, "Easy", ref _easy90RotJsonPos, ref _easy90RotJson);
        RotationEventDisplay(ref _showMedium90RotJson, "Medium", ref _medium90RotJsonPos, ref _medium90RotJson);
        RotationEventDisplay(ref _showHard90RotJson, "Hard", ref _hard90RotJsonPos, ref _hard90RotJson);
        RotationEventDisplay(ref _showExpert90RotJson, "Expert", ref _expert90RotJsonPos, ref _expert90RotJson);
    }

    private void RotationEventDisplay(ref bool showJson, string foldoutText, ref Vector2 scrollPos, ref string json)
    {
        EditorGUILayout.BeginHorizontal();
        {
            showJson = EditorGUILayout.Foldout(showJson, $"{foldoutText} 90 Rotation Json");
            if (GUILayout.Button("Test Json"))
            {
                Debug.Log(JsonUtility.FromJson(json, typeof(Evs)) != null ? "Success" : "Failure");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (showJson)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            json = EditorGUILayout.TextArea(json);
            EditorGUILayout.EndScrollView();
        }
    }

    private async UniTaskVoid ConvertToLocalMaps()
    {
        var localPath = "D:\\Projects\\FitnessGame\\Assets\\Music\\Songs";
        foreach (var song in _customSongs)
        {
            await AssetManager.ConvertToLocal(song.Value.fileLocation, localPath, new System.Threading.CancellationToken());
            Debug.Log($"{song.Key} converted To local song.");
        }
    }
    private async UniTaskVoid GenerateMaps(KeyValuePair<string, SongInfo> song)
    {
        var path = $"{AssetManager.SongsPath}{song.Value.fileLocation}";
        _customSongs[song.Key] = await AssetManager.GetSingleCustomSong(path, new System.Threading.CancellationToken());

    }

    private async UniTaskVoid GetCorrectBPS(SongInfo info)
    {
        var song = await AssetManager.LoadCustomSong(info.fileLocation, info, new System.Threading.CancellationToken());
        _correctBPMs[info.SongName] = UniBpmAnalyzer.AnalyzeBpm(song);
    }

    private async UniTaskVoid CorrectBPS(SongInfo info)
    {
        _fileNames.Clear();
        var beatsTime = _correctBPMs[info.SongName] / info.BeatsPerMinute;

        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];
            Array.Sort(set.DifficultyInfos, (x, y) => x.DifficultyRank.CompareTo(y.DifficultyRank));
            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];
                if (_fileNames.Contains(difInfo.FileName))
                {
                    Debug.LogWarning($"Skipping:{info.SongFilename} Mode:{set.MapGameMode} Difficulty:{difInfo.Difficulty}, {difInfo.FileName} already updated.");
                    continue;
                }
                _fileNames.Add(difInfo.FileName);
                var choreography = Choreography.LoadFromSongInfo(info, difInfo);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
                if (choreography != null)
                {
                    for (var k = 0; k < choreography.Notes.Length; k++)
                    {
                        var note = choreography.Notes[k];
                        var time = note.Time * beatsTime;
                        if (_roundToInt)
                        {
                            var previousTime = k > 0 ? choreography.Notes[k - 1].Time : 0;
                            time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                        }

                        choreography.Notes[k] = new ChoreographyNote(time, note.LineIndex, note.LineLayer, note.HitSideType, note.CutDir, note.IsSuperNote);
                    }

                    if (!_skipObstacles)
                    {

                        for (var k = 0; k < choreography.Obstacles.Length; k++)
                        {
                            var obst = choreography.Obstacles[k];
                            var time = obst.Time * beatsTime;
                            if (_roundToInt)
                            {
                                var previousTime = k > 0 ? choreography.Obstacles[k - 1].Time : 0;
                                time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                            }
                            choreography.Obstacles[k] = new ChoreographyObstacle(time, obst.Duration, obst.Type, obst.LineIndex, obst.Width);
                        }
                    }

                    for (var k = 0; k < choreography.Events.Length; k++)
                    {
                        var even = choreography.Events[k];
                        var time = even.Time * beatsTime;
                        if (_roundToInt)
                        {
                            var previousTime = k > 0 ? choreography.Events[k - 1].Time : 0;
                            time = RoundTime(time, difInfo.DifficultyAsEnum, previousTime);
                        }
                        choreography.Events[k] = new ChoreographyEvent(time, even.Type, (RotateEventValue)even.Value);
                    }
                }
                await WriteCustomSong(info.fileLocation, difInfo.FileName, choreography);
                Debug.Log($"{info.SongName} Mode:{set.MapGameMode} Difficulty:{difInfo.Difficulty} COMPLETE");
            }
        }
        info.SetBPS(_correctBPMs[info.SongName]);
        await WriteSongInfo(info);
        Debug.Log($"{info.SongName} COMPLETE");
    }

    private async UniTaskVoid RenameBeatmaps(SongInfo info)
    {
        await AssetManager.ConvertFromBeatSage(info.fileLocation, new System.Threading.CancellationToken());
        Debug.Log($"{info.fileLocation} converted.");
    }

    private async UniTaskVoid SetRotationEvents(SongInfo info)
    {
        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];
            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];

                var choreography = Choreography.LoadFromSongInfo(info, difInfo);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                if (set.MapGameMode == GameModeManagement.GameMode.Degrees90)
                {
                    if (choreography != null)
                    {
                        choreography.SetEvents(GetRotationEvents(difInfo.DifficultyAsEnum));
                    }
                }
                else
                {
                    if (choreography != null)
                    {
                        choreography.SetObstacles(null);
                    }
                }

                await WriteCustomSong(info.fileLocation, difInfo.FileName, choreography);
                Debug.Log($"{info.SongName} Mode:{set.MapGameMode} Difficulty:{difInfo.Difficulty} COMPLETE");
            }
        }
    }

    private async UniTaskVoid SetObstacles(SongInfo info)
    {
        for (var i = 0; i < info.DifficultySets.Length; i++)
        {
            var set = info.DifficultySets[i];

            for (var j = set.DifficultyInfos.Length - 1; j >= 0; j--)
            {
                var difInfo = set.DifficultyInfos[j];

                var choreography = Choreography.LoadFromSongInfo(info, difInfo);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                if (choreography != null)
                {
                    choreography.SetObstacles(GetObstacles(difInfo.DifficultyAsEnum));
                }

                await WriteCustomSong(info.fileLocation, difInfo.FileName, choreography);
                Debug.Log($"{info.SongName} Mode:{set.MapGameMode} Difficulty:{difInfo.Difficulty} COMPLETE");
            }
        }
    }

    private ChoreographyObstacle[] GetObstacles(DifficultyInfo.DifficultyEnum difficulty)
    {
        switch (difficulty)
        {
            case DifficultyInfo.DifficultyEnum.Easy:
            case DifficultyInfo.DifficultyEnum.Normal:
                return ((Obs)JsonUtility.FromJson(_normalObstaclesJson, typeof(Obs))).Obstacles;
            case DifficultyInfo.DifficultyEnum.Hard:
            case DifficultyInfo.DifficultyEnum.Expert:
                return ((Obs)JsonUtility.FromJson(_expertObstaclesJson, typeof(Obs))).Obstacles;
            case DifficultyInfo.DifficultyEnum.Unset:
                Debug.LogWarning("Song has Unset difficulty this shouldnt be happening");
                return ((Obs)JsonUtility.FromJson(_normalObstaclesJson, typeof(Obs))).Obstacles;
        }
        return null;
    }
    private ChoreographyEvent[] GetRotationEvents(DifficultyInfo.DifficultyEnum difficulty)
    {
        switch (difficulty)
        {
            case DifficultyInfo.DifficultyEnum.Easy:
                return ((Evs)JsonUtility.FromJson(_easy90RotJson, typeof(Evs))).Events;
            case DifficultyInfo.DifficultyEnum.Normal:
                return ((Evs)JsonUtility.FromJson(_medium90RotJson, typeof(Evs))).Events;
            case DifficultyInfo.DifficultyEnum.Hard:
                return ((Evs)JsonUtility.FromJson(_hard90RotJson, typeof(Evs))).Events;
            case DifficultyInfo.DifficultyEnum.Expert:
                return ((Evs)JsonUtility.FromJson(_expert90RotJson, typeof(Evs))).Events;
            case DifficultyInfo.DifficultyEnum.Unset:
                Debug.LogWarning("Song has Unset difficulty this shouldnt be happening");
                return ((Evs)JsonUtility.FromJson(_easy90RotJson, typeof(Evs))).Events;
        }
        return null;
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

    private async UniTask WriteSongInfo(SongInfo songInfo)
    {
        var path = $"{AssetManager.SongsPath}{songInfo.fileLocation}/Info.dat";

        using (var streamWriter = new StreamWriter(path))
        {
            await streamWriter.WriteAsync(JsonUtility.ToJson(songInfo));
            streamWriter.Close();
        }
    }

    private float RoundTime(float value, DifficultyInfo.DifficultyEnum difficulty, float previousTime)
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

    [Serializable]
    private struct Obs
    {
        [SerializeField]
        private ChoreographyObstacle[] _obstacles;
        public ChoreographyObstacle[] Obstacles => _obstacles;
    }

    [Serializable]
    private struct Evs
    {
        [SerializeField]
        private ChoreographyEvent[] _events;
        public ChoreographyEvent[] Events => _events;
    }
}

#endif