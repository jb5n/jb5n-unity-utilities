using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script allows us to subscribe to built-in Unity events happening to this game object, like OnDestroy()
// Add more delegates as necessary, using OnDestroy as a template
public class BuiltInDelegates : MonoBehaviour {
	// Subscribe using builtInDelegatesComponent.Event += MyMethod;
	public delegate void OnDestroyDelegate(MonoBehaviour instance);
	public event OnDestroyDelegate OnDestroyEvent;

	void OnDestroy() {
		if(OnDestroyEvent != null) {
			OnDestroyEvent(this);
		}
	}

	void OnApplicationQuit() {
		RemoveAllDelegates();
	}

	private void RemoveAllDelegates() {
		OnDestroyEvent = null;
	}
}