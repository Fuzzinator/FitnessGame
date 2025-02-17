using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptimalStrikePositioning : MonoBehaviour
{
    [SerializeField]
    private Transform _optimalJabStrikePoint;
    [SerializeField]
    private Transform _optimalDirectionalStrikePoint;

    private bool _setStrikePoints = false;

    private UnityEvent<float> _onPositionChanged = new UnityEvent<float>();

    public UnityEvent<float> OnPositionChanged => _onPositionChanged;

    public Vector3 JabStrikePoint
    {
        get
        {
            if (!_setStrikePoints)
            {
                GetAndSetStrikePoints();
            }
            return _optimalJabStrikePoint.position;
        }
    }

    public Vector3 DirectionalStrikePoint
    {
        get
        {
            if (!_setStrikePoints)
            {
                GetAndSetStrikePoints();
            }
            return _optimalDirectionalStrikePoint.position;
        }
    }

    private void GetAndSetStrikePoints()
    {
        var averageArmLength = SettingsManager.GetCachedFloat(SettingsManager.AverageArmLength, .75f);


        SettingsManager.TrySubscribeToCachedfloat(SettingsManager.AverageArmLength, SetStrikePoints);
    }

    private void SetStrikePoints(float averageArmLength)
    {
        if(_optimalDirectionalStrikePoint == null || _optimalJabStrikePoint == null || _optimalDirectionalStrikePoint == null)
        {
            return;
        }

        var strikePointPosition = _optimalDirectionalStrikePoint.position;
        _optimalJabStrikePoint.position = new Vector3(strikePointPosition.x, strikePointPosition.y, averageArmLength);
        _optimalDirectionalStrikePoint.position = new Vector3(strikePointPosition.x, strikePointPosition.y, averageArmLength * .85f);

        _setStrikePoints = true;
    }
}
