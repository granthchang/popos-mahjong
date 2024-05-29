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
  private int _turnIndex;

  public event Action<Player, Player, int> OnRoundFinished;
  private Player _winner = null;
  private Player _loser = null;

  public Deck _deck = null;
  private Card _lastDiscard = null;
  private Player _lastDiscarder = null;

  public void StartRound(List<Player> players, int startIndex, int wind) {
    if (PhotonNetwork.IsMasterClient) {
      _players = players;
      _turnIndex = startIndex;
      _winner = null;
      _loser = null;
      _deck = new Deck();
      _deck.Shuffle();

      Debug.Log("--- ROUND STARTED ---");
      Debug.Log($"Starting player: {_players[startIndex].NickName}");
      Debug.Log($"Starting wind: {Constants.IntToWind(wind)}");

      photonView.RPC("RpcClientHandleRoundStarted", RpcTarget.All);

      PlayerManager.Singleton.ClearHands();
      for (int i = 0; i < _players.Count; i++) {
        Player currPlayer = _players[(startIndex + i) % _players.Count];
        List<Card> cards = new List<Card>();
        for (int j = 0; j < 13; j++) {
          Card c = _deck.Draw();
          while (c.Suit == Suit.Flower) {
            PlayerManager.Singleton.RevealFlower(currPlayer, c);
            c = _deck.Draw();
          }
          cards.Add(c);
        }
        PlayerManager.Singleton.SendCards(currPlayer, cards);
      }

      PlayerManager.Singleton.StartTurn(_players[startIndex], null, null);
    }
  }

  public void RequestDraw() {
    photonView.RPC("RpcMasterHandleDrawRequested", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcMasterHandleDrawRequested(Player sender) {
    Card c = _deck.Draw();
    while (c.Suit == Suit.Flower) {
      PlayerManager.Singleton.RevealFlower(sender, c);
      c = _deck.Draw();
    }
    PlayerManager.Singleton.SendCard(sender, c);
    PlayerManager.Singleton.RequestDiscard(sender);
  }

  public void Discard(Card discard) {
    photonView.RPC("RpcMasterHandleDiscard", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, discard);
  }

  [PunRPC]
  private void RpcMasterHandleDiscard(Player sender, Card discard) {
    PlayerManager.Singleton.Discard(sender, discard);
    _lastDiscard = discard;
    _lastDiscarder = sender;
    _turnIndex = (_players.IndexOf(sender) + 1) % _players.Count;
    PlayerManager.Singleton.StartTurn(_players[_turnIndex], discard, sender);
  }

  public void ConsiderDiscard() {
    photonView.RPC("RpcMasterHandleDiscardConsidered", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcMasterHandleDiscardConsidered(Player sender) {
    PlayerManager.Singleton.ConsiderDiscard(sender, _players[_turnIndex] == sender, _lastDiscard);
  }

  public void LockCards(LockableWrapper wrapper) {
    photonView.RPC("RpcMasterHandleCardsLocked", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, wrapper);
  }

  [PunRPC]
  private void RpcMasterHandleCardsLocked(Player sender, LockableWrapper wrapper) {
    PlayerManager.Singleton.LockCards(sender, wrapper);

    foreach(Set setToLock in wrapper.Sets) {
      // If there's an eye in the locked set, it's a winning hand.
      if (setToLock.Type == SetType.Eye) {
        EndRound(sender, wrapper.Discard == null ? null : _lastDiscarder, PlayerManager.Singleton.HandDictionary[sender].LockedSets);
        return;
      }
      // Otherwise, if the set was a kong, send a card to replace the extra card in the set.
      if (setToLock.Type == SetType.Kong) {
        Card c = _deck.Draw();
        while (c.Suit == Suit.Flower) {
          PlayerManager.Singleton.RevealFlower(sender, c);
          c = _deck.Draw();
        }
        PlayerManager.Singleton.SendCard(sender, c);
      }
      // Request discard once we've sent any necessary cards.
      PlayerManager.Singleton.RequestDiscard(sender);
    }
  }

  public void CancelConsiderDiscard() {
    photonView.RPC("RpcMasterHandleConsiderDiscardCancelled", RpcTarget.MasterClient);
  }

  [PunRPC]
  private void RpcMasterHandleConsiderDiscardCancelled() {
    PlayerManager.Singleton.StartTurn(_players[_turnIndex], _lastDiscard, _lastDiscarder);
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

  private void EndRound(Player winner, Player loser, List<Set> hand) {
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
    OnRoundFinished?.Invoke(winner, loser, fans);
  }
}