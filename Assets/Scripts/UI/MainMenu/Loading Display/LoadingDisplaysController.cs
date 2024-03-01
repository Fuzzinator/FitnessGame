using UnityEngine;

public class LoadingDisplaysController : MonoBehaviour
{
    [SerializeField]
    private LoadingDisplay _displayPrefab;
    
    private PoolManager _poolManager;

    private bool _initialized = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if(_initialized)
        {
            return;
        }
        Initialize();
    }

    private void Initialize()
    {
        _poolManager = new PoolManager(_displayPrefab, transform);
        _initialized = true;
    }

    public LoadingDisplay DisplayNewLoading(string message)
    {
        if (!_initialized)
        {
            Initialize();
        }

        var display = _poolManager.GetNewPoolable() as LoadingDisplay;
        if (display == null)
        {
            return null;
        }
        display.SetUp(message);
        
        var displayTransform = display.transform;
        displayTransform.SetParent(transform);
        displayTransform.localPosition = Vector3.zero;
        displayTransform.localScale = Vector3.one;
        displayTransform.localEulerAngles = Vector3.zero;
        
        return display;
    }

    public void CancelAll(bool skipAwait = false)
    {
        foreach (LoadingDisplay item in _poolManager.ActiveObjs)
        {
            item.DisplayFailedAsync(skipAwait).Forget();
        }
    }
}
