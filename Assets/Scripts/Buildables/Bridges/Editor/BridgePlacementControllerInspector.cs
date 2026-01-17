using System;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace BridgePlacement
{
    [CustomEditor(typeof(BridgeBuildHandler))]
    public class BridgePlacementControllerInspector : Editor
    {
        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnUndoRedo()
        {
            var controller = target as BridgeBuildHandler;
            if (controller == null || !controller)
            {
                return;
            }
            
            controller.RebuildCompletely();
        }

        void OnSceneGUI()
        {
            var controller = target as BridgeBuildHandler;
            if (controller == null || !controller)
            {
                return;
            }

            void ControlTransform(Transform transform, ref bool needsRebuild)
            {
                if (!transform.IsValid()) return;
                Vector3 pos = transform.position;
                Vector3 newPos = Handles.PositionHandle(transform.position, Quaternion.identity);
                if (newPos != pos)
                {
                    needsRebuild = true;
                    Undo.RecordObject(transform, $"Moving Object: {transform.gameObject.name}");
                    transform.position = newPos;
                }
            }

            bool needsRebuild = false;

            if (controller.BridgeStart.IsValid())
            {
                ControlTransform(controller.BridgeStart, ref needsRebuild);
            }
            
            if (controller.BridgeEnd.IsValid())
            {
                ControlTransform(controller.BridgeEnd, ref needsRebuild);
            }

            if (needsRebuild)
            {
                controller.RebuildCompletely();
            }
        }
    }
}