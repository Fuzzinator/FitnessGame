using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NonVRCameraRotator : MonoBehaviour
{
    [SerializeField]
    private float _sensitivity = 5f;
    [SerializeField]
    private Transform _body;
    [SerializeField]
    private Transform _head;
    [SerializeField]
    private Camera _camera;
    private float _rotationX = 0f;
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.VRMode)
        {
            Destroy(gameObject);
            return;
        }
        _camera.transform.SetParent(_head);
        _camera.transform.localPosition = Vector3.zero;
        InputManager.Instance.MainInput["FollowCamera"].performed += StartFollowCamera;
    }

    private void OnDestroy()
    {
        if(InputManager.Instance == null)
        {
            return;
        }
        InputManager.Instance.MainInput["FollowCamera"].performed -= StartFollowCamera;
    }

    private void StartFollowCamera(InputAction.CallbackContext context)
    {
        FollowCamera().Forget();
    }

    private async UniTaskVoid FollowCamera()
    {
        while (Mouse.current.rightButton.isPressed)
        {
            var mouseDelta = Mouse.current.delta.ReadValue() * _sensitivity * Time.deltaTime;
            _body.Rotate(Vector3.up * mouseDelta.x);
            _rotationX -= mouseDelta.y;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
            _head.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            await UniTask.DelayFrame(1);
        }
    }
}
