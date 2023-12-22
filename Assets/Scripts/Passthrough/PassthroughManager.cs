using Cysharp.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PassthroughManager : MonoBehaviour
{
    public static PassthroughManager DynamicInstance { get; private set; }

    [Tooltip("Specify if Insight Passthrough should be enabled. Passthrough layers can only be used if passthrough is enabled.")]
    public bool isInsightPassthroughEnabled = false;
    [SerializeField]
    private bool _mainMenu;
    [SerializeField]
    private Animator _animator;

    private const string AnimatorPassthrough = "Passthrough";

    private void Awake()
    {
#if UNITY_STANDALONE_WIN
        Destroy(gameObject);
        return;
#endif
        if (DynamicInstance == null)
        {
            DynamicInstance = this;
        }
        else
        {
            Destroy(DynamicInstance);
            DynamicInstance = this; 
        }
    }

    private void OnEnable()
    {
#if UNITY_STANDALONE_WIN
        Destroy(gameObject);
#endif
        if (!_mainMenu)
        {
            return;
        }
        ProfileManager.Instance.activeProfileUpdated.AddListener(CheckOnProfileChange);
        SettingsManager.CachedBoolSettingChanged.AddListener(UpdateFromSettingChange);

        if (ProfileManager.Instance.ActiveProfile != null)
        {
            CheckOnProfileChange();
        }
    }

    private void Start()
    {
        if (!_mainMenu && XRPassthroughController.Instance != null)
        {
            XRPassthroughController.Instance.PassthroughEnabled = isInsightPassthroughEnabled;
        }
    }

    private void OnDisable()
    {
        if (!_mainMenu)
        {
            return;
        }
        ProfileManager.Instance.activeProfileUpdated.RemoveListener(CheckOnProfileChange);
        SettingsManager.CachedBoolSettingChanged.RemoveListener(UpdateFromSettingChange);
    }

    private void UpdateFromSettingChange(string settingName, bool value)
    {
        if (!_mainMenu)
        {
            return;
        }
        if (string.Equals(SettingsManager.PassthroughInMenu, settingName))
        {
            isInsightPassthroughEnabled = value;
            _animator.SetBool(AnimatorPassthrough, value);

            if (XRPassthroughController.Instance != null)
            {
                XRPassthroughController.Instance.PassthroughEnabled = isInsightPassthroughEnabled;
            }
        }
    }

    private void CheckOnProfileChange()
    {
        if (!_mainMenu)
        {
            return;
        }
        isInsightPassthroughEnabled = SettingsManager.GetCachedBool(SettingsManager.PassthroughInMenu, false);
        _animator.SetBool(AnimatorPassthrough, isInsightPassthroughEnabled);

        if (XRPassthroughController.Instance != null)
        {
            XRPassthroughController.Instance.PassthroughEnabled = isInsightPassthroughEnabled;
        }
    }
}