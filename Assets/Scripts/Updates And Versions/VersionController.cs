using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class VersionController : MonoBehaviour
{
    public static VersionController Instance { get; private set; }
    [field: SerializeField]
    public string CurrentVersion { get; private set; }

    [field: SerializeField]
    public UpdateDescriptionObject[] VersionDescriptions { get; private set; }
    public UpdateDescriptionObject MostRecentUpdate
    {
        get
        {
            for (var i = VersionDescriptions.Length - 1; i >= 0; i--)
            {
                var description = VersionDescriptions[i];
                switch (description.TargetPlatform)
                {
                    case TargetPlatform.All:
#if UNITY_ANDROID
                case TargetPlatform.Android:
#elif UNITY_STANDALONE_WIN
                    case TargetPlatform.PCVR:
#endif
                        return description;
                    default:
                        continue;
                }
            }
            return null;
        }
    }

    [SerializeField]
    private UnityEvent _versionChanged = new UnityEvent();

    private const string Version = "CURRENT_VERSION";

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnValidate()
    {
        CurrentVersion = MostRecentUpdate?.name;
    }

    private void OnEnable()
    {
        MainMenuUIController.OnMenuPageChange.AddListener(RequestCheckVersioning);
    }

    private async UniTaskVoid WaitForMainMenuReturn()
    {
        await UniTask.DelayFrame(1);

        if (MainMenuUIController.Instance.ActivePage != 0)
        {
            return;
        }
        else
        {
            CheckVersion();
        }
    }

    private void CheckVersion()
    {
        var version = SettingsManager.GetSetting<string>(Version, null);
        if (CurrentVersion.Equals(version, System.StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        _versionChanged.Invoke();
        SettingsManager.SetSetting(Version, CurrentVersion);
    }

    private void RequestCheckVersioning(int activePage)
    {
        if (activePage != 0)
        {
            return;
        }
        WaitForMainMenuReturn().Forget();
    }
}
