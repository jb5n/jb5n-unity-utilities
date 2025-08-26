using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	protected static T m_instance;

	public static T instance {
		get {
			if (m_instance == null) {
#if UNITY_6000_0_OR_NEWER
				m_instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
					m_instance = FindObjectOfType<T>(true);
#endif
			}

			return m_instance;
		}
	}

	protected virtual void Awake() {
		if (m_instance == null && this is T) {
			m_instance = this as T;
		}
		else {
			Debug.Log("Deleting duplicate instance of " + GetType().ToString() + ", " + gameObject.name + ": class is marked as singleton.");
			Destroy(gameObject);
		}
	}
}
