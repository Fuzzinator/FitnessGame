using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class PermissionsRequester : MonoBehaviour
{
    public static PermissionsRequester Instance { get; private set; }

    [field: SerializeField]
    public UnityEvent<bool> PermissionsUpdated { get; private set; }

    private bool _hasReadPermissions;
    private bool _hasWritePermissions;

    public static bool HasRequestedPermissions { get; private set; }

    private bool _isRequestingReadPermissions;
    private bool _isRequestingWritePermissions;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _isRequestingReadPermissions = false;
        _isRequestingWritePermissions = false;
        HasRequestedPermissions = false;
    }

    public async UniTask<bool> HasReadAndWritePermissions()
    {
        var hasPermissions = await GetReadWritePermissions();
        return hasPermissions;
    }

    public async UniTask<bool> GetReadWritePermissions()
    {
        if (HasRequestedPermissions)
        {
            await UniTask.NextFrame();
            var hasRead = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
            var hasWrite = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
            return hasRead && hasWrite;
        }
        if (!_isRequestingReadPermissions)
        {
            RequestWritePermissions();
            RequestReadPermissions();
        }
        await UniTask.WaitWhile(() => _isRequestingReadPermissions && _isRequestingWritePermissions);

        return _hasReadPermissions && _hasWritePermissions;
    }

    private void RequestReadPermissions()
    {
#if UNITY_ANDROID
        var readPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        if (!readPermission)
        {
            _isRequestingReadPermissions = true;

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += ReadPermissionsDenied;
            callbacks.PermissionGranted += ReadPermissionAllowed;
            callbacks.PermissionDeniedAndDontAskAgain += ReadPermissionsDenied;

            Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
        }
        else
        {
            ReadPermissionAllowed(null);
        }
#else
        ReadPermissionAllowed(null);
#endif
    }

    private void RequestWritePermissions()
    {
#if UNITY_ANDROID
        var writePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
        if (!writePermission)
        {
            _isRequestingWritePermissions = true;

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += WritePermissionsDenied;
            callbacks.PermissionGranted += WritePermissionAllowed;
            callbacks.PermissionDeniedAndDontAskAgain += WritePermissionsDenied;

            Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);
        }
        else
        {
            WritePermissionAllowed(null);
        }
#else
        WritePermissionAllowed(null);
#endif
    }

    private void ReadPermissionAllowed(string permissionName)
    {
        _isRequestingReadPermissions = false;
        _hasReadPermissions = true;
        HasRequestedPermissions = true;
    }

    private void ReadPermissionsDenied(string permissionName)
    {
        _isRequestingReadPermissions = false;
        _hasReadPermissions = false;
        HasRequestedPermissions = true;
    }

    private void WritePermissionAllowed(string permissionName)
    {
        _isRequestingWritePermissions = false;
        _hasWritePermissions = true;
        HasRequestedPermissions = true;
    }

    private void WritePermissionsDenied(string permissionName)
    {
        _isRequestingWritePermissions = false;
        _hasWritePermissions = false;
        HasRequestedPermissions = true;
    }

    private async UniTaskVoid CheckPermissions()
    {
        if(await HasReadAndWritePermissions())
        {
            return;
        }
        var visuals = new Notification.NotificationVisuals("Shadow BoXR needs storage permissions to support custom songs. Please close Shadow BoXR and enable storage permissions in your settings under App Permissions.", "Can't Access Local Data", "Close Game", "Ignore");

        NotificationManager.RequestNotification(visuals, Application.Quit, () => Debug.LogWarning("User did not give permissions cannot access custom files"));
    }
}
