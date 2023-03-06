using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HitQualityDisplayManager : MonoBehaviour
{
    public static HitQualityDisplayManager Instance { get; private set; }
    [SerializeField]
    private Canvas _qualityDisplayCanvas;

    [SerializeField]
    private HitQualityDisplay _qualityDisplay;

    [SerializeField]
    private float _displayTime = 1f;

    [SerializeField]
    private AnimationCurve _displayScaleCurve = new AnimationCurve();
    [SerializeField]
    private AnimationCurve _displayAlphaCurve = new AnimationCurve();

    private PoolManager _qualityDisplayPool;

    private List<ActiveHitDisplay> _activeHitDisplays = new List<ActiveHitDisplay>();

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

    // Start is called before the first frame update
    void Start()
    {
        _qualityDisplayPool = new PoolManager(_qualityDisplay, _qualityDisplayCanvas.transform, 20);
        TrackActiveHitQualityDisplays(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid TrackActiveHitQualityDisplays(CancellationToken cancellationToken)
    {

        while (!cancellationToken.IsCancellationRequested)
        {
            while (_activeHitDisplays.Count > 0)
            {
                var timeRemaining = (_activeHitDisplays[0].ActiveTime + _displayTime - Time.time).Normalized(0, 1);
                while (timeRemaining <= 0f && _activeHitDisplays.Count > 0)
                {
                    var display = _activeHitDisplays[0].QualityDisplay;
                    _activeHitDisplays.RemoveAt(0);
                    display.ReturnToPool();
                    if (_activeHitDisplays.Count > 0)
                    {
                        timeRemaining = (_activeHitDisplays[0].ActiveTime + _displayTime - Time.time).Normalized(0, 1);
                    }
                }
                foreach (var hit in _activeHitDisplays)
                {
                    timeRemaining = 1 - (hit.ActiveTime + _displayTime - Time.time).Normalized(0, 1);
                    var scale = _displayScaleCurve.Evaluate(timeRemaining);
                    var alpha = _displayAlphaCurve.Evaluate(timeRemaining);
                    hit.QualityDisplay.UpdateScaleAndAlpha(scale, alpha);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(.03f));
            }
            await UniTask.Delay(TimeSpan.FromSeconds(.1f));
        }
    }

    public static HitQualityDisplay GetHitQualityDisplay()
    {
        var qualityDisplay = Instance._qualityDisplayPool.GetNewPoolable() as HitQualityDisplay;
        qualityDisplay.MyPoolManager = Instance._qualityDisplayPool;
        Instance._activeHitDisplays.Add(new ActiveHitDisplay(Time.time, qualityDisplay));
        return qualityDisplay;
    }

    private struct ActiveHitDisplay
    {
        public float ActiveTime { get; private set; }
        public HitQualityDisplay QualityDisplay { get; private set; }

        public ActiveHitDisplay(float activeTime, HitQualityDisplay display)
        {
            ActiveTime = activeTime;
            QualityDisplay = display;
        }
    }
}
