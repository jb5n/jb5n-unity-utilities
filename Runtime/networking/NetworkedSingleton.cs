using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public abstract class NetworkedSingleton<T> : NetworkBehaviour where T : NetworkBehaviour {

	private static T instance = null;
	private static UnityEvent<T> onSingletonBecomesAvailable = new();

	// Override to return true if this should not prohibit multiple instances owned by different clients, but only allow a singleton as a local player instance
	protected virtual bool IsLocalPlayerSingleton() { return false; }

	public override void OnStartClient() {
		base.OnStartClient();
		if (IsLocalPlayerSingleton()) {
			return;
		}
		InitializeInstance();
	}

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
		if (!IsLocalPlayerSingleton()) {
			return;
		}
		InitializeInstance();
	}

	private void InitializeInstance() {
		if (instance != null) {
			Debug.LogWarning($"Duplicate networked singleton instances: {instance.name} and {name}. Class is marked as singleton.");
		}
		instance = this as T;
		onSingletonBecomesAvailable.Invoke(instance);
		onSingletonBecomesAvailable.RemoveAllListeners();
	}

	public override void OnStopClient() {
		base.OnStopClient();
		if (IsLocalPlayerSingleton()) {
			return;
		}
		if (instance == this) {
			instance = null;
		}
	}

	public override void OnStopLocalPlayer() {
		base.OnStopLocalPlayer();
		if (!IsLocalPlayerSingleton()) {
			return;
		}
		if (instance == this) {
			instance = null;
		}
	}

	public static void OnSingletonBecomesAvailable(Action<T> action) {
		if (instance != null) {
			action.Invoke(instance);
		}
		else {
			onSingletonBecomesAvailable.AddListener(new UnityAction<T>(action));
		}
	}

	public static bool HasInstance() {
		return instance != null;
	}

	public static T GetInstance() {
		if (instance == null) {
			Debug.LogWarning("Accessing NetworkedSingleton instance before it is initialized. Returning null.");
			return null;
		}
		return instance;
	}

	public bool IsInstance() {
		return instance == this;
	}
}
