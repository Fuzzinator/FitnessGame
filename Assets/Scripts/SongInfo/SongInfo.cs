using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SongInfo
{
   public float BeatsPerMinute => _beatsPerMinute;

   public string SongName => _songName;
   public string SongFilename => _songFilename;

   public string SongAuthorName => _songAuthorName;

   public string LevelAuthorName => _levelAuthorName;
   
   [SerializeField]
   private string _songName;
   [SerializeField]
   private string _version;
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
   private string _songFilename;

   [SerializeField]
   private DifficultySet[] _difficultyBeatmapSets;

   public DifficultyInfo TryGetActiveDifficultySet(string difficulty)
   {
      for (var i = 0; i < _difficultyBeatmapSets.Length; i++)
      {
         var difficulties = _difficultyBeatmapSets[i];
         for (var j = 0; j < difficulties.DifficultyInfos.Length; j++)
         {
            var info = difficulties.DifficultyInfos[j];
            if (info.Difficulty == difficulty)
            {
               return info;
            }
         }
      }
      return new DifficultyInfo();
   }
   
   
   
   [Serializable]
   public struct DifficultySet
   {
      [SerializeField]
      private DifficultyInfo[] _difficultyBeatmaps;

      public DifficultyInfo[] DifficultyInfos => _difficultyBeatmaps;
   }
}
