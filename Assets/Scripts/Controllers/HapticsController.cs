using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticsController : MonoBehaviour
{
    public static HapticsController Instance { get; private set; }

    [SerializeField]
    private AudioClip _missClip;

    public OVRHapticsClip MissHaptics {get; private set;}

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Start()
    {
        MissHaptics = new OVRHapticsClip(_missClip);
    }
}
