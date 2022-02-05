using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    [SerializeField]
    private TextMeshProUGUI _plusSymbol;
    

    private bool _delayingUpdate = false;
    private uint _increaseAmount;
    private CancellationToken _token;
    

    private void Start()
    {
        _token = this.GetCancellationTokenOnDestroy();
    }

    public async void ScoreUpdated(uint increaseAmount)
    {
        _increaseAmount = increaseAmount;
        _plusSymbol.gameObject.SetActive(true);
        _scoreIncrease.SetText(((int)increaseAmount).TryGetCachedIntString());
        if (_delayingUpdate)
        {
            _currentScore.SetText((ScoringManager.Instance.CurrentScore-increaseAmount).ToString());
        }

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delayLength), cancellationToken: _token);
            if (_increaseAmount == increaseAmount)
            {
                _currentScore.SetText((ScoringManager.Instance.CurrentScore).ToString());

                _plusSymbol.gameObject.SetActive(false);
                _scoreIncrease.SetText(string.Empty);
            }

            _delayingUpdate = false;
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            return;
        }
    }
}
