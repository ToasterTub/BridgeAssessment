using BridgePlacement;
using Extensions;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bridge Style", menuName = "ScriptableObjects/Styles/Buildings/Bridge", order = 0)]
    public class BridgeStyle : ScriptableObject
    {
        [SerializeField] BridgeSegment start;
        [SerializeField] BridgeSegment middle;
        [SerializeField] BridgeSegment extension;
        [SerializeField] BridgeSegment end;

        public bool HasAnySegment() => start.IsValid() || middle.IsValid() || extension.IsValid() || end.IsValid();

        public BridgeSegment StartPrefab => start;
        public BridgeSegment MiddlePrefab => middle;
        public BridgeSegment ExtensionPrefab => extension;
        public BridgeSegment EndPrefab => end;
    }
}