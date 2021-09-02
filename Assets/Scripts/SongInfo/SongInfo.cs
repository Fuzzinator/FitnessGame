using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SongInfo
{
   public float BeatsPerMinute => _beatsPerMinute;
   public float NoteJumpMovementSpeed => _noteJumpMovementSpeed;
   
   [SerializeField]
   private string _version;
   [SerializeField]
   private string _songName;
   [SerializeField]
   private string _songSubName;
   [SerializeField]
   private string _songAuthorName;
   [SerializeField]
   private string _levelAuthorName;

   [SerializeField]
   private float _beatsPerMinute;
   [SerializeField]
   private float _songTimeOffset;
   [SerializeField]
   private float _noteJumpMovementSpeed;
}
