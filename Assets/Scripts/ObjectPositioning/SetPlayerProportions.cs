using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerProportions : MonoBehaviour
{
    [SerializeField]
    private Transform _head;


    private const string RESETHEADSET = "Reset Headset";
    // Start is called before the first frame update
    private void Start()
    {
#pragma warning disable 4014
        ResetHeadsetWithDelay();
#pragma warning restore 4014
    }

    private void OnEnable()
    {
        InputManager.Instance.MainInput[RESETHEADSET].performed += ResetHeadset;
    }

    private void OnDisable()
    {
        InputManager.Instance.MainInput[RESETHEADSET].performed -= ResetHeadset;
    }

    private async UniTaskVoid ResetHeadsetWithDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy(),
            ignoreTimeScale: false);
        ResetHeadset(new InputAction.CallbackContext());
    }
    
    private void ResetHeadset(InputAction.CallbackContext context)
    {
        GlobalSettings.UserHeight = _head.position.y;
    }
}
