using System;
using System.Collections.Generic;
using PropertyAttributes;
using Buildables;
using Buildables.Interfaces;
using Extensions;
using JetBrains.Annotations;
using ScriptableObjects;
using UI.EditMode;
using UnityEngine;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace BridgePlacement
{
    public class BridgeBuildHandler : BuildableHandlerBase, IEditable
    {
        [SerializeField] BridgeStyle style;
        [SerializeField] bool allowOddExtensionCounts;
        [SerializeField] float minimumBridgeLength = 1f;
        [SerializeField] EditableCollider editCollider;
        [SerializeField] float bridgeWidth = 3f;
        [SerializeField] GameObject dragNotifier;
        
        [SerializeField, DisableGUI] GameObject bridgeStart;
        [SerializeField, DisableGUI] GameObject bridgeEnd;
        [SerializeField, DisableGUI] GameObject bridgeSegmentContainer;
        
        [SerializeField, DisableGUI] BridgeBuildData currentBridgeData;
        [SerializeField, DisableGUI] BridgeBuildObjects buildObjects = new();
        
        [SerializeField, HideInInspector] List<BridgeSegment> removedSegmentCache = new();
        [SerializeField, HideInInspector] List<BridgeSegment> addedSegmentCache = new();

        bool _isEditing = false;
        bool _isValidLength = false;
        bool _isColliderValid = false;
        Action _editableDeregister = null;
        Collider[] _invalidCollisionCache = new Collider[1];
        

        [CanBeNull]
        public Transform BridgeStart => bridgeStart?.transform;
        [CanBeNull]
        public Transform BridgeEnd => bridgeEnd?.transform;

        void Awake()
        {
            ValidateControllerObjects();
            editCollider.Setup(this);
        }
        
        public void RebuildCompletely()
        {
#if UNITY_EDITOR
            if (this == null || PrefabUtility.IsPartOfPrefabAsset(gameObject) ||
                PrefabStageUtility.GetPrefabStage(gameObject) != null)
            {
                return;
            }
#endif
            ResetData();
            RefreshBridge();
        }
        
        public override void OnBuildStart(Vector3 worldPosition)
        {
            _isValidLength = false;
            
            bridgeStart.transform.position = worldPosition;
            bridgeEnd.transform.position = worldPosition;
            ResetData();
            dragNotifier.gameObject.SetActive(true);
        }
        
        public override void OnBuildDrag(Vector3 worldPosition)
        {
            bridgeEnd.transform.position = worldPosition;
            
            float totalLength = (bridgeEnd.transform.position - bridgeStart.transform.position).magnitude;
            bool newValidLength = totalLength >= minimumBridgeLength;

            if (!newValidLength)
            {
                if (_isValidLength)
                {
                    dragNotifier.gameObject.SetActive(true);
                    _isValidLength = false;
                    ResetData();
                }
                
                return;
            }
            
            dragNotifier.gameObject.SetActive(false);
            _isValidLength = true;
            RefreshBridge();
        }

        public override void OnBuildEnd(Vector3 worldPosition, out bool buildIsValid)
        {
            dragNotifier.gameObject.SetActive(false);
            
            bridgeEnd.transform.position = worldPosition;
            
            float totalLength = (bridgeEnd.transform.position - bridgeStart.transform.position).magnitude;
            _isValidLength = totalLength >= minimumBridgeLength;
            buildIsValid = _isValidLength;

            if (!buildIsValid)
            {
                buildIsValid = false;
                ResetData();
                return;
            }
            
            RefreshBridge();
            
            if (!_isColliderValid)
            {
                buildIsValid = false;
                ResetData();
                return;
            }
            
            HandleCollidersOnComplete();
        }
        
        public override void OnBuildCancel()
        {
            ResetData();
            dragNotifier.gameObject.SetActive(false);
        }
        
        public void StartEdit(EditModeController editor)
        {
            if (_isEditing)
            {
                return;
            }

            _isEditing = true;
            ReturnToEditMode();
            editor.RegisterPositionHandle(bridgeStart.transform, OnBridgeStartMoved, OnFailedResetMaterials, CheckHandleValidity);
            editor.RegisterPositionHandle(bridgeEnd.transform, OnBridgeEndMoved, OnFailedResetMaterials, CheckHandleValidity);

            _editableDeregister = () =>
            {
                if (bridgeStart.IsValid())
                {
                    editor.RemovePositionHandle(bridgeStart.transform);   
                }

                if (bridgeEnd.IsValid())
                {
                    editor.RemovePositionHandle(bridgeEnd.transform);   
                }
            };
        }

        public void EndEdit()
        {
            _isEditing = false;
            _editableDeregister?.Invoke();
            _editableDeregister = null;
            HandleCollidersOnComplete();
        }

        public void EditModeHoverUpdate()
        {
            int index = 0;
            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                float signedTime = Mathf.Sin(Time.time * 5f + index * .5f);
                float scale = 1f + signedTime * .2f;
                if (segment.IsValid())
                {
                    segment.transform.localScale = Vector3.one * scale;
                }

                index++;
            }
        }

        public void EditModeHoverExit()
        {
            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                if (segment.IsValid())
                {
                    segment.transform.localScale = Vector3.one;
                }
            }
        }

        void ValidateControllerObjects()
        {
            ValidateGameObject(ref bridgeStart, "Start");
            ValidateGameObject(ref bridgeEnd, "End");
            ValidateGameObject(ref bridgeSegmentContainer, "Bridge Segment Container");
        }
        
        void ValidateGameObject(ref GameObject go, string newGoName)
        {
            if (!go.IsValid())
            {
                go = new GameObject(newGoName);
                go.transform.SetParent(transform);
                go.transform.localPosition = default;
                go.transform.localEulerAngles = default;
                go.transform.localScale = Vector3.one;
            }
        }

#if UNITY_EDITOR
        
        void OnValidate()
        {
            ValidateControllerObjects();
            EditorApplication.delayCall += RebuildCompletely;
        }
#endif

        void RefreshBridge()
        {
#if UNITY_EDITOR
            if (this == null || PrefabUtility.IsPartOfPrefabAsset(gameObject) ||
                PrefabStageUtility.GetPrefabStage(gameObject) != null)
            {
                return;
            }
#endif
            
            if (style == null || !style.HasAnySegment() || !bridgeStart.IsValid() || !bridgeEnd.IsValid())
            {
                return;
            }
            
            var newData = CollectBridgeData();
            
            
            ValidateBridgeObjects(currentBridgeData, newData);
            currentBridgeData = newData;

            ApplyBridgeObjectsLayout();
            HandleRemovalsAndAdditions();
        }

        void HandleRemovalsAndAdditions()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                foreach (BridgeSegment removeItem in removedSegmentCache)
                {
                    if (removeItem.IsValid())
                    {
                        removeItem.gameObject.DestroyAnywhere();   
                    }
                }
                removedSegmentCache.Clear();
                addedSegmentCache.Clear();
                return;
            }
#endif
            
            foreach (BridgeSegment removeItem in removedSegmentCache)
            {
                removeItem.BuildCollider.enabled = false;
                if (removeItem.IsValid())
                {
                    removeItem.StartDestroyAnimation();
                }
            }
            
            foreach (BridgeSegment addItem in addedSegmentCache)
            {
                if (addItem.IsValid())
                {
                    addItem.StartCreateAnimation();
                }
            }
            
            removedSegmentCache.Clear();
            addedSegmentCache.Clear();
        }
        
        [ContextMenu("Reset Data")]
        void ResetData()
        {
            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                if (segment.IsValid())
                {
                    segment.gameObject.DestroyAnywhere();
                }
            }

            buildObjects = new();
            currentBridgeData = new();
            removedSegmentCache.Clear();
            addedSegmentCache.Clear();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void ApplyBridgeObjectsLayout()
        {
            Vector3 origin = bridgeStart.transform.position.SetAxis(Axis.Y, 0);
            Vector3 end = bridgeEnd.transform.position.SetAxis(Axis.Y, 0);
            Vector3 direction = (end - origin).normalized;

            Vector3 currentPosition = origin;

            _isColliderValid = true;

            void AddToLayout(BridgeSegment segment)
            {
                segment.transform.right = -direction;
                Vector3 offset = direction * segment.Length;
                segment.transform.position = currentPosition + (offset / 2f);
                currentPosition += offset;
                ValidateSegmentCollision(segment);
            }

            if (style.StartPrefab.IsValid() && buildObjects.start.IsValid())
            {
                AddToLayout(buildObjects.start);
            }

            if (style.ExtensionPrefab.IsValid() && buildObjects.extensions.Count > 0)
            {
                for (int i = 0; i < buildObjects.extensions.Count / 2; i++)
                {
                    AddToLayout(buildObjects.extensions[i]);
                }
            }
            
            if (style.MiddlePrefab.IsValid() && buildObjects.middles.Count > 0)
            {
                foreach (BridgeSegment segment in buildObjects.middles)
                {
                    AddToLayout(segment);
                }
            }
            
            if (style.ExtensionPrefab.IsValid() && buildObjects.extensions.Count > 0)
            {
                for (int i = buildObjects.extensions.Count / 2; i < buildObjects.extensions.Count; i++)
                {
                    AddToLayout(buildObjects.extensions[i]);
                }
            }
            
            if (style.EndPrefab.IsValid() && buildObjects.end.IsValid())
            {
                AddToLayout(buildObjects.end);
            }
        }
        
        void ValidateSegmentCollision(BridgeSegment segment)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            var hits = Physics.OverlapBoxNonAlloc(segment.BuildCollider.bounds.center,
                segment.BuildCollider.size * .9f, _invalidCollisionCache,
                segment.BuildCollider.transform.rotation, 1 << LayerMask.NameToLayer("Default"));

            if (hits > 0)
            {
                _isColliderValid = false;
                segment.SetMaterialInvalid();
                return;
            }
            
            segment.SetMaterialValid();
        }

        void ValidateBridgeObjects(BridgeBuildData oldData, BridgeBuildData newData)
        {
            void ValidateObject(ref BridgeSegment existing, BridgeSegment prefab, bool previousValue, bool newValue)
            {
                if (previousValue == newValue) return;
                
                if (existing.IsValid())
                {
                    removedSegmentCache.Add(existing);
                    existing = null;
                }
                
                if (newValue)
                {
                    existing = Instantiate(prefab, bridgeSegmentContainer.transform);
                    addedSegmentCache.Add(existing);
                }
            }
            
            void ValidateObjectCount(List<BridgeSegment> list, BridgeSegment prefab, int newCount)
            {
                newCount = Mathf.Max(newCount, 0);

                while (list.Count > newCount)
                {
                    var toRemove = list[^1];
                    if (toRemove != null)
                    {
                        removedSegmentCache.Add(toRemove);
                    }
                    list.RemoveAt(list.Count - 1);
                }

                while (list.Count < newCount)
                {
                    var newSegment = Instantiate(prefab, bridgeSegmentContainer.transform);
                    list.Add(newSegment);
                    addedSegmentCache.Add(newSegment);
                }
            }
            
            ValidateObject(ref buildObjects.start, style.StartPrefab, oldData.hasStart, newData.hasStart);
            ValidateObject(ref buildObjects.end, style.EndPrefab, oldData.hasEnd, newData.hasEnd);
            ValidateObjectCount(buildObjects.middles, style.MiddlePrefab, newData.middleCount);
            ValidateObjectCount(buildObjects.extensions, style.ExtensionPrefab, newData.extensionCount);
        }

        BridgeBuildData CollectBridgeData()
        {
            float bridgeLength = Vector3.Distance(bridgeStart.transform.position, bridgeEnd.transform.position);
            float remainingLength = bridgeLength;

            bool hasStart = style.StartPrefab.IsValid() && remainingLength > style.StartPrefab.Length;
            remainingLength -= hasStart ? style.StartPrefab.Length : 0;
            
            bool hasEnd = style.EndPrefab.IsValid() && remainingLength > style.EndPrefab.Length;
            remainingLength -= hasEnd ? style.EndPrefab.Length : 0;
            
            int middleCount = style.MiddlePrefab.IsValid() ? 
                Mathf.FloorToInt(remainingLength/style.MiddlePrefab.Length) : 0;
            remainingLength -= middleCount > 0 ? style.MiddlePrefab.Length * middleCount : 0;
            
            int extensionCount = style.ExtensionPrefab.IsValid() ? 
                Mathf.FloorToInt(remainingLength/style.ExtensionPrefab.Length) : 0;

            if (!allowOddExtensionCounts)
            {
                if (extensionCount > 0 && extensionCount % 2 != 0)
                {
                    extensionCount--;
                }   
            }

            return new BridgeBuildData()
            {
                hasStart = hasStart,
                hasEnd = hasEnd,
                middleCount = middleCount,
                extensionCount = extensionCount,
            };
        }

        void HandleCollidersOnComplete()
        {
            if (!bridgeEnd.IsValid() || !bridgeStart.IsValid()) return;
            
            Vector3 direction = (bridgeEnd.transform.position - bridgeStart.transform.position).normalized;
            float length = Vector3.Distance(bridgeStart.transform.position, bridgeEnd.transform.position);
            editCollider.gameObject.SetActive(true);
            editCollider.transform.position = (bridgeEnd.transform.position + bridgeStart.transform.position) / 2f;
            editCollider.transform.forward = direction;
            editCollider.BoxCollider.size = new Vector3(bridgeWidth, 10f, length);
            editCollider.BoxCollider.enabled = true;

            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                segment.BuildCollider.enabled = false;
            }
        }

        void ReturnToEditMode()
        {
            editCollider.BoxCollider.enabled = false;
            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                segment.BuildCollider.enabled = true;
            }
        }

        bool CheckHandleValidity()
        {
            RefreshBridge();
            return _isColliderValid && _isValidLength;
        }

        void OnFailedResetMaterials()
        {
            foreach (BridgeSegment segment in buildObjects.AllSegments())
            {
                if (segment.IsValid())
                {
                    segment.SetMaterialValid();
                }
            }
        }

        void OnBridgeStartMoved(Vector3 worldPosition)
        {
            bridgeStart.transform.position = worldPosition.SetAxis(Axis.Y, 0);
            RefreshBridge();
        }
        
        void OnBridgeEndMoved(Vector3 worldPosition)
        {
            bridgeEnd.transform.position = worldPosition.SetAxis(Axis.Y, 0);
            RefreshBridge();
        }
        
        [System.Serializable]
        class BridgeBuildObjects
        {
            public BridgeSegment start;
            public BridgeSegment end;
            public List<BridgeSegment> middles = new();
            public List<BridgeSegment> extensions = new();

            public IEnumerable<BridgeSegment> AllSegments()
            {
                if (start.IsValid())
                {
                    yield return start;   
                }
                foreach (BridgeSegment middle in middles)
                {
                    yield return middle;
                }
                foreach (BridgeSegment extension in extensions)
                {
                    yield return extension;
                }

                if (end.IsValid())
                {
                    yield return end;   
                }
            }
        }
        
        [System.Serializable]
        class BridgeBuildData
        {
            public bool hasStart;
            public bool hasEnd;
            public int middleCount;
            public int extensionCount;
        }
    }
}