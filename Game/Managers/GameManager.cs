using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks {
  public static GameManager Singleton;
  // Starting/stopping game
  public event Action OnGameAboutToStart;
  public event Action OnGameStarted;
  public event Action OnGameStopped;
  public event Action<List<Player>> OnGameFinished;
  private bool _bHasGameFinished = true;
  private IEnumerator _currentCoroutine;
  // Player order
  public List<Player> PlayerList { get; private set; }
  public event Action<List<Player>> OnPlayerListUpdated;
  // Game settings
  public event Action<int> OnMinFansSet;
  public RoomSettings RoomSettings;
  // Game info
  public event Action<int> OnCurrentWindUpdated;
  private int _currentWind;
  private int _currentBank;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;

    if (PhotonNetwork.IsMasterClient) {
      RoundManager.Singleton.OnRoundFinished += HandleRoundFinished;
    }
    RoomSettings = ScriptableObject.CreateInstance<RoomSettings>();
  }

  public void StartGame() {
    if (PhotonNetwork.IsMasterClient) {
      _currentCoroutine = StartGameCoroutine();
      StartCoroutine(_currentCoroutine);
      PlayerList = null;
      photonView.RPC("RpcClientHandleGameAboutToStart", RpcTarget.All);
    }
  }

  // Called on all clients in OnPlayerLeftRoom
  public void StopGame() {
    if (_currentCoroutine != null) {
      StopCoroutine(_currentCoroutine);
    }
    PlayerList = null;
    RoundManager.Singleton.StopRound();
    PropertyManager.Singleton.StopCallback();
    PlayerManager.Singleton.Reset();
    Debug.Log("--- GAME STOPPED ---");
    OnGameStopped?.Invoke();
  }

  [PunRPC]
  private void RpcClientHandleGameAboutToStart() {
    OnGameAboutToStart?.Invoke();
  }

  [PunRPC]
  private void RpcClientHandleGameStarted() {
    Debug.Log("--- GAME STARTED ---");
    OnGameStarted?.Invoke();
  }

  private IEnumerator StartGameCoroutine() {
    int time = RoomSettings.TimeToStart;
    yield return new WaitForSeconds(time);

    // Shuffle player list
    PlayerList = new List<Player>(PhotonNetwork.PlayerList);
    int n = PlayerList.Count;
    while (n > 1) {
      n--;
      int k = (int)UnityEngine.Random.Range(0, n + 1);
      Player temp = PlayerList[k];
      PlayerList[k] = PlayerList[n];
      PlayerList[n] = temp;
    }
    photonView.RPC("RpcClientHandleNewPlayerList", RpcTarget.All, PlayerList);
    // Continues game setup in RpcClientHandleNewPlayerList once we receive the sync
  }

  private void InitializePlayerProperties() {
    // Set player starting values
    for (int i = 0; i < PlayerList.Count; i++) {
      Hashtable hash = new Hashtable();
      hash.Add(Constants.ScoreKey, RoomSettings.StartingScore);
      hash.Add(Constants.FlowerKey, i + 1);
      PlayerList[i].SetCustomProperties(hash);
    }
  }

  private void HandlePlayerPropertiesInitialized() {
    _bHasGameFinished = false;
    RoundManager.Singleton.StartRound(PlayerList, _currentBank, _currentWind);
  }

  // Separate from RpcClientHandleCurrentWindUpdated because this only needs to be called once at the very beginning of the game. It
  // shouldn't after each round because PlayerList is only used to dictate turn order, which will not change.
  [PunRPC]
  private void RpcClientHandleNewPlayerList(List<Player> list) {
    PlayerList = list;
    OnPlayerListUpdated?.Invoke(PlayerList);
    // Continue setting up game now that we have the playerlist
    if (PhotonNetwork.IsMasterClient) {
      PlayerManager.Singleton.SetHandOwners(list);
      _currentBank = 0;
      _currentWind = 1;
      photonView.RPC("RpcClientHandleCurrentWindUpdated", RpcTarget.All, _currentWind);
      photonView.RPC("RpcClientHandleGameStarted", RpcTarget.All);
      PropertyManager.Singleton.UpdatePropertiesWithCallback(InitializePlayerProperties, HandlePlayerPropertiesInitialized);
    }
  }

  [PunRPC]
  private void RpcClientHandleCurrentWindUpdated(int newWind) {
    _currentWind = newWind;
    OnCurrentWindUpdated?.Invoke(_currentWind);
  }

  public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
    RoomSettings.UpdateSettings(propertiesThatChanged);
    if (propertiesThatChanged.ContainsKey(Constants.MinimumFansKey)) {
      OnMinFansSet?.Invoke(RoomSettings.MinimumFans);
    }
  }

  public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
    if (PlayerList != null) {
      OnPlayerListUpdated?.Invoke(PlayerList);
    } else {
      OnPlayerListUpdated?.Invoke(new List<Player>(PhotonNetwork.PlayerList));
    }
  }

  private void HandleRoundFinished(Player winner, Player loser, int fans) {
    if (winner == null) {
      RoundManager.Singleton.StartRound(PlayerList, _currentBank, _currentWind);
    } else {
      PropertyManager.Singleton.UpdatePropertiesWithCallback(
        () => { UpdatePlayerScores(winner, loser, fans); },
        () => { HandlePlayerScoresUpdated(winner); }
      );
    }
  }

  private void UpdatePlayerScores(Player winner, Player loser, int fans) {
    int cost = Constants.GetCostForFans(fans);
    int totalDiff = 0;
    foreach (Player p in PlayerList) {
      if (p != winner) {
        if (loser == null || loser == p) {
          totalDiff += PlayerUtilities.ChangePlayerScore(p, -(cost * 2));
        } else {
          totalDiff += PlayerUtilities.ChangePlayerScore(p, -cost);
        }
      }
    }
    PlayerUtilities.ChangePlayerScore(winner, totalDiff);
  }

  private void HandlePlayerScoresUpdated(Player winner) {
    // Count broke players
    int brokePlayers = 0;
    foreach (Player p in PlayerList) {
      if ((int)p.CustomProperties[Constants.ScoreKey] == 0) {
        brokePlayers++;
      }
    }
    if (brokePlayers >= RoomSettings.MaxBrokePlayers) {
      FinishGame();
      return;
    }
    // Update current bank if the winner wasn't the current bank
    if (PlayerList[_currentBank] != winner) {
      _currentBank = (_currentBank + 1) % PlayerList.Count;
      if (_currentBank == 0) {
        _currentWind++;
        if (_currentWind > RoomSettings.MaxCycles) {
          FinishGame();
          return;
        }
        foreach (Player p in PlayerList) {
          PlayerUtilities.AdvancePlayerFlower(p);
        }
        photonView.RPC("RpcClientHandleCurrentWindUpdated", RpcTarget.All, _currentWind);
      }
    }
    RoundManager.Singleton.StartRound(new List<Player>(PlayerList), _currentBank, _currentWind);
  }

  private void FinishGame() {
    if (!_bHasGameFinished) {
      _bHasGameFinished = true;
      // Sort players in order of score
      List<Player> finalPlacements = new List<Player>(PlayerList);
      Player temp;
      for (int i = 0; i < finalPlacements.Count - 1; i++) {
        for (int j = 0; j < finalPlacements.Count - 1; j++) {
          int leftScore = (int)finalPlacements[j].CustomProperties[Constants.ScoreKey];
          int rightScore = (int)finalPlacements[j + 1].CustomProperties[Constants.ScoreKey];
          if (rightScore > leftScore) {
            temp = finalPlacements[j + 1];
            finalPlacements[j + 1] = finalPlacements[j];
            finalPlacements[j] = temp;
          }
        }
      }
      photonView.RPC("RpcClientHandleGameFinished", RpcTarget.All, finalPlacements);
    }
  }

  [PunRPC]
  private void RpcClientHandleGameFinished(List<Player> finalPlacements) {
    OnGameFinished?.Invoke(finalPlacements);
    Debug.Log("--- GAME FINISHED ---");
    foreach (Player p in finalPlacements) {
    }
  }
}