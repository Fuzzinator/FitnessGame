using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using YUR.Core;

namespace YUR.Interaction.Gaze
{
    public class YURGazeController : Singleton<MonoBehaviour>
    {
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
        public Vector3 rayOffset = new Vector3(0, 0.05f, 0);

        public YURGazeReticule gazeReticule;

        [SerializeField]
        private float _progressSpeed = 1;
        private float _currentProgress;

        private YURGazeTargetElement _currentGazeTarget;
        private YURGazeTargetElement _lastGazeTarget;

        private Vector3 _hitPosition;
        private Vector3 _hitNormal;
        private int _hitLinePosition;
        private bool _hitValidTarget;

        private RaycastHit? _raycastHitMesh;
        private int _raycastHitMeshIndex;
        private RaycastResult? _raycastHitUI;
        private int _raycastHitUIIndex;
        private bool _isUIHitClose;

        protected override void Awake()
        {
            base.Awake();
            GameObject headRef = FindObjectOfType<YURHMD>().gameObject;
            transform.SetParent(headRef.transform.parent);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            rayInteractor.transform.localPosition = rayOffset;

            // base.Awake();
            // // transform.SetParent(Camera.main.transform);
            // // set to null because it does not need a reference and will figure position on it's own
            // transform.SetParent(null);
            // transform.localPosition = Vector3.zero;
            // transform.localRotation = Quaternion.Euler(Vector3.zero);
            // rayInteractor.transform.localPosition = rayOffset;
        }

        private void Update()
        {
            ProcessGaze();
        }

        private void ProcessGaze()
        {
            if (!rayInteractor || !gazeReticule)
                return;
            SetReticlePosition();
            UpdateGazeTargets();
            ProcessGazeTargets();
        }

        private void ProcessGazeTargets()
        {
            if (!_currentGazeTarget && !_lastGazeTarget)
                return;

            if (_currentGazeTarget != _lastGazeTarget)
            {
                CheckCurrentGaze();

                CheckLastGaze();
            }
            else if (_currentGazeTarget.Equals(_lastGazeTarget))
            {
                CheckGazeProgress();
            }
        }

        private void CheckCurrentGaze()
        {
            if (_currentGazeTarget)
            {
                _currentGazeTarget.Enter();
                UpdateProgress(0);
            }
        }
        private void CheckLastGaze()
        {
            if (_lastGazeTarget)
            {
                _lastGazeTarget.Exit();
                UpdateProgress(0);
            }
        }

        private void CheckGazeProgress()
        {
            _currentProgress = Mathf.Clamp(_currentProgress + Time.deltaTime * _progressSpeed, 0, 1);
            UpdateProgress(_currentProgress);
            if (_currentProgress >= 1)
            {
                _currentGazeTarget.Complete();
            }
        }

        private void UpdateGazeTargets()
        {
            _lastGazeTarget = _currentGazeTarget;
            _currentGazeTarget = GetGazeTargetElement();
        }

        private void UpdateProgress(float value)
        {
            _currentProgress = value;
            if (value != 0)
                gazeReticule.SetProgress(value);
            else
                gazeReticule.ResetProgress();
        }

        private YURGazeTargetElement GetGazeTargetElement()
        {
            rayInteractor.TryGetCurrentRaycast(out _raycastHitMesh, out _raycastHitMeshIndex, out _raycastHitUI, out _raycastHitUIIndex, out _isUIHitClose);
            YURGazeTargetElement targetElement = null;

            if (_raycastHitUI.HasValue && !targetElement)
            {
                _raycastHitUI.Value.gameObject.TryGetComponent(out targetElement);
            }

            if (_raycastHitMesh.HasValue && !targetElement)
            {
                _raycastHitMesh.Value.transform.TryGetComponent(out targetElement);
            }

            return targetElement;
        }

        private void SetReticlePosition()
        {
            rayInteractor.TryGetHitInfo(out _hitPosition, out _hitNormal, out _hitLinePosition, out _hitValidTarget);
            gazeReticule.transform.position = _hitPosition;
            gazeReticule.transform.up = _hitNormal;
        }
    }
}