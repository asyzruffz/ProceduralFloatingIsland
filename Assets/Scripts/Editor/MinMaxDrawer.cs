using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (MinMaxAttribute))]
class MinMaxDrawer : PropertyDrawer {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		if (property.propertyType == SerializedPropertyType.Vector2) {
			Vector2 range = property.vector2Value;

			label = EditorGUI.BeginProperty(position, label, property);

			Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            contentPosition.width /= 2;// 0.5f;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 30f;

            EditorGUI.BeginChangeCheck ();
            range.x = EditorGUI.FloatField (contentPosition, "Min", range.x);
            if (EditorGUI.EndChangeCheck ()) {
                range.y = Mathf.Max (range.x, range.y);
            }

            contentPosition.x += contentPosition.width;

            EditorGUI.BeginChangeCheck ();
            range.y = EditorGUI.FloatField (contentPosition, "Max", range.y);
            if (EditorGUI.EndChangeCheck ()) {
                range.x = Mathf.Min (range.x, range.y);
            }

            property.vector2Value = range;
            EditorGUI.EndProperty();

		} else {
			EditorGUI.LabelField (position, label, "Use only with Vector2");
		}
	}
}
