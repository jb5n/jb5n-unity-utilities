
using UnityEngine;

public interface ICollisionRecipient {
	virtual void ReceiveOnTriggerEnter(Collider theirCol, Collider ourCol) { }
	virtual void ReceiveOnTriggerStay(Collider theirCol, Collider ourCol) { }
	virtual void ReceiveOnTriggerExit(Collider theirCol, Collider ourCol) { }
	virtual void ReceiveOnCollisionEnter(Collision col) { }
	virtual void ReceiveOnCollisionStay(Collision col) { }
	virtual void ReceiveOnCollisionExit(Collision col) { }
}