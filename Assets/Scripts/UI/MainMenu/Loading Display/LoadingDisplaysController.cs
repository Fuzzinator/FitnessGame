using UnityEngine;

public class LoadingDisplaysController : MonoBehaviour
{
    [SerializeField]
    private LoadingDisplay _displayPrefab;
    
    private PoolManager _poolManager;
    
    // Start is called before the first frame update
    void Start()
    {
        _poolManager = new PoolManager(_displayPrefab, transform);
    }

    public LoadingDisplay DisplayNewLoading(string message)
    {
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
}
