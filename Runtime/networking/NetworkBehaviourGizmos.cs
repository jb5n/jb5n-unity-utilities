using Mirror;
using UnityEngine;

// Common base behaviour class for networked objects that need to draw gizmos. Requires Mirror and ALINE.
namespace Drawing {
	public abstract class NetworkBehaviourGizmos : NetworkBehaviour, IDrawGizmos {
		public NetworkBehaviourGizmos() {
#if UNITY_EDITOR
			DrawingManager.Register(this);
#endif
		}

		/// <summary>
		/// An empty OnDrawGizmosSelected method.
		/// Why an empty OnDrawGizmosSelected method?
		/// This is because only objects with an OnDrawGizmos/OnDrawGizmosSelected method will show up in Unity's menu for enabling/disabling
		/// the gizmos per object type (upper right corner of the scene view). So we need it here even though we don't use normal gizmos.
		///
		/// By using OnDrawGizmosSelected instead of OnDrawGizmos we minimize the overhead of Unity calling this empty method.
		/// </summary>
		void OnDrawGizmosSelected() {
		}

		/// <summary>
		/// Draw gizmos for this object.
		///
		/// The gizmos will be visible in the scene view, and the game view, if gizmos have been enabled.
		///
		/// This method will only be called in the Unity Editor.
		///
		/// See: <see cref="Draw"/>
		/// </summary>
		public virtual void DrawGizmos() {
		}
	}
}
