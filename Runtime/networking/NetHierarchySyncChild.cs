using jb5n;
using UnityEngine;

namespace j5bn {
	public class NetHierarchySyncChild : MonoBehaviour {
		private string hierarchyPath;
		private NetHierarchySyncParent syncParent;

		public string GetGameObjectPath() {
			GameObject obj = gameObject;
			string path = "/" + obj.name;
			while (obj.transform.parent != null) {
				obj = obj.transform.parent.gameObject;
				path = "/" + obj.name + path;
			}
			return path;
		}

		void Awake() {
			if (!transform.root.TryGetComponent<NetHierarchySyncParent>(out syncParent)) {
				Debug.LogError($"{gameObject.name} has sync child component but no sync parent component at root {transform.root.name}!");
				return;
			}

			hierarchyPath = GetGameObjectPath();

			syncParent.RegisterChild(this);
		}

		public string GetHierarchyPath() {
			return hierarchyPath;
		}

		public uint GetNetId() {
			if (syncParent == null) {
				Debug.LogError($"Cannot get netId for {gameObject.name}: No sync parent found");
				return uint.MaxValue;
			}

			// Find our index in the parent's children list
			uint childIndex = syncParent.GetIndexOfChild(this);
			if (childIndex == uint.MaxValue) {
				Debug.LogError($"Cannot get netId for {gameObject.name}: Not registered in parent's children list");
				return uint.MaxValue;
			}

			// Encode using arithmetic encoding: offset = parentNetId * maxChildCount + childIndex
			uint offset = syncParent.netId * NetworkUtils.maxChildCount + childIndex;
			return NetworkUtils.maxNormalNetId + offset;
		}
	}
}