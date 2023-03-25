using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHitRecorder : MonoBehaviour
{
    public static DebugHitRecorder Instance { get; private set; }

    [SerializeField]
    private List<HitInfo> _hits= new List<HitInfo>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddToList(HitInfo info)
    {
        _hits.Add(info);
    }
}
