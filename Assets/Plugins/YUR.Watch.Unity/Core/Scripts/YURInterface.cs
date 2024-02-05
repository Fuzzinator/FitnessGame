using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using YUR.Core;

namespace YUR.Core
{
    public class YURInterface : Singleton<YURInterface>
    {
        #region VARIABLES
        internal YUR_SDK.CYurFitSDK sdk = new YUR_SDK.CYurFitSDK();

        public float RefreshTime = 5; // Minutes
        [SerializeField]
        private bool debug = true;
        public bool debugCalc = false;

        internal string Token => _googleResponse != null ? _googleResponse.idToken : "";
        internal bool HasLogin => _bDataOk;
        internal YURProfile yurProfile => _yurProfile;
        internal YURWidgets widgets => _widgets;
        internal YUR_SDK.CResults cResults => _cResults;

        private YUR_SDK.RefreshResponse _googleResponse = null;
        private YUR_SDK.ShortCodeResponse _codeResponse;
        private float _waitTime = 5.0f;
        private float _timer = 0.0f;
        private bool _bDataOk = false;
        private bool _bWaitCodeValidation = true;
        public bool _bIsTokenReaded = false;
        private bool _processingLoging = true;
        private bool _initializedLogin = false;

        public const float _workoutUpdateRepeatSeconds = 5.0f;
        
        private float _lastDataRefresh;

        private YURProfile _yurProfile;
        private YURWidgets _widgets;

        public Transform HMD => YURHMD.Instance?.transform;
        public Transform Left => YURLeftHand.Instance?.transform;
        public Transform Right => YURRightHand.Instance?.transform;

        private YUR_SDK.CResults _cResults;

        #endregion

        #region EVENTS
        public Action<string,string> OnSDKCodeGenerated;
        public Action<YURWidgets> OnWidgetDataLoaded;
        public Action<YUR_SDK.CResults> OnCResultsLoaded;

        public Action OnStartLoadData;
        public Action OnLoadedDataInitialize;
        public Action<YURProfile> OnLoadedSDKInitialized;

        public Action OnLogout;

        private Coroutine _refreshCO;
        #endregion

        #region INIT SDK

        public void Start()
        {
        }

        public void Begin(GameInfo info, string UserID)
        {
            sdk.Init();
            RegisterEvents();
            RegisterInnerEvents();

            Login(UserID);

            InitGameInfo(info);
            if (!sdk.IsOnline())
            {
                Invoke("OfflineAdd", 0.05f);
            }
            StartCoroutine(TryStartWorkoutCoroutine());
        }

        public void Login(string UserID)
        {
            string publicPath = GetPublicDataPath();   // path shared between yur-integrated devices
            string privatePath = GetPrivateDataPath(); // path unique to this specific application

            // core SDK will not create the paths, so we have to!
            if (!publicPath.Equals(""))
                System.IO.Directory.CreateDirectory(publicPath);
            if (!privatePath.Equals(""))
                System.IO.Directory.CreateDirectory(privatePath);

            sdk.StartNetwork(privatePath, UserID, publicPath);
        }

        void OfflineAdd()
        {
            string sj = "{\"watchConfig\":{\"widgets\":{\"sleeve4\":{\"widgetTypeID\":\"YUR_Squats\",\"widgetSettings\":{}},\"face\":{\"widgetTypeID\":\"YUR_HeartRate\",\"widgetSettings\":{}},\"sleeve2\":{\"widgetTypeID\":\"YUR_Time\",\"widgetSettings\":{}},\"sleeve3\":{\"widgetSettings\":{},\"widgetTypeID\":\"YUR_WorkoutTime\"},\"sleeve1\":{\"widgetTypeID\":\"YUR_TodayCalories\",\"widgetSettings\":{}}}}}";
            var config = JsonUtility.FromJson<YURWatchConfig01>(sj);
            _widgets = config.watchConfig.widgets;
            OnWidgetDataLoaded?.Invoke(_widgets);
            _yurProfile = new YURProfile();
            OnLoadedDataInitialize?.Invoke();
            FinisLoadProcedure();
        }

        IEnumerator TryStartWorkoutCoroutine()
        {
            yield return new WaitForSeconds(1f);
            if (_bIsTokenReaded)
            {
                sdk.BridgeStartWorkout();
                InvokeRepeating("UpdateWorkout", _workoutUpdateRepeatSeconds, _workoutUpdateRepeatSeconds);
            } else
            {
                StartCoroutine(TryStartWorkoutCoroutine());
            }
        }

        void UpdateWorkout()
        {
            sdk.BridgeUpdateWorkout();
        }

        public void Stop()
        {
            sdk.BridgeFinalizeWorkout();
        }

        private void RegisterEvents()
        {
            YUR_SDK.Events.OnServerCodeEvent = OnServerCodeEvent;
            YUR_SDK.Events.OnDeviceCodeEvent = OnDeviceCodeEvent;
            YUR_SDK.Events.OnInitialTokensEvent = OnInitialTokensEvent;
            YUR_SDK.Events.OnRefreshResponseEvent = OnRefreshResponseEvent;
            YUR_SDK.Events.OnBioInformationEvent = OnBioInformationEvent;
            YUR_SDK.Events.OnProfileJSONEvent = OnProfileJSONEvent;
            YUR_SDK.Events.OnErrorEvent = OnErrorEvent;
            YUR_SDK.Events.OnGetWidgetdataJSONEvent = OnGetWidgetdataJSONEvent;
            YUR_SDK.Events.OnPreferencesJSONEvent = OnPreferencesJSONEvent;
            YUR_SDK.Events.OnLevelsJSONEvent = DisplayJsons;
            YUR_SDK.Events.OnStartWorkoutJSONEvent = DisplayJsons;
            YUR_SDK.Events.OnUpdateWorkoutJSONEvent = DisplayJsons;
            YUR_SDK.Events.OnFinalizeWorkoutJSONEvent = DisplayJsons;
            YUR_SDK.Events.OnGetTokenEvent = DisplayJsons;
            YUR_SDK.Events.OnCheckEIDEvent = DisplayJsons;
            YUR_SDK.Events.OnRemoveIDEvent = DisplayJsons;
        }

        public YUR_SDK.CResults GetResults()
        {
            return _cResults;
        }

        private static string GetPublicDataPath()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string path = Application.persistentDataPath; // default
            path = Environment.ExpandEnvironmentVariables(@"%AppData%");
            return Path.Combine(path, ".yur/watch/");
#elif UNITY_ANDROID
            
            // good way 
            return "";

            /* old way 
            string path = Application.persistentDataPath; // default
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            Debug.Log($"YUR perm {context.Call<int>("checkSelfPermission", "android.permission.WRITE_EXTERNAL_STORAGE")}");
            if (0 == context.Call<int>("checkSelfPermission", "android.permission.WRITE_EXTERNAL_STORAGE"))
            {
                path = System.Environment.GetEnvironmentVariable("EXTERNAL_STORAGE");
            }

            return Path.Combine(path, ".yur/watch/");
            */
#endif
        }

        private static string GetPrivateDataPath()
        {
            return Path.Combine(Application.persistentDataPath, ".yur/watch/");
        }

        private void InitGameInfo(GameInfo info)
        {
            sdk.SetGameInfo(info.license, info.gameName, info.version,info.subPlatform);
        }

        private void RegisterInnerEvents()
        {
            OnLogout += StopRefresh;
        }
#endregion

#region EVENTS
        private void OnServerCodeEvent(YUR_SDK.ShortCodeResponse response)
        {
            _codeResponse = response;
            if (debug) Debug.Log($"YUR code {response.shortcode}");
            OnSDKCodeGenerated?.Invoke(response.shortcode, response.verification_url);
        }
        private void OnDeviceCodeEvent(string sDeviceKey)
        {
            if (debug) if(sDeviceKey!=null &&sDeviceKey.Length!=0) Debug.Log("sIntCode " + sDeviceKey);
            if (sDeviceKey != "" && sDeviceKey.ToLower() != "invalid")
            {
                _bWaitCodeValidation = false;
                sdk.GetInitialTokensAsync(sDeviceKey);
            }

        }
        private void OnInitialTokensEvent(YUR_SDK.RefreshResponse response)
        {
            _googleResponse = response;
            _bIsTokenReaded = true;
            // sdk.SaveRefreshToken(_googleResponse.refreshToken);
        }
        private void OnRefreshResponseEvent(YUR_SDK.RefreshResponse response)
        {
            _googleResponse = response;
            _bIsTokenReaded = true;
            // sdk.SaveRefreshToken(_googleResponse.refreshToken);
        }

        private void OnBioInformationEvent(uint weightKG, uint heightCM, uint age, YUR_SDK.Sex sex)
        {
            if (_yurProfile == null)
            {
                _yurProfile = new YURProfile();
            }

            if (_yurProfile != null)
            {
                _yurProfile.bio.weight = weightKG;
                _yurProfile.bio.height = heightCM;
                _yurProfile.bio.age = (int)age;
                _yurProfile.bio.sex = sex;
            }
            if (debug) Debug.Log("### BioReponse Profile:" + _yurProfile.bio.weight + " H:" + _yurProfile.bio.height + " age:" + _yurProfile.bio.age + " sex:" + _yurProfile.bio.sex);

             sdk.SetBioData(_yurProfile.bio.age, _yurProfile.bio.sex, _yurProfile.bio.weight, _yurProfile.bio.height);

            _executedDataEvents.Add(YURDataEvents.BioInformation);
        }
        private void OnProfileJSONEvent(string sJsonProfile)
        {
            if (debug) Debug.Log("sJsonProfile: " + sJsonProfile);

            _yurProfile = JsonUtility.FromJson<YURProfile>(sJsonProfile);

            _executedDataEvents.Add(YURDataEvents.Profile);
        }

        private void OnGetWidgetdataJSONEvent(string sJsonWidget)
        {
            if (debug) Debug.Log("sJson Widget: " + sJsonWidget);

            //TODO: Define JSON Data Model

            _executedDataEvents.Add(YURDataEvents.WidgetData);
        }

        private void OnPreferencesJSONEvent(string sJsonPreferences)
        {
            if (debug) Debug.Log("sJson Preferences: " + sJsonPreferences);

            var config = JsonUtility.FromJson<YURWatchConfig>(sJsonPreferences);

            _widgets = config.widgets;

            _executedDataEvents.Add(YURDataEvents.Preferences);
        }

        private void OnErrorEvent(string sError)
        {
            if (debug) Debug.Log("sError: " + sError);

        }

        public void DisplayJsons(string sJson)
        {
            if (debug) Debug.Log("sJson: " + sJson);
        }
#endregion

        private void Update()
        {
            HandleLoadData();

            UpdateWatch();

        }

#region LOAD AND RELOAD
        private void HandleLoadData()
        {
            sdk.Update();
            if (!_bIsTokenReaded)
            {

                if (_codeResponse != null)
                {
                    _timer += Time.deltaTime;
                    if (_bWaitCodeValidation)
                    {

                        if (_timer > _waitTime)
                        {
                            _timer = _timer - _waitTime;
                            sdk.SendDeviceCodeAsync(_codeResponse.shortcode, _codeResponse.devicecode);
                        }
                    }
                }
            }
            else
            {

                if (_googleResponse != null && _bDataOk == false)
                {
                    _bDataOk = true;
                    _processingLoging = true;
                    if (!_initializedLogin)
                    {
                        StartCoroutine(ReloadProcedureCO());
                    }
                    else
                    {
                        if (_processingLoging)
                        {
                            OnStartLoadData?.Invoke();
                        }
                        StartCoroutine(ReloadProcedureCO());
                    }
                }
            }
        }

        private void LateUpdate()
        {
            UpdateData();
        }

        private void FinisLoadProcedure()
        {
            _lastDataRefresh = Time.time;
            _initializedLogin = false;
        }

        private List<YURDataEvents> _executedDataEvents = new List<YURDataEvents>();
        private Coroutine _waitLoadDataCO;
        private IEnumerator ReloadProcedureCO()
        {
            _executedDataEvents.Clear();

            _waitLoadDataCO = StartCoroutine(WaitLoadData(YURDataEvents.BioInformation));
            sdk.GetBioInformationsAsync(_googleResponse.idToken);
            if (debug) Debug.Log("Waiting get bio procedure!");
            yield return _waitLoadDataCO;

            _waitLoadDataCO = StartCoroutine(WaitLoadData(YURDataEvents.Profile));
            sdk.GetProfileAsync(_googleResponse.idToken);
            if (debug) Debug.Log("Waiting get profile procedure!");
                yield return _waitLoadDataCO;

            _waitLoadDataCO = StartCoroutine(WaitLoadData(YURDataEvents.WidgetData));
            sdk.GetWidgetdataAsync(_googleResponse.idToken);
            if (debug) Debug.Log("Waiting get widget data procedure!");
                yield return _waitLoadDataCO;

            _waitLoadDataCO = StartCoroutine(WaitLoadData(YURDataEvents.Preferences));
            sdk.GetPreferencesAsync(_googleResponse.idToken);
            if (debug) Debug.Log("Waiting get preferences data procedure!");
                yield return _waitLoadDataCO;

            if (debug) Debug.Log("Finished procedures!");

            if (_processingLoging)
            {
                OnLoadedSDKInitialized?.Invoke(_yurProfile);
            }

            OnWidgetDataLoaded?.Invoke(_widgets);

            if (_processingLoging)
            {
                OnLoadedDataInitialize?.Invoke();
                _processingLoging = false;
            }

            FinisLoadProcedure();
        }

        private IEnumerator WaitLoadData(YURDataEvents dataEvent)
        {
            while (!_executedDataEvents.Contains(dataEvent))
            {
                yield return null;
            }
            _waitLoadDataCO = null;
        }

        public void Logout()
        {
            sdk.DisconnectToken();
            _bIsTokenReaded = false;
            _codeResponse = null;
            _bWaitCodeValidation = true;
            _bDataOk = false;
            _initializedLogin = true;
            OnLogout?.Invoke();
        }
#endregion

#region WATCH

        private void UpdateWatch()
        {
            UpdateDeviceSamples();
        }

        private void UpdateData()
        {
            if (_initializedLogin || !_bDataOk || _processingLoging)
                return;

            if ((Time.time - _lastDataRefresh) > RefreshTime * 60)
            {
                if (_refreshCO == null)
                    _refreshCO = StartCoroutine(RefreshDataCO());
            }
        }

        private void StopRefresh()
        {
            if (_refreshCO != null)
            {
                StopCoroutine(_refreshCO);
                _refreshCO = null;
            }
        }

        private IEnumerator RefreshDataCO()
        {
            yield return StartCoroutine(ReloadProcedureCO());
            _refreshCO = null;
            _lastDataRefresh = Time.time;
        }

        // update the hmd, l, & r, device samples with position info from the xr rig
        private void UpdateDeviceSamples()
        {
            if(YURHMD.IsNull || YURLeftHand.IsNull || YURRightHand.IsNull)
            {
                return;
            }

            //YUR_SDK.CResults results = sdk.Calculator(HMD.AsYURDeviceSample(), Left.AsYURDeviceSample(), Right.AsYURDeviceSample());
	        sdk.CalculatorAsync(HMD.AsYURDeviceSample(), Left.AsYURDeviceSample(), Right.AsYURDeviceSample());

            YUR_SDK.CResults results = sdk.GetAsyncResult();

            if (debugCalc)
            {
                Debug.Log("Calories:              " + results.Calories);
                Debug.Log("HmdDistanceTravelled:  " + results.HmdDistanceTravelled);
                Debug.Log("LeftDistanceTravelled: " + results.LeftDistanceTravelled);
                Debug.Log("RightDistanceTravelled:" + results.RightDistanceTravelled);
                Debug.Log("RightDistanceTravelled:" + results.RightDistanceTravelled);
                Debug.Log("Est Heart Rate:        " + results.EstHeartRate);
                Debug.Log("Squats:                " + results.Squats);
                Debug.Log("Timestamp:             " + results.Timestamp);
                Debug.Log("LinuxTimestamp:        " + results.LinuxTimestamp);
                Debug.Log("eActivityLevel:        " + results.eActivityLevel);
                Debug.Log("m_nStepLeft:           " + results.m_nStepLeft);
                Debug.Log("m_nStepRight:          " + results.m_nStepRight);
                Debug.Log("m_nJump:               " + results.m_nJump);
                Debug.Log("m_fLastStepTime:       " + results.m_fLastStepTime);
                Debug.Log("m_fMediumStepTime:     " + results.m_fMediumStepTime);
                Debug.Log("m_fStepAccel:          " + results.m_fStepAccel);
            }
            _cResults = results;
            OnCResultsLoaded?.Invoke(_cResults);
        }

#endregion

#region TAGS

        public void SetTag(string tag)
        {
            sdk.SetTag(tag);
        }

#endregion

    }

    [System.Serializable]
    public class YURProfile
    {
        public string username;
        public string name;
        public string photo_url;
        public string current_xp;
        public string uid;
        public string all_time_xp;
        public string total_workouts;

        public YURLevel level;
        public bool prestige;

        public YURBioLevel bio = new YURBioLevel();
    }

    [System.Serializable]
    public class YURLevel
    {
        public string name;
        public int level;
        public string required_xp;
        public string colorHex;
    }

    [System.Serializable]
    public class YURBioLevel
    {
        public uint weight;
        public uint height;
        public int age;
        public YUR_SDK.Sex sex;
    }
    [System.Serializable]
    public class YURWatchConfig01
    {
        public YURWatchConfig watchConfig;
    }

    [System.Serializable]
    public class YURWatchConfig
    {
        public YURWidgets widgets;
    }

    [System.Serializable]
    public class YURWidgets
    {
        public YUWidgetItem sleeve1;
        public YUWidgetItem sleeve2;
        public YUWidgetItem sleeve3;
        public YUWidgetItem sleeve4;
        public YUWidgetItem face;
    }

    [System.Serializable]
    public class YUWidgetItem
    {
        public string widgetTypeID;
    }

    [System.Serializable]
    public class GameInfo
    {
        public string license = "";
        public string gameName = Application.productName;
        public string version = Application.version;
        public YUR_SDK.SubPlatoform subPlatform = YUR_SDK.SubPlatoform.Android_Quest;
        
        public GameInfo(string _license, string _gameName, string _version, YUR_SDK.SubPlatoform pt)
        {
            license = _license;
            gameName = _gameName;
            version = _version;
            subPlatform = pt;
        }
    }
}
