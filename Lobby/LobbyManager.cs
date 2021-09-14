// Author: Grant Chang
// Date: 14 August 2021

using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// LobbyManager handles basic networking concerns for the lobby scene like connecting to the
/// master server and hosting/finding a room. When a room is created or found, use RoomManager.
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  public event Action<string> OnInvalidPlayerName;
  public event Action<string> OnInvalidRoomCode;

  public static LobbyManager Singleton;

  private const string _roomCodeNull = "Room code may not be null, empty, or whitespace.";
  private const string _validNamePattern = @"^[A-Za-z0-9 ]+$";
  private const string _playerNameNull = "Player names may not be null, empty, or whitespace.";
  private const string _playerNameInvalid = "Player names must be alphanumeric or spaces.";

  public string TempPlayerName { get; set; }
  public string TempRoomCode { get; set; }

  #endregion
  #region Constructors / Initializers

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  private void Start() {
    if (!PhotonNetwork.IsConnected) {
      PhotonNetwork.ConnectUsingSettings();
    }
  }

  #endregion
  #region Host / Join a Room

  public void CreateRoom() {
    if (TrySetPlayerName(TempPlayerName)) {
      RoomOptions roomOptions = new RoomOptions();
      roomOptions.MaxPlayers = 4;
      EnterRoomParams enterRoomParams = new EnterRoomParams();
      PhotonNetwork.CreateRoom(GenerateRoomCode(), roomOptions);
    }
  }

  public void FindRoom() {
    if (string.IsNullOrWhiteSpace(TempRoomCode)) {
      OnInvalidRoomCode?.Invoke(_roomCodeNull);
      return;
    }

    if (TrySetPlayerName(TempPlayerName)) {
      PhotonNetwork.JoinRoom(TempRoomCode);
    }
  }

  public void JoinRandomRoom() {
    if (TrySetPlayerName(TempPlayerName)) {
      PhotonNetwork.JoinRandomRoom();
    }
  }

  public override void OnJoinedRoom() {
    PhotonNetwork.LoadLevel("Room");
  }

  #endregion
  #region Disconnect / Quit

  public void QuitGame() {
    PhotonNetwork.Disconnect();
    Application.Quit();
  }

  #endregion
  #region Helper Methods

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
      OnInvalidPlayerName?.Invoke(_playerNameNull);
      return false;
    }

    string cleaned = name.Trim();

    if (!Regex.IsMatch(cleaned, _validNamePattern)) {
      OnInvalidPlayerName?.Invoke(_playerNameInvalid);
      return false;
    }

    PhotonNetwork.LocalPlayer.NickName = cleaned;
    return true;
  }

  #endregion
}
