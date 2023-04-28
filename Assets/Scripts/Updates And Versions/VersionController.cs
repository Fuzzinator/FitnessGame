using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class VersionController : MonoBehaviour
{
    public static VersionController Instance { get; private set; }
    [field: SerializeField] 
    public string CurrentVersion { get; private set; }

    [field: SerializeField]
    public UpdateDescriptionObject[] VersionDescriptions { get; private set; }
    public UpdateDescriptionObject MostRecentUpdate => VersionDescriptions?[^1];

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

        if(MainMenuUIController.Instance.ActivePage != 0)
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
        if(activePage != 0)
        {
            return;
        }
        WaitForMainMenuReturn().Forget();
    }
}
