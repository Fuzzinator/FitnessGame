using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerProportions : MonoBehaviour
{
    private const string RESETHEADSET = "Reset Headset";

    // Start is called before the first frame update
    private void Start()
    {
        var height = GlobalSettings.UserHeight;
        if (height < 0)
        {
            height = Head.Instance.transform.position.y;
            
            GlobalSettings.UserHeight = height;
        }
    }

    /*private void OnEnable()
    {
        //InputManager.Instance.MainInput[RESETHEADSET].performed += ResetHeadset;
    }*/

    /*private void OnDisable()
    {
        if (InputManager.Instance == null)
        {
            return;
        }

        //InputManager.Instance.MainInput[RESETHEADSET].performed -= ResetHeadset;
    }*/

    private async UniTaskVoid ResetHeadsetWithDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy(),
            ignoreTimeScale: false);
        ResetHeadset(new InputAction.CallbackContext());
    }

    private void ResetHeadset(InputAction.CallbackContext context)
    {
        GlobalSettings.UserHeight = Head.Instance.transform.position.y;
    }
}