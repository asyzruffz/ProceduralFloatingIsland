﻿using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;

		// Check if the propery we want to draw should be enabled
		bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

		// Enable/disable the property
		bool wasEnabled = GUI.enabled;
		GUI.enabled = enabled;

		// Check if we should draw the property
		if (!condHAtt.HideInInspector || enabled)
		{
			EditorGUI.PropertyField(position, property, label, true);
		}

		//Ensure that the next property that is being drawn uses the correct settings
		GUI.enabled = wasEnabled;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
		bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

		if (!condHAtt.HideInInspector || enabled) {
			return EditorGUI.GetPropertyHeight(property, label);
		} else {
			//The property is not being drawn
			//We want to undo the spacing added before and after the property
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}

	private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property) {
		bool enabled = true;

		//Look for the sourcefield within the object that the property belongs to
		SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
		if (sourcePropertyValue != null) {
			enabled = condHAtt.InvertBool ? !sourcePropertyValue.boolValue : sourcePropertyValue.boolValue;
		} else {
			Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
		}

		return enabled;
	}
}
