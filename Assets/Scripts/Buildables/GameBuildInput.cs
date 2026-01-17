using UnityEngine;
using UnityEngine.EventSystems;

namespace Buildables
{
    public class GameBuildInput : MonoBehaviour
    {
        [SerializeField] GameBuildController controller;
        [SerializeField] Camera worldCamera;
        
        Plane _groundPlane;
        bool _inputDown = false;
        Vector3 _lastDragPosition;

        void Awake()
        {
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        bool IsCursorOverUI()
        {
            if (EventSystem.current != null)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }

            return false;
        }
        
        void Update()
        {
            if (!controller.BuildModeEnabled)
            {
                return;
            }

            UpdateInput();
        }

        void UpdateInput()
        {
            var cursorRay = worldCamera.ScreenPointToRay(Input.mousePosition);
            bool cursorOverUI = IsCursorOverUI();

            if (_groundPlane.Raycast(cursorRay, out float distance))
            {
                Vector3 worldPosition = cursorRay.origin + (distance * cursorRay.direction);

                if (Input.GetMouseButtonDown(0) && !cursorOverUI)
                {
                    _inputDown = true;
                    controller.BuildInputStart(worldPosition);
                    _lastDragPosition = worldPosition;
                    
                    return;
                }

                if (_inputDown && !Input.GetMouseButton(0))
                {
                    controller.BuildInputDrag(worldPosition);
                    controller.BuildInputEnd(worldPosition);
                    _inputDown = false;
                    return;
                }
                
                if (_inputDown && _lastDragPosition != worldPosition)
                {
                    _lastDragPosition = worldPosition;
                    controller.BuildInputDrag(worldPosition);
                }
            }
        }
    }
}