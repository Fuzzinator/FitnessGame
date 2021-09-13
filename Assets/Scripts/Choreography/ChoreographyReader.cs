using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ChoreographyReader : MonoBehaviour
{
    public static ChoreographyReader Instance { get; private set; }

    /*
    [TextArea]
    public string json;*/

    [SerializeField]
    private Choreography _choreography;

    private List<ChoreographyFormation> _formations;
    public ChoreographyNote[] Notes => _choreography.Notes;
    public ChoreographyEvent[] Events => _choreography.Events;
    public ChoreographyObstacle[] Obstacles => _choreography.Obstacles;

    [Header("Settings")]
    [SerializeField]
    private float _minTargetSpace = .25f; //This should go into a difficulty setting

    [SerializeField]
    private float _minObstacleSpace = .75f; //This should go into a difficulty setting

    public UnityEvent finishedLoadingSong = new UnityEvent();
    
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

    public async void LoadJson(PlaylistItem item)
    {
        using (var streamReader = new StreamReader($"{Application.dataPath}\\Resources\\{item.FileLocation}\\{item.Difficulty}.dat"))
        {
            var reading = streamReader.ReadToEndAsync();
            await reading;
            var json = reading.Result;
            _choreography = JsonUtility.FromJson<Choreography>(json);
            finishedLoadingSong?.Invoke();
        }
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
                    var minGap = (lastSequenceable is ChoreographyNote ? _minTargetSpace : _minObstacleSpace);
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
                        switch (thisTimeObstacle.HitSideType)
                        {
                            case HitSideType.Block:
                                if (note.LineLayer != ChoreographyNote.LineLayerType.Low)
                                {
                                    continue;
                                }

                                break;
                            case HitSideType.Left:
                                if (note.LineIndex < 2)
                                {
                                    continue;
                                }

                                break;
                            case HitSideType.Right:
                                if (note.LineIndex > 1)
                                {
                                    continue;
                                }

                                break;
                            default:
                                continue;
                        }
                    }

                    if (note.CutDir == ChoreographyNote.CutDirection.Uppercut ||
                        note.CutDir == ChoreographyNote.CutDirection.UppercutLeft ||
                        note.CutDir == ChoreographyNote.CutDirection.UppercutRight)
                    {
                        if (note.LineLayer == ChoreographyNote.LineLayerType.Low)
                        {
                            note.SetToBasicJab();
                        }

                        /*var randomValue = Random.Range(-1, 1);//We'll Test this later
                        if (randomValue < 0)
                        {
                            note.SetToBasicJab();
                        }*/
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
                    if (thisTimeNote != null)
                    {
                        if (((ChoreographyNote) thisTimeNote).LineLayer == ChoreographyNote.LineLayerType.High)
                        {
                            continue;
                        }

                        if (obstacle.Type == ChoreographyObstacle.ObstacleType.Crouch &&
                            ((ChoreographyNote) thisTimeNote).LineLayer != ChoreographyNote.LineLayerType.Low)
                        {
                            continue;
                        }

                        switch (thisTimeNote.HitSideType)
                        {
                            case HitSideType.Block:
                                break;
                            case HitSideType.Left:
                                if (obstacle.LineIndex < 2)
                                {
                                    continue;
                                }

                                break;
                            case HitSideType.Right:
                                if (obstacle.LineIndex > 1)
                                {
                                    continue;
                                }

                                break;
                            default:
                                continue;
                        }
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