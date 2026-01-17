using Buildables;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Buildable Preset", menuName = "ScriptableObjects/Buildable/Preset", order = 0)]
    public class BuildablePreset : ScriptableObject
    {
        [SerializeField] BuildableHandlerBase buildableHandlerPrefab;
        public BuildableHandlerBase BuildableHandlerPrefab => buildableHandlerPrefab;
    }
}