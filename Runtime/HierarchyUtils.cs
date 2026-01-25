using UnityEngine;

public static class HierarchyUtils {
	public static Transform FindParentWithTag(this Transform childTransform, string tag) {
		Transform currentTransform = childTransform.transform;

		// Loop while there is a parent
		while (currentTransform.parent != null) {
			// Move to the parent's transform
			currentTransform = currentTransform.parent;

			// Check if the current parent has the target tag
			if (currentTransform.CompareTag(tag)) {
				return currentTransform; // Found the parent with the tag
			}
		}

		return null; // No parent with the specified tag was found
	}

	public static void SetLayerRecursively(this GameObject obj, string layerName) {
		// Convert the layer name to its integer ID
		int newLayer = LayerMask.NameToLayer(layerName);
		if (newLayer == -1) {
			Debug.LogError("Layer name \"" + layerName + "\" does not exist.");
			return;
		}

		// Set the layer for the current object
		obj.layer = newLayer;

		// Iterate through all child transforms and call the function recursively
		foreach (Transform child in obj.transform) {
			SetLayerRecursively(child.gameObject, layerName);
		}
	}
}