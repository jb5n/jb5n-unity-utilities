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
}