using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace jb5n {
	// Helper function so we can stay in the gameplay scene and automatically load the Main Menu scene when playing in the editor
	public class LoadSceneAtStart : MonoBehaviour {
		public string networkManagerName;
		public string mainMenuSceneName;

		void Awake() {
			if (GameObject.Find(networkManagerName) == null) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
			}
		}
	}
}
