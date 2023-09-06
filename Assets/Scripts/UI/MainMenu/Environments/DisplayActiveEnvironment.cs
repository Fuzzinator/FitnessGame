using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
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
    private Image _environmentImage;
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
        CustomEnvironmentsController.ClearCustomEnvironmentInfo();
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

            _environmentName.SetTextZeroAlloc(ActiveCustomEnvironment.EnvironmentName, true);
            _skyboxName.SetTextZeroAlloc(skyboxName, true);
            _skyboxDepthName.SetTextZeroAlloc(skyboxDepthName, true);
            _skyboxBrightness.SetTextZeroAlloc(ActiveCustomEnvironment.SkyboxBrightness, true);
            SetImageAsync(ActiveCustomEnvironment.SkyboxPath).Forget();
        }
        else
        {
            _environmentName.ClearText();
            _environmentImage.sprite = null;
            _environmentImage.enabled = false;
        }
    }

    private async UniTaskVoid SetImageAsync(string imagePath)
    {
        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(ActiveCustomEnvironment.SkyboxPath))
        {
            sprite = await CustomEnvironmentsController.GetEnvironmentImageAsync(imagePath, _cancellationToken);
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
        _availableEnvironmentsUIController.EditEnvironment(_activeCustomEnvironment);
    }

    public void TryDeleteEnvironment()
    {
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
