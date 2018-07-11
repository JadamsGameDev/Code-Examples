using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Expresion))]
public class BoolExpresionDrawer : PropertyDrawer {
	float _Height = 5;
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		float height = base.GetPropertyHeight(property, label);
		height *= _Height;
		return height;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		//_Height = 5;
		EditorGUI.BeginProperty(position, label, property);
		position.height = base.GetPropertyHeight(property, label);
		//position.y += position.height;
		SerializedProperty values = property.FindPropertyRelative("value");
		EditorGUI.PropertyField(position, values);

		position.y += position.height;
		SerializedProperty upperLimit = property.FindPropertyRelative("upperLimit");
		EditorGUI.PropertyField(position, upperLimit);

		position.y += position.height;
		SerializedProperty lowerLimit = property.FindPropertyRelative("lowerLimit");
		EditorGUI.PropertyField(position, lowerLimit);


		EditorGUI.EndProperty();
	}
}
