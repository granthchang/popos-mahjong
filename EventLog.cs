using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class EventLog : MonoBehaviourPunCallbacks
{
    // Text object
    [SerializeField] private TMP_Text textObj;


    /* -------------------- SETUP -------------------- */

	// Subscribe to network events
	private void Start()
	{
        NetworkManager.singleton.OnInvalidPlayerName += s => textObj.text = s;
		NetworkManager.singleton.OnInvalidRoomCode += s => textObj.text = s;
	}


    /* -------------------- CONNECTING TO MASTER SERVER -------------------- */

    public override void OnConnectedToMaster()
	{
        textObj.text = "Successfully connected to master server";
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


    /* -------------------- JOINING ROOM -------------------- */

    public override void OnJoinedRoom()
	{
        if (PhotonNetwork.IsMasterClient)
            return;

        textObj.text = "Successfully joined room.";
    }

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
        textObj.text = message + ". " + returnCode;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        textObj.text = message + ". " + returnCode;
    }


	/* -------------------- ROOM NOTIFICATIONS -------------------- */

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
        textObj.text = newPlayer.NickName + " joined.";
    }

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
        textObj.text = otherPlayer.NickName + " left.";
    }


	/* -------------------- DISCONNECTING AND ERRORS -------------------- */

	public override void OnLeftRoom()
	{
        textObj.text = "You left the room.";
    }

	public override void OnDisconnected(DisconnectCause cause)
	{
        textObj.text = "You disconnected from the server.";
	}

	public override void OnErrorInfo(ErrorInfo errorInfo)
	{
        textObj.text = "An error occurred on the server. " + errorInfo.Info;
    }
}
