using Mirror;
using MoreMountains.Feedbacks;
using UnityEngine;

// Supports sending MoreMountains Feedbacks over the network
public class NetworkFeedbacks : NetworkBehaviour {

	// These feedbacks will be played on all machines
	public MMF_Player feedbacks;
	// These feedbacks will only be triggered on the machine that calls PlayFeedbacks()
	public MMF_Player localFeedbacks;

	public void PlayFeedbacks(bool playLocalFeedbacks = true) {
		if (localFeedbacks != null && playLocalFeedbacks) {
			localFeedbacks.PlayFeedbacks();
		}
		if (feedbacks != null) {
			feedbacks.PlayFeedbacks();
			CmdPlayFeedbacks();
		}
		else {
			Debug.LogWarning($"NetworkFeedbacks {gameObject.name} called without a feedbacks property set!");
		}
	}

	public void StopFeedbacks(bool stopLocalFeedbacks = true) {
		if (localFeedbacks != null && stopLocalFeedbacks) {
			localFeedbacks.StopFeedbacks();
		}
		if (feedbacks != null) {
			feedbacks.StopFeedbacks();
			CmdStopFeedbacks();
		}
		else {
			Debug.LogWarning($"NetworkFeedbacks {gameObject.name} called without a feedbacks property set!");
		}
	}

	[Command(requiresAuthority = false)]
	private void CmdPlayFeedbacks(NetworkConnectionToClient sender = null) {
		feedbacks.PlayFeedbacks();
		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values) {
			if (conn != sender) {
				TargetPlayFeedbacks(conn);
			}
		}
	}

	[Command(requiresAuthority = false)]
	private void CmdStopFeedbacks(NetworkConnectionToClient sender = null) {
		feedbacks.StopFeedbacks();
		foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values) {
			if (conn != sender) {
				TargetStopFeedbacks(conn);
			}
		}
	}

	[TargetRpc]
	private void TargetPlayFeedbacks(NetworkConnectionToClient conn) {
		feedbacks.PlayFeedbacks();
	}

	[TargetRpc]
	private void TargetStopFeedbacks(NetworkConnectionToClient conn) {
		feedbacks.StopFeedbacks();
	}
}
