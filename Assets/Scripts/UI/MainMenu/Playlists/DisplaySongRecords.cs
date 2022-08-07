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
        [FormerlySerializedAs("_playlistRecordScore")] [SerializeField] 
        private TextMeshProUGUI _songRecordScore;
        [FormerlySerializedAs("_playlistRecordStreak")] [SerializeField] 
        TextMeshProUGUI _songRecordStreak;

        [SerializeField]
        private SetAndShowSongOptions _showSongOptions;
        
        private SongInfo _songInfo;
        
        private CancellationToken _cancellationToken;

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
            
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.Append(songRecord.Score);

                var buffer = sb.AsArraySegment();
                _songRecordScore.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
            }
            using (var sb = ZString.CreateStringBuilder(true))
            {
                sb.Append(songRecord.Streak);

                var buffer = sb.AsArraySegment();
                _songRecordStreak.SetCharArray(buffer.Array, buffer.Offset, buffer.Count);
            }
        }

        private async UniTask<SongAndPlaylistRecord> GetSongRecord()
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken);
            var songFullName = SongInfoReader.GetFullSongName(_songInfo, _showSongOptions.SelectedDifficulty, _showSongOptions.SelectedGameMode);
            var exists = await PlayerStatsFileManager.SongKeyExists(songFullName);
            if (!exists)
            {
                return new SongAndPlaylistRecord(0, 0);
            }
            return (SongAndPlaylistRecord) await PlayerStatsFileManager.GetSongValue<SongAndPlaylistRecord>(
                songFullName, _cancellationToken);
        }
    }
}