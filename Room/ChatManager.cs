// Author: Grant Chang
// Date: 14 August 2021

using Photon.Pun;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// ChatManager sends messages between clients with RPCs and shows room information like new
/// players joining the room. ChatManager is entirely separate from the RoomManager so that it
/// will function at all times regardless of the game state.
/// </summary>
public class ChatManager : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  [Header("References")]
  [SerializeField] private TMP_Text _chatLog;
  [SerializeField] private TMP_InputField _chatField;

  [Header("Color Settings")]
  [SerializeField] private Color _defaultColor;
  [SerializeField] private Color _serverMessageColor;
  private string _serverColorHex;
  private StringBuilder _stringBuilder;

  private bool _wasInputFocused;

  #endregion
  #region Constructors / Initializers

  private void Start() {
    _chatLog.color = _defaultColor;
    _serverColorHex = ColorUtility.ToHtmlStringRGB(_serverMessageColor);
    _stringBuilder = new StringBuilder();

    RpcShowMessage("You joined the game.");
  }

  #endregion
  #region Network Event Handlers

  public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
    RpcShowMessage($"{newPlayer.NickName} joined the game.");
  }

  public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
    RpcShowMessage($"{otherPlayer.NickName} left the game.");
  }

  #endregion
  #region Send Message

  private void Update() {
    // If enter was hit while focusing input field, send contents as message
    if (Input.GetKeyDown(KeyCode.Return) && _wasInputFocused && !_chatField.isFocused) {
      if (!string.IsNullOrWhiteSpace(_chatField.text)) {
        SendChatMessage(_chatField.text);
      }
    }
    _wasInputFocused = _chatField.isFocused;
  }

  #endregion
  #region Remote Procedure Calls (RPCs)

  [PunRPC]
  private void RpcShowMessage(string sender, string message) {
    _stringBuilder.Append($"<b>{sender}: </b>{message}\n");
    _chatLog.text = _stringBuilder.ToString();
  }

  [PunRPC]
  private void RpcShowMessage(string message) {
    _stringBuilder.Append($"<color=#{_serverColorHex}>{message}</color>\n");
    _chatLog.text = _stringBuilder.ToString();
  }

  #endregion
  #region Helper Methods

  private void SendChatMessage(string message) {
    photonView.RPC("RpcShowMessage", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName,
                   message);
    _chatField.text = "";
    _chatField.Select();
    _chatField.ActivateInputField();
  }

  #endregion
}