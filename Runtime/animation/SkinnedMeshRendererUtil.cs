using UnityEngine;
using VInspector;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshRendererUtil : MonoBehaviour {
	[Header("Copy Bones")]
	public SkinnedMeshRenderer sourceBoneMap;
	public bool copyBonesOnAwake = false;
	public bool copyBonesFromSibling = false;
	public string siblingName = "";

	void Awake() {
		if (copyBonesOnAwake) {
			CopyBones();
		}
	}

	[Button("Copy Bones")]
	[ContextMenu("Copy Bones")]
	public void CopyBones() {
		SkinnedMeshRenderer sourceSMR = sourceBoneMap;
		if (sourceSMR == null) {
			if (copyBonesFromSibling && !string.IsNullOrEmpty(siblingName)) {
				var sibling = transform.parent?.Find(siblingName);
				if (sibling != null && sibling.TryGetComponent<SkinnedMeshRenderer>(out var siblingRenderer)) {
					sourceSMR = siblingRenderer;
				}
			}
			if (sourceSMR == null) {
				Debug.LogWarning("Source bone map is not assigned.");
				return;
			}
		}

		var targetRenderer = GetComponent<SkinnedMeshRenderer>();
		if (targetRenderer == null) {
			Debug.LogWarning("No SkinnedMeshRenderer found on this GameObject.");
			return;
		}

		targetRenderer.rootBone = sourceSMR.rootBone;
		targetRenderer.bones = sourceSMR.bones;
		UpdateBounds();
	}

	[Button("Update Bounds")]
	[ContextMenu("Update Bounds")]
	public void UpdateBounds() {
		var targetRenderer = GetComponent<SkinnedMeshRenderer>();
		if (targetRenderer == null) {
			Debug.LogWarning("No SkinnedMeshRenderer found on this GameObject.");
			return;
		}

		if (targetRenderer.sharedMesh == null) {
			Debug.LogWarning("No mesh assigned to the SkinnedMeshRenderer.");
			return;
		}

		var rootBone = targetRenderer.rootBone;
		if (rootBone == null) {
			Debug.LogWarning("SkinnedMeshRenderer has no root bone assigned.");
			return;
		}

		targetRenderer.updateWhenOffscreen = true;
		Bounds bounds = new(targetRenderer.localBounds.center, targetRenderer.localBounds.size);
		targetRenderer.updateWhenOffscreen = false;
		targetRenderer.localBounds = bounds;
	}
}
