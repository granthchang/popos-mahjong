using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class RoomManager : MonoBehaviourPunCallbacks {
  public static RoomManager Singleton;
  public event Action OnRoomManagerStarted;
  public event Action<List<Player>> OnPlayerListUpdated;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  private void Start() {
#if UNITY_EDITOR
    // Prevents PIE errors before DefaultSceneLoader loads the initial scene
    if (PhotonNetwork.CurrentRoom == null) {
      return;
    }
#endif
    Reset();
  }

  public void Reset() {
    PlayerUtilities.ClearPlayerProperties(PhotonNetwork.LocalPlayer);
    OnRoomManagerStarted?.Invoke();
    GameManager.Singleton.StopGame();
    this.OnPlayerEnteredRoom(PhotonNetwork.LocalPlayer);
  }

  public override void OnPlayerEnteredRoom(Player newPlayer) {
    OnPlayerListUpdated?.Invoke(new List<Player>(PhotonNetwork.PlayerList));
    if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
      GameManager.Singleton.StartGame();
    }
  }

  public override void OnPlayerLeftRoom(Player otherPlayer) {
    GameManager.Singleton.StopGame();
    PlayerUtilities.ClearPlayerProperties(PhotonNetwork.LocalPlayer);
    OnPlayerListUpdated?.Invoke(new List<Player>(PhotonNetwork.PlayerList));
  }

  public void LeaveRoom() {
    PhotonNetwork.LeaveRoom();
  }

  public override void OnLeftRoom() {
    PhotonNetwork.LoadLevel("Lobby");
  }

  public override void OnDisconnected(DisconnectCause cause) {
    PhotonNetwork.LoadLevel("Lobby");
  }
}