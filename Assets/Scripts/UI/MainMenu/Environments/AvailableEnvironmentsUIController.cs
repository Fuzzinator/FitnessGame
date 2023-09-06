using UnityEngine;
using UI.Scrollers;
using Cysharp.Threading.Tasks;

public class AvailableEnvironmentsUIController : MonoBehaviour
{
    [SerializeField]
    private CustomEnvironmentCreator _environmentCreator;
    [SerializeField]
    private DisplayActiveEnvironment _environmentDisplay;
    [SerializeField]
    private int _environmentCreatorPage;

    [SerializeField]
    private AvailableCustomEnvironmentsScrollerController _scrollerController;

    private void OnEnable()
    {
        DisplayAvailable().Forget();
    }

    private void OnDisable()
    {
        CustomEnvironmentsController.ClearCustomEnvironmentInfo();
    }

    public void StartCreateNewEnvironment()
    {
        MainMenuUIController.Instance.SetActivePage(_environmentCreatorPage);
        _environmentCreator.StartCreateNewEnvironment();
    }

    public void EditEnvironment(CustomEnvironment environment)
    {
        MainMenuUIController.Instance.SetActivePage(_environmentCreatorPage);
        _environmentCreator.StartEditEnvironment(environment);
    }

    public void CompleteEditEnvironment(CustomEnvironment environment)
    {
        _environmentDisplay.SetActiveCustomEnvironment(environment);
        _environmentDisplay.UpdateDisplay();
    }

    public void RequestUpdateDisplay()
    {
        DisplayAvailable().Forget();
    }
    private async UniTaskVoid DisplayAvailable()
    {
        CustomEnvironmentsController.GetAvailableCustomEnvironments();
        await CustomEnvironmentsController.LoadCustomEnvironments();
        _scrollerController.Refresh();
    }
}
