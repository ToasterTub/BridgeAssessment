using System;
using System.Collections.Generic;
using Buildables;
using Buildables.Interfaces;
using UnityEngine;

namespace UI.EditMode
{
    public class EditModeController : MonoBehaviour
    {
        [SerializeField] EditModePositionHandle positionHandlePrefab;
        [SerializeField] Camera worldCamera;
        
        Dictionary<Transform, EditModePositionHandle> _registeredHandles = new();
        Plane _groundPlane;
        IEditable _currentEditing;
        IEditable _hovering;
        
        public void RegisterPositionHandle(Transform t, Action<Vector3> onHandleMoved, Action onFailed, Func<bool> isValid)
        {
            var handle = Instantiate(positionHandlePrefab, transform);
            if (_registeredHandles.TryAdd(t, handle))
            {
                handle.Setup(v => OnHandleMoved(v, onHandleMoved),
                    onFailed,
                    isValid,
                    () => GetHandleWorldPosition(handle));
                return;
            }
            
            Destroy(handle.gameObject);
            
            Debug.LogError($"Handle is already registered for: {t?.gameObject.name}", t);
        }
        
        public void RemovePositionHandle(Transform t)
        {
            if (_registeredHandles.TryGetValue(t, out var handle))
            {
                Destroy(handle.gameObject);
                _registeredHandles.Remove(t);
            }
        }

        void Awake()
        {
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void OnDisable()
        {
            ClearEdit();
        }

        void ClearEdit()
        {
            if (_currentEditing != null)
            {
                _currentEditing.EndEdit();
                _currentEditing = null;
            }
        }

        void LateUpdate()
        {
            UpdateHandles();
            
            var ray = worldCamera.ScreenPointToRay(Input.mousePosition);

            void TryClearHover()
            {
                if (_hovering != null)
                {
                    _hovering.EditModeHoverExit();
                    _hovering = null;
                }
            }

            IEditable hitEditable = null;
            
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform.gameObject.TryGetComponent<EditableCollider>(out var editableCollider))
                {
                    editableCollider.TryGetEditable(out hitEditable);
                }
            }
            
            if (hitEditable != null)
            {
                if (hitEditable != _currentEditing)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ClearEdit();
                        TryClearHover();
                        hitEditable.StartEdit(this);
                        _currentEditing = hitEditable;
                        return;
                    }
                    
                    if (_hovering != null && _hovering != hitEditable)
                    {
                        TryClearHover();
                    }
                            
                    _hovering = hitEditable;
                    _hovering.EditModeHoverUpdate();
                    return;
                }
            }
            
            TryClearHover();
        }

        void UpdateHandles()
        {
            foreach (var pair in _registeredHandles)
            {
                pair.Value.transform.position = RectTransformUtility.WorldToScreenPoint(worldCamera, pair.Key.position);
            }
        }

        Vector3 GetHandleWorldPosition(EditModePositionHandle handle)
        {
            var screenRay = RectTransformUtility.ScreenPointToRay(worldCamera, handle.transform.position);
            if (_groundPlane.Raycast(screenRay, out float distance))
            {
                return screenRay.origin + (screenRay.direction * distance);
            }
            
            Debug.LogError("Ground Plane intersection failed!");
            return default;
        }

        void OnHandleMoved(Vector3 worldPosition, Action<Vector3> onHandleMoved)
        {
            onHandleMoved?.Invoke(worldPosition);
        }
    }
}