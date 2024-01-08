using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchPlayerHeight : MonoBehaviour
{
    [SerializeField]
    private float _offset;

    // Start is called before the first frame update
    private void Start()
    {
        UpdatePosition();
    }

    private void OnEnable()
    {
        GlobalSettings.UserHeightChanged.AddListener(UpdatePosition);
    }

    private void OnDisable()
    {
        GlobalSettings.UserHeightChanged.RemoveListener(UpdatePosition);
    }

    private void UpdatePosition()
    {
        var height = GlobalSettings.TotalHeight;
        var t = transform;
        var currentPos = t.position;
        t.position = new Vector3(currentPos.x, height + _offset, currentPos.z);
    }
}
