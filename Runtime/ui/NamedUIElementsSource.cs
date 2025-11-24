using UnityEngine;

// This component has a serialized dictionary of UI elements with string keys for easy lookup by name.
public class NamedUIElementsSource : MonoBehaviour {
	public SerializedMultimap<string, GameObject> namedUIElements = new();

	public T GetNamedElement<T>(string name) where T : Component {
		if (namedUIElements.TryGetValue(name, out GameObject element)) {
			return element.GetComponent<T>();
		}
		return null;
	}
}
