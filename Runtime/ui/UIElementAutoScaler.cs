using UnityEngine;

namespace jb5n {
	public class UIElementAutoScaler : MonoBehaviour {
		public float referenceCameraOrthoSize;

		private void LateUpdate() {
			float curCamOrthoScale = Camera.main.orthographicSize / referenceCameraOrthoSize;
			transform.localScale = Vector3.one * curCamOrthoScale;
		}
	}
}