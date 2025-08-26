using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jb5n {
	public static class LinecastExtensions {
		public static RaycastHit2D BoxLinecast(Vector2 origin, Vector2 destination, Vector2 size, LayerMask mask) {
			Vector2 heading = destination - origin;
			return Physics2D.BoxCast(origin, size, 0f, heading, heading.magnitude, mask);
		}

		public static RaycastHit2D[] BoxLinecastAll(Vector2 origin, Vector2 destination, Vector2 size, LayerMask mask) {
			Vector2 heading = destination - origin;
			return Physics2D.BoxCastAll(origin, size, 0f, heading, heading.magnitude, mask);
		}

		// Returns true if we hit a gameobject with our box linecast that isn't on our ignore list
		public static bool SelectiveBoxLinecast(Vector2 origin, Vector2 destination, Vector2 size, LayerMask mask,
			List<GameObject> toIgnore, bool searchRoot) {
			foreach (RaycastHit2D hit in BoxLinecastAll(origin, destination, size, mask)) {
				if (toIgnore.Contains(hit.collider.gameObject) || (searchRoot && toIgnore.Contains(hit.collider.transform.root.gameObject))) {
					continue;
				}
				else {
					return true;
				}
			}
			return false;
		}

		// Returns true if we hit a gameobject with our linecast that isn't on our ignore list
		public static bool SelectiveLinecast(Vector2 origin, Vector2 destination, LayerMask mask, List<GameObject> toIgnore, bool searchRoot) {
			foreach (RaycastHit2D hit in Physics2D.LinecastAll(origin, destination, mask)) {
				if (toIgnore.Contains(hit.collider.gameObject) || (searchRoot && toIgnore.Contains(hit.collider.transform.root.gameObject))) {
					continue;
				}
				else {
					return true;
				}
			}
			return false;
		}
	}
}
