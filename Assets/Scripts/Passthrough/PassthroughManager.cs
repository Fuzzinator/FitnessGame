using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    public static PassthroughManager Instance { get; private set; }

    [Tooltip("Specify if Insight Passthrough should be enabled. Passthrough layers can only be used if passthrough is enabled.")]
    public bool isInsightPassthroughEnabled = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if(XRPassthroughController.Instance != null)
        {
            XRPassthroughController.Instance.PassthroughEnabled = isInsightPassthroughEnabled;
        }
    }
}
