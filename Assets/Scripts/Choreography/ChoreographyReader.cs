using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoreographyReader : MonoBehaviour
{
    public static ChoreographyReader Instance { get; private set; }

    [TextArea]
    public string json;

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
        _choreography = JsonUtility.FromJson<Choreography>(json);
    }

    public List<ChoreographyFormation> GetOrderedFormations()
    {
        if (_formations == null || _formations.Count == 0)
        {
            _formations = new List<ChoreographyFormation>();
            var sequenceables = GetChoreographSequenceables();
            sequenceables.Sort((sequenceable0, sequenceable1) => sequenceable0.Time.CompareTo(sequenceable1.Time));
            UpdateFormation(sequenceables);
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

        for (int i = 0; i < Events.Length; i++)
        {
            sequenceables.Add(Events[i]);
        }

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

            if (Mathf.Abs(lastTime - sequenceable.Time) < .01f)
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