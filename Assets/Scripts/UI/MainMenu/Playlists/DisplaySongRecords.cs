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
        [FormerlySerializedAs("_songRecordScore")]
        [SerializeField]
        private TextMeshProUGUI _songRecordScores;
        [FormerlySerializedAs("_songRecordStreak")]
        [SerializeField]
        TextMeshProUGUI _songRecordStreaks;

        [SerializeField]
        private Image _statusDisplayBackground;
        [SerializeField]
        private TextMeshProUGUI _statusDisplay;

        [SerializeField]
        private SetAndShowSongOptions _showSongOptions;

        private SongInfo _songInfo;

        private bool _showOnline = false;

        private CancellationTokenSource _cancellationToken;
        private bool _tokenDisposed = false;

        private const string NEWLINE = "\n";
        private const string AllowOnlineLeaderboards = "AllowOnlineLeaderboards";

        private void OnDisable()
        {
            if (_cancellationToken != null && !_tokenDisposed)
            {
                _cancellationToken.Cancel();
                _cancellationToken.Dispose();
                _tokenDisposed = true;
            }
        }

        public void SetSongInfo(SongInfo songInfo)
        {
            _songInfo = songInfo;
        }

        public void RefreshDisplay()
        {
            _statusDisplay.ClearText();
            _statusDisplayBackground.gameObject.SetActive(false);
            if (_songInfo == null)
            {
                _statusDisplay.SetText("Select a song to view it's leaderboards.");
                _statusDisplayBackground.gameObject.SetActive(true);
                return;
            }
            if (_cancellationToken != null && !_tokenDisposed)
                _cancellationToken?.Cancel();
            if (_showOnline)
            {
                ShowOnlineInfo().Forget();
            }
            else
            {
                ShowLocalInfo().Forget();
            }
        }

        public void ShowLocalRecords(bool enabled)
        {
            if (!enabled)
            {
                return;
            }
            _showOnline = false;
            RefreshDisplay();
        }

        public void ShowOnlineRecords(bool enabled)
        {
            if (!enabled)
            {
                return;
            }
            if (!SettingsManager.GetCachedBool(AllowOnlineLeaderboards, false))
            {
                var notificationVisuals = new Notification.NotificationVisuals()
                {
                    header = "Enable Online Leaderboards?",
                    message = "Enabling online leaderboards will share with us your Shadow BoXR profile information and song scores and streaks. This is only used for providing leaderboards services. Would you like to enable online leaderboards?",
                    button1Txt = "Enable",
                    button2Txt = "Cancel",
                };
                NotificationManager.RequestNotification(notificationVisuals, EnableOnlineLeaderboards);
                return;
            }
            _showOnline = true;
            RefreshDisplay();
        }

        private void EnableOnlineLeaderboards()
        {
            SettingsManager.SetCachedBool(AllowOnlineLeaderboards, true);
            ShowOnlineRecords(true);
        }


        public async UniTaskVoid ShowLocalInfo()
        {
            if (_cancellationToken?.IsCancellationRequested ?? false && !_tokenDisposed)
            {
                await UniTask.DelayFrame(1);
                _cancellationToken.Dispose();
                _tokenDisposed = true;
            }
            if (_cancellationToken == null || _tokenDisposed)
            {
                _cancellationToken = new CancellationTokenSource();
                _tokenDisposed = false;
            }

            var songRecord = await GetSongRecord();
            SetFields(songRecord);
        }

        public async UniTaskVoid ShowOnlineInfo()
        {
            ClearFields();
            if (_cancellationToken?.IsCancellationRequested ?? false)
            {
                await UniTask.DelayFrame(1);
                _cancellationToken.Dispose();
                _tokenDisposed = true;
            }
            if (_cancellationToken == null || _tokenDisposed)
            {
                _cancellationToken = new CancellationTokenSource();
                _tokenDisposed = false;
            }
            if (!NetworkConnectionManager.Instance.NetworkConnected)
            {
                _statusDisplay.SetText("Not connected to the internet. Online leaderboards unavailable.");
                _statusDisplayBackground.gameObject.SetActive(true);
                return;
            }
            else if (!AzureSqlManager.Instance.ServerIsRunning)
            {
                _statusDisplay.SetText("Contacting server. If this is the first time this session, it may take some time.");
                _statusDisplayBackground.gameObject.SetActive(true);
            }
            var songRecord = await AzureSqlManager.Instance.GetTopLeaderboard(_songInfo, _showSongOptions.DifficultyAsEnum, _showSongOptions.SelectedGameMode, _cancellationToken.Token);
            _statusDisplay.ClearText();
            _statusDisplayBackground.gameObject.SetActive(false);
            if (songRecord != null)
            {
                SetFields(songRecord);
            }
        }

        private void SetFields(SongRecord[] songRecord)
        {
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
            _songRecordStreaks.SetText(streaksSb);
            _songScoreNames.SetText(namesSb);
        }
        private void SetFields(LeaderboardObject[] songRecord)
        {
            using var namesSb = ZString.CreateStringBuilder(false);
            using var scoresSb = ZString.CreateStringBuilder(false);
            using var streaksSb = ZString.CreateStringBuilder(false);

            foreach (var score in songRecord)
            {
                if (string.IsNullOrWhiteSpace(score.ProfileName))
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
            _songRecordStreaks.SetText(streaksSb);
            _songScoreNames.SetText(namesSb);
        }

        private void ClearFields()
        {
            _songRecordScores.ClearText();
            _songRecordStreaks.ClearText();
            _songScoreNames.ClearText();
        }

        private async UniTask<SongRecord[]> GetSongRecord()
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancellationToken.Token);
            var records = await PlayerStatsFileManager.TryGetRecords(_songInfo, _showSongOptions.DifficultyAsEnum,
                _showSongOptions.SelectedGameMode, _cancellationToken.Token);

            return records;
        }
    }
}