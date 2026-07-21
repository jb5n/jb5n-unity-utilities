using Mirror;
using UnityEngine;
using j5bn;

namespace jb5n {
	public class NetworkUtils {
		// If a net ID is >= this value, it indicates that it's a child object under a parent with a NetworkIdentity
		public static uint maxNormalNetId = 2147483647;
		// Maximum number of children per parent object
		public static uint maxChildCount = 100;

		public static GameObject FindGameObjectByNetId(uint netId) {
			if (netId == uint.MaxValue) {
				return null;
			}
			if (netId >= maxNormalNetId) {
				// This is a child object under a parent with a NetworkIdentity
				// Decode the netId using arithmetic encoding
				uint offset = netId - maxNormalNetId;
				uint parentNetId = offset / maxChildCount;
				uint childIndex = offset % maxChildCount;

				if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parentIdentity)) {
					if (parentIdentity.TryGetComponent<NetHierarchySyncParent>(out var syncParent)) {
						NetHierarchySyncChild child = syncParent.GetChildAtIndex(childIndex);
						return child != null ? child.gameObject : null;
					}
				}
				return null;
			}
			else {
				// This is a normal NetworkIdentity object
				if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity)) {
					return identity.gameObject;
				}
				return null;
			}
		}

		// General-purpose utility that can either look up the object's NetworkIdentity or construct one from a NetHierarchySyncChild
		public static uint GetGameObjectNetId(GameObject obj, bool suppressError = false) {
			if (obj != null) {
				if (obj.TryGetComponent<NetworkIdentity>(out var identity)) {
					return identity.netId;
				}

				if (obj.TryGetComponent<NetHierarchySyncChild>(out var syncChild)) {
					return syncChild.GetNetId();
				}
			}

			if (!suppressError) {
				Debug.LogError("Failed to get netId for object " + obj.name);
			}
			return uint.MaxValue;
		}
	}
}
