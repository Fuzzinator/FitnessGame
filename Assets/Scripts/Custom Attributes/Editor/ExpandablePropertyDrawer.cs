
using UI.Scrollers;
using UnityEngine;

namespace CustomAttributes.Editor
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(Expandable))]
    public class ExpandablePropertyDrawer : PropertyDrawer
    {
        private Editor editor = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            {
                if (property.objectReferenceValue is ScrollerController controller)
                {
                    label.text = $"{label.text}:{controller.scriptIdentifier}";
                }

                // Draw label
                EditorGUI.PropertyField(position, property, label, true);
                // Draw foldout arrow
                /*if (property.objectReferenceValue != null)
                {
                    property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
                }

                // Draw foldout properties
                if (property.isExpanded)
                {
                    // Make child fields be indented
                    EditorGUI.indentLevel++;

                    // Draw object properties
                    //if (editor == null)
                    //{
                    //    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
                    //}
                    //if (editor != null)
                    //{
                        //editor.OnInspectorGUI();
                        //editor.serializedObject.ApplyModifiedProperties();
                    //}

                    // Set indent back to what it was
                    EditorGUI.indentLevel--;
                }*/
            }
        }
    }
}
