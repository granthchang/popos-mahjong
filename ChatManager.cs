using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class ChatManager : MonoBehaviourPunCallbacks
{
	[Header("References")]
	[SerializeField] private TMP_Text chatLog;
	[SerializeField] private TMP_InputField chatField;

	[Header("Color Settings")]
	[SerializeField] private Color defaultColor;
	[SerializeField] private Color serverMessageColor;
	private string serverColorHex;


	// Sets up colors and show lobby information
	private void Start()
	{
		chatLog.color = defaultColor;
		serverColorHex = ColorUtility.ToHtmlStringRGB(serverMessageColor);

		ShowServerMessage("You joined the game.");
	}


	/* -------------------- CHAT MESSAGES -------------------- */
	private bool wasFocused;
	private void Update()
	{
		// If enter was hit while focusing input field
		if (Input.GetKeyDown(KeyCode.Return) && wasFocused && !chatField.isFocused)
		{
			if (!string.IsNullOrWhiteSpace(chatField.text))
			{
				photonView.RPC("ShowChatMessage", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, chatField.text);
				chatField.text = "";
				chatField.Select();
				chatField.ActivateInputField();
			}
		}
		wasFocused = chatField.isFocused;
	}

	[PunRPC]
	private void ShowChatMessage(string sender, string message)
	{
		chatLog.text += "<b>" + sender + ": </b>" + message + "\n";
	}


	/* -------------------- SERVER MESSAGES -------------------- */

	private void SendServerMessage(string message)
	{
		photonView.RPC("ShowServerMessage", RpcTarget.All, message);
	}

	[PunRPC]
	private void ShowServerMessage(string message)
	{
		chatLog.text += "<color=#" + serverColorHex + ">" + message + "</color>\n";
	}

	/* -------------------- SERVER EVENTS -------------------- */

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		ShowServerMessage(newPlayer.NickName + " joined the game.");
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		ShowServerMessage(otherPlayer.NickName + " left the game.");
	}
}