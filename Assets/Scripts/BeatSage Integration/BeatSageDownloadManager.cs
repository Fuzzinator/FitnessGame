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

public class BeatSageDownloadManager
{
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
        return AddDownload(filePath);
    }

    public static Download AddDownload(string filePath)
    {
        filePath = filePath.TrimEnd('\r', '\n');

        var download = new Download(filePath);
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
                await CreateCustomLevelFromFile(download, cts);
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
            }

            download.IsAlive = false;
            cts.Dispose();
        }
        cts.Dispose();
    }

    private static async UniTask CreateCustomLevelFromFile(Download download, CancellationTokenSource cts)
    {
        download.Status = "Uploading File";
        //Update displayed progress
        download.Progress = 0.1;

        var tagFile = TagLib.File.Create(download.FilePath);

        var artistName = "Unknown";
        var trackName = "Unknown";
        byte[] imageData = null;

        var invalids = Path.GetInvalidFileNameChars();

        if (tagFile.Tag.FirstPerformer != null)
        {
            artistName = string.Join("_", tagFile.Tag.FirstPerformer.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        if (tagFile.Tag.Title != null)
        {
            trackName = string.Join("_", tagFile.Tag.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
        else
        {
            trackName = Path.GetFileNameWithoutExtension(download.FilePath);
        }

        if (tagFile.Tag.Pictures.Count() > 0)
        {
            if (tagFile.Tag.Pictures[0].Data.Data != null)
            {
                imageData = tagFile.Tag.Pictures[0].Data.Data;
            }
        }


        download.Artist = artistName;
        download.Title = trackName;

        string fileName = "[BSD] " + trackName + " - " + artistName;

        byte[] bytes = File.ReadAllBytes(download.FilePath);

        //Update displayed progress
        download.Progress = 0.2;

        string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
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

        content.Add(new StringContent(trackName), "audio_metadata_title");
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

        JObject jsonString = JObject.Parse(responseString);

        string levelID = (string)jsonString["id"];

        await CheckDownload(levelID, trackName, artistName, download, cts);
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
                break;
            }

        }

        if (download.downloadStatus == "DONE")
        {
            //Update displayed progress
            download.Progress = 0.8;
            await RetrieveDownload(levelId, trackName, artistName, download);
        }
        else if (download.downloadStatus == "FAILED")
        {
            download.Progress = -1;
            Downloads.Remove(download);
        }
    }

    private static async UniTask RetrieveDownload(string levelId, string trackName, string artistName, Download download)
    {
        download.Status = "Downloading";
        //Update displayed progress
        download.Progress = .85;

        var url = "https://beatsage.com/beatsaber_custom_level_download/" + levelId;

        var client = new WebClient();
        var uri = new Uri(url);

        var fileName = "[BSD] " + trackName + " - " + artistName;
        fileName = fileName.RemoveIllegalIOCharacters();
        var songBytes = await client.DownloadDataTaskAsync(uri);

        download.Status = "Extracting";
        try
        {
            ZipFileManagement.ExtractAndSaveZippedSong(fileName, songBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{fileName} cant be saved might have illegal characters {ex.Message} -- {ex.StackTrace}");
        }

        //Update displayed progress
        download.Progress = .95;

        download.Status = "Completed";
        download.IsAlive = false;

        //UpdateImage(filePath, download);

        await UniTask.DelayFrame(1);
        if (SongInfoFilesReader.Instance != null)
        {
            SongInfoFilesReader.Instance.LoadNewSong(fileName, "LOCAL", 0).Forget();
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

        public Download(string filePath)
        {
            Number = Downloads.Count + 1;
            YoutubeID = "";
            Title = "???";
            Artist = "???";
            Status = "Queued";
            Difficulties = "Expert,ExpertPlus,Normal,Hard";
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
