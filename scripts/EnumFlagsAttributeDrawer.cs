using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace jb5n {
	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	public class EnumFlagsAttributeDrawer : PropertyDrawer {
		public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
			_property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
		}
	}
}

#endif