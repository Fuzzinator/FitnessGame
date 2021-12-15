using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class ChoreographyReader : MonoBehaviour
{
    public static ChoreographyReader Instance { get; private set; }


    [SerializeField]
    private Choreography _choreography;

    private DifficultyInfo _difficultyInfo;

    private List<ChoreographyFormation> _formations;
    public ChoreographyNote[] Notes => _choreography.Notes;
    public ChoreographyEvent[] Events => _choreography.Events;
    public ChoreographyObstacle[] Obstacles => _choreography.Obstacles;

    [Header("Settings")]
    public UnityEvent finishedLoadingSong = new UnityEvent();

    private CancellationTokenSource _cancellationSource;

    #region Const Strings

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ANDROIDPATHSTART = "file://";
#elif UNITY_EDITOR
    private const string UNITYEDITORLOCATION = "/LocalCustomSongs/Songs/";
#endif

    private const string SONGSFOLDER = "/Resources/Songs/";
    private const string LOCALSONGSFOLDER = "Assets/Music/Songs/";
    private const string DAT = ".dat";
    private const string TXT = ".txt";
    private const string EASY = "Easy";

    #endregion

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
        _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadJson(PlaylistItem item)
    {
#pragma warning disable 4014
        AsyncLoadJson(item);

#pragma warning restore 4014
    }

    public void CancelLoad()
    {
        _cancellationSource?.Cancel();
    }
    private async UniTaskVoid AsyncLoadJson(PlaylistItem item)
    {
        _difficultyInfo = item.SongInfo.TryGetActiveDifficultySet(item.Difficulty);

        if (_cancellationSource.IsCancellationRequested)
        {
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }
        
        if (item.IsCustomSong)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var path =
 $"{Application.persistentDataPath}{SONGSFOLDER}{item.FileLocation}/{_difficultyInfo.FileName}";
#elif UNITY_EDITOR
            var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            var path = $"{dataPath}{UNITYEDITORLOCATION}{item.FileLocation}/{_difficultyInfo.FileName}";
#endif

            try
            {
                var streamReader = new StreamReader(path);

                var json = await streamReader.ReadToEndAsync().AsUniTask()
                    .AttachExternalCancellation(_cancellationSource.Token);
                _choreography = null;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _choreography = JsonUtility.FromJson<Choreography>(json);
                }

                if (_choreography == null)
                {
                    LevelManager.Instance.LoadFailed();
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s choreography failed to load.");
                    if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
                    {
                        _cancellationSource =
                            CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                    }

                    return;
                }
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
                if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
                {
                    _cancellationSource =
                        CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                }

                return;
            }
        }
        else
        {
            try
            {
                var txtVersion = _difficultyInfo.FileName;
                if (txtVersion.EndsWith(DAT))
                {
                    txtVersion = txtVersion.Replace(DAT, TXT);
                }

                var request =
                    Addressables.LoadAssetAsync<TextAsset>($"{LOCALSONGSFOLDER}{item.FileLocation}/{txtVersion}")
                        .WithCancellation(_cancellationSource.Token);

                var json = await request;
                if (json == null)
                {
                    NotificationManager.ReportFailedToLoadInGame($"{item.SongName}'s choreography failed to load.");
                    if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
                    {
                        _cancellationSource =
                            CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                    }

                    return;
                }

                _choreography = JsonUtility.FromJson<Choreography>((json).text);
            }
            catch (Exception e)when (e is OperationCanceledException)
            {
                if (_cancellationSource.IsCancellationRequested && this?.gameObject != null)
                {
                    _cancellationSource =
                        CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                }

                return;
            }
        }

        finishedLoadingSong?.Invoke();
    }

    public void ResetForNextSequence()
    {
        _formations = null;
    }

    public List<ChoreographyFormation> GetOrderedFormations()
    {
        if (_formations == null || _formations.Count == 0)
        {
            _formations = new List<ChoreographyFormation>();
            var sequenceables = GetChoreographSequenceables();
            sequenceables.Sort((sequenceable0, sequenceable1) => sequenceable0.Time.CompareTo(sequenceable1.Time));
            UpdateFormation(sequenceables);
            _choreography = new Choreography();
        }

        return _formations;
    }

    private List<ISequenceable> GetChoreographSequenceables()
    {
        var sequenceables = new List<ISequenceable>();

        for (int i = 0; i < Notes.Length; i++)
        {
            sequenceables.Add(Notes[i]);
        }

        for (int i = 0; i < Obstacles.Length; i++)
        {
            sequenceables.Add(Obstacles[i]);
        }

        /*for (int i = 0; i < Events.Length; i++)//Need to process Events differently. TODO: Figure this out
        {
            sequenceables.Add(Events[i]);
        }*/

        return sequenceables;
    }

    private void UpdateFormation(List<ISequenceable> target)
    {
        _formations.Clear();

        var lastTime = -1f;

        ISequenceable thisTimeNote = null;
        ISequenceable thisTimeObstacle = null;
        ISequenceable thisTimeEvent = null;

        ISequenceable lastSequenceable = null;
        for (var i = 0; i < target.Count; i++)
        {
            var sequenceable = target[i];
            if (lastTime < sequenceable.Time)
            {
                if (lastSequenceable == null)
                {
                    lastTime = sequenceable.Time;
                }
                else
                {
                    var minGap = (lastSequenceable is ChoreographyNote ? _difficultyInfo.MinTargetSpace : .5f);
                    if (lastTime + minGap < sequenceable.Time)
                    {
                        lastTime = sequenceable.Time;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (Mathf.Abs(lastTime - sequenceable.Time) < .01f) //if lastTime == sequenceable.Time but for floats
            {
                if (sequenceable is ChoreographyNote note && thisTimeNote == null)
                {
                    if (thisTimeObstacle != null)
                    {
                        note.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                        note.SetLineLayer(ChoreographyNote.LineLayerType.Low);

                        switch (thisTimeObstacle.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                note.SetLineIndex(2);
                                break;
                            case HitSideType.Right:
                                note.SetLineIndex(1);
                                break;
                            default:
                                continue;
                        }
                    }

                    if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                    {
                        if (note.CutDir == ChoreographyNote.CutDirection.Uppercut)
                        {
                            note.SetToBasicJab();
                        }
                        else if (note.CutDir == ChoreographyNote.CutDirection.UppercutLeft ||
                                 note.CutDir == ChoreographyNote.CutDirection.UppercutRight)
                        {
                            note.SetToBlock();
                        }
                    }
                    else if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                    {
                        if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                            note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                            note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                        {
                            note.SetToBlock();
                        }
                    }

                    if (_difficultyInfo.DifficultyRank < 5)
                    {
                        if (note.LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            if (_difficultyInfo.DifficultyRank == 1 && note.CutDir == ChoreographyNote.CutDirection.Jab)
                            {
                                note.SetToBlock();
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.JabDown ||
                                     note.CutDir == ChoreographyNote.CutDirection.HookLeftDown ||
                                     note.CutDir == ChoreographyNote.CutDirection.HookRightDown)
                            {
                                note.SetToBasicJab();
                            }
                        }
                    }

                    switch (note.HitSideType)
                    {
                        case HitSideType.Block:
                            note.SetLineIndex(1);
                            break;
                        case HitSideType.Left:
                            if (note.LineIndex > 1 ||
                                (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 1))
                            {
                                note.SetLineIndex(1);
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.HookLeft)
                            {
                                note.SetToBasicJab();
                            }

                            break;
                        case HitSideType.Right:
                            if (note.LineIndex < 2 ||
                                (note.CutDir == ChoreographyNote.CutDirection.Jab && note.LineIndex != 2))
                            {
                                note.SetLineIndex(2);
                            }
                            else if (note.CutDir == ChoreographyNote.CutDirection.HookRight)
                            {
                                note.SetToBasicJab();
                            }

                            break;
                    }

                    thisTimeNote = note;
                }
                else if (sequenceable is ChoreographyObstacle obstacle && thisTimeObstacle == null)
                {
                    if (thisTimeNote != null && thisTimeNote is ChoreographyNote tempNote)
                    {
                        switch (tempNote.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                if (obstacle.LineIndex < 2)
                                {
                                    tempNote.SetLineIndex(2);
                                }

                                break;
                            case HitSideType.Right:
                                if (obstacle.LineIndex > 1)
                                {
                                    tempNote.SetLineIndex(1);
                                }

                                break;
                            default:
                                continue;
                        }

                        tempNote.SetCutDirection(ChoreographyNote.CutDirection.Jab);
                        tempNote.SetLineLayer(ChoreographyNote.LineLayerType.Low);
                        thisTimeNote = tempNote;
                    }

                    thisTimeObstacle = obstacle;
                }

                //TODO: Come back and add ChoreographyEvent
                lastSequenceable = sequenceable;
            }

            if ((i + 1 < target.Count && target[1 + i].Time > lastTime) || i + 1 == target.Count)
            {
                _formations.Add(new ChoreographyFormation(lastTime, note: thisTimeNote, obstacle: thisTimeObstacle,
                    choreographyEvent: thisTimeEvent));

                thisTimeNote = null;
                thisTimeObstacle = null;
                thisTimeEvent = null;
            }
        }
    }
}