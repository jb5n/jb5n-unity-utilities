using UnityEditor;
using UnityEngine;

namespace jb5n.editor {
	/// <summary>
	/// Recovery for a wedged scene view: clears a leaked <see cref="GUIUtility.hotControl"/>.
	///
	/// hotControl is a single global for the whole editor IMGUI system. While it is non-zero every
	/// mouse event is routed to that one control id and handed to everything else as
	/// EventType.Ignore, so any tool that grabs it and then misses its MouseUp -- a drag released
	/// over another window, a tool deactivated mid-click, an exception thrown before the release --
	/// leaves it pinned to a control that no longer exists. Every handle in the editor then stops
	/// taking clicks: the scene view orientation gizmo, collider bounds, transform handles. Nothing
	/// is logged and nothing looks wrong, because Repaint is unaffected and it all still draws.
	///
	/// The real fix is always for the offending tool to release its own grab. This is the escape
	/// hatch, so that working out which tool that is does not cost an editor restart.
	/// </summary>
	internal static class SceneInputRecovery {
		private static int _pendingRelease;

		[MenuItem("Tools/jb5n/Unstick Scene Input")]
		private static void Unstick() {
			int stuck = GUIUtility.hotControl;
			if (stuck == 0) {
				Debug.Log("Unstick Scene Input: nothing is holding hotControl, so scene input is not wedged this way.");
				return;
			}

			// Released twice on purpose. The direct assignment is enough on its own most of the
			// time, but hotControl is native-backed state that IMGUI otherwise only ever touches
			// from inside an event, so the same release is also queued to run in a real scene view
			// GUI pass. Repaints keep arriving while input is stuck, so that pass will happen.
			GUIUtility.hotControl = 0;

			_pendingRelease = stuck;
			SceneView.duringSceneGui -= ReleaseInGuiContext;
			SceneView.duringSceneGui += ReleaseInGuiContext;
			SceneView.RepaintAll();

			Debug.LogWarning($"Unstick Scene Input: released hotControl {stuck}. A scene view tool took the "
				+ "mouse and never gave it back. If this keeps happening, whichever tool was active at the "
				+ "time is missing a release path -- most often a drag that ends outside the scene view, "
				+ "which arrives as EventType.Ignore rather than MouseUp.");
		}

		private static void ReleaseInGuiContext(SceneView sceneView) {
			SceneView.duringSceneGui -= ReleaseInGuiContext;

			// Only if that exact grab is still in place. If the assignment above already took, this
			// is a no-op; if this callback instead runs much later (no scene view was open when the
			// menu item was used) it must not steal a legitimate drag that has started since.
			if (GUIUtility.hotControl == _pendingRelease) {
				GUIUtility.hotControl = 0;
			}

			_pendingRelease = 0;
		}
	}
}
