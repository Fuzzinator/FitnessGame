using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameModeManagement;
using InfoSaving;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class DisplaySongRecords : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI _songScoreNames;
        [SerializeField] 
        TextMeshProUGUI _songStreakNames;
        [FormerlySerializedAs("_songRecordScore")] [SerializeField] 
        private TextMeshProUGUI _songRecordScores;
        [FormerlySerializedAs("_songRecordStreak")] [SerializeField] 
        TextMeshProUGUI _songRecordStreaks;

        [SerializeField]
        private SetAndShowSongOptions _showSongOptions;
        
        private SongInfo _songInfo;
        
        private CancellationToken _cancellationToken;

        private const string STREAK = "Streak:";
        private const string SCORE = "Score:";
        private const string NEWLINE = "\n";
        
        public void ShowRecords(SongInfo songInfo)
        {
            _songInfo = songInfo;
            ShowInfo().Forget();
        }

        public void RefreshDisplay()
        {
            ShowInfo().Forget();
        }
        

        public async UniTaskVoid ShowInfo()
        {
            var songRecord = await GetSongRecord();
            
            using (var scoresSb = ZString.CreateStringBuilder(true))
            {
                using (var namesSb = ZString.CreateStringBuilder(false))
                {
                    foreach (var score in songRecord.scores)
                    {
                        if (!score.IsValid)
                        {
                            continue;
                        }

                        scoresSb.Append(score.Score);
                        namesSb.Append(score.ProfileName);
                        scoresSb.Append(NEWLINE);
                        namesSb.Append(NEWLINE);
                    }

                    var scoresBuffer = scoresSb.AsArraySegment();
                    _songRecordScores.SetCharArray(scoresBuffer.Array, scoresBuffer.Offset, scoresBuffer.Count);
                    var namesBuffer = namesSb.AsArraySegment();
                    _songScoreNames.SetCharArray(namesBuffer.Array, namesBuffer.Offset, namesBuffer.Count);
                }
            }
            using (var streaksSb = ZString.CreateStringBuilder(true))
            {
                using (var namesSb = ZString.CreateStringBuilder(false))
                {
                    foreach (var streaks in songRecord.streaks)
                    {
                        if (!streaks.IsValid)
                        {
                            continue;
                        }

                        streaksSb.Append(streaks.Streak);
                        namesSb.Append(streaks.ProfileName);
                        streaksSb.Append(NEWLINE);
                        namesSb.Append(NEWLINE);
                    }

                    var streaksBuffer = streaksSb.AsArraySegment();
                    _songRecordStreaks.SetCharArray(streaksBuffer.Array, streaksBuffer.Offset, streaksBuffer.Count);
                    var namesBuffer = namesSb.AsArraySegment();
                    _songStreakNames.SetCharArray(namesBuffer.Array, namesBuffer.Offset, namesBuffer.Count);
                }
            }
        }

        private async UniTask<SongAndPlaylistRecords> GetSongRecord()
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            var records = await PlayerStatsFileManager.TryGetRecords(_songInfo, _showSongOptions.DifficultyAsEnum,
                _showSongOptions.SelectedGameMode, _cancellationToken);

            return records;
        }
    }
}