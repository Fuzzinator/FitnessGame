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
    }

    private void OnEnable()
    {
        ActiveCustomEnvironment = null;
    }

    private void OnDisable()
    {
        CustomEnvironmentsController.ClearCustomEnvironmentInfo();
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

    private void UpdateDisplay()
    {
        if(ActiveCustomEnvironment != null)
        {
            _environmentName.SetTextZeroAlloc(ActiveCustomEnvironment.EnvironmentName, true);
            _skyboxName.SetTextZeroAlloc(ActiveCustomEnvironment.SkyboxName, true);
            _skyboxDepthName.SetTextZeroAlloc(ActiveCustomEnvironment.SkyboxDepthName, true);
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
        var sprite = await CustomEnvironmentsController.GetEnvironmentImageAsync(imagePath, _cancellationToken);

        ActiveCustomEnvironment.SetSkyboxSprite(sprite);
        _environmentImage.sprite = sprite;
        _environmentImage.enabled = sprite != null;
    }
}
