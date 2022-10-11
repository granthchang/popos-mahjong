using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviourPunCallbacks {
  public static RoundManager Singleton;
  private IEnumerator _currentCoroutine;

  public event Action OnRoundStarted;
  public event Action OnRoundStopped;
  private List<Player> _players;

  public event Action<Player, Player, int> OnRoundFinished;
  private Player _winner = null;
  private Player _loser = null;

  private Deck _deck = null;

  public void StartRound(List<Player> players, int startIndex, int wind) {
    if (PhotonNetwork.IsMasterClient) {
      _players = players;
      _winner = null;
      _loser = null;
      _deck = new Deck();
      _deck.Shuffle();

      Debug.Log("--- ROUND STARTED ---");
      Debug.Log($"Starting player: {_players[startIndex].NickName}");
      Debug.Log($"Starting wind: {Constants.IntToWind(wind)}");

      photonView.RPC("RpcClientHandleRoundStarted", RpcTarget.All);

      for (int i = 0; i < _players.Count; i++) {
        Player currPlayer = _players[(startIndex + i) % _players.Count];
        List<Card> cards = new List<Card>();
        for (int j = 0; j < 13; j++) {
          Card c = _deck.Draw();
          while (c.Suit == Suit.Flower) {
            PlayerManager.Singleton.RevealFlower(c, currPlayer);
            c = _deck.Draw();
          }
          cards.Add(c);
        }
        PlayerManager.Singleton.SendCards(cards, currPlayer);
      }

      // // TEST CASE
      // _currentCoroutine = SetRandomWinner();
      // StartCoroutine(_currentCoroutine);
    }
  }

  public void StopRound() {
    if (_currentCoroutine != null) {
      StopCoroutine(_currentCoroutine);
    }
    _players = null;
    _winner = null;
    _loser = null;
    _deck = null;
    FanApprovalManager.Singleton.StopFanApprovals();
    Debug.Log("--- ROUND STOPPED ---");
    OnRoundStopped?.Invoke();
  }

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;

    if (PhotonNetwork.IsMasterClient) {
      FanApprovalManager.Singleton.OnAllFansApproved += (fans) => { FinishRound(_winner, _loser, fans); };
    }
  }

  // For testing purposes. Sets winner to a random player, and the loser to a random other player
  private IEnumerator SetRandomWinner() {
    yield return new WaitForSeconds(5);

    System.Random rand = new System.Random();
    int winnerIndex = rand.Next(0, GameManager.Singleton.PlayerList.Count);
    _winner = GameManager.Singleton.PlayerList[winnerIndex];

    int loserIndex;
    do {
      loserIndex = rand.Next(0, GameManager.Singleton.PlayerList.Count);
    } while (loserIndex == winnerIndex);
    _loser = GameManager.Singleton.PlayerList[loserIndex];

    List<CardUtilities.Card> hand = new List<CardUtilities.Card>();
    for (int i = 0; i < 13; i++) {
      hand.Add(_deck.Draw());
    }

    EndRound(_winner, _loser, hand);
  }

  private void EndRound(Player winner, Player loser, List<CardUtilities.Card> hand) {
    _winner = winner;
    _loser = loser;
    FanApprovalManager.Singleton.StartFanApprovals(winner, loser, hand);
  }

  public void FinishRound(Player winner, Player loser, int fans) {
    photonView.RPC("RpcClientHandleRoundFinished", RpcTarget.All, winner, loser, fans);
  }

  [PunRPC]
  private void RpcClientHandleRoundStarted() {
    OnRoundStarted?.Invoke();
  }

  [PunRPC]
  private void RpcClientHandleRoundFinished(Player winner, Player loser, int fans) {
    Debug.Log("--- ROUND FINISHED ---");
    Debug.Log($"Winner: {winner.NickName}");
    Debug.Log($"Loser: {loser.NickName}");
    Debug.Log($"Fans: {fans}");
    OnRoundFinished?.Invoke(winner, loser, fans);
  }
}