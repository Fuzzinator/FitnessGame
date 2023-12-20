using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnDisableEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onDisable = new UnityEvent();

    private void OnDisable()
    {
        _onDisable.Invoke();
    }
}
