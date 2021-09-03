using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChoreographySequencer : MonoBehaviour
{
    [SerializeField]
    private FormationHolder _formationHolderPrefab;

    private PoolManager _formationHolderPool;

    [SerializeField]
    private BaseTarget _jabTarget;

    private PoolManager _jabPool;

    [SerializeField]
    private BaseTarget _leftHookTarget;

    private PoolManager _leftHookPool;

    [SerializeField]
    private BaseTarget _rightHookTarget;

    private PoolManager _rightHookPool;

    [SerializeField]
    private BaseTarget _uppercutTarget;

    private PoolManager _uppercutPool;

    [SerializeField]
    private Transform _formationStart;

    [SerializeField]
    private Transform _formationEnd;


    private Sequence _sequence;

    [SerializeField]
    private Transform[] _sequenceStartPoses;

    [SerializeField]
    private Transform[] _sequenceEndPoses;

    private float _meterDistance;

    private List<Tween> _activeTweens = new List<Tween>();

    public bool test = false;

    // Start is called before the first frame update
    void Start()
    {
        var thisTransform = transform;
        _formationHolderPool = new PoolManager(_formationHolderPrefab, thisTransform);
        _jabPool = new PoolManager(_jabTarget, thisTransform);
        _leftHookPool = new PoolManager(_leftHookTarget, thisTransform);
        _rightHookPool = new PoolManager(_rightHookTarget, thisTransform);
        _uppercutPool = new PoolManager(_uppercutTarget, thisTransform);

        _meterDistance = Vector3.Distance(_formationStart.position, _formationEnd.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            test = false;
            InitializeSequence();
        }
    }

    public void TempStart()
    {
        TryGetComponent(out AudioSource source);
        InitializeSequence();
        source.Play();
    }

    public void InitializeSequence()
    {
        if (ChoreographyReader.Instance == null)
        {
            Debug.LogError("No ChoreographyReader");
            return;
        }

        var formations = ChoreographyReader.Instance.GetOrderedFormations();
        if (formations == null || formations.Count <= 0)
        {
            return;
        }

        DOTween.Init(true, false);
        _sequence = DOTween.Sequence();

        var formationSequence = CreateSequence(formations[0], 1);
    }

    private Sequence CreateSequence(ChoreographyFormation formation, int nextFormationIndex)
    {
        var formationHolder = _formationHolderPool.GetNewPoolable() as FormationHolder;
        formationHolder.gameObject.SetActive(true);
        var formationTransform = formationHolder.transform;
        formationTransform.SetParent(transform);
        formationTransform.position = _formationStart.position;

        var tween = formationHolder.transform.DOLocalPath(new[] {formationTransform.position, _formationEnd.position},
            _meterDistance / SongInfoReader.Instance.NoteSpeed);
        tween.SetEase(Ease.Linear);

        TweenCallback onStart = () => SpawnFormationObjects(formationHolder, formation);
        onStart += () => TryCreateNextSequence(nextFormationIndex);
        onStart += () => _activeTweens.Add(tween);

        TweenCallback onComplete = () => ClearFormationObjects(formationHolder);
        onComplete += () => _activeTweens.Remove(tween);
        tween.OnStart(onStart);

        tween.OnComplete(onComplete
        );

        var sequence = DOTween.Sequence();
        var beatsTime = SongInfoReader.Instance.BeatsPerMinute / 60;
        var delay = Mathf.Max(0,
            (formation.Time / beatsTime) - Time.time - _meterDistance / SongInfoReader.Instance.NoteSpeed);

        sequence.Insert(delay, tween);

        return sequence;
    }

    private void TryCreateNextSequence(int nextFormationIndex)
    {
        var formations = ChoreographyReader.Instance.GetOrderedFormations();
        if (nextFormationIndex < formations.Count)
        {
            var tween = CreateSequence(formations[nextFormationIndex], ++nextFormationIndex);

            //_sequence.Insert(formations[nextFormationIndex].Time, tween);
        }
    }

    private void SpawnFormationObjects(FormationHolder formationHolder, ChoreographyFormation formation)
    {
        //foreach (var sequenceable in formation.sequenceables)
        //{
        if (formation.HasObstacle)
        {
        }

        if (formation.HasNote)
        {
            var target = GetTarget(formation.Note.CutDir);
            target.transform.SetParent(formationHolder.transform);
            target.transform.position = (GetTargetParent(formation.Note)).position;
            target.gameObject.SetActive(true);
            ActiveTargetManager.Instance.AddActiveTarget(target);
            if (formationHolder.children == null)
            {
                formationHolder.children = new List<IPoolable>();
            }

            formationHolder.children.Add(target);
        }

        //}
    }

    private BaseTarget GetTarget(ChoreographyNote.CutDirection cutDirection) => cutDirection switch
    {
        ChoreographyNote.CutDirection.Jab => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.JabDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookLeft => _leftHookPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookLeftDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookRight => _rightHookPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.HookRightDown => _jabPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.Uppercut => _uppercutPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.UppercutLeft => _uppercutPool.GetNewPoolable(),
        ChoreographyNote.CutDirection.UppercutRight => _uppercutPool.GetNewPoolable(),
        _ => null,
    } as BaseTarget;

    private Transform GetTargetParent(ChoreographyNote note)
    {
        switch (note.HitSideType)
        {
            case HitSideType.Block:
            case HitSideType.Left:
                return _sequenceStartPoses[(1 + (int) note.LineLayer * 4)];
            case HitSideType.Right:
                return _sequenceStartPoses[(2 + (int) note.LineLayer * 4)];
            default:
                return null;
        }

        //return _sequenceStartPoses[(note.LineIndex + (int) note.LineLayer * 4)];//If supporting 4 horizontal positions instead of 2
    }

    private void ClearFormationObjects(FormationHolder formationHolder)
    {
        formationHolder.ReturnRemainingChildren();
    }
}