using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NineSliceController : MonoBehaviour
{
    [SerializeField]
    private float _globalSizeMod = 1f;
    [SerializeField]
    private Vector3 _offsetAdjust = new Vector3(0.12736f, 0f, 0.113157f);

    [SerializeField]
    private Vector3 _finiteSizeMod = new Vector3(1, 1, 1);
    [SerializeField]
    private Vector3 _originalSize = new Vector3(1, 1, 1);
    [SerializeField]
    private Vector3 _cornerSize = new Vector3(1, 1, 1);
    [SerializeField]
    private bool _shouldUpdate = false;


    [Header("Main Pieces")]
    [SerializeField]
    private Transform _bottomBack;
    [SerializeField]
    private Transform _bottomLeft;
    [SerializeField]
    private Transform _bottomRight;
    [SerializeField]
    private Transform _bottomFront;
    [SerializeField]
    private Transform _topBack;
    [SerializeField]
    private Transform _topLeft;
    [SerializeField]
    private Transform _topRight;
    [SerializeField]
    private Transform _topFront;

    [Header("Corner Pieces")]
    [SerializeField]
    private Transform _bottomBackLeft;
    [SerializeField]
    private Transform _bottomBackRight;
    [SerializeField]
    private Transform _bottomFrontLeft;
    [SerializeField]
    private Transform _bottomFrontRight;
    [SerializeField]
    private Transform _topBackLeft;
    [SerializeField]
    private Transform _topBackRight;
    [SerializeField]
    private Transform _topFrontLeft;
    [SerializeField]
    private Transform _topFrontRight;


    void Update()
    {
        if (!_shouldUpdate)
        {
            return;
        }

        var scaleFactor = _finiteSizeMod * _globalSizeMod;
        var adjustMod =  scaleFactor + Vector3.Scale((scaleFactor - Vector3.one), _offsetAdjust);

        // Scale main pieces
        _bottomBack.localScale = new Vector3(adjustMod.x, 1, 1);
        _bottomLeft.localScale = new Vector3(1, 1, adjustMod.z);
        _bottomRight.localScale = new Vector3(1, 1, adjustMod.z);
        _bottomFront.localScale = new Vector3(adjustMod.x, 1, 1);
        _topBack.localScale = new Vector3(adjustMod.x, 1, 1);
        _topLeft.localScale = new Vector3(1, 1, adjustMod.z);
        _topRight.localScale = new Vector3(1, 1, adjustMod.z);
        _topFront.localScale = new Vector3(adjustMod.x, 1, 1);

        // Corner pieces should not scale
        _bottomBackLeft.localScale = Vector3.one;
        _bottomBackRight.localScale = Vector3.one;
        _bottomFrontLeft.localScale = Vector3.one;
        _bottomFrontRight.localScale = Vector3.one;
        _topBackLeft.localScale = Vector3.one;
        _topBackRight.localScale = Vector3.one;
        _topFrontLeft.localScale = Vector3.one;
        _topFrontRight.localScale = Vector3.one;

        // Position parts
        PositionParts(_originalSize, scaleFactor);
    }

    private Vector3 CalculateCornerSize()
    {
        var scale = _topFront.position.x + (_originalSize.x * .5f) - _topFrontRight.position.x;
        return new Vector3(scale, scale, scale);
    }

    private void PositionParts(Vector3 originalSize, Vector3 scaleFactor)
    {
        float halfWidth = originalSize.x * 0.5f * scaleFactor.x;
        float halfHeight = originalSize.y * 0.5f * scaleFactor.y;
        float halfDepth = originalSize.z * 0.5f * scaleFactor.z;
        var botY = _bottomBackLeft.localPosition.y;
        // Position corners
        _bottomBackLeft.localPosition = new Vector3(-halfWidth, botY, -halfDepth);
        _bottomBackRight.localPosition = new Vector3(halfWidth, botY, -halfDepth);
        _bottomFrontLeft.localPosition = new Vector3(-halfWidth, botY, halfDepth);
        _bottomFrontRight.localPosition = new Vector3(halfWidth, botY, halfDepth);
        _topBackLeft.localPosition = new Vector3(-halfWidth, halfHeight, -halfDepth);
        _topBackRight.localPosition = new Vector3(halfWidth, halfHeight, -halfDepth);
        _topFrontLeft.localPosition = new Vector3(-halfWidth, halfHeight, halfDepth);
        _topFrontRight.localPosition = new Vector3(halfWidth, halfHeight, halfDepth);

        // Position edges
        _bottomBack.localPosition = new Vector3(0, botY, -halfDepth);
        _bottomLeft.localPosition = new Vector3(-halfWidth, botY, 0);
        _bottomRight.localPosition = new Vector3(halfWidth, botY, 0);
        _bottomFront.localPosition = new Vector3(0, botY, halfDepth);
        _topBack.localPosition = new Vector3(0, halfHeight, -halfDepth);
        _topLeft.localPosition = new Vector3(-halfWidth, halfHeight, 0);
        _topRight.localPosition = new Vector3(halfWidth, halfHeight, 0);
        _topFront.localPosition = new Vector3(0, halfHeight, halfDepth);
    }
    public Vector3 Divide(Vector3 value, Vector3 scale)
    {
        return new Vector3(value.x / scale.x, value.y / scale.y, value.z / scale.z);
    }
}
