using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks {
  public static PlayerManager Singleton;
  private Dictionary<Player, HandDisplay> _handDictionary;
  [Header("Local Avatar")]
  [SerializeField] private PlayerListItem _localPlayerAvatar;

  [Header("Hand Displays")]
  [Tooltip("Array of hand displays in turn order, starting with the local player. The index of each avatar should match their hand display.")]
  [SerializeField] private HandDisplay[] _handDisplays4p;
  [SerializeField] private HandDisplay[] _handDisplays3p;
  [SerializeField] private HandDisplay[] _handDisplays2p;

  private List<Card> _localHand;

  public event Action<Player, Card> OnTurnStarted;
  public event Action<Player> OnDiscardRequested;
  public event Action<Card> OnDiscard;
  public event Action<bool> OnCanUseDiscardChecked;

  public event Action<Player> OnDiscardConsidered;
  public event Action<Card> OnDiscardUsed;

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
    if (_localPlayerAvatar != null) {
      _localPlayerAvatar.SetItem(PhotonNetwork.LocalPlayer, 0);
    }
  }

  public void Reset() {
    _handDictionary = null;
    _localHand = null;
    foreach (HandDisplay hd in _handDisplays4p) {
      hd.ActivatePanel(false);
      hd.Reset();
    }
  }

  public void SetHandOwners(List<Player> players) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleHandOwnersSet", RpcTarget.All, players);
    }
  }

  [PunRPC]
  private void RpcClientHandleHandOwnersSet(List<Player> players) {
    HandDisplay[] hands;
    switch (players.Count) {
      case 4:
        hands = _handDisplays4p;
        break;
      case 3:
        hands = _handDisplays3p;
        break;
      case 2:
        hands = _handDisplays2p;
        break;
      default:
        Debug.LogError($"No current support for {players.Count} players.");
        return;
    }
    _handDictionary = new Dictionary<Player, HandDisplay>();
    int localIndex = players.IndexOf(PhotonNetwork.LocalPlayer);
    for (int i = 0; i < players.Count; i++) {
      Player p = players[(localIndex + i) % players.Count];
      HandDisplay h = hands[i];
      h.Reset();
      _handDictionary.Add(p, h);
      h.SetPlayer(p);
      h.ActivatePanel(true);
    }
  }

  public void SendCard(Player target, Card card) {
    if (PhotonNetwork.IsMasterClient) {
      foreach (Player p in PhotonNetwork.PlayerList) {
        if (p == target) {
          photonView.RPC("RpcClientHandleCardReceived", p, target, card);
        } else {
          photonView.RPC("RpcClientHandleCardReceived", p, target, Card.Unknown);
        }
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleCardReceived(Player target, Card card) {
    _handDictionary[target].AddCard(card);
    if (target == PhotonNetwork.LocalPlayer) {
      _localHand.Add(card);
    }
  }

  public void SendCards(Player target, List<Card> cards) {
    if (PhotonNetwork.IsMasterClient) {
      List<Card> unknownCards = new List<Card>();
      for (int i = 0; i < cards.Count; i++) {
        unknownCards.Add(Card.Unknown);
      }
      foreach (Player p in PhotonNetwork.PlayerList) {
        if (p == target) {
          photonView.RPC("RpcClientHandleCardsReceived", p, target, cards);
        } else {
          photonView.RPC("RpcClientHandleCardsReceived", p, target, unknownCards);
        }
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleCardsReceived(Player target, List<Card> cards) {
    foreach (Card c in cards) {
      _handDictionary[target].AddCard(c);
      if (target == PhotonNetwork.LocalPlayer) {
        _localHand.Add(c);
      }
    }
  }

  public void RevealFlower(Player revealer, Card card) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleFlowerRevealed", RpcTarget.All, revealer, card);
    }
  }

  [PunRPC]
  private void RpcClientHandleFlowerRevealed(Player revealer, Card card) {
    _handDictionary[revealer].RevealFlower(card);
  }

  public void StartTurn(Player turnPlayer, Card lastDiscard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleTurnStarted", RpcTarget.All, turnPlayer, lastDiscard);
    }
  }

  [PunRPC]
  private void RpcClientHandleTurnStarted(Player turnPlayer, Card lastDiscard) {
    OnTurnStarted?.Invoke(turnPlayer, lastDiscard);
  }

  public void RequestDiscard(Player requestedPlayer) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscardRequested", RpcTarget.All, requestedPlayer);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscardRequested(Player requestedPlayer) {
    OnDiscardRequested?.Invoke(requestedPlayer);
    if (requestedPlayer == PhotonNetwork.LocalPlayer) {
      _handDictionary[requestedPlayer].SetDiscardEnabled(true);
    }
  }

  public void Discard(Player target, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscard", RpcTarget.All, target, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscard(Player target, Card discard) {
    if (target == PhotonNetwork.LocalPlayer) {
      _localHand.Remove(discard);
    }
    List<List<Card>> possibleSets = GetAllPossibleSets(discard);
    _handDictionary[target].RemoveFromHand(discard);
    OnDiscard?.Invoke(discard);
  }

  public void CheckCanUseDiscard(Player discarder, Player turnPlayer, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleCanUseDiscardChecked", RpcTarget.All, discarder, turnPlayer, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleCanUseDiscardChecked(Player sender, Player turnPlayer, Card discard) {
    List<List<Card>> possibleSets;
    if (sender == PhotonNetwork.LocalPlayer) {
      return;
    } else if (turnPlayer == PhotonNetwork.LocalPlayer) {
      possibleSets = GetAllPossibleSets(discard);
    } else {
      possibleSets = GetPossiblePongs(discard);
    }
    OnCanUseDiscardChecked?.Invoke(possibleSets.Count > 0);
  }

  private List<List<Card>> GetPossiblePongs(Card discard) {
    List<List<Card>> usableSets = new List<List<Card>>();
    int duplicateCount = 0;
    foreach (Card c in _localHand) {
      if (c == discard) {
        duplicateCount++;
      }
    }
    if (duplicateCount >= 2) {
      List<Card> pong = new List<Card>();
      pong.Add(new Card(discard.ID));
      pong.Add(new Card(discard.ID));
      pong.Add(new Card(discard.ID));
      usableSets.Add(pong);
      if (duplicateCount >= 3) {
        List<Card> kong = new List<Card>(pong);
        kong.Add(new Card(discard.ID));
        usableSets.Add(kong);
      }
    }
    return usableSets;
  }

  private List<List<Card>> GetAllPossibleSets(Card discard) {
    List<List<Card>> usableSets = GetPossiblePongs(discard);
    if (!(discard.Suit == Suit.Circle || discard.Suit == Suit.Man || discard.Suit == Suit.Stick)) {
      return usableSets;
    }
    if (discard.Value >= 3) {
      Card c1 = new Card(discard.Suit, discard.Value - 2, 0);
      Card c2 = new Card(discard.Suit, discard.Value - 1, 0);
      if (_localHand.Contains(c1) && _localHand.Contains(c2)) {
        List<Card> set = new List<Card>();
        set.Add(c1);
        set.Add(c2);
        set.Add(discard);
        usableSets.Add(set);
      }
    }
    if (discard.Value >= 2 && discard.Value <= 8) {
      Card c1 = new Card(discard.Suit, discard.Value - 1, 0);
      Card c2 = new Card(discard.Suit, discard.Value + 1, 0);
      if (_localHand.Contains(c1) && _localHand.Contains(c2)) {
        List<Card> set = new List<Card>();
        set.Add(c1);
        set.Add(discard);
        set.Add(c2);
        usableSets.Add(set);
      }
    }
    if (discard.Value <= 7) {
      Card c1 = new Card(discard.Suit, discard.Value + 1, 0);
      Card c2 = new Card(discard.Suit, discard.Value + 2, 0);
      if (_localHand.Contains(c1) && _localHand.Contains(c2)) {
        List<Card> set = new List<Card>();
        set.Add(discard);
        set.Add(c1);
        set.Add(c2);
        usableSets.Add(set);
      }
    }
    return usableSets;
  }

  public void ConsiderDiscard(Player sender, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscardConsidered", RpcTarget.All, sender, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscardConsidered(Player target, Card discard) {
    if (target == PhotonNetwork.LocalPlayer) {
      _handDictionary[PhotonNetwork.LocalPlayer].OpenLockModal(GetAllPossibleSets(discard), discard);
    }
    OnDiscardConsidered?.Invoke(target);
  }

  public void LockCards(Player target, List<Card> set, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleCardsLocked", RpcTarget.All, target, set, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleCardsLocked(Player target, List<Card> set, Card discard) {
    _handDictionary[target].LockCards(set, discard);
    if (target == PhotonNetwork.LocalPlayer) {
      List<Card> cardsToRemove = new List<Card>(set);
      cardsToRemove.Remove(discard);
      foreach(Card c in cardsToRemove) {
        _localHand.Remove(c);
      }
    }
    OnDiscardUsed?.Invoke(discard);
  }

  public void ClearHands() {
    photonView.RPC("RpcClientHandleHandsCleared", RpcTarget.All);
  }

  [PunRPC]
  private void RpcClientHandleHandsCleared() {
    foreach (KeyValuePair<Player, HandDisplay> pair in _handDictionary) {
      pair.Value.Reset();
      pair.Value.SetPlayer(pair.Key);
    }
    _localHand = new List<Card>();
  }
}