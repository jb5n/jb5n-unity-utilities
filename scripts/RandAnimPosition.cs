using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jb5n {
	// Starts the animator at a random position
	public class RandAnimPosition : MonoBehaviour {
		void Start() { // will do nothing if called from Awake()
			Animator anim = GetComponent<Animator>();
			AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0); //could replace 0 by any other animation layer index
			anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
		}
	}
}
