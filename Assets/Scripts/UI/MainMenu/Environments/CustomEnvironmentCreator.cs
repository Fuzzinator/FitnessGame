using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private NewEnvGloveSetter _newEnvGloveSetter;
    [SerializeField]
    private NewEnvTargetSetter _newEnvTargetSetter;
    [SerializeField]
    private NewEnvObstacleSetter _newEnvObstacleSetter;

    [SerializeField]
    private SkyboxTextureController _skyboxTextureController;
    [SerializeField]
    private AvailableEnvironmentsUIController _availableEnvironmentsController;

    private CustomEnvironment _activeEnvironment;
    private CancellationToken _cancellationToken;
    private bool _tokenCreated = false;

    private string _environmentName;
    private string _skyboxPath;
    private string _skyboxName;
    private string _skyboxDepthName;
    private string _skyboxDepthPath;
    private float _environmentBrightness;

    public EnvAssetReference GlovesRef { get; private set; }
    public EnvAssetReference TargetsRef { get; private set; }
    public EnvAssetReference ObstaclesRef { get; private set; }

    private string _originalEnvironmentName;

    private Sprite _skyboxColor;
    private Sprite _skyboxDepth;

    private void OnEnable()
    {
        if (!_tokenCreated)
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            _tokenCreated = true;
        }
        CustomEnvironmentsController.CustomEnvironmentsUpdated.AddListener(RefreshDisplay);
    }

    private void OnDisable()
    {
        ClearFields();
        CustomEnvironmentsController.CustomEnvironmentsUpdated.RemoveListener(RefreshDisplay);
    }

    public void StartCreateNewEnvironment()
    {
        _activeEnvironment = null;
        _originalEnvironmentName = null;
        _environmentNameField.text = null;
        _skyboxBrightnessSlider.value = 0;
        _skyboxBrightnessSlider.value = 1;
    }

    public void StartEditEnvironment(CustomEnvironment environment)
    {
        _activeEnvironment = environment;
        _originalEnvironmentName = environment.EnvironmentName;
        RefreshDisplay();
        SetEnvironmentName(environment.EnvironmentName);
        SetSkyboxBrightness(environment.SkyboxBrightness);
        SetGloves(environment.Gloves);
        SetTargets(environment.Targets);
        SetObstacles(environment.Obstacles);
        _newEnvGloveSetter.TrySetText(environment.Gloves?.AssetName);
        _newEnvTargetSetter.TrySetText(environment.Targets?.AssetName);
        _newEnvObstacleSetter.TrySetText(environment?.Obstacles?.AssetName);
    }

    public void RefreshDisplay()
    {
        if (_activeEnvironment == null)
        {
            return;
        }

        _environmentNameField.SetTextWithoutNotify(_activeEnvironment.EnvironmentName);
        _skyboxBrightnessSlider.SetValueWithoutNotify(_activeEnvironment.SkyboxBrightness);
        if (_activeEnvironment.SkyboxSprite != null)
        {
            SetSprite(_skyboxColorImage, _activeEnvironment.SkyboxSprite);
            SetSkyboxTexture(_activeEnvironment.SkyboxName, _activeEnvironment.SkyboxPath, false);
            _skyboxColor = _activeEnvironment.SkyboxSprite;
        }
        else
        {
            SetSkyboxTexture(_activeEnvironment.SkyboxName, _activeEnvironment.SkyboxPath, true);
            SetSprite(_skyboxColorImage, null);
        }

        if (_activeEnvironment.DepthSprite != null)
        {
            SetSprite(_skyboxDepthImage, _activeEnvironment.DepthSprite);
            SetSkyboxDepthTexture(_activeEnvironment.SkyboxDepthName, _activeEnvironment.SkyboxDepthPath, false);
            _skyboxDepth = _activeEnvironment.DepthSprite;
        }
        else
        {
            SetSkyboxDepthTexture(_activeEnvironment.SkyboxDepthName, _activeEnvironment.SkyboxDepthPath, true);
            SetSprite(_skyboxDepthImage, null);
        }
    }
    public void RenameSkybox(string skyboxName)
    {
        var skyboxPath = _skyboxPath.Substring(0, skyboxName.LastIndexOf("/") + 1);
        var name = skyboxName.Substring(skyboxName.LastIndexOf("/") + 1);
        _skyboxName = name;
        _skyboxColorNameField.SetTextZeroAlloc(_skyboxName, true);
        _skyboxPath = skyboxPath;
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

    public void SetSkyboxTexture(string skyboxName, string skyboxPath, bool loadSprite)
    {
        _skyboxName = skyboxName.Substring(skyboxName.LastIndexOf("/") + 1);
        _skyboxColorNameField.SetTextZeroAlloc(_skyboxName, true);
        _skyboxPath = skyboxPath;

        if (loadSprite)
        {
            SetColorSprite(skyboxName, skyboxPath).Forget();
        }
    }

    public void SetSkyboxTexture(string skyboxName, string skyboxPath, Sprite thumbnail)
    {
        var pathIsBlank = string.IsNullOrWhiteSpace(skyboxPath);
        var name = pathIsBlank ? null : skyboxName.Substring(skyboxName.LastIndexOf("/") + 1);
        _skyboxName = name;
        _skyboxColorNameField.SetTextZeroAlloc(_skyboxName, true);
        _skyboxPath = skyboxPath;
        _skyboxColor = thumbnail;
        _skyboxColorImage.sprite = thumbnail;
        _skyboxColorImage.enabled = thumbnail != null;
    }

    public void SetSkyboxDepthTexture(string depthName, string skyboxPath, bool loadSprite)
    {
        var pathIsBlank = string.IsNullOrWhiteSpace(skyboxPath);
        var name = pathIsBlank ? null : depthName.Substring(depthName.LastIndexOf("/") + 1);
        _skyboxDepthName = name;
        _skyboxDepthNameField.SetTextZeroAlloc(_skyboxDepthName, true);
        _skyboxDepthPath = skyboxPath;

        if (loadSprite)
        {
            SetDepthSprite(depthName, skyboxPath).Forget();
        }
    }
    public void SetSkyboxDepthTexture(string depthName, string texture, Sprite thumbnail)
    {
        _skyboxDepthName = texture.Substring(texture.LastIndexOf("/") + 1);
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

    public void SetGloves(EnvAssetReference gloves)
    {
        if (gloves == null || string.IsNullOrWhiteSpace(gloves.AssetName))
        {
            return;
        }
        GlovesRef = gloves;
    }

    public void SetTargets(EnvAssetReference targets)
    {
        if (targets == null || string.IsNullOrWhiteSpace(targets.AssetName))
        {
            return;
        }
        TargetsRef = targets;
    }

    public void SetObstacles(EnvAssetReference obstacles)
    {
        if (obstacles == null || string.IsNullOrWhiteSpace(obstacles.AssetName))
        {
            return;
        }
        ObstaclesRef = obstacles;
    }

    public void UpdateEnvironment()
    {
        var editing = _activeEnvironment != null;
        var originalName = _environmentName;
        if (!editing)
        {
            if (string.IsNullOrWhiteSpace(_environmentName))
            {
                _environmentName = $"{DateTime.Now:yyyy-MM-dd} - {DateTime.Now:hh-mm}";
            }
            _activeEnvironment = CustomEnvironmentsController.CreateCustomEnvironment(_environmentName, _skyboxPath, _environmentBrightness, GlovesRef, TargetsRef, ObstaclesRef);

            CustomEnvironmentsController.AddNewEnvironment(_activeEnvironment);
        }
        else
        {
            originalName = _activeEnvironment.EnvironmentName;
            _activeEnvironment.SetName(_environmentName);
            _activeEnvironment.SetSkyboxName(_skyboxName);
            _activeEnvironment.SetSkyboxPath(_skyboxPath);
            _activeEnvironment.SetSkyboxBrightness(_environmentBrightness);
            _activeEnvironment.SetGloves(GlovesRef);
            _activeEnvironment.SetTargets(TargetsRef);
            _activeEnvironment.SetObstacles(ObstaclesRef);

        }
        EnvironmentControlManager.Instance.UpdateEnvironment(originalName, _activeEnvironment);

        _activeEnvironment.SetSkyboxSprite(_skyboxColor);
        _activeEnvironment.SetDepthSprite(_skyboxDepth);


        TrySaveEnvironment(_activeEnvironment, editing, editing).Forget();

        if (editing)
        {
            _availableEnvironmentsController.CompleteEditEnvironment(_activeEnvironment);
            if (!string.IsNullOrWhiteSpace(_originalEnvironmentName) && !_originalEnvironmentName.Equals(_environmentName))
            {

                CustomEnvironmentsController.TryDeleteEnvironment(_originalEnvironmentName);
            }
        }
    }

    public async UniTaskVoid TrySaveEnvironment(CustomEnvironment customEnvironment, bool overwrite, bool updating)
    {
        var canSave = await CustomEnvironmentsController.TrySaveEnvironment(customEnvironment, overwrite, updating);
        if (!canSave)
        {
            var display = new NotificationVisuals($"Environment with name of {customEnvironment.EnvironmentName} already exists.", "Failed to save", autoTimeOutTime: 2f);
            NotificationManager.RequestNotification(display);
            return;
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

    private async UniTaskVoid SetColorSprite(string skyboxName, string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath))
        {
            return;
        }

        _skyboxColor = await SetSkyboxTextureAsync(skyboxName, texturePath, _skyboxColorImage);
        _skyboxColorImage.sprite = _skyboxColor;
    }

    private async UniTaskVoid SetDepthSprite(string skyboxName, string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath))
        {
            return;
        }

        _skyboxDepth = await SetSkyboxTextureAsync(skyboxName, texturePath, _skyboxColorImage);
        _skyboxDepthImage.sprite = _skyboxDepth;
    }

    private async UniTask<Sprite> SetSkyboxTextureAsync(string skyboxName, string texturePath, Image targetImage)
    {
        if (string.IsNullOrWhiteSpace(texturePath))
        {
            return null;
        }
        var sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(skyboxName, texturePath, _cancellationToken);
        SetSprite(targetImage, sprite);
        return sprite;
    }

    private void SetSprite(Image targetImage, Sprite sprite)
    {
        if (sprite == null)
        {
            targetImage.enabled = false;
            targetImage.sprite = null;
        }
        else
        {
            targetImage.sprite = sprite;
            targetImage.enabled = true;
        }
    }

    private void ClearFields()
    {
        _activeEnvironment = null;

        _environmentNameField.SetTextWithoutNotify(null);
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
        if (string.IsNullOrWhiteSpace(skyboxName))
        {
            return;
        }

        if (depth)
        {
            if (skyboxName.Equals(_skyboxDepthName))
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
