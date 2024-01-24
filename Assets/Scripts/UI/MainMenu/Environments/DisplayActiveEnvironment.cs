using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class DisplayActiveEnvironment : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _environmentName;
    [SerializeField]
    private TextMeshProUGUI _skyboxName;
    [SerializeField]
    private TextMeshProUGUI _skyboxDepthName;
    [SerializeField]
    private TextMeshProUGUI _skyboxBrightness;
    [SerializeField]
    private TextMeshProUGUI _glovesName;
    [SerializeField]
    private TextMeshProUGUI _targetsName;
    [SerializeField]
    private TextMeshProUGUI _obstaclesName;
    [SerializeField]
    private Image _environmentImage;
    [SerializeField]
    private MultiGraphicButton _editButton;
    [SerializeField]
    private Button _deleteButton;
    [SerializeField]
    private AvailableEnvironmentsUIController _availableEnvironmentsUIController;

    private CancellationToken _cancellationToken;

    private CustomEnvironment _activeCustomEnvironment;
    private CustomEnvironment ActiveCustomEnvironment
    {
        get => _activeCustomEnvironment;
        set
        {
            _activeCustomEnvironment = value;
            UpdateDisplay();
        }
    }

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        ActiveCustomEnvironment = null;
    }

    private void OnDisable()
    {
        ActiveCustomEnvironment = null;
    }

    public void SetActiveCustomEnvironment(int index)
    {
        if (index >= 0 && index < CustomEnvironmentsController.CustomEnvironmentCount)
        {
            ActiveCustomEnvironment = CustomEnvironmentsController.GetCustomEnvironment(index);
        }
        else
        {
            ActiveCustomEnvironment = null;
        }
    }

    public void SetActiveCustomEnvironment(CustomEnvironment environment)
    {
        ActiveCustomEnvironment = environment;
    }

    public void UpdateDisplay()
    {
        if (ActiveCustomEnvironment != null)
        {
            var skyboxName = ActiveCustomEnvironment.SkyboxName;
            if (skyboxName != null)
            {
                skyboxName = skyboxName.Substring(skyboxName.LastIndexOf("/") + 1);
            }

            var skyboxDepthName = ActiveCustomEnvironment.SkyboxDepthName;
            if (skyboxDepthName != null)
            {
                skyboxDepthName = skyboxDepthName.Substring(skyboxDepthName.LastIndexOf("/") + 1);
            }

            _environmentName.text = ActiveCustomEnvironment.EnvironmentName;
            _skyboxName.text = skyboxName;
            _skyboxDepthName.text = skyboxDepthName;
            _glovesName.text = ActiveCustomEnvironment.GlovesName;
            _targetsName.text = ActiveCustomEnvironment.TargetsName;
            _obstaclesName.text = ActiveCustomEnvironment.ObstaclesName;
            _skyboxBrightness.SetTextZeroAlloc(ActiveCustomEnvironment.SkyboxBrightness, true);

            SetImageAsync(ActiveCustomEnvironment.SkyboxName, ActiveCustomEnvironment.SkyboxPath).Forget();
            _editButton.interactable = true;
            _deleteButton.interactable = true;
        }
        else
        {
            _environmentName.ClearText();
            _skyboxName.ClearText();
            _glovesName.ClearText();
            _targetsName.ClearText();
            _obstaclesName.ClearText();
            _environmentImage.sprite = null;
            _environmentImage.enabled = false;
            _editButton.interactable = false;
            _deleteButton.interactable = false;
        }
    }

    private async UniTaskVoid SetImageAsync(string skyboxName, string skyboxPath)
    {
        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(ActiveCustomEnvironment.SkyboxPath))
        {
            sprite = await CustomEnvironmentsController.GetEnvironmentThumbnailAsync(skyboxName, skyboxPath, _cancellationToken);
        }
        if (ActiveCustomEnvironment == null)
        {
            return;
        }
        ActiveCustomEnvironment.SetSkyboxSprite(sprite);
        _environmentImage.sprite = sprite;
        _environmentImage.enabled = sprite != null;
    }

    public void EditEnvironment()
    {
        if(_activeCustomEnvironment == null)
        {
            return;
        }
        _availableEnvironmentsUIController.EditEnvironment(_activeCustomEnvironment);
    }

    public void TryDeleteEnvironment()
    {
        if (_activeCustomEnvironment == null)
        {
            return;
        }
        var visuals = CustomEnvironmentsController.ConfirmDeleteEnvironment;
        NotificationManager.RequestNotification(visuals, () => ConfirmDeleteEnvironment());
    }

    public void ConfirmDeleteEnvironment()
    {
        var deleted = CustomEnvironmentsController.TryDeleteEnvironment(_activeCustomEnvironment);
        ActiveCustomEnvironment = null;
        _availableEnvironmentsUIController.RequestUpdateDisplay();
    }
}
