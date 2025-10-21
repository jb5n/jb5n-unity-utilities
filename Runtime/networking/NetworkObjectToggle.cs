using System.Collections.Generic;
using Mirror;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class NetworkObjectToggle : NetworkBehaviour {
	public enum ToggleMode {
		NonServerOnly, // Will only be enabled if NOT on the server
		ServerOnly, // Will only be enabled if on the server
		LocalPlayerOnly, // Will only be enabled for the local player
		RemotePlayerOnly // Will only be enabled for clients/server that are NOT local player
	}

	public SerializedMultimap<GameObject, ToggleMode> objectsToToggle;
	public SerializedMultimap<MonoBehaviour, ToggleMode> componentsToToggle;

	public override void OnStartClient() {
		base.OnStartClient();
		UpdateToggles();
	}

	public override void OnStartServer() {
		base.OnStartServer();
		UpdateToggles();
	}

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
		UpdateToggles();
	}

	private void UpdateToggles() {
		foreach (KeyValuePair<GameObject, ToggleMode> kv in objectsToToggle) {
			GameObject obj = kv.Key;
			ToggleMode mode = kv.Value;

			switch (mode) {
				case ToggleMode.NonServerOnly:
					obj.SetActive(!isServer);
					break;
				case ToggleMode.ServerOnly:
					obj.SetActive(isServer);
					break;
				case ToggleMode.LocalPlayerOnly:
					obj.SetActive(isLocalPlayer);
					break;
				case ToggleMode.RemotePlayerOnly:
					obj.SetActive(!isLocalPlayer);
					break;
			}
		}

		foreach (KeyValuePair<MonoBehaviour, ToggleMode> kv in componentsToToggle) {
			MonoBehaviour comp = kv.Key;
			ToggleMode mode = kv.Value;

			switch (mode) {
				case ToggleMode.NonServerOnly:
					comp.enabled = !isServer;
					break;
				case ToggleMode.ServerOnly:
					comp.enabled = isServer;
					break;
				case ToggleMode.LocalPlayerOnly:
					comp.enabled = isLocalPlayer;
					break;
				case ToggleMode.RemotePlayerOnly:
					comp.enabled = !isLocalPlayer;
					break;
			}
		}
	}
}
