// Adapted from https://gist.github.com/irfanbaysal/87c9063f157f4a3942a85c09ac2a19da

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class StartSceneWindow : EditorWindow {
	private void OnGUI() {
		EditorSceneManager.playModeStartScene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Start Scene"),
			EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);
	}

	[MenuItem("Window/Start Scene Settings")]
	private static void StartScene() {
		GetWindow<StartSceneWindow>();
	}
}
#endif // UNITY_EDITOR
