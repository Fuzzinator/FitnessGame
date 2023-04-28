using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetOffsetter : MonoBehaviour, ITargetInitializer
{
    [SerializeField]
    private float _positionChangeDistance;
    [SerializeField]
    private float _positionChangeRange;
    [SerializeField]
    private Vector3 _positionChange;
    [SerializeField]
    private float _disatanceRotation;

    private BaseTarget _target;
    private Vector3 _startPosition;

    private readonly Vector3 LeftOffset = new Vector3(-2, 1, 0);
    private readonly Vector3 RightOffset = new Vector3(2, 1, 0);
    private readonly Vector3 TopOffset = new Vector3(0, 1, 0);

    private const float LeftRotation = -45;
    private const float RightRotation = 45;

    public void Initialize(BaseTarget target)
    {
        _target = target;
        _startPosition = transform.localPosition;
        switch (_target.HitSideType)
        {
            case HitSideType.Left:
                _positionChange = LeftOffset;
                _disatanceRotation = LeftRotation;
                break;
            case HitSideType.Right:
                _positionChange = RightOffset;
                _disatanceRotation = RightRotation;
                break;
            case HitSideType.Block:
                _positionChange = TopOffset;
                _disatanceRotation = 0;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var t = transform;
        var distance = GetDistance();
        var targetPos = _startPosition + _positionChange;
        var targetRot = _disatanceRotation;

        targetPos = Vector3.Lerp(targetPos, _startPosition, distance);
        targetRot = Mathf.Lerp(targetRot, 0, distance);

        t.localPosition = targetPos;
        t.localEulerAngles = new Vector3(0, targetRot, 0);
    }

    private float GetDistance()
    {
        var t = transform.parent;
        if (t == null)
        {
            return 0f;
        }
        var thisPos = new Vector2(t.position.x, t.position.z);
        var targetPos = new Vector2(_target.OptimalHitPoint.x, _target.OptimalHitPoint.z);
        var distance = Vector2.Distance(thisPos, targetPos);

        var maxRange = _positionChangeDistance + _positionChangeRange;
        distance = Mathf.Clamp(distance, _positionChangeDistance, maxRange);
        distance = 1 + (distance - _positionChangeDistance) * (0 - 1) / (maxRange - _positionChangeDistance);
        distance = 1 - distance;
        distance *= distance;
        distance = 1 - distance;
        return distance;
    }
}
