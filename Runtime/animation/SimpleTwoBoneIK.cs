using UnityEngine;

namespace jb5n {
	// A lightweight analytic two-bone IK solver (e.g. shoulder->elbow->hand or hip->knee->foot).
	// Assign the three bones of the chain plus a target (where the tip should reach) and a hint
	// (a.k.a. pole) transform that controls which way the middle joint bends.
	[ExecuteInEditMode]
	public class SimpleTwoBoneIK : MonoBehaviour {
		[Header("Bone chain (root -> mid -> tip)")]
		[Tooltip("Upper bone, e.g. the upper arm / thigh.")]
		public Transform upper;
		[Tooltip("Middle joint bone, e.g. the forearm / shin.")]
		public Transform mid;
		[Tooltip("Tip bone, e.g. the hand / foot.")]
		public Transform tip;

		[Header("IK controls")]
		[Tooltip("World-space goal the tip will try to reach.")]
		public Transform target;
		[Tooltip("Pole target the mid joint bends toward. Without it the bend direction is ambiguous.")]
		public Transform hint;

		[Header("Settings")]
		[Range(0f, 1f)]
		[Tooltip("Blend between the original animated pose (0) and the fully solved IK pose (1).")]
		public float weight = 1f;
		[Tooltip("When enabled the tip is also rotated to match the target's rotation.")]
		public bool matchTargetRotation = false;

		void LateUpdate() {
			Solve();
		}

		void Solve() {
			if (upper == null || mid == null || tip == null || target == null) return;
			if (weight <= 0f) return;

			// Cache the pre-IK orientations so we can blend by weight.
			Quaternion upperRot0 = upper.rotation;
			Quaternion midRot0 = mid.rotation;

			Vector3 rootPos = upper.position;
			Vector3 midPos = mid.position;
			Vector3 tipPos = tip.position;

			float upperLen = Vector3.Distance(rootPos, midPos);
			float lowerLen = Vector3.Distance(midPos, tipPos);
			if (upperLen <= Mathf.Epsilon || lowerLen <= Mathf.Epsilon) return;

			Vector3 toTarget = target.position - rootPos;
			float targetDist = toTarget.magnitude;
			if (targetDist <= Mathf.Epsilon) return;

			// Clamp the reach so the triangle is always solvable (and leave a sliver so it never
			// fully locks straight, which keeps the bend direction stable).
			float maxReach = upperLen + lowerLen;
			float minReach = Mathf.Abs(upperLen - lowerLen);
			float clampedDist = Mathf.Clamp(targetDist, minReach + 1e-4f, maxReach - 1e-4f);

			Vector3 targetDir = toTarget / targetDist;

			// Law of cosines: interior angle at the root between the upper bone and the target line.
			float cosRoot = (upperLen * upperLen + clampedDist * clampedDist - lowerLen * lowerLen)
				/ (2f * upperLen * clampedDist);
			float rootAngle = Mathf.Acos(Mathf.Clamp(cosRoot, -1f, 1f)) * Mathf.Rad2Deg;

			// Bend axis: perpendicular to the plane formed by the target line and the hint.
			Vector3 bendAxis = ComputeBendAxis(rootPos, midPos, tipPos, targetDir);

			// 1) Aim the upper bone straight at the target, then bend it up by the root angle so the
			//    chain forms the correct triangle, bending toward the hint.
			Vector3 upperDir = mid.position - rootPos;
			Quaternion aimUpper = Quaternion.FromToRotation(upperDir, targetDir);
			Quaternion bendUpper = Quaternion.AngleAxis(-rootAngle, bendAxis);
			upper.rotation = bendUpper * aimUpper * upper.rotation;

			// 2) Aim the mid bone so the tip lands on the target.
			Vector3 lowerDir = tip.position - mid.position;
			Vector3 desiredLowerDir = target.position - mid.position;
			Quaternion aimLower = Quaternion.FromToRotation(lowerDir, desiredLowerDir);
			mid.rotation = aimLower * mid.rotation;

			// Optionally drive the tip's rotation from the target.
			if (matchTargetRotation) {
				tip.rotation = target.rotation;
			}

			// Blend back toward the original pose by weight.
			if (weight < 1f) {
				upper.rotation = Quaternion.Slerp(upperRot0, upper.rotation, weight);
				mid.rotation = Quaternion.Slerp(midRot0, mid.rotation, weight);
			}
		}

		// Returns the axis the joint rotates around. The hint (pole) defines which side of the
		// target line the mid joint swings toward; without one we fall back to the current bend.
		Vector3 ComputeBendAxis(Vector3 rootPos, Vector3 midPos, Vector3 tipPos, Vector3 targetDir) {
			Vector3 polePos;
			if (hint != null) {
				polePos = hint.position;
			} else {
				// No hint: keep bending in the chain's existing plane.
				polePos = midPos;
			}

			Vector3 poleDir = polePos - rootPos;
			Vector3 axis = Vector3.Cross(targetDir, poleDir);
			if (axis.sqrMagnitude < 1e-8f) {
				// Pole is colinear with the target line; derive a plane from the current pose instead.
				axis = Vector3.Cross(targetDir, midPos - rootPos);
				if (axis.sqrMagnitude < 1e-8f) {
					axis = Vector3.Cross(targetDir, tipPos - rootPos);
				}
				if (axis.sqrMagnitude < 1e-8f) {
					// Fully degenerate; pick any perpendicular.
					axis = Vector3.Cross(targetDir, Vector3.up);
					if (axis.sqrMagnitude < 1e-8f) axis = Vector3.Cross(targetDir, Vector3.right);
				}
			}
			return axis.normalized;
		}

		void OnDrawGizmosSelected() {
			if (upper == null || mid == null || tip == null) return;
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(upper.position, mid.position);
			Gizmos.DrawLine(mid.position, tip.position);
			Gizmos.DrawWireSphere(mid.position, 0.02f);
			if (target != null) {
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(target.position, 0.03f);
			}
			if (hint != null) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(hint.position, 0.03f);
				Gizmos.DrawLine(mid.position, hint.position);
			}
		}
	}
}
