using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Choreography
{
    public string Version => _version;
    public ChoreographyEvent[] Events => _events;
    public ChoreographyNote[] Notes => _notes;
    public ChoreographyObstacle[] Obstacles => _obstacles;
    public ChoreographyCustomData CustomData => _customData;
    
    
    [SerializeField]
    private string _version;

    [SerializeField]
    private ChoreographyEvent[] _events;

    [SerializeField]
    private ChoreographyNote[] _notes;

    [SerializeField]
    private ChoreographyObstacle[] _obstacles;

    [SerializeField]
    private ChoreographyCustomData _customData;
}
