// Author: Grant Chang
// Date: 14 July 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

/// <summary>
/// ChatManager sends messages between clients with RPCs and shows room information like new
/// players joining the room.
/// </summary>
public class ChatManager : MonoBehaviourPunCallbacks
{
	#region Inspector Elements
	
	[Header("References")]
	[SerializeField] private TMP_Text chatLog;
	[SerializeField] private TMP_InputField chatField;

	[Header("Color Settings")]
	[SerializeField] private Color defaultColor;
	[SerializeField] private Color serverMessageColor;
	private string serverColorHex;

	#endregion
	#region Monobehaviour
	
	/// <summary>
    /// When the scene loads, set up colors for server vs chat messages.
    /// </summary>
	private void Start()
	{
		chatLog.color = defaultColor;
		serverColorHex = ColorUtility.ToHtmlStringRGB(serverMessageColor);

		ShowServerMessage("You joined the game.");
	}

	// Remembers if chat input field was focused last frame. Used to determine whether Return
	// was hit.
	private bool wasFocused;
	
	/// <summary>
    /// If Return was hit and the chat input field was focused last frame and is no longer focused,
    /// send the text in the input field as a message. Refocus the input field.
    /// </summary>
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

	#endregion
	#region Pun RPCs
	
	/// <summary>
	/// Show chat message in the chat log.
	/// </summary>
	[PunRPC]
	private void ShowChatMessage(string sender, string message)
	{
		chatLog.text += "<b>" + sender + ": </b>" + message + "\n";
	}

	/// <summary>
    /// Shows server message in the chat log
    /// </summary>
	[PunRPC]
	private void ShowServerMessage(string message)
	{
		chatLog.text += "<color=#" + serverColorHex + ">" + message + "</color>\n";
	}

	#endregion
	#region Server Events

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		ShowServerMessage(newPlayer.NickName + " joined the game.");
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		ShowServerMessage(otherPlayer.NickName + " left the game.");
	}

	#endregion
}