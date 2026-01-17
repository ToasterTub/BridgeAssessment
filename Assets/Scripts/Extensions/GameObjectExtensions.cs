using UnityEngine;

namespace Extensions
{
    public static class GameObjectExtensions
    {
        public static void DestroyAnywhere(this GameObject go)
        {
            if (go && go != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(go);
                }
                else
                {
                    Object.DestroyImmediate(go);
                }
            }
        }
    }
}