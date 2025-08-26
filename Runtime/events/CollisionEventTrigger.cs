using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace jb5n {
	// Simple utility component that invokes UnityEvents on collision events
	public class CollisionEventTrigger : MonoBehaviour {
		[System.Serializable]
		public class CollisionEvent : UnityEvent<Collision2D> { }

		[System.Serializable]
		public class TriggerEvent : UnityEvent<Collider2D> { }

		[Header("Collisions")]
		public CollisionEvent collisionEnterEvent;
		public CollisionEvent collisionStayEvent;
		public CollisionEvent collisionExitEvent;
		[Header("Triggers")]
		public TriggerEvent triggerEnterEvent;
		public TriggerEvent triggerStayEvent;
		public TriggerEvent triggerExitEvent;

		public void OnCollisionEnter2D(Collision2D col) {
			collisionEnterEvent?.Invoke(col);
		}

		public void OnCollisionStay2D(Collision2D col) {
			collisionStayEvent?.Invoke(col);
		}

		public void OnCollisionExit2D(Collision2D col) {
			collisionExitEvent?.Invoke(col);
		}

		public void OnTriggerEnter2D(Collider2D col) {
			triggerEnterEvent?.Invoke(col);
		}

		public void OnTriggerStay2D(Collider2D col) {
			triggerStayEvent?.Invoke(col);
		}

		public void OnTriggerExit2D(Collider2D col) {
			triggerExitEvent?.Invoke(col);
		}
	}
}