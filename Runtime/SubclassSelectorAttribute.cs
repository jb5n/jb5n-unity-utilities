using System;
using UnityEngine;

namespace jb5n {
	/// <summary>
	/// Put this on a <c>[SerializeReference]</c> field (including a List or array of
	/// such a field) to get an inspector dropdown that lets you pick any concrete
	/// subclass of the field's declared type and edit its serialized fields inline.
	///
	/// The target type (and its subclasses) must be plain <c>[Serializable]</c>
	/// classes -- NOT deriving from UnityEngine.Object -- and each selectable type
	/// needs a public parameterless constructor.
	///
	/// <code>
	/// [SerializeReference, SubclassSelector] List&lt;A&gt; entries;
	/// </code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SubclassSelectorAttribute : PropertyAttribute {
		/// <summary>Whether the dropdown offers a "(null)" choice to clear the entry.</summary>
		public bool IncludeNull { get; }

		public SubclassSelectorAttribute(bool includeNull = true) {
			IncludeNull = includeNull;
		}
	}
}
