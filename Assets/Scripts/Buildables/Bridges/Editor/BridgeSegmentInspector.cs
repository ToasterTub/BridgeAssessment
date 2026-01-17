using Data_Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace BridgePlacement
{
    [CustomEditor(typeof(BridgeSegment))]
    public class BridgeSegmentInspector : Editor
    {
        bool _showLengthOptions;
        BridgeSegment Segment => target as BridgeSegment;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            LengthOptionsArea();
        }

        void LengthOptionsArea()
        {
            _showLengthOptions = EditorGUILayout.Foldout(_showLengthOptions, "Size Setup", true);
            
            if (_showLengthOptions)
            {
                EditorGUILayout.HelpBox(
                    "These buttons detect the size of this object on the given axis using child renderers.", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
            
                SetLengthFromMeshRendererAxisButton(Axis.X);
                SetLengthFromMeshRendererAxisButton(Axis.Y);
                SetLengthFromMeshRendererAxisButton(Axis.Z);
            
                EditorGUILayout.EndHorizontal();   
            }
        }

        void SetLengthFromMeshRendererAxisButton(Axis axis)
        {
            if (GUILayout.Button($"{axis}", GUILayout.Width(24)))
            {
                ApplyRendererBoundsAxis(axis);
            }
        }

        void ApplyRendererBoundsAxis(Axis axis)
        {
            var parent = Segment.gameObject;

            var holdTransformState = new LocalTransformState(parent.transform);
            parent.transform.eulerAngles = Vector3.zero;
            
            var renderers = parent.GetComponentsInChildren<Renderer>();

            if (renderers.Length < 1)
            {
                Debug.LogWarning($"{parent.name} has no child renderers for size detection!", parent);
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
            holdTransformState.Apply(parent.transform);

            switch (axis)
            {
                case Axis.X: UpdateSizeProperties(axis, bounds.size.x); break;
                case Axis.Y: UpdateSizeProperties(axis, bounds.size.y); break;
                case Axis.Z: UpdateSizeProperties(axis, bounds.size.z); break;
            }
        }

        void UpdateSizeProperties(Axis axis, float length)
        {
            UpdateTransformProperties(axis, length);
            UpdateLengthProperty();
        }

        void UpdateLengthProperty()
        {
            if (!ValidateTargetTransforms(out var startObj, out var endObj))
            {
                return;
            }
            var lengthProperty = serializedObject.FindProperty("length");
            lengthProperty.floatValue = (startObj.transform.position - endObj.transform.position).magnitude;
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateTransformProperties(Axis axis, float length)
        {
            if (!ValidateTargetTransforms(out var startObj, out var endObj))
            {
                return;
            }
            
            UpdateTransformPosition(startObj, false);
            UpdateTransformPosition(endObj, true); 
            
            void UpdateTransformPosition(Transform transform, bool flip)
            {
                Vector3 localPosition = Vector3.zero;
                
                float distance = (length / 2f) * (flip ? -1 : 1);
                
                switch (axis)
                {
                    case Axis.X: localPosition.x = distance; 
                        break;
                    case Axis.Y: localPosition.y = distance;
                        break;
                    case Axis.Z: localPosition.z = distance;
                        break;
                }

                transform.localPosition = localPosition;
            }
        }

        bool ValidateTargetTransforms(out Transform startPoint, out Transform endPoint)
        {
            var startProperty = serializedObject.FindProperty("startPoint");
            var endProperty = serializedObject.FindProperty("endPoint");
            
            ValidateTransformProperty(startProperty, "StartPoint");
            ValidateTransformProperty(endProperty, "EndPoint");
            
            startPoint = startProperty.objectReferenceValue as Transform;
            endPoint = endProperty.objectReferenceValue as Transform;

            return startPoint && startPoint != null && endPoint && endPoint != null;
        }
        
        void ValidateTransformProperty(SerializedProperty targetProperty, string targetName)
        {
            if (targetProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            if (targetProperty.objectReferenceValue == null || !targetProperty.objectReferenceValue)
            {
                var newTarget = new GameObject(targetName);
                newTarget.transform.SetParent(Segment.transform);
                targetProperty.objectReferenceValue = newTarget;
                
                Undo.RegisterCreatedObjectUndo(newTarget, $"Created Bridge Segment Target: {targetName}");
                serializedObject.ApplyModifiedProperties();
            }
        }

        void OnSceneGUI()
        {
            if (!_showLengthOptions)
            {
                return;
            }
            
            ValidateTargetTransforms(out var startPoint, out var endPoint);

            ControlTransform(startPoint);
            ControlTransform(endPoint);
        }

        void ControlTransform(Transform transform)
        {
            if (transform == null)
            {
                return;
            }
            
            Vector3 position = transform.position;
            Vector3 newPosition = Handles.PositionHandle(position, Quaternion.identity);
            if (newPosition != position)
            {
                Undo.RecordObject(transform, "Moving target");
                transform.position = newPosition;
                UpdateLengthProperty();
            }
        }
    }
}