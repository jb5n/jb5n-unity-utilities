using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DelayedDestroy : MonoBehaviour {
	public float delay;
	public bool netDestroy = false;

	IEnumerator Start() {
		yield return new WaitForSeconds(delay);
		if(netDestroy) {
			if(NetworkServer.active) {
				NetworkServer.Destroy(gameObject);
			}
		}
		else {
			Destroy(gameObject);
		}
	}
}