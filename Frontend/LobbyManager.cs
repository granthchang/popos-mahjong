using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks {
  public static LobbyManager Singleton;
  public event Action<string> OnServerEvent;
  public string TempPlayerName { get; set; }
  public string TempRoomCode { get; set; }
  
  [Header("Room Settings")]
  [SerializeField] private RoomSettings _roomSettings;

  [Header("Room Code Settings")]
  [SerializeField] private string _validRoomPattern = @"^[A-Za-z]{4}$";
  [SerializeField] private string _roomCodeNullText = "Room code may not be null, empty, or whitespace.";
  [SerializeField] private string _roomCodeInvalidText = "Room code must be a 4-letter code.";

  [Header("Player Name Settings")]
  [SerializeField] private string _validNamePattern = @"^[A-Za-z0-9 ]+$";
  [SerializeField] private string _playerNameNullText = "Player name may not be null, empty, or whitespace.";
  [SerializeField] private string _playerNameInvalidText = "Player name must be alphanumeric or spaces.";

  [Header("Server Status Text")]
  [SerializeField] private string _connectingText = "Connecting...";
  [SerializeField] private string _connectedText = "Successfully connected to server.";
  [SerializeField] private string _disconnectedText = "Disconnected from server.";
  [SerializeField] private string _notConnectedText = "Not connected to server.";
  [SerializeField] private string _serverErrorText = "An error occurred on the server.";
  [SerializeField] private string _createdRoomText = "Successfully created room with code:";
  [SerializeField] private string _hostRoomFailedText = "Failed to host room.";
  [SerializeField] private string _foundRoomText = "Successfully found room. Loading game...";

  private bool _isConnected;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  private void Start() {
    if (!PhotonNetwork.IsConnected) {
      _isConnected = false;
      PhotonNetwork.ConnectUsingSettings();
      OnServerEvent?.Invoke(_connectingText);
    }
  }

  public override void OnConnectedToMaster() {
    _isConnected = true;
    OnServerEvent?.Invoke(_connectedText);
  }

  public override void OnDisconnected(DisconnectCause cause) {
    OnServerEvent?.Invoke(_disconnectedText);
  }

  public override void OnErrorInfo(ErrorInfo errorInfo) {
    OnServerEvent?.Invoke(_serverErrorText);
  }

  public void HostRoom() {
    if (!TrySetPlayerName(TempPlayerName)) {
      return;
    }
    if (!_isConnected) {
      OnServerEvent?.Invoke(_notConnectedText);
      return;
    }
    RoomOptions roomOptions = new RoomOptions();
    roomOptions.MaxPlayers = (byte)_roomSettings.RoomSize;
    roomOptions.BroadcastPropsChangeToAll = true;
    PhotonNetwork.CreateRoom(GenerateRoomCode(), roomOptions);
  }

  public override void OnCreatedRoom() {
    OnServerEvent?.Invoke($"{_createdRoomText} {PhotonNetwork.CurrentRoom.Name}");
    PhotonNetwork.CurrentRoom.SetCustomProperties(_roomSettings.ToCustomProperties());
  }

  public override void OnCreateRoomFailed(short returnCode, string message) {
    OnServerEvent?.Invoke(_hostRoomFailedText);
  }

  public void FindRoom() {
    if (!TrySetPlayerName(TempPlayerName) || !TrySetRoomCode(TempRoomCode)) {
      return;
    }
    if (!_isConnected) {
      OnServerEvent?.Invoke(_notConnectedText);
      return;
    }
    PhotonNetwork.JoinRoom(TempRoomCode);
  }

  public void FindRandomRoom() {
    if (!TrySetPlayerName(TempPlayerName)) {
      return;
    }
    if (!_isConnected) {
      OnServerEvent?.Invoke(_notConnectedText);
      return;
    }
    PhotonNetwork.JoinRandomRoom();
  }

  public override void OnJoinRoomFailed(short returnCode, string message) {
    OnServerEvent?.Invoke(message);
  }

  public override void OnJoinRandomFailed(short returnCode, string message) {
    OnServerEvent?.Invoke(message);
  }

  public override void OnJoinedRoom() {
    if (!PhotonNetwork.IsMasterClient) {
      OnServerEvent?.Invoke(_foundRoomText);
    }
    PhotonNetwork.LoadLevel("Game");
  }

  public void QuitGame() {
    if (_isConnected) {
      PhotonNetwork.Disconnect();
    }
    Application.Quit();
  }

  private string GenerateRoomCode() {
    string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string code = "";
    System.Random random = new System.Random();
    for (int i = 0; i < 4; i++) {
      code += chars[random.Next(0, 25)];
    }
    return code;
  }

  private bool TrySetPlayerName(string name) {
    if (string.IsNullOrWhiteSpace(name)) {
      OnServerEvent?.Invoke(_playerNameNullText);
      return false;
    }
    string cleaned = name.Trim();
    if (!Regex.IsMatch(cleaned, _validNamePattern)) {
      OnServerEvent?.Invoke(_playerNameInvalidText);
      return false;
    }
    PhotonNetwork.LocalPlayer.NickName = cleaned;
    return true;
  }

  private bool TrySetRoomCode(string code) {
    if (string.IsNullOrWhiteSpace(code)) {
      OnServerEvent?.Invoke(_roomCodeNullText);
      return false;
    }
    if (!Regex.IsMatch(code, _validRoomPattern)) {
      OnServerEvent?.Invoke(_roomCodeInvalidText);
      return false;
    }
    return true;
  }
}