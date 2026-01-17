using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.EditMode
{
    public class EditModePositionHandle : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        Action<Vector3> _onMoved;
        Func<bool> _isValid;
        Func<Vector3> _getWorldPosition;
        Action _onFailed;
        Vector3 _startDragWorldPosition;

        bool _initialized = false;

        public void Setup(Action<Vector3> onMoved, Action onFailed, Func<bool> isValid, Func<Vector3> getWorldPosition)
        {
            _onFailed = onFailed;
            _isValid = isValid;
            _getWorldPosition = getWorldPosition;
            _onMoved = onMoved;

            _initialized = false;

            if (onMoved == null)
            {
                Debug.LogError("Position Handle Initialization failed! No onMoved Action!", this);
                return;
            }
            
            if (onFailed == null)
            {
                Debug.LogError("Position Handle Initialization failed! No onFailed Action!", this);
                return;
            }
            
            if (isValid == null)
            {
                Debug.LogError("Position Handle Initialization failed! No isValid Func!", this);
                return;
            }
            
            if (getWorldPosition == null)
            {
                Debug.LogError("Position Handle Initialization failed! No getWorldPosition Func!", this);
                return;
            }

            _initialized = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_initialized || eventData.button != PointerEventData.InputButton.Left) return;

            _startDragWorldPosition = _getWorldPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_initialized || eventData.button != PointerEventData.InputButton.Left) return;

            if (!_isValid())
            {
                _onMoved.Invoke(_startDragWorldPosition);
                _onFailed.Invoke();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_initialized || eventData.button != PointerEventData.InputButton.Left) return;
            
            transform.position += (Vector3)eventData.delta;
            _onMoved.Invoke(_getWorldPosition());  
        }
    }
}