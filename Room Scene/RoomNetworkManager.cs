// Author: Grant Chang
// Date: 14 July 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// RoomNetworkManager handles basic networking concerns for the room scene like leaving the room.
/// When in the lobby scene, use LobbyNetworkManager.
/// </summary>
public class RoomNetworkManager : MonoBehaviourPunCallbacks
{
    /// <summary>
	/// Leaves current room. If not in one, does nothing.
	/// </summary>
	public void LeaveRoom()
	{
		if (PhotonNetwork.CurrentRoom != null)
			PhotonNetwork.LeaveRoom();
	}

	/// <summary>
	/// On leaving room, load the Lobby scene.
	/// </summary>
	public override void OnLeftRoom()
	{
		PhotonNetwork.LoadLevel("Lobby");
	}
}
