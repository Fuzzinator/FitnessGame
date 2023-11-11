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
        [FormerlySerializedAs("_songRecordScore")] [SerializeField] 
        private TextMeshProUGUI _songRecordScores;
        [FormerlySerializedAs("_songRecordStreak")] [SerializeField] 
        TextMeshProUGUI _songRecordStreaks;

        [SerializeField]
        private SetAndShowSongOptions _showSongOptions;
        
        private SongInfo _songInfo;
        
        private CancellationToken _cancellationToken;

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

            using var namesSb = ZString.CreateStringBuilder(false);
            using var scoresSb = ZString.CreateStringBuilder(false);
            using var streaksSb = ZString.CreateStringBuilder(false);

            foreach (var score in songRecord)
            {
                if (!score.IsValid)
                {
                    continue;
                }

                scoresSb.Append(score.Score);
                streaksSb.Append(score.Streak);
                namesSb.Append(score.ProfileName);

                scoresSb.Append(NEWLINE);
                streaksSb.Append(NEWLINE);
                namesSb.Append(NEWLINE);
            }

            _songRecordScores.SetText(scoresSb);
            _songRecordScores.SetText(streaksSb);
            _songScoreNames.SetText(namesSb);
        }

        private async UniTask<SongRecord[]> GetSongRecord()
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            var records = await PlayerStatsFileManager.TryGetRecords(_songInfo, _showSongOptions.DifficultyAsEnum,
                _showSongOptions.SelectedGameMode, _cancellationToken);

            return records;
        }
    }
}