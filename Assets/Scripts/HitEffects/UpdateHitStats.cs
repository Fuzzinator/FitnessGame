using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHitStats : MonoBehaviour
{
    [SerializeField]
    private bool _isTarget;
    
    public void RegisterHit()
    {
        ScoringManager.Instance.RegisterHit();
    }

    public void RegisterMiss()
    {
        ScoringManager.Instance.RegisterMiss(_isTarget);
    }
}
