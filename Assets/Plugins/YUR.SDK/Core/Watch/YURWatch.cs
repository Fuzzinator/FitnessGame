using UnityEngine;
using YUR.SDK.Core.Configuration;
using YUR.SDK.Core.Initialization;
using YUR.SDK.Core.Enums;
using UnityEngine.Events;
using System;
using UnityEngine.Serialization;

namespace YUR.SDK.Core.Watch
{
    /// <summary>
    /// Prefab used to create the YURWatch implementation of the YUR.SDK
    /// </summary>
    public class YURWatch : MonoBehaviour
    {
        public static YURWatch Instance { get; private set; }
        internal UnityEvent OnHandChanged { get; set; }

        // Settings Field
        [SerializeField, FormerlySerializedAs("YURSettingsAsset")]
        private YUR_Settings _yURSettingsAsset = null;
        public YUR_Settings YURSettingsAsset => _yURSettingsAsset;

        //Default Watch
        [SerializeField, FormerlySerializedAs("DefaultWatch")]
        private GameObject _defaultWatch = null;
        public GameObject DefaultWatch => _defaultWatch;

        // Hand Fields
        internal static GameObject head = null;
        internal static Transform leftHandAnchor = null;
        internal static Transform rightHandAnchor = null;

        // Dynamically Set References
        private GameObject _watchContainer = null;
        private GameObject _watch = null;
        private GameObject _defaultWatchInstance = null;
        private Vector3 _watchpos = Vector3.zero;
        private Vector3 _watcheuler = Vector3.zero;
        private Vector3 _watchscale = Vector3.one;

        private Action<GameObject, int> _setLayer = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Instantiate initial watch pieces and set their layer
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            CreateWatch();
            SetDefaultSettings();
            SetLayers();
        }

        //Create structure for watch and loads watch prefab from Resources
        private void CreateWatch()
        {
            try
            {
                _watchContainer = new GameObject("YUR.WatchContainer");
                _watchContainer.transform.SetParent(transform);

                _watch = Instantiate(Resources.Load("YURWatch\\YUR.Watch", typeof(GameObject)) as GameObject);
                _watch.transform.SetParent(_watchContainer.transform);
                _defaultWatchInstance = Instantiate(_defaultWatch, _watch.transform);
                if (YUR_Manager.Instance.YURSettings.DisableWatchModel)
                {
                    _defaultWatchInstance.SetActive(false);
                }
                _watchpos = _watch.transform.position;
                _watcheuler = _watch.transform.eulerAngles;
                _watchscale = _watch.transform.localScale;

            }
            catch (UnityException e)
            {
                Debug.Log("Error Creating Watch: " + e.Message);
            }
        }

        // 
        private void SetDefaultSettings()
        {
            try
            {
                if (YURSettingsAsset.WatchAndTileShaderOverride != null)
                {
                    var childCount = _defaultWatch.transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        _defaultWatch.transform.GetChild(i).GetComponent<MeshRenderer>().sharedMaterial.shader = YURSettingsAsset.WatchAndTileShaderOverride;
                    }
                }
            }
            catch (UnityException e)
            {
                Debug.Log(e.Message);
            }
        }

        // Recursively set layers of the watch
        internal void SetLayers()
        {
            try
            {
                _setLayer = (go, layer) =>
                {
                    go.layer = layer;
                    for (int i = 0; i < go.transform.childCount; i++)
                    {
                        var t = go.transform.GetChild(i);
                        _setLayer(t.gameObject, layer);
                    }
                };

                var setLayer = LayerMask.NameToLayer(YURSettingsAsset.LayerToSet);
                if (setLayer < 0)
                {
                    setLayer = 0;
                }

                _setLayer(_watch, setLayer);
            }
            catch (UnityException e)
            {
                Debug.Log("Could not set layers on the watch. Please make sure you have a proper layer set in your YURSettings: " + e.Message);
            }
        }

        // Follow the Hand Being Used (in the config asset)
        private void Update()
        {
            if (_watch != null && _watch.activeSelf)
            {
                UpdateWatchTransform(YURSettingsAsset);
            }
        }

        // Updates the watch pos and rot
        private void UpdateWatchTransform(YUR_Settings settings)
        {
            try
            {
                if (!(leftHandAnchor is null) && !(rightHandAnchor is null))
                {
                    switch (settings.HandBeingUsed)
                    {
                        case HandState.Left:
                            _watchpos = leftHandAnchor.position;
                            _watcheuler = leftHandAnchor.eulerAngles;
                            _watch.transform.localPosition = settings.LeftPositionOffset;
                            _watch.transform.localEulerAngles = settings.LeftEulerOffset;
                            _watch.transform.localScale = new Vector3(-_watchscale.x, _watchscale.y, -_watchscale.z);
                            break;
                        case HandState.Right:
                            _watchpos = rightHandAnchor.position;
                            _watcheuler = rightHandAnchor.eulerAngles;
                            _watch.transform.localPosition = settings.RightPositionOffset;
                            _watch.transform.localEulerAngles = settings.RightEulerOffset;
                            _watch.transform.localScale = new Vector3(-_watchscale.x, -_watchscale.y, -_watchscale.z);
                            break;
                        default:
                            _watchpos = Vector3.zero;
                            _watcheuler = Vector3.zero;
                            _watch.transform.localScale = _watchscale;
                            break;
                    }

                    _watchContainer.transform.position = _watchpos;
                    _watchContainer.transform.eulerAngles = _watcheuler;
                }
            }
            catch (MissingReferenceException e)
            {
                YUR_Manager.Instance.Log(e.Message);
                gameObject.SetActive(false);
            }
        }

        public void ToggleWatch(bool isActive)
        {
            if (_defaultWatchInstance != null)
            {
                _defaultWatchInstance.SetActive(isActive);
            }
            else
            {
                Debug.Log("There is default watch to toggle!");
            }
        }
    }
}
