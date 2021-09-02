using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChoreographyFormation
{
    public float Time { get; private set; }

    public bool HasNote { get; private set; }
    public ChoreographyNote Note { get; private set;}
    
    public bool HasObstacle  { get; private set; }
    public ChoreographyObstacle Obstacle { get; private set;}
    
    public bool HasEvent  { get; private set; }
    
    public ChoreographyEvent Event { get; private set; }
    
    //public ISequenceable[] sequenceables {get; set; }

    public ChoreographyFormation(float time, ISequenceable note = null, ISequenceable obstacle = null, ISequenceable choreographyEvent = null)
    {
        this.Time = time;

        if (note != null && note is ChoreographyNote actualNote)
        {
            Note = actualNote;
            HasNote = true;
        }
        else
        {
            Note = new ChoreographyNote();
            HasNote = false;
        }

        if (obstacle != null && obstacle is ChoreographyObstacle actualObstacle)
        {
            Obstacle = actualObstacle;
            HasObstacle = true;
        }
        else
        {
            Obstacle = new ChoreographyObstacle();
            HasObstacle = false;
        }

        if (choreographyEvent != null && choreographyEvent is ChoreographyEvent actualEvent)
        {
            Event = actualEvent;
            HasEvent = true;
        }
        else
        {
            Event = new ChoreographyEvent();
            HasEvent = false;
        }
    }
}
