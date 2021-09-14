// Author: Grant Chang
// Date: 14 August 2021

using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

/// <summary>
/// EventLog displays information about server actions and responses in the Lobby scene. It shows
/// information like failing to create a room or being unable to find an open game. If creating
/// a room or joining/finding a game is successful, the scene switches to the Room scene.
/// </summary>
public class EventLog : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  private const string _connectedToMasterText = "Successfully connected to master server.";
  private const string _disconnectedFromMasterText = "You disconnected from the server.";
  private const string _errorText = "An error occurred on the server.";
  private const string _createdRoom = "Successfully created room with code:";
  private const string _joinedRoom = "Successfully joined room.";
  [SerializeField] private TMP_Text _displayText;

  #endregion
  #region Constructors / Initializers

  private void Start() {
    LobbyManager.Singleton.OnInvalidPlayerName += s => _displayText.text = s;
    LobbyManager.Singleton.OnInvalidRoomCode += s => _displayText.text = s;
  }

  #endregion
  #region Master Server Events

  public override void OnConnectedToMaster() {
    _displayText.text = _connectedToMasterText;
  }

  public override void OnDisconnected(DisconnectCause cause) {
    _displayText.text = _disconnectedFromMasterText;
  }

  public override void OnErrorInfo(ErrorInfo errorInfo) {
    _displayText.text = $"{_errorText} {errorInfo.Info}";
  }

  #endregion
  #region Create / Join Room

  public override void OnCreatedRoom() {
    _displayText.text = $"{_createdRoom} {PhotonNetwork.CurrentRoom.Name}";
  }

  public override void OnCreateRoomFailed(short returnCode, string message) {
    _displayText.text = $"{message}. {returnCode}";
  }

  public override void OnJoinedRoom() {
    if (!PhotonNetwork.IsMasterClient) {
      _displayText.text = _joinedRoom;
    }
  }

  public override void OnJoinRoomFailed(short returnCode, string message) {
    _displayText.text = message + ".";
  }

  public override void OnJoinRandomFailed(short returnCode, string message) {
    _displayText.text = message + ".";
  }

  #endregion
}
