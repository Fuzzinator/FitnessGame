using Unity.Burst;
using UnityEngine;

[System.Serializable]
[BurstCompile]
public struct ChoreographyFormation
{
   [SerializeField]
   private float _time;

   [SerializeField]
   private HitSideType _hitSideType;

   [SerializeField]
   private bool _isValid;
   
   [SerializeField]
   private ChoreographyNote _note;

   [SerializeField]
   private ChoreographyObstacle _obstacle;

   [SerializeField]
   private ChoreographyEvent _event;

   [SerializeField]
   private bool _hasNote;

   [SerializeField]
   private bool _hasObstacle;

   [SerializeField]
   private bool _hasEvent;

   public float Time => _time;
   public HitSideType HitSideType => _hitSideType;
   public bool IsValid => _isValid;
   public ChoreographyNote Note => _note;
   public ChoreographyObstacle Obstacle => _obstacle;
   public ChoreographyEvent Event => _event;
   public bool HasNote => _hasNote;
   public bool HasObstacle => _hasObstacle;
   public bool HasEvent => _hasEvent;

   public ChoreographyFormation SetNote(ChoreographyNote note)
   {
      _note = note;
      _hasNote = true;
      _time = note.Time;
      _isValid = true;
      return this;
   }

   public ChoreographyFormation SetObstacle(ChoreographyObstacle obstacle)
   {
      _obstacle = obstacle;
      _hasObstacle = true;
      _isValid = true;
      _time = obstacle.Time;
      return this;
   }
   
   public ChoreographyFormation SetEvent(ChoreographyEvent e)
   {
      _event = e;
      _hasEvent = true;
      _isValid = true;
      _time = e.Time;
      return this;
   }

   public ChoreographyFormation(ChoreographyNote note)
   {
      _time = note.Time;
      _hitSideType = note.HitSideType;
      _note = note;
      _hasNote = true;
      _obstacle = new ChoreographyObstacle();
      _hasObstacle = false;
      _event = new ChoreographyEvent();
      _hasEvent = false;
      _isValid = true;
   }
   
   public ChoreographyFormation(ChoreographyObstacle obstacle)
   {
      _time = obstacle.Time;
      _hitSideType = obstacle.HitSideType;
      _note = new ChoreographyNote();
      _hasNote = false;
      _obstacle = obstacle;
      _hasObstacle = true;
      _event = new ChoreographyEvent();
      _hasEvent = false;
      _isValid = true;
   }
   
   public ChoreographyFormation(ChoreographyEvent e)
   {
      _time = e.Time;
      _hitSideType = e.HitSideType;
      _note = new ChoreographyNote();
      _hasNote = false;
      _obstacle = new ChoreographyObstacle();
      _hasObstacle = false;
      _event = e;
      _hasEvent = true;
      _isValid = true;
   }
}
