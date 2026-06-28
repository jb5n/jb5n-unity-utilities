using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace jb5n {
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer {
		// baseType -> selectable concrete types, cached across the editor session.
		static readonly Dictionary<Type, List<Type>> _typeCache = new();

		public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
			if (_property.propertyType != SerializedPropertyType.ManagedReference) {
				EditorGUI.LabelField(_position, _label.text,
					"[SubclassSelector] requires a [SerializeReference] field.");
				return;
			}

			EditorGUI.BeginProperty(_position, _label, _property);

			Type currentType = GetType(_property.managedReferenceFullTypename);
			bool hasChildren = currentType != null && _property.hasVisibleChildren;

			// Header line: keep the foldout's clickable area on the label only, so it
			// doesn't swallow clicks meant for the type dropdown on the value side.
			Rect headerRect = _position;
			headerRect.height = EditorGUIUtility.singleLineHeight;

			Rect labelRect = headerRect;
			labelRect.width = EditorGUIUtility.labelWidth;
			if (hasChildren) {
				_property.isExpanded = EditorGUI.Foldout(labelRect, _property.isExpanded, _label, true);
			} else {
				EditorGUI.LabelField(labelRect, _label);
			}

			Rect dropdownRect = headerRect;
			dropdownRect.x += EditorGUIUtility.labelWidth + 2f;
			dropdownRect.width -= EditorGUIUtility.labelWidth + 2f;

			string buttonLabel = currentType == null ? "(none)" : NiceName(currentType);
			if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(buttonLabel), FocusType.Keyboard)) {
				ShowTypeMenu(_property, currentType);
			}

			// Children of the current instance, drawn manually below the header.
			if (hasChildren && _property.isExpanded) {
				EditorGUI.indentLevel++;
				Rect childRect = _position;
				childRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

				SerializedProperty end = _property.GetEndProperty();
				SerializedProperty iter = _property.Copy();
				bool enterChildren = true;
				while (iter.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iter, end)) {
					enterChildren = false;
					childRect.height = EditorGUI.GetPropertyHeight(iter, true);
					EditorGUI.PropertyField(childRect, iter, true);
					childRect.y += childRect.height + EditorGUIUtility.standardVerticalSpacing;
				}
				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
			float height = EditorGUIUtility.singleLineHeight;

			bool hasChildren = _property.propertyType == SerializedPropertyType.ManagedReference
				&& _property.hasVisibleChildren
				&& !string.IsNullOrEmpty(_property.managedReferenceFullTypename);

			if (hasChildren && _property.isExpanded) {
				SerializedProperty end = _property.GetEndProperty();
				SerializedProperty iter = _property.Copy();
				bool enterChildren = true;
				while (iter.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iter, end)) {
					enterChildren = false;
					height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(iter, true);
				}
			}

			return height;
		}

		void ShowTypeMenu(SerializedProperty _property, Type _currentType) {
			Type baseType = GetType(_property.managedReferenceFieldTypename);
			var menu = new GenericMenu();
			var attr = (SubclassSelectorAttribute)attribute;

			if (attr.IncludeNull) {
				menu.AddItem(new GUIContent("(none)"), _currentType == null, () => {
					AssignManagedReference(_property, null);
				});
				menu.AddSeparator(string.Empty);
			}

			foreach (Type type in GetSelectableTypes(baseType)) {
				Type captured = type;
				menu.AddItem(new GUIContent(NiceName(type)), type == _currentType, () => {
					AssignManagedReference(_property, Activator.CreateInstance(captured));
				});
			}

			menu.ShowAsContext();
		}

		static void AssignManagedReference(SerializedProperty _property, object _value) {
			// Re-fetch a writable copy so the change applies even from the menu callback.
			_property.serializedObject.Update();
			_property.managedReferenceValue = _value;
			_property.serializedObject.ApplyModifiedProperties();
		}

		static List<Type> GetSelectableTypes(Type _baseType) {
			if (_baseType == null) return new List<Type>();
			if (_typeCache.TryGetValue(_baseType, out List<Type> cached)) return cached;

			var types = TypeCache.GetTypesDerivedFrom(_baseType)
				.Where(IsSelectable)
				.ToList();

			if (IsSelectable(_baseType)) types.Add(_baseType);

			types = types.OrderBy(t => t.Name).ToList();
			_typeCache[_baseType] = types;
			return types;
		}

		static bool IsSelectable(Type _type) {
			return !_type.IsAbstract
				&& !_type.IsGenericTypeDefinition
				&& !typeof(UnityEngine.Object).IsAssignableFrom(_type)
				&& _type.GetConstructor(Type.EmptyTypes) != null;
		}

		static string NiceName(Type _type) {
			return ObjectNames.NicifyVariableName(_type.Name);
		}

		// Parses the "AssemblyName Namespace.TypeName" format Unity stores for
		// managed references back into a Type.
		static Type GetType(string _typename) {
			if (string.IsNullOrEmpty(_typename)) return null;
			int split = _typename.IndexOf(' ');
			if (split < 0) return null;
			string assembly = _typename.Substring(0, split);
			string fullName = _typename.Substring(split + 1);
			return Type.GetType($"{fullName}, {assembly}");
		}
	}
}

#endif // UNITY_EDITOR
