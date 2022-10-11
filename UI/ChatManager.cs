using Photon.Pun;
using Photon.Realtime;
using System.Text;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviourPunCallbacks {
  [Header("References")]
  [SerializeField] private TMP_Text _chatLog;
  [SerializeField] private TMP_InputField _chatField;

  [Header("Color Settings")]
  [SerializeField] private Color _defaultColor;
  [SerializeField] private Color _serverMessageColor;
  private string _serverColorHex;
  private StringBuilder _stringBuilder;

  private bool _wasInputFocused;

  private void Start() {
    _chatLog.color = _defaultColor;
    _serverColorHex = ColorUtility.ToHtmlStringRGB(_serverMessageColor);
    _stringBuilder = new StringBuilder();

    RpcClientShowMessage("You joined the game.");
  }

  public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
    RpcClientShowMessage($"{newPlayer.NickName} joined the game.");
  }

  public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
    RpcClientShowMessage($"{otherPlayer.NickName} left the game.");
  }

  private void Update() {
    // If enter was hit while focusing input field, send contents as message
    if (Input.GetKeyDown(KeyCode.Return) && _wasInputFocused && !_chatField.isFocused) {
      if (!string.IsNullOrWhiteSpace(_chatField.text)) {
        SendChatMessage(_chatField.text);
      }
    }
    _wasInputFocused = _chatField.isFocused;
  }

  [PunRPC]
  private void RpcClientShowMessage(Player sender, string message) {
    _stringBuilder.Append($"<b>{sender.NickName}: </b>{message}\n");
    _chatLog.text = _stringBuilder.ToString();
  }

  [PunRPC]
  private void RpcClientShowMessage(string message) {
    _stringBuilder.Append($"<color=#{_serverColorHex}>{message}</color>\n");
    _chatLog.text = _stringBuilder.ToString();
  }

  private void SendChatMessage(string message) {
    photonView.RPC("RpcClientShowMessage", RpcTarget.All, PhotonNetwork.LocalPlayer, message);
    _chatField.text = "";
    _chatField.Select();
    _chatField.ActivateInputField();
  }
}