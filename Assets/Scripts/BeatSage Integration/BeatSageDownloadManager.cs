using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static BeatSageDownloadManager;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BeatSageDownloadManager : MonoBehaviour
{
    private string downloadStatus;

    [field:SerializeField]
    public List<string> FileLocations { get; private set; } = new List<string>();
    public static List<Download> downloads = new List<Download>();

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

    /*private void OnValidate()
    {
        if (FileLocations.Count > 0 && !string.IsNullOrWhiteSpace(FileLocations[0]))
        {
            AddDownloads();
            RunDownloads().Forget();
        }
    }*/

    public static CancellationTokenSource cts;

    public static Label newUpdateAvailableLabel;

    public void AddDownloads()
    {
        string selectedDifficulties = "Expert,ExpertPlus,Normal,Hard";
        string selectedGameModes = "Standard,90Degree,NoArrows,OneSaber";
        string selectedSongEvents = "DotBlocks,Obstacles,Bombs";
        string selectedEnvironment = "DefaultEnvironment";
        string selectedModelVersion = "v2-flow";

        for (int i = 0; i < FileLocations.Count; i++)
        {
            if (FileLocations[i].Contains(".mp3"))
            {
                string filePath = FileLocations[i].TrimEnd('\r', '\n');

                Console.WriteLine("File Path: " + filePath);

                Add(new Download()
                {
                    Number = downloads.Count + 1,
                    YoutubeID = "",
                    Title = "???",
                    Artist = "???",
                    Status = "Queued",
                    Difficulties = selectedDifficulties,
                    GameModes = selectedGameModes,
                    SongEvents = selectedSongEvents,
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Environment = selectedEnvironment,
                    ModelVersion = selectedModelVersion,
                    IsAlive = false
                });
            }
        }
    }

    public async UniTaskVoid RunDownloads()
    {
        Console.WriteLine("RunDownloads Started");

        int previousNumberOfDownloads = downloads.Count;

        //SaveDownloads();

        cts = new CancellationTokenSource();

        List<Download> incompleteDownloads = new List<Download>();

        foreach (Download download in downloads)
        {
            if (download.Status == "Queued")
            {
                incompleteDownloads.Add(download);
            }
        }

        if (incompleteDownloads.Count >= 1)
        {
            Download currentDownload = incompleteDownloads[0];
            currentDownload.IsAlive = true;

            if ((currentDownload.FilePath != "") && (currentDownload.FilePath != null))
            {
                try
                {
                    await CreateCustomLevelFromFile(currentDownload);
                }
                catch
                {
                    currentDownload.Status = "Unable To Create Level";
                }

                currentDownload.IsAlive = false;
                cts.Dispose();
            }

        }

        cts.Dispose();
    }

    public void Add(Download download)
    {
        downloads.Add(download);
    }

    async UniTask CreateCustomLevelFromFile(Download download)
    {
        download.Status = "Uploading File";

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

        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseString);

        JObject jsonString = JObject.Parse(responseString);

        string levelID = (string)jsonString["id"];

        Console.WriteLine(levelID);

        await CheckDownload(levelID, trackName, artistName, download);
    }

    async UniTask CheckDownload(string levelId, string trackName, string artistName, Download download)
    {
        download.Status = "Generating Custom Level";

        string url = "https://beatsage.com/beatsaber_custom_level_heartbeat/" + levelId;

        Console.WriteLine(url);

        downloadStatus = "PENDING";


        while (downloadStatus == "PENDING")
        {
            try
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine(downloadStatus);

                await UniTask.Delay(1000);

                //POST the object to the specified URI 
                var response = await HttpClient.GetAsync(url, cts.Token);

                //Read back the answer from server
                var responseString = await response.Content.ReadAsStringAsync();

                JObject jsonString = JObject.Parse(responseString);

                downloadStatus = (string)jsonString["status"];

            }
            catch
            {
            }

        }

        if (downloadStatus == "DONE")
        {
            RetrieveDownload(levelId, trackName, artistName, download);
        }
    }

    void RetrieveDownload(string levelId, string trackName, string artistName, Download download)
    {
        download.Status = "Downloading";

        string url = "https://beatsage.com/beatsaber_custom_level_download/" + levelId;

        Console.WriteLine(url);

        WebClient client = new WebClient();
        Uri uri = new Uri(url);

        var songsPath = AssetManager.SongsPath;

        int pathLength = songsPath.Count();

        string fileName = "[BSD] " + trackName + " - " + artistName;

        var filePath = (songsPath + @"\" + fileName);//.Substring(0, 244 - pathLength);
        
        download.Status = "Extracting";

        if (Directory.Exists("temp.zip"))
        {
            Directory.Delete("temp.zip");
        }

        client.DownloadFile(uri, "temp.zip");

        if (Directory.Exists(filePath))
        {
            Directory.Delete(filePath, true);
        }

        ZipFile.ExtractToDirectory("temp.zip", filePath);

        if (File.Exists("temp.zip"))
        {
            File.Delete("temp.zip");
        }

        /*if (Settings.Default.automaticExtraction)
        {
            
        }
        else
        {

            if (File.Exists(filePath + ".zip"))
            {
                File.Delete(filePath + ".zip");
            }

            client.DownloadFile(uri, filePath + ".zip");
        }*/


        download.Status = "Completed";
        download.IsAlive = false;
    }

   /* public void SaveDownloads()
    {
        List<Download> downloadsList = new List<Download>();

        foreach (Download download in downloads)
        {
            downloadsList.Add(download);
        }

        if (downloadsList.Count > 0)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, downloadsList);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
            }
        }
    }*/

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
    /*internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Downloads")]
        public string outputDirectory
        {
            get
            {
                return ((string)(this["outputDirectory"]));
            }
            set
            {
                this["outputDirectory"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Expert,ExpertPlus")]
        public string previousDifficulties
        {
            get
            {
                return ((string)(this["previousDifficulties"]));
            }
            set
            {
                this["previousDifficulties"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Standard")]
        public string previousGameModes
        {
            get
            {
                return ((string)(this["previousGameModes"]));
            }
            set
            {
                this["previousGameModes"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string previousGameEvents
        {
            get
            {
                return ((string)(this["previousGameEvents"]));
            }
            set
            {
                this["previousGameEvents"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Default")]
        public string previousEnvironment
        {
            get
            {
                return ((string)(this["previousEnvironment"]));
            }
            set
            {
                this["previousEnvironment"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("V2")]
        public string previousModelVersion
        {
            get
            {
                return ((string)(this["previousModelVersion"]));
            }
            set
            {
                this["previousModelVersion"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool automaticExtraction
        {
            get
            {
                return ((bool)(this["automaticExtraction"]));
            }
            set
            {
                this["automaticExtraction"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool overwriteExisting
        {
            get
            {
                return ((bool)(this["overwriteExisting"]));
            }
            set
            {
                this["overwriteExisting"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string savedDownloads
        {
            get
            {
                return ((string)(this["savedDownloads"]));
            }
            set
            {
                this["savedDownloads"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool saveDownloadsQueue
        {
            get
            {
                return ((bool)(this["saveDownloadsQueue"]));
            }
            set
            {
                this["saveDownloadsQueue"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool enableLocalYouTubeDownload
        {
            get
            {
                return ((bool)(this["enableLocalYouTubeDownload"]));
            }
            set
            {
                this["enableLocalYouTubeDownload"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("v1.2.6")]
        public string currentVersion
        {
            get
            {
                return ((string)(this["currentVersion"]));
            }
            set
            {
                this["currentVersion"] = value;
            }
        }
    }*/

}
