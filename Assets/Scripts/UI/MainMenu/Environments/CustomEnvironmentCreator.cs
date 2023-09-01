using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Notification;

public class CustomEnvironmentCreator : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _environmentNameField;
    [SerializeField]
    private TextMeshProUGUI _skyboxColorNameField;
    [SerializeField]
    private Image _skyboxColorImage;
    [SerializeField]
    private TextMeshProUGUI _skyboxDepthNameField;
    [SerializeField]
    private Image _skyboxDepthImage;
    [SerializeField]
    private Slider _skyboxBrightnessSlider;
    [SerializeField]
    private TextMeshProUGUI _brightnessLevelDisplay;
    [SerializeField]
    private CanvasGroup _myCanvasController;

    [SerializeField]
    private SkyboxTextureController _skyboxTextureController;

    private CustomEnvironment _activeEnvironment;
    private CancellationToken _cancellationToken;
    private bool _tokenCreated = false;

    private string _environmentName;
    private string _skyboxPath;
    private string _skyboxName;
    private string _skyboxDepthName;
    private string _skyboxDepthPath;
    private float _environmentBrightness;

    private Sprite _skyboxColor;
    private Sprite _skyboxDepth;

    private void OnEnable()
    {
        if (!_tokenCreated)
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            _tokenCreated = true;
        }
    }

    private void OnDisable()
    {
        ClearFields();
    }

    public void StartCreateNewEnvironment()
    {
        _activeEnvironment = null;
    }

    public void StartEditEnvironment(CustomEnvironment environment)
    {
        _activeEnvironment = environment;
        _environmentNameField.SetTextWithoutNotify(environment.EnvironmentName);
        _skyboxBrightnessSlider.SetValueWithoutNotify(environment.SkyboxBrightness);
        SetEnvironmentName(environment.EnvironmentName);
        if (environment.SkyboxSprite != null)
        {
            SetSkyboxTexture(environment.SkyboxName, environment.SkyboxPath, false);
            _skyboxColor = environment.SkyboxSprite;
            SetSprite(_skyboxColorImage, _skyboxColor);
        }
        else
        {
            SetSkyboxTexture(environment.SkyboxName, environment.SkyboxPath, true);
        }
        if (environment.DepthSprite != null)
        {
            SetSkyboxDepthTexture(environment.SkyboxDepthName, environment.SkyboxDepthPath, false);
            _skyboxDepth = environment.DepthSprite;
            SetSprite(_skyboxDepthImage, _skyboxDepth);
        }
        else
        {
            SetSkyboxDepthTexture(environment.SkyboxDepthName, environment.SkyboxDepthPath, true);
        }

        SetSkyboxBrightness(environment.SkyboxBrightness);
    }

    public void StartEditSkyboxColor()
    {
        _myCanvasController.SetGroupState(1, false);
        _skyboxTextureController.StartEditSkyboxTexture();
    }

    public void StartEditSkyboxDepth()
    {
        _myCanvasController.SetGroupState(1, false);
        _skyboxTextureController.StartEditSkyboxDepthTexture();
    }

    public void SetEnvironmentName(string envName)
    {
        _environmentName = envName;
    }

    public void SetSkyboxTexture(string skyboxName, string texture, bool loadSprite)
    {
        _skyboxName = texture.Substring(texture.LastIndexOf("\\") + 1);
        _skyboxColorNameField.SetTextZeroAlloc(_skyboxName, true);
        _skyboxPath = texture;

        if (loadSprite)
        {
            SetColorSprite(skyboxName).Forget();
        }
    }

    public void SetSkyboxTexture(string skyboxName, string texture, Sprite thumbnail)
    {
        _skyboxName = texture.Substring(texture.LastIndexOf("/") + 1);
        _skyboxColorNameField.SetTextZeroAlloc(_skyboxName, true);
        _skyboxPath = texture;
        _skyboxColor = thumbnail;
        _skyboxColorImage.sprite = thumbnail;
        _skyboxColorImage.enabled = thumbnail != null;
    }

    public void SetSkyboxDepthTexture(string depthName, string texture, bool loadSprite)
    {
        _skyboxDepthName = texture.Substring(texture.LastIndexOf("\\") + 1);
        _skyboxDepthNameField.SetTextZeroAlloc(_skyboxDepthName, true);
        _skyboxDepthPath = texture;

        if (loadSprite)
        {
            SetDepthSprite(texture).Forget();
        }
    }
    public void SetSkyboxDepthTexture(string depthName, string texture, Sprite thumbnail)
    {
        _skyboxDepthName = texture.Substring(texture.LastIndexOf("\\") + 1);
        _skyboxDepthNameField.SetTextZeroAlloc(_skyboxDepthName, true);
        _skyboxDepthPath = texture;
        _skyboxDepth = thumbnail;
        _skyboxDepthImage.sprite = thumbnail;
        _skyboxDepthImage.enabled = thumbnail != null;
    }

    public void SetSkyboxBrightness(float brightness)
    {
        _environmentBrightness = brightness;
        _brightnessLevelDisplay.SetTextZeroAlloc(brightness, true);
    }

    public void UpdateEnvironment()
    {
        var editing = _activeEnvironment != null;
        if (!editing)
        {
            _activeEnvironment = CustomEnvironmentsController.CreateCustomEnvironment(_environmentName, _skyboxPath, _environmentBrightness);

            CustomEnvironmentsController.AddNewEnvironment(_activeEnvironment);
        }
        else
        {
            _activeEnvironment.SetName(_environmentName);
            _activeEnvironment.SetSkyboxName(_skyboxName);
            _activeEnvironment.SetSkyboxPath(_skyboxPath);
            _activeEnvironment.SetSkyboxBrightness(_environmentBrightness);
        }

        _activeEnvironment.SetSkyboxSprite(_skyboxColor);
        _activeEnvironment.SetDepthSprite(_skyboxDepth);

        TrySaveEnvironment(_activeEnvironment, editing).Forget();
    }

    public async UniTaskVoid TrySaveEnvironment(CustomEnvironment customEnvironment, bool overwrite)
    {
        var canSave = await CustomEnvironmentsController.TrySaveEnvironment(customEnvironment, overwrite);
        if (!canSave)
        {
            var display = new NotificationVisuals($"Environment with name of {customEnvironment.EnvironmentName} already exists.", "Failed to save", autoTimeOutTime: 2f);
            NotificationManager.RequestNotification(display);
        }
    }

    public void TryGoBack()
    {
        MainMenuUIController.Instance.SetActivePage(9);
    }

    public void GoBack()
    {
        MainMenuUIController.Instance.SetActivePage(9);
    }

    private async UniTaskVoid SetColorSprite(string texture)
    {
        _skyboxColor = await SetSkyboxTextureAsync(texture, _skyboxColorImage);
        _skyboxColorImage.sprite = _skyboxColor;
    }

    private async UniTaskVoid SetDepthSprite(string texture)
    {
        _skyboxDepth = await SetSkyboxTextureAsync(texture, _skyboxColorImage);
        _skyboxDepthImage.sprite = _skyboxDepth;
    }

    private async UniTask<Sprite> SetSkyboxTextureAsync(string texture, Image targetImage)
    {
        var sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(texture, _cancellationToken);
        SetSprite(targetImage, sprite);
        return sprite;
    }

    private void SetSprite(Image targetImage, Sprite sprite)
    {
        if (sprite == null)
        {
            targetImage.enabled = false;
        }
        else
        {
            targetImage.enabled = true;
            targetImage.sprite = sprite;
        }
    }

    private void ClearFields()
    {
        _activeEnvironment = null;

        _environmentNameField.ClearText(false);
        _skyboxColorNameField.ClearText();
        _skyboxDepthNameField.ClearText();
        _skyboxColorImage.sprite = null;
        _skyboxColorImage.enabled = false;
        _skyboxDepthImage.sprite = null;
        _skyboxDepthImage.enabled = false;
        _skyboxBrightnessSlider.value = 1;

        _environmentName = null;
        _skyboxPath = null;
        _skyboxName = null;
        _environmentBrightness = 1;

        _skyboxColor = null;
        _skyboxDepth = null;
    }

    public void CompleteSetSkybox()
    {
        _myCanvasController.SetGroupState(true);
    }

    public void CheckSkyboxDeleted(string skyboxName, bool depth)
    {
        if(string.IsNullOrWhiteSpace(skyboxName))
        {
            return;
        }

        if(depth)
        {
            if(skyboxName.Equals(_skyboxDepthName))
            {
                SetSkyboxDepthTexture(string.Empty, string.Empty, null);
            }
        }
        else
        {
            if (skyboxName.Equals(_skyboxName))
            {
                SetSkyboxTexture(string.Empty, string.Empty, null);
            }
        }
    }
}
