// Author: Grant Chang
// Date: 14 July 2021

using Photon.Pun;
using Photon.Realtime;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// LobbyNetworkManager handles basic networking concerns for the lobby scene like connecting to the
/// master server and hosting/finding a room. When a room is created or found, use RoomNetworkManager.
/// </summary>
public class LobbyNetworkManager : MonoBehaviourPunCallbacks
{
	#region Singleton

	public static LobbyNetworkManager singleton;
	private void Awake()
	{
		if (singleton != null && singleton != this)
			this.gameObject.SetActive(false);
		singleton = this;
	}
	
	#endregion
	#region Start / Loading Scenes

	/* -------------------- CONNECTING TO MASTER SERVER -------------------- */

	/// <summary>
    /// When the client opens, start connecting to the master server
	/// </summary>
	private void Start()
    {
		if (!PhotonNetwork.IsConnected)
			PhotonNetwork.ConnectUsingSettings();
	}

	/// <summary>
	/// On connection to a room, switch to room scene.
	/// </summary>
	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Room");
	}

	#endregion
	#region Rooms

	/* -------------------- HOSTING ROOM -------------------- */

	/// <summary>
	/// Checks that player name has been set. If so, creates a new room with a random room code
	/// </summary>
	public void CreateRoom()
	{
        if (TrySetPlayerName(tempPlayerName))
        {
			RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 4;
            EnterRoomParams enterRoomParams = new EnterRoomParams();
            PhotonNetwork.CreateRoom(GenerateRoomCode(), roomOptions);
        }
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
		if (TrySetPlayerName(tempPlayerName) && TrySetRoomCode(tempRoomCode))
			PhotonNetwork.JoinRoom(tempRoomCode);
	}

	/// <summary>
	/// Validates name. If it passes, joins a random room on the master server
	/// </summary>
	public void JoinRandomRoom()
	{
		if (TrySetPlayerName(tempPlayerName))
			PhotonNetwork.JoinRandomRoom();
	}

	/// <summary>
	/// Disconnects from master server
	/// </summary>
	public void Disconnect()
	{
		PhotonNetwork.Disconnect();
	}

	#endregion
	#region Validators

	public string tempPlayerName { get; set; }
	public event Action<string> OnInvalidPlayerName;
	
	public string tempRoomCode { get; set; }
	public event Action<string> OnInvalidRoomCode;


	/// <summary>
	/// Provided player name is trimmed and validated. If the name passes validation, sets player
    /// name on the network. To pass validation, player names may not be null, empty, or whitespace
    /// and must be entirely alphanumeric or spaces. Returns true if succeeded.
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

		tempRoomCode = code.Trim().ToUpper();
		return true;
	}

	#endregion
}
