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
        _environmentDisplay.SetActiveCustomEnvironment(-1);
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

    private async UniTaskVoid DisplayAvailable()
    {
        await CustomEnvironmentsController.LoadCustomEnvironments();
        _scrollerController.Refresh();
    }
}
