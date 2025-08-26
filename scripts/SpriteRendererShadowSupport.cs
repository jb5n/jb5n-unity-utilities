using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jb5n {
	[ExecuteAlways]
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteRendererShadowSupport : MonoBehaviour {
		public UnityEngine.Rendering.ShadowCastingMode ShadowCastingMode;
		public bool ReceiveShadows;

		void OnValidate() {
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			sr.shadowCastingMode = ShadowCastingMode;
			sr.receiveShadows = ReceiveShadows;
		}
	}
}
