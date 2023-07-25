using UnityEngine;

public class BaseOptimalHitIndicator : MonoBehaviour, IInitializer
{
    [SerializeField]
    private BaseTarget _baseTarget;

    [SerializeField]
    private Renderer _renderer;

    [SerializeField]
    private string _propertyName;

    private int _propertyHash;

    public void Initialize(BaseTarget target)
    {
        if (_renderer == null)
        {
            return;
        }
        _baseTarget = target;
        _propertyHash = Shader.PropertyToID(_propertyName);
        _renderer.material.SetVector(_propertyHash, Vector3.zero);

        OnEnable();
    }

    public void Initialize(BaseObstacle obstacle) { }


    private void OnEnable()
    {
        if (_propertyHash == 0)
        {
            return;
        }

        _renderer.material.SetVector(_propertyHash, _baseTarget.OptimalHitPoint);
    }

    private void OnDestroy()
    {
        if (_renderer == null || _renderer.material == null)
        {
            return;
        }

        Destroy(_renderer.material);
    }
}