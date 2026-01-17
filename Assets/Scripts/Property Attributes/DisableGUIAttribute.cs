using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PropertyAttributes
{
    public class DisableGUIAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableGUIAttribute))]
    public class DisableGUIDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool holdEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = holdEnabled;
        }
    }
#endif
}