using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
  public event Action<int> OnDeckSizeChanged;
  
  private Card _lastDiscard = null;
  private Player _lastDiscarder = null;
  private bool _hasLockedSetThisTurn;

  public void StartRound(List<Player> players, int startIndex) {
    if (PhotonNetwork.IsMasterClient) {
      _players = players;
      _turnIndex = startIndex;
      _winner = null;
      _loser = null;
      _deck = new Deck();
      _deck.OnSizeChanged += HandleDeckSizeChanged;
      _deck.Shuffle();

      Debug.Log("--- ROUND STARTED ---");

      photonView.RPC("RpcClientHandleRoundStarted", RpcTarget.All);

      PlayerManager.Singleton.ClearHands();
      for (int i = 0; i < _players.Count; i++) {
        Player currPlayer = _players[(startIndex + i) % _players.Count];
        List<Card> cards = new List<Card>();
        if (currPlayer == PhotonNetwork.LocalPlayer && Constants.ForceDealAngels > 0) {
          cards.AddRange(Constants.Angels);
        }
        else {
          for (int j = 0; j < 13; j++) {
            Card c = _deck.Draw();
            while (c.Suit == Suit.Flower) {
              PlayerManager.Singleton.RevealFlower(currPlayer, c);
              c = _deck.Draw();
            }
            cards.Add(c);
          }
        }
        PlayerManager.Singleton.SendCards(currPlayer, cards);
      }
      _hasLockedSetThisTurn = false;
      PlayerManager.Singleton.StartTurn(_players[startIndex], null, null, true, true);
    }
  }

  public void RequestDraw() {
    photonView.RPC("RpcMasterHandleDrawRequested", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcMasterHandleDrawRequested(Player requester) {
    if (DrawCard(requester)) {
      PlayerManager.Singleton.RequestDiscard(requester);
    }
    // If player can't draw successfully, start next turn with no discard to trigger the deck exhaustion behavior.
    else {
      _turnIndex = (_players.IndexOf(requester) + 1) % _players.Count;
      _hasLockedSetThisTurn = false;
      PlayerManager.Singleton.StartTurn(_players[_turnIndex], null, null, false, false);
    }
  }

  // Returns true if a playable card was sent to the target player.
  // Returns false if the deck is has been exhausted and a playable card cannot be sent.
  private bool DrawCard(Player target) {
    if (_deck.Size == 0) {
      return false;
    }
    Card c = _deck.Draw();
    while (c.Suit == Suit.Flower) {
      PlayerManager.Singleton.RevealFlower(target, c);
      if (_deck.Size == 0) {
        return false;
      }
      c = _deck.Draw();
    }
    PlayerManager.Singleton.SendCard(target, c);
    return true;
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
    _hasLockedSetThisTurn = false;
    PlayerManager.Singleton.StartTurn(_players[_turnIndex], discard, sender, _deck.Size > 0, false);
  }

  public void ConsiderDiscard() {
    photonView.RPC("RpcMasterHandleDiscardConsidered", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcMasterHandleDiscardConsidered(Player sender) {
    PlayerManager.Singleton.ConsiderDiscard(sender, _players[_turnIndex] == sender, _lastDiscard);
  }

  public void ConsiderKong(Card kongCard) {
    photonView.RPC("RpcMasterHandleKongConsidered", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, kongCard);
  }

  [PunRPC]
  private void RpcMasterHandleKongConsidered(Player sender, Card kongCard) {
    PlayerManager.Singleton.ConsiderKong(sender, _players[_turnIndex], kongCard);
  }
  
  public void CancelConsider() {
    photonView.RPC("RpcMasterHandleConsiderCancelled", RpcTarget.MasterClient);
  }

  [PunRPC]
  private void RpcMasterHandleConsiderCancelled() {
    if (_hasLockedSetThisTurn) {
      PlayerManager.Singleton.RequestDiscard(_players[_turnIndex]);
    } else {
      PlayerManager.Singleton.StartTurn(_players[_turnIndex], _lastDiscard, _lastDiscarder, _deck.Size > 0, false);
    }
  }

  public void LockCards(LockableWrapper wrapper) {
    photonView.RPC("RpcMasterHandleCardsLocked", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, wrapper);
  }

  [PunRPC]
  private void RpcMasterHandleCardsLocked(Player sender, LockableWrapper wrapper) {
    PlayerManager.Singleton.LockCards(sender, wrapper);

    foreach (Set setToLock in wrapper.Sets) {
      // If there's an eye in the locked set, it's a winning hand.
      if (setToLock.Type == SetType.Eye || setToLock.Type == SetType.Other) {
        EndRound(sender, ((wrapper.Discard == null) && (!_hasLockedSetThisTurn)) ? null : _lastDiscarder, PlayerManager.Singleton.HandDictionary[sender].LockedSets);
        return;
      }
      // Otherwise, if the set was a kong, send a card to replace the extra card in the set.
      if (setToLock.Type == SetType.Kong) {
        if (!DrawCard(sender)) {
          return;
        }
      }
    }
    _hasLockedSetThisTurn = true;
    PlayerManager.Singleton.RequestDiscard(sender);
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

  public void HandleDeckSizeChanged(int newSize) {
    photonView.RPC("RpcClientHandleDeckSizeChanged", RpcTarget.All, newSize);
  }

  [PunRPC]
  private void RpcClientHandleDeckSizeChanged(int newSize) {
    OnDeckSizeChanged?.Invoke(newSize);
  }

  public void EndRoundFromExhaustedDeck() {
    photonView.RPC("RpcMasterHandleEndRoundFromExhaustedDeck", RpcTarget.MasterClient);
  }

  [PunRPC]
  private void RpcMasterHandleEndRoundFromExhaustedDeck() {
    EndRound(null, null, new List<Set>());
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