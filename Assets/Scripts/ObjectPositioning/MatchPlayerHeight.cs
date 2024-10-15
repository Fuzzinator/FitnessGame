using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchPlayerHeight : MonoBehaviour, IOrderedInitialize
{
    public bool Initialized { get; private set; }

    [SerializeField]
    private float _offset;


    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        UpdatePosition();
        Initialized = true;
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
