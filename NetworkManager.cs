// Author: Grant Chang
// Date: 9 July 2021

using Photon.Pun;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// NetworkManager handles basic networking concerns to establish connection between the client
/// and the server. Meant to be accessed by other scripts, but does not access other scripts itself.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
	// Singleton Object
	public static NetworkManager singleton;
	private void Awake()
	{
		if (singleton != null && singleton != this)
			this.gameObject.SetActive(false);
		singleton = this;
	}


	public string roomCode { get; set; }
	public string playerName { get; set; }

	public event Action<string> OnInvalidPlayerName;
	public event Action<string> OnInvalidRoomCode;


	/* -------------------- CONNECTING TO MASTER SERVER -------------------- */

	// When the client opens, start connecting to the master server
	private void Start()
    {
		if (!PhotonNetwork.IsConnected)
			PhotonNetwork.ConnectUsingSettings();
	}


	/* -------------------- HOSTING ROOM -------------------- */

	/// <summary>
	/// Checks that player name has been set. If so, creates a new room with a random room code
	/// </summary>
	public void CreateRoom()
	{
		if (TrySetPlayerName(playerName))
			PhotonNetwork.CreateRoom(GenerateRoomCode());
	}

	/// <summary>
	/// Generates a random uppercase 4-letter string
	/// </summary>
	private string GenerateRoomCode()
	{
		string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		string code = "";
		System.Random random = new System.Random();
		for (int i = 0; i < 4; i++)
			code += chars[random.Next(0, 25)];
		return code;
	}


	/* -------------------- JOINING ROOM -------------------- */

	/// <summary>
	/// Valiates name and room code. If they pass, searches for a room with the room code
	/// </summary>
	public void FindRoom()
	{
		if (TrySetPlayerName(playerName) && TrySetRoomCode(roomCode))
			PhotonNetwork.JoinRoom(roomCode);
	}

	/// <summary>
	/// Validates name. If it passes, joins a random room on the master server
	/// </summary>
	public void JoinRandomRoom()
	{
		if (TrySetPlayerName(playerName))
			PhotonNetwork.JoinRandomRoom();
	}


	/* -------------------- DISCONNECTING -------------------- */

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

	/// <summary>
	/// Disconnects from master server
	/// </summary>
	public void Disconnect()
	{
		PhotonNetwork.Disconnect();
	}


	/* -------------------- VALIDATORS -------------------- */

	/// <summary>
	/// Provided player name is trimmed and validated. If the name passes validation, sets player name on the network.
	/// To pass validation, player names may not be null, empty, or whitespace and must be entirely alphanumeric or spaces.
	/// Returns true if succeeded.
	/// </summary>
	private bool TrySetPlayerName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			OnInvalidPlayerName?.Invoke("Player names may not be null, empty, or whitespace.");
			return false;
		}

		string cleaned = name.Trim();

		if (!Regex.IsMatch(cleaned, @"[A-Za-z0-9 ]+"))
		{
			OnInvalidPlayerName?.Invoke("Player names must be alphanumeric or spaces.");
			return false;
		}

		PhotonNetwork.LocalPlayer.NickName = cleaned;
		return true;
	}

	/// <summary>
	/// Sets room code to provided code. Does not accept null values. Returns true if succeeded.
	/// </summary>
	private bool TrySetRoomCode(string code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			OnInvalidRoomCode?.Invoke("Room code may not be null, empty, or whitespace.");
			return false;
		}

		roomCode = code.Trim().ToUpper();
		return true;
	}


	/* -------------------- LOADING SCENES -------------------- */

	/// <summary>
	/// On connection to a room, switch to room scene.
	/// </summary>
	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Room");
	}
}
