using Oculus.Platform;
using System.Reflection;
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class TriggerableOVRCanvasOverlay : OVROverlayCanvas
{
    [SerializeField]
    private bool _updateRequested = false;

    [SerializeField]
    private Vector3 _cameraPositionOffset = new Vector3(0, 0, 0);

    [SerializeField]
    private float _nearClipPlane = .99f;

    [SerializeField]
    private float _farClipPlane = 1.01f;

    [SerializeField]
    private Vector3 _meshRendererOffsetPosition = new Vector3(0, 0, 0);

    [SerializeField]
    private Vector3 _meshRendererRotation = new Vector3(0, 0, 0);

    [SerializeField]
    private Camera _renderingCamera;

    [SerializeField]
    private MeshRenderer _imposterMeshRenderer;

    [SerializeField]
    private bool _showCameraInEditor = false;

    [SerializeField]
    private bool _showMeshRendererInEditor = false;

    private void OnValidate()
    {
        if (_renderingCamera == null)
        {
            _renderingCamera = (Camera)typeof(OVROverlayCanvas).GetField("_camera", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }

        if (_imposterMeshRenderer == null)
        {
            _imposterMeshRenderer = (MeshRenderer)typeof(OVROverlayCanvas).GetField("_meshRenderer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }

        var hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy;

        if (_renderingCamera != null)
        {
            _renderingCamera.gameObject.hideFlags = _showCameraInEditor ? HideFlags.None : hideFlags;


            _renderingCamera.transform.position = transform.position - _renderingCamera.transform.forward + _cameraPositionOffset;
            _renderingCamera.nearClipPlane = _nearClipPlane;
            _renderingCamera.farClipPlane = _farClipPlane;
        }
        if(_imposterMeshRenderer != null)
        {
            _imposterMeshRenderer.gameObject.hideFlags = _showMeshRendererInEditor ? HideFlags.None : hideFlags;
        }
    }

    private void LateUpdate()
    {
        if (_imposterMeshRenderer != null)
        {
            if (shape == CanvasShape.Flat)
            {
                _imposterMeshRenderer.transform.localPosition = _meshRendererOffsetPosition;
            }
            else
            {
                _imposterMeshRenderer.transform.localPosition = new Vector3(0, 0, -curveRadius / transform.lossyScale.z);
            }

            _imposterMeshRenderer.transform.rotation = Quaternion.Euler(_meshRendererRotation) * transform.rotation;
        }
    }

    public void RequestFrameUpdate()
    {
        _updateRequested = true;
    }

    protected override bool ShouldRender()
    {
        var shouldRender = _updateRequested || base.ShouldRender();
        _updateRequested = false;
        return shouldRender;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TriggerableOVRCanvasOverlay))]
public class TriggerableOVRCanvasOverlayEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TriggerableOVRCanvasOverlay myScript = (TriggerableOVRCanvasOverlay)target;
        if (GUILayout.Button("Request Frame Update"))
        {
            myScript.RequestFrameUpdate();
        }
    }
}
#endif