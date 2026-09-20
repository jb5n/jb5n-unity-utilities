#if UNITY_EDITOR
using UnityEditor;
using VInspector;

namespace jb5n {
	// WeightedList<T> is a SerializedDictionary<T, float>; register vInspector's dictionary
	// drawer for it explicitly so it draws as a single value/weight table regardless of how
	// Unity resolves useForChildren across open generic types.
	[CustomPropertyDrawer(typeof(WeightedList<>))]
	public class WeightedListDrawer : SerializedDictionaryDrawer { }
}
#endif
