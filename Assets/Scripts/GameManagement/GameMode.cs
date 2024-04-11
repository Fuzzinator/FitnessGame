using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModeManagement
{
    [Serializable]
    public enum GameMode
    {
        Unset = 0,
        Normal = 1,
        JabsOnly = 2,
        OneHanded = 3,
        Degrees90 = 4,
        Degrees360 = 5,
        LegDay = 6,
        NoObstacles = 7,
        LightShow = 8,
        Lawless = 9
    }

    public static class GameModeExtensions
    {
        #region DifficultySetName String Const

        private const string UNSET = "Unset";
        private const string NORMAL = "Standard";
        private const string JABSONLY = "NoArrows";
        private const string ONEHANDED = "OneSaber";
        private const string DEGREE90 = "90Degree";
        private const string DEGREE360 = "360Degree";
        private const string LIGHTSHOW = "Lightshow";
        private const string LEGDAY = "LegDay";
        private const string NOOBSTACLES = "NoObstacles";
        private const string LAWLESS = "Lawless";
        private const string LEGACY = "Legacy";
        
        private const string DISPLAYUNSET = "Default";
        private const string DISPLAYNORMAL = "Standard";
        private const string DISPLAYJABSONLY = "Jabs Only";
        private const string DISPLAYONEHANDED = "One Handed";
        private const string DISPLAYDEGREE90 = "90 Degree";
        private const string DISPLAYDEGREE360 = "360 Degree";
        private const string DISPLAYLIGHTSHOW = "Lightshow";
        private const string DISPLAYLEGDAY = "Leg Day";
        private const string DISPLAYNOOBSTACLES = "No Obstacles";
        private const string DISPLAYLAWLESS = "Lawless";

        #endregion

        private static readonly string[] _difficultySetNames = new[]
        {
            UNSET,
            NORMAL,
            JABSONLY,
            ONEHANDED,
            DEGREE90,
            DEGREE360,
            LEGDAY,
            NOOBSTACLES,
            LIGHTSHOW,
            LAWLESS
        };

        private static readonly string[] _difficultySetDisplayNames = new[]
        {
            DISPLAYUNSET,
            DISPLAYNORMAL,
            DISPLAYJABSONLY,
            DISPLAYONEHANDED,
            DISPLAYDEGREE90,
            DISPLAYDEGREE360,
            DISPLAYLEGDAY,
            DISPLAYNOOBSTACLES,
            DISPLAYLIGHTSHOW,
            DISPLAYLAWLESS
        };
            
        public static string GetDifficultySetName(this GameMode gameMode)
        {
            return _difficultySetNames[(int)gameMode];
        }
        
        public static string Readable(this GameMode gameMode)
        {
            return _difficultySetDisplayNames[(int) gameMode];
        }
        
        public static GameMode GetGameMode(this string gameModeName)
        {
            GameMode gameMode;
            switch (true)
            {
                case true when UNSET.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.Unset;
                    break;
                case true when NORMAL.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                case true when LEGACY.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.Normal;
                    break;
                case true when JABSONLY.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.JabsOnly;
                    break;
                case true when ONEHANDED.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.OneHanded;
                    break;
                case true when DEGREE90.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.Degrees90;
                    break;
                case true when DEGREE360.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.Degrees360;
                    break;
                case true when LIGHTSHOW.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.LightShow;
                    break;
                case true when LEGDAY.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.LegDay;
                    break;
                case true when NOOBSTACLES.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.NoObstacles;
                    break;
                case true when LAWLESS.Equals(gameModeName, StringComparison.InvariantCultureIgnoreCase):
                    gameMode = GameMode.Lawless;
                    break;
                default:
                    gameMode = GameMode.Normal;
                    Debug.LogError($"{gameModeName} is an invalid game mode. Returning Normal.");
                    break;
            }

            return gameMode;
        }

        public static string[] DifficultyDisplayNames => _difficultySetDisplayNames;
    }
}
