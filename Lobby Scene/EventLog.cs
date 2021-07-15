// Author: Grant Chang
// Date: 14 July 2021

using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// EventLog displays information about server actions and responses in the Lobby scene. It shows
/// information like failing to create a room or being unable to find an open game. If creating
/// a room or joining/finding a game is successful, the scene switches to the Room scene. 
/// </summary>
public class EventLog : MonoBehaviourPunCallbacks
{
    #region References

    /// <summary>
    /// Text object to display information on the screen
    /// </summary>
    [SerializeField] private TMP_Text textObj;

    #endregion
    #region Start

	private void Start()
	{
        LobbyNetworkManager.singleton.OnInvalidPlayerName += s => textObj.text = s;
		LobbyNetworkManager.singleton.OnInvalidRoomCode += s => textObj.text = s;
	}

    #endregion
    #region Pun Callbacks

    /* -------------------- MASTER SERVER EVENTS -------------------- */

    public override void OnConnectedToMaster()
	{
        textObj.text = "Successfully connected to master server.";
	}

    public override void OnDisconnected(DisconnectCause cause)
	{
        textObj.text = "You disconnected from the server.";
	}

	public override void OnErrorInfo(ErrorInfo errorInfo)
	{
        textObj.text = "An error occurred on the server. " + errorInfo.Info;
    }


    /* -------------------- CREATING ROOM -------------------- */
    
    public override void OnCreatedRoom()
	{
        textObj.text = "Successfully created room with code: " + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
	{
        textObj.text = message + ". " + returnCode;
    }


    /* -------------------- JOINING/FINDING ROOM -------------------- */

    public override void OnJoinedRoom()
	{
        if (!PhotonNetwork.IsMasterClient)
            textObj.text = "Successfully joined room.";
    }

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
        textObj.text = message + ".";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        textObj.text = message + ".";
    }

    #endregion
}
