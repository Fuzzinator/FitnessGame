using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YUR.Core;

public class YUR_CameraSetter : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas;
    void Start()
    {
        if (YURHMD.Instance != null && YURHMD.Instance.TryGetComponent(out Camera cam))
        {
            _canvas.worldCamera = cam;
        }
    }
}
