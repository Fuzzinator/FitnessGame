using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Head : MonoBehaviour
{
    [SerializeField]
    private LayerMask _layerMask;
    
    [SerializeField]
    protected UnityEvent _hitHeadEvent = new UnityEvent();

    protected void OnTriggerEnter(Collider other)
    {
        if (!IsHit(other))
        {
            return;
        }
        _hitHeadEvent?.Invoke();
    }

    protected bool IsHit(Collider col)
    {
        return _layerMask == (_layerMask.value | (1 << col.gameObject.layer));
    }
}
