using System.Collections.Generic;
using j5bn;
using Mirror;
using UnityEngine;

namespace jb5n {
	public class NetHierarchySyncParent : NetworkBehaviour {

		private List<NetHierarchySyncChild> syncedChildren = new();

		public void RegisterChild(NetHierarchySyncChild child) {
			if (!syncedChildren.Contains(child)) {
				syncedChildren.Add(child);
				if (syncedChildren.Count > NetworkUtils.maxChildCount) {
					Debug.LogError($"NetHierarchySyncParent {gameObject.name} has exceeded the maximum number of synced children ({NetworkUtils.maxChildCount})!");
				}
			}
			syncedChildren.Sort((a, b) => a.GetHierarchyPath().CompareTo(b.GetHierarchyPath()));
		}

		public NetHierarchySyncChild GetChildAtIndex(uint index) {
			if (index >= syncedChildren.Count) {
				return null;
			}
			return syncedChildren[(int)index];
		}

		public uint GetIndexOfChild(NetHierarchySyncChild child) {
			// Returns uint.MaxValue if not found
			return (uint)syncedChildren.IndexOf(child);
		}
	}
}
