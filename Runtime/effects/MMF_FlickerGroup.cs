using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

[AddComponentMenu("")]
[FeedbackHelp("This feedback works like Flicker but is applied to all children of RendererParent")]
[FeedbackPath("Renderer/Flicker Group")]
public class MMF_FlickerGroup : MMF_Flicker {
	[Tooltip("All MeshRenderers that are children of this transform will be flickered")]
	public Transform RendererParent;

	protected override void CustomInitialization(MMF_Player owner) {
		List<Renderer> allRenderers = new();
		foreach (MeshRenderer mr in RendererParent.GetComponentsInChildren<MeshRenderer>()) {
			allRenderers.Add(mr);
		}
		foreach (SkinnedMeshRenderer smr in RendererParent.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			allRenderers.Add(smr);
		}
		if (allRenderers.Count > 0) {
			BoundRenderer = allRenderers[0];
			allRenderers.RemoveAt(0);
			ExtraBoundRenderers = allRenderers;
		}

		base.CustomInitialization(owner);
	}
}
