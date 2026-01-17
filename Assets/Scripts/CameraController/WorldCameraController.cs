using Extensions;
using UnityEngine;
using UnityEngine.Animations;

namespace CameraController
{
    public class WorldCameraController : MonoBehaviour
    {
        [SerializeField] Transform cameraRoot;
        [SerializeField] new Camera camera;
        [SerializeField] float rotateSensitivity = 720;
        [SerializeField, Range(0f,1f)] float zoomSensitivity = .1f;
        [SerializeField] float zoomMagnitudeMin = 10;
        [SerializeField] float zoomMagnitudeMax = 50;
        Plane _groundPlane;
        Vector3 _lastGroundPosition;
        Vector3 _lastMouseInputPosition;
        Vector3 _rotatePivot;

        float _zoomVelocity = 0f;
        float _zoomTarget = 0f;
        float _zoomCurrent = 0f;

        void Awake()
        {
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            _zoomTarget = camera.transform.localPosition.magnitude;
            _zoomCurrent = _zoomTarget;
        }

        void Update()
        {
            RotAndPan();
            Zoom();
        }

        void Zoom()
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                _zoomTarget += _zoomTarget * -Input.mouseScrollDelta.y * zoomSensitivity;
                _zoomTarget = Mathf.Clamp(_zoomTarget, zoomMagnitudeMin, zoomMagnitudeMax);
            }

            _zoomCurrent = Mathf.SmoothDamp(_zoomCurrent, _zoomTarget, ref _zoomVelocity, .1f);
            
            camera.transform.localPosition = camera.transform.localPosition.normalized * _zoomCurrent;
        }

        void RotAndPan()
        {
            Vector3 groundPosition = GetGroundPosition();
            
            if (Input.GetMouseButton(2))
            {
                Pan(_lastGroundPosition - groundPosition);
                UpdatePositions();
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                _rotatePivot = groundPosition;
            }

            if (Input.GetMouseButton(1))
            {
                Rotate(_lastMouseInputPosition - Input.mousePosition);
            }

            UpdatePositions();
        }

        void UpdatePositions()
        {
            _lastGroundPosition = GetGroundPosition();
            _lastMouseInputPosition = Input.mousePosition;
        }

        Vector3 GetGroundPosition()
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out var rayLength))
            {
                return ray.origin + (ray.direction * rayLength);
            }

            return _lastGroundPosition;
        }

        void Rotate(Vector3 mouseDelta)
        {
            cameraRoot.transform.RotateAround(_rotatePivot, Vector3.up, -(mouseDelta.x * rotateSensitivity)/Screen.width);
        }

        void Pan(Vector3 worldDelta)
        {
            cameraRoot.transform.position += worldDelta.SetAxis(Axis.Y, 0);
        }
    }
}