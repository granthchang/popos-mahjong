// Author: Grant Chang
// Date: 23 August 2021

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// GameManager is the master logic system for the overall game. Each round is handled by
/// an individual HandManager
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  public event Action OnGameStarted;
  public event Action OnGameStopped;
  public event Action OnGameComplete;
  public event Action<List<Player>> OnPlayerListUpdated;
  public static GameManager Singleton;
  public bool IsPlaying { get; private set; }

  private List<Player> _players;

  #endregion
  #region Constructors / Initizlizers

  private void Awake() {
    if (Singleton != null && Singleton != this)
      this.gameObject.SetActive(false);
    Singleton = this;
  }

  private void OnEnable() {
    RoomManager.Singleton.OnPlayerListUpdated += OnRoomListUpdated;
  }

  private void Start() {
    IsPlaying = false;
  }

  #endregion
  #region Start Game

  private IEnumerator StartGameCoroutine(List<Player> players) {
    yield return new WaitForSeconds(Constants.TimeToStart);
    StartGame(players);
  }

  public void StartGame(List<Player> players) {
    _players = players;
    ShufflePlayers();
    photonView.RPC("RpcInvokeOnGameStarted", RpcTarget.All);
  }

  #endregion
  #region Network Event Handlers

  public void OnRoomListUpdated(List<Player> players) {
    if (players.Count >= Constants.PlayersNeeded) {
      StartCoroutine(StartGameCoroutine(players));
    } else {
      OnGameStopped?.Invoke();
      IsPlaying = false;
    }
  }

  #endregion
  #region Remote Procedure Calls (RPCs)

  [PunRPC]
  private void RpcInvokeOnGameStarted() {
    OnGameStarted?.Invoke();
  }

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

  public void ShufflePlayers() {
    lock (_players) {
      int n = _players.Count;
      while (n > 1) {
        n--;
        int k = (int)UnityEngine.Random.Range(0, n + 1);
        Player temp = _players[k];
        _players[k] = _players[n];
        _players[n] = temp;
      }
      ForceSyncPlayerList();
    }
  }

  private void ForceSyncPlayerList() {
    foreach (Player p in _players) {
      photonView.RPC("RpcRemovePlayer", RpcTarget.Others, p);
    }
    foreach (Player p in _players) {
      photonView.RPC("RpcAddPlayer", RpcTarget.Others, p);
    }
  }

  #endregion;
}