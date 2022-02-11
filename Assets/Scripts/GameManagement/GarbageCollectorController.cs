using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class GarbageCollectorController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetGarbageCollectorState(bool enabled)
    {
        GarbageCollector.GCMode = enabled ? 
            GarbageCollector.Mode.Enabled :
            GarbageCollector.Mode.Disabled;
    }

    public void RunGarbageCollector()
    {
    }
}