using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UpdateScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private float _delayLength = .5f;

    [SerializeField]
    private TextMeshProUGUI _scoreIncrease;
    
    [SerializeField]
    private TextMeshProUGUI _currentScore;

    private ulong _previousScore;
    private bool _update = false;
    private bool _delayingUpdate = false;
    private uint _increaseAmount;
    private CancellationToken _token;

    private const string ADD = "+";
    
    private void Start()
    {
        _token = this.GetCancellationTokenOnDestroy();
        MonitorScoreUpdate().Forget();
    }

    public void ScoreUpdated(uint increaseAmount)
    {
        _increaseAmount = increaseAmount;
        
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(ADD);
            sb.Append(increaseAmount);

            var buffer = sb.AsArraySegment();
            _scoreIncrease.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
        
        if (_delayingUpdate)
        {
            SetScoreDisplay(ScoringManager.Instance.CurrentScore-increaseAmount);
        }

        _update = true;
    }

    private void UpdateToNewestScore(uint increaseAmount)
    {
        if (ScoringManager.Instance.CurrentScore == _previousScore)
        {
            SetScoreDisplay(ScoringManager.Instance.CurrentScore);

            _scoreIncrease.SetText(string.Empty);
        }

        _delayingUpdate = false;
    }

    private async UniTaskVoid AsyncScoreUpdate(uint increaseAmount)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delayLength), cancellationToken: _token);
            if (_increaseAmount == increaseAmount)
            {
                SetScoreDisplay(ScoringManager.Instance.CurrentScore);

                _scoreIncrease.SetText(string.Empty);
            }

            _delayingUpdate = false;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
        }
    }
    
    public async UniTaskVoid MonitorScoreUpdate()
    {
        while (!_token.IsCancellationRequested)
        {
            if (!_update)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.05f), cancellationToken: _token);
                continue;
            }

            _update = false;
            _previousScore = ScoringManager.Instance.CurrentScore;
            await UniTask.Delay(TimeSpan.FromSeconds(_delayLength), cancellationToken: _token);
            UpdateToNewestScore(_increaseAmount);
        }
    }

    public void SetScoreDisplay(ulong score)
    {
        using (var sb = ZString.CreateStringBuilder(true))
        {
            sb.Append(score);

            var buffer = sb.AsArraySegment();
            _currentScore.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
        }
    }
}
