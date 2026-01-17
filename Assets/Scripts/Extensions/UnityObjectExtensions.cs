namespace Extensions
{
    public static class UnityObjectExtensions
    {
        public static bool IsValid(this UnityEngine.Object obj) => obj && obj != null;
    }
}