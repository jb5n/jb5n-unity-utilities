using System.Collections;
using UnityEngine;

namespace jb5n {
	public class DelayedDestroy : MonoBehaviour {
		public float delay;

		IEnumerator Start() {
			yield return new WaitForSeconds(delay);
			Destroy(gameObject);
		}
	}
}