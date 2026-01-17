using UnityEngine;

namespace Data_Types
{
    [System.Serializable]
    public struct LocalTransformState
    {
        [SerializeField] Vector3 localPosition;
        [SerializeField] Vector3 localEulerAngles;
        [SerializeField] Vector3 localScale;
        
        public LocalTransformState(Transform transform)
        {
            localPosition = transform.localPosition;
            localEulerAngles = transform.localEulerAngles;
            localScale = transform.localScale;
        }

        public void Apply(Transform transform)
        {
            transform.localPosition = localPosition;
            transform.localEulerAngles = localEulerAngles;
            transform.localScale = localScale;
        }
    }
}