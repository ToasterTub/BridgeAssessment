using Extensions;
using UnityEngine;
using UnityEngine.Animations;

namespace BridgePlacement
{
    public class BridgeDragRing : MonoBehaviour
    {
        [SerializeField] GameObject ringObj;
        [SerializeField] float pulseSpeed = 1f;
        [SerializeField] float pulseScale = .1f;
        Camera _mainCamera;

        void OnEnable()
        {
            _mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            Vector3 camDirection = (transform.position - _mainCamera.transform.position).SetAxis(Axis.Y, 0).normalized;
            transform.forward = camDirection;

            float unsignedPulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            ringObj.transform.localScale = Vector3.one * (1f + unsignedPulse * pulseScale);
        }
    }
}