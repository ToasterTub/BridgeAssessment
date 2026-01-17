using UnityEngine;

namespace Buildables
{
    public abstract class BuildableHandlerBase : MonoBehaviour
    {
        public abstract void OnBuildStart(Vector3 worldPosition);
        public abstract void OnBuildEnd(Vector3 worldPosition, out bool buildIsValid);
        public abstract void OnBuildDrag(Vector3 worldPosition);
        public abstract void OnBuildCancel();
    }
}