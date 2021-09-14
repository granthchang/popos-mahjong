// Author: Grant Chang
// Date: 23 August 2021

using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

/// <summary>
/// RoomManager manages a list of the players in the room. There is one "MasterClient" responsible
/// for syncing new clients to it.
/// </summary>
public class RoomManager : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  public event Action<List<Player>> OnPlayerListUpdated;
  public static RoomManager Singleton;
  private List<Player> _players = new List<Player>();

  #endregion
  #region Constructors / Initializers

  private void Awake() {
    if (Singleton != null && Singleton != this)
      this.gameObject.SetActive(false);
    Singleton = this;
  }

  private void Start() {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcAddPlayer", PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer);
    }
  }

  #endregion
  #region Disconnect Functions

  public void LeaveRoom() {
    PhotonNetwork.LeaveRoom();
  }

  #endregion
  #region Network Event Handlers

  public override void OnLeftRoom() {
    PhotonNetwork.LoadLevel("Lobby");
  }

  public override void OnPlayerEnteredRoom(Player newPlayer) {
    lock (_players) {
      _players.Add(newPlayer);
      OnPlayerListUpdated?.Invoke(new List<Player>(_players));
      if (PhotonNetwork.IsMasterClient) {
        foreach (Player p in _players) {
          photonView.RPC("RpcAddPlayer", newPlayer, p);
        }
      }
    }
  }

  public override void OnPlayerLeftRoom(Player otherPlayer) {
    lock (_players) {
      _players.Remove(otherPlayer);
      OnPlayerListUpdated?.Invoke(new List<Player>(_players));
    }
  }

  #endregion
  #region Remote Procedure Calls (RPCs)

  [PunRPC]
  private void RpcAddPlayer(Player newPlayer) {
    lock (_players) {
      _players.Add(newPlayer);
      OnPlayerListUpdated?.Invoke(new List<Player>(_players));
    }
  }

  [PunRPC]
  private void RpcRemovePlayer(Player otherPlayer) {
    lock (_players) {
      _players.Remove(otherPlayer);
      OnPlayerListUpdated?.Invoke(new List<Player>(_players));
    }
  }

  #endregion
  #region Helper Methods

  #endregion
}