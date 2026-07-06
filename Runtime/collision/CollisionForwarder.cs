using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionForwarder : MonoBehaviour {
	[Flags]
	public enum CollisionForwardType {
		None = 0,
		OnTriggerEnter = 1,
		OnTriggerStay = 2,
		OnTriggerExit = 4,
		OnCollisionEnter = 8,
		OnCollisionStay = 16,
		OnCollisionExit = 32
	}

	public CollisionForwardType typesToForward;

	private ICollisionRecipient recipient;
	private Collider ourCollider;

	void Awake() {
		ourCollider = GetComponent<Collider>();
		recipient = GetComponentInParent<ICollisionRecipient>();
		if (recipient == null) {
			Debug.LogError($"CollisionForwarder {gameObject.name} has no ICollisionRecipient in parent hierarchy.");
			typesToForward = CollisionForwardType.None;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (typesToForward.HasFlag(CollisionForwardType.OnTriggerEnter)) {
			recipient.ReceiveOnTriggerEnter(other, ourCollider);
		}
	}

	void OnTriggerStay(Collider other) {
		if (typesToForward.HasFlag(CollisionForwardType.OnTriggerStay)) {
			recipient.ReceiveOnTriggerStay(other, ourCollider);
		}
	}

	void OnTriggerExit(Collider other) {
		if (typesToForward.HasFlag(CollisionForwardType.OnTriggerExit)) {
			recipient.ReceiveOnTriggerExit(other, ourCollider);
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (typesToForward.HasFlag(CollisionForwardType.OnCollisionEnter)) {
			recipient.ReceiveOnCollisionEnter(collision);
		}
	}

	void OnCollisionStay(Collision collision) {
		if (typesToForward.HasFlag(CollisionForwardType.OnCollisionStay)) {
			recipient.ReceiveOnCollisionStay(collision);
		}
	}

	void OnCollisionExit(Collision collision) {
		if (typesToForward.HasFlag(CollisionForwardType.OnCollisionExit)) {
			recipient.ReceiveOnCollisionExit(collision);
		}
	}
}
