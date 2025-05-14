using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.IO;
using static BeatSageDownloadManager;
using UnityEngine.Events;
using TagLib;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class BeatSageDownloadManager
{
    private const string FailedToParse = "Unexpected character encountered while parsing value";
    private const string FailedToParseHeader = "Unable to load song";
    private const string FailedToParseMessage1 = "The song ";
    private const string FailedToParseMessage2 = " failed to load. It may be incompatible or corrupted.";
    private const string Delete = "Delete Song";
    private const string Ignore = "Ignore";
    public static List<Download> Downloads { get; private set; } = new List<Download>();

    private static HttpClient _httpClient;
    public static HttpClient HttpClient
    {
        get
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("Host", "beatsage.com");
                _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "BeatSage-Downloader/1.2.6");
            }
            return _httpClient;
        }
    }

    public static Download TryAddDownload(string songName, string filePath)
    {
        if (Downloads.Any((i) => i.FilePath == filePath))
        {
            var fileName = Path.GetFileName(filePath);
            var visuals = new Notification.NotificationVisuals($"{fileName} is already being converted.", "Conversion Failed", autoTimeOutTime: 1.5f, popUp: true);
            NotificationManager.RequestNotification(visuals);
            return null;
        }
        return AddDownload(songName, filePath);
    }

    public static Download AddDownload(string songName, string filePath)
    {
        filePath = filePath.TrimEnd('\r', '\n');

        var download = new Download(songName, filePath);
        Downloads.Add(download);

        HandleDownload(download).Forget();
        return download;
    }

    private static async UniTaskVoid HandleDownload(Download download)
    {

        var cts = new CancellationTokenSource();

        download.IsAlive = true;
        if ((download.FilePath != "") && (download.FilePath != null))
        {
            try
            {
                var success = await TryCreateCustomLevelFromFile(download, cts);
                if(!success)
                {
                    download.Status = "Unable To Create Level";

                    download.Progress = -1;
                    Downloads.Remove(download);
                }
                if (download.Progress > 1)
                {
                    download.Progress = 1;
                }
            }
            catch (Exception e)
            {
                download.Status = "Unable To Create Level";

                download.Progress = -1;
                Downloads.Remove(download);
                if (e is not OperationCanceledException)
                {
                    Debug.LogError($"{e.Message} {e.StackTrace}");
                }
            }

            download.IsAlive = false;
            cts.Dispose();
        }
        cts.Dispose();
    }

    private static async UniTask<bool> TryCreateCustomLevelFromFile(Download download, CancellationTokenSource cts)
    {
        download.Status = "Uploading File";
        //Update displayed progress
        download.Progress = 0.1;


        var artistName = "Unknown";
        byte[] imageData = null;
        try
        {
            var tagFile = TagLib.File.Create(download.FilePath);


            var invalids = Path.GetInvalidFileNameChars();

            if (tagFile.Tag.FirstPerformer != null)
            {
                artistName = string.Join("_", tagFile.Tag.FirstPerformer.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            var trackName = "Unknown";
            if (tagFile.Tag.Title != null)
            {
                trackName = string.Join("_", tagFile.Tag.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }
            else
            {
                trackName = Path.GetFileNameWithoutExtension(download.FilePath);
            }
            download.Title = trackName;

            if (tagFile.Tag.Pictures.Count() > 0)
            {
                if (tagFile.Tag.Pictures[0].Data.Data != null)
                {
                    imageData = tagFile.Tag.Pictures[0].Data.Data;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                Debug.LogError($"{ex.GetType()}\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        download.Artist = artistName;

        var bytes = System.IO.File.ReadAllBytes(download.FilePath);

        //Update displayed progress
        download.Progress = 0.2;

        var boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
        var content = new MultipartFormDataContent(boundary);

        content.Add(new ByteArrayContent(bytes), "audio_file", download.FileName);

        if (imageData != null)
        {
            var imageContent = new ByteArrayContent(imageData);
            imageContent.Headers.Remove("Content-Type");
            imageContent.Headers.Add("Content-Disposition", "form-data; name=\"cover_art\"; filename=\"cover\"");
            imageContent.Headers.Add("Content-Type", "image/jpeg");
            content.Add(imageContent);
        }

        content.Add(new StringContent(download.Title), "audio_metadata_title");
        content.Add(new StringContent(artistName), "audio_metadata_artist");
        content.Add(new StringContent(download.Difficulties), "difficulties");
        content.Add(new StringContent(download.GameModes), "modes");
        content.Add(new StringContent(download.SongEvents), "events");
        content.Add(new StringContent(download.Environment), "environment");
        content.Add(new StringContent(download.ModelVersion), "system_tag");

        var response = await HttpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content, cts.Token);

        //Update displayed progress
        download.Progress = 0.25;

        var responseString = await response.Content.ReadAsStringAsync();
        try
        {
            JObject jsonString = JObject.Parse(responseString);

            string levelID = (string)jsonString["id"];

            await CheckDownload(levelID, download.Title, artistName, download, cts);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains(FailedToParse))
            {
                return false;
            }
        }

        return true;
    }

    private static async UniTask CheckDownload(string levelId, string trackName, string artistName, Download download, CancellationTokenSource cts)
    {
        download.Status = "Generating Custom Level";

        string url = "https://beatsage.com/beatsaber_custom_level_heartbeat/" + levelId;

        download.downloadStatus = "PENDING";

        var progress = 0.25;
        while (download.downloadStatus == "PENDING")
        {
            try
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.Delay(1000);

                //POST the object to the specified URI 
                var response = await HttpClient.GetAsync(url, cts.Token);

                //Read back the answer from server
                var responseString = await response.Content.ReadAsStringAsync();

                JObject jsonString = JObject.Parse(responseString);

                download.downloadStatus = (string)jsonString["status"];
                if (progress < .75f)
                {
                    progress += Time.deltaTime * .5f;
                    //Update displayed progress
                    download.Progress = progress;
                }

            }
            catch (Exception e)
            {
                download.downloadStatus = "FAILED";
                if (e is not OperationCanceledException)
                {
                    Debug.LogError($"{e.Message} {e.StackTrace}");
                }
                break;

            }

        }

        if (download.downloadStatus == "DONE")
        {
            //Update displayed progress
            download.Progress = 0.8;
            await UniTask.DelayFrame(1, cancellationToken: cts.Token);
            await RetrieveDownload(levelId, trackName, artistName, download, cts);
        }
        else if (download.downloadStatus == "FAILED")
        {
            download.Progress = -1;
            Downloads.Remove(download);
        }
    }

    private static async UniTask RetrieveDownload(string levelId, string trackName, string artistName, Download download, CancellationTokenSource cts, int attempts = 0)
    {
        download.Status = "Downloading";
        //Update displayed progress
        download.Progress = .85;

        var url = "https://beatsage.com/beatsaber_custom_level_download/" + levelId;

        var client = new WebClient();
        var uri = new Uri(url);

        var fileName = $"{trackName} - {artistName}";
        fileName = fileName.RemoveIllegalIOCharacters();

        byte[] songBytes = null;
        try
        {
            songBytes = await client.DownloadDataTaskAsync(uri);
        }
        catch (Exception ex)
        {
            if (ex is WebException)
            {
                if (attempts < 5)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cts.Token);
                    attempts++;
                    await RetrieveDownload(levelId, trackName, artistName, download, cts, attempts);
                    return;
                }
                else
                {
                    var visuals = new Notification.NotificationVisuals($"Beat Sage Failed to convert {download.Title}. Would you like to try again?", "Conversion Failed", "Try Again", "Cancel");
                    NotificationManager.RequestNotification(visuals, HandleDownload(download).Forget, () =>
                    {
                        download.Status = "Unable To Create Level";
                        download.Progress = -1;
                        Downloads.Remove(download);
                    });
                    return;
                }
            }
            Debug.Log($"{ex.Message}\n{ex.StackTrace}");

            download.Status = "Unable To Create Level";

            download.Progress = -1;
            Downloads.Remove(download);
            return;
        }
        if (songBytes == null)
        {
            download.Status = "Unable To Create Level";

            download.Progress = -1;
            Downloads.Remove(download);
            return;
        }

        var targetDir = $"{AssetManager.SongsPath}{fileName}";
        if (Directory.Exists(targetDir))
        {
            Directory.Delete(targetDir, true);
        }
        await UniTask.DelayFrame(1);
        download.Status = "Extracting";
        try
        {
            ZipFileManagement.ExtractAndSaveZippedSong(fileName, songBytes);
        }
        catch (Exception ex)
        {
            if (ex is InvalidDataException)
            {
                var inMainMenu = GameStateManager.Instance.CurrentGameState == GameState.InMainMenu;
                var visuals = inMainMenu ?
                    new Notification.NotificationVisuals($"Downloading {fileName} failed due to corrupted data. Please try again later.", "Download Failed", "Okay") :
                    new Notification.NotificationVisuals($"Downloading {fileName} failed due to corrupted data. Please try again later.", "Download Failed", autoTimeOutTime: 5f, popUp: true);
                NotificationManager.RequestNotification(visuals);

            }
            else
            {
                Debug.LogError($"{fileName} cant be saved might have illegal characters {ex.Message} -- {ex.StackTrace}");
            }
            download.Progress = -1;
            Downloads.Remove(download);
            return;
        }

        //Update displayed progress
        download.Progress = .95;

        download.Status = "Completed";
        download.IsAlive = false;

        //UpdateImage(filePath, download);

        await UniTask.DelayFrame(1);
        if (SongInfoFilesReader.Instance != null)
        {
            /*var path = $"{AssetManager.SongsPath}{fileName}/song.ogg";
            var clip = await AssetManager.LoadCustomSong(fileName, cts.Token, AudioType.OGGVORBIS, false);
            if(clip != null)
            {
                var bpm = await UniBpmAnalyzer.TryAnalyzeBpmWithJobs(clip);
                if(bpm > -1)
                {
                    AssetManager.
                }
            }*/
            var songInfo = await SongInfoFilesReader.Instance.LoadNewSong(fileName, "LOCAL", 0, true);
            PlaylistFilesReader.Instance.RefreshPlaylistsValidStates().Forget();
        }
        //Update displayed progress
        download.Progress = 1;
        Downloads.Remove(download);
    }

    //maybe later?
    private static void UpdateImage(string fileLocation, Download download)
    {
        var info = new DirectoryInfo(fileLocation);
        var creationDate = info.CreationTime;
        var files = info.GetFiles();
        if (files.Length == 0)
        {
            info.Delete(true);
        }
        foreach (var file in files)
        {
            if (file == null || !file.Extension.Equals(".jpg"))
            {
                continue;
            }

            var fileName = file.Name;
            file.Delete();

        }
    }


    [Serializable]
    public class Download
    {
        private int number;
        private string youtubeID;
        private string title;
        private string artist;
        private string status;
        private string difficulties;
        private string gameModes;
        private string songEvents;
        private string filePath;
        private string fileName;
        private string identifier;
        private string environment;
        private string modelVersion;
        private bool isAlive;
        public string downloadStatus;
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                ProgressUpdated?.Invoke(value);
            }
        }
        public UnityEvent<double> ProgressUpdated { get; }

        public Download(string songName, string filePath)
        {
            Number = Downloads.Count + 1;
            YoutubeID = "";
            Title = songName;
            Artist = "???";
            Status = "Queued";
            Difficulties = "ExpertPlus, Expert, Hard, Normal";
            GameModes = "Standard,90Degree,NoArrows,OneSaber";
            SongEvents = "DotBlocks,Obstacles,Bombs";
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Environment = "DefaultEnvironment";
            ModelVersion = "v2-flow";
            IsAlive = false;
            ProgressUpdated = new UnityEvent<double>();
        }

        public int Number
        {
            get
            {
                return number;
            }
            set
            {
                number = value;
                RaiseProperChanged();
            }
        }
        public string YoutubeID
        {
            get
            {
                return youtubeID;
            }
            set
            {
                youtubeID = value;
                if ((FileName == "") || (FileName == null))
                {
                    Identifier = value;
                }
                RaiseProperChanged();
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaiseProperChanged();
            }
        }

        public string Artist
        {
            get
            {
                return artist;
            }
            set
            {
                artist = value;
                RaiseProperChanged();
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                RaiseProperChanged();
            }
        }

        public string Difficulties
        {
            get
            {
                return difficulties;
            }
            set
            {
                difficulties = value;
                RaiseProperChanged();
            }
        }

        public string GameModes
        {
            get
            {
                return gameModes;
            }
            set
            {
                gameModes = value;
                RaiseProperChanged();
            }
        }

        public string SongEvents
        {
            get
            {
                return songEvents;
            }
            set
            {
                songEvents = value;
                RaiseProperChanged();
            }
        }

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                RaiseProperChanged();
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                if ((YoutubeID == "") || (YoutubeID == null))
                {
                    Identifier = fileName;
                }
                RaiseProperChanged();
            }
        }

        public string Identifier
        {
            get
            {
                return identifier;
            }
            set
            {
                identifier = value;
                RaiseProperChanged();
            }
        }

        public string Environment
        {
            get
            {
                return environment;
            }
            set
            {
                environment = value;
                RaiseProperChanged();
            }
        }

        public string ModelVersion
        {
            get
            {
                return modelVersion;
            }
            set
            {
                modelVersion = value;
                RaiseProperChanged();
            }
        }

        public bool IsAlive
        {
            get
            {
                return isAlive;
            }
            set
            {
                isAlive = value;
                RaiseProperChanged();
            }
        }

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperChanged([CallerMemberName] string caller = "")
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
}
