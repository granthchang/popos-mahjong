using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks {
  public static PlayerManager Singleton;

  [Header("Local Avatar")]
  [SerializeField] private PlayerListItem _localPlayerAvatar;

  [Header("Hand Displays")]
  [Tooltip("Array of hand displays in turn order, starting with the local player. The index of each avatar should match their hand display.")]
  [SerializeField] private HandDisplay[] _handDisplays4p;
  [SerializeField] private HandDisplay[] _handDisplays3p;
  [SerializeField] private HandDisplay[] _handDisplays2p;

  public event Action<Player, Card, bool> OnTurnStarted;
  public event Action<Player> OnDiscardRequested;
  public event Action<Card> OnSelectedCardChanged;
  public event Action<Card> OnDiscard;
  public event Action<Player> OnDiscardConsidered;
  public event Action<Card> OnDiscardUsed;

  public Dictionary<Player, PlayerHand> HandDictionary { get; private set; }

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
    HandDictionary = null;
    foreach (HandDisplay hd in _handDisplays4p) {
      hd.ActivatePanel(false);
      hd.Reset();
    }
  }

  public void ClearHands() {
    photonView.RPC("RpcClientHandleHandsCleared", RpcTarget.All);
  }

  [PunRPC]
  private void RpcClientHandleHandsCleared() {
    foreach (KeyValuePair<Player, PlayerHand> pair in HandDictionary) {
      pair.Value.Reset();
    }
  }

  public void SetHandOwners(List<Player> players) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleHandOwnersSet", RpcTarget.All, players);
    }
  }

  [PunRPC]
  private void RpcClientHandleHandOwnersSet(List<Player> players) {
    HandDisplay[] handDisplays;
    switch (players.Count) {
      case 4:
        handDisplays = _handDisplays4p;
        break;
      case 3:
        handDisplays = _handDisplays3p;
        break;
      case 2:
        handDisplays = _handDisplays2p;
        break;
      default:
        Debug.LogError($"No current support for {players.Count} players.");
        return;
    }
    HandDictionary = new Dictionary<Player, PlayerHand>();
    int localIndex = players.IndexOf(PhotonNetwork.LocalPlayer);
    for (int i = 0; i < players.Count; i++) {
      Player player = players[(localIndex + i) % players.Count];
      PlayerHand hand = new PlayerHand(player, handDisplays[i]);
      if (i == 0) {
        hand.OnSelectedCardChanged += HandleSelectedCardChanged;
      }
      HandDictionary.Add(player, hand);
    }
  }

  public void SendCard(Player target, Card card) {
    foreach (Player p in PhotonNetwork.PlayerList) {
      if (p == target) {
        photonView.RPC("RpcClientHandleCardReceived", p, target, card);
      } else {
        photonView.RPC("RpcClientHandleCardReceived", p, target, Card.Unknown);
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleCardReceived(Player target, Card card) {
    HandDictionary[target].AddCardToHand(card);
  }

  public void SendCards(Player target, List<Card> cards) {
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

  [PunRPC]
  private void RpcClientHandleCardsReceived(Player target, List<Card> cards) {
    foreach (Card c in cards) {
      HandDictionary[target].AddCardToHand(c);
    }
  }

  public void RevealFlower(Player revealer, Card card) {
    photonView.RPC("RpcClientHandleFlowerRevealed", RpcTarget.All, revealer, card);
  }

  [PunRPC]
  private void RpcClientHandleFlowerRevealed(Player revealer, Card card) {
    HandDictionary[revealer].RevealFlower(card);
  }

  public void StartTurn(Player turnPlayer, Card lastDiscard, Player discarder) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleTurnStarted", RpcTarget.All, turnPlayer, lastDiscard, discarder);
    }
  }

  [PunRPC]
  private void RpcClientHandleTurnStarted(Player turnPlayer, Card lastDiscard, Player discarder) {
    bool canUseDiscard = false;
    if (discarder != PhotonNetwork.LocalPlayer) {
      List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(lastDiscard, false);
      lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockablePongsAndKongs(lastDiscard));
      if (turnPlayer == PhotonNetwork.LocalPlayer) {
        lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockableRuns(lastDiscard));
      }
      canUseDiscard = lockableWrappers.Count > 0;
    }
    OnTurnStarted?.Invoke(turnPlayer, lastDiscard, canUseDiscard);
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
      HandDictionary[requestedPlayer].SetDiscardEnabled(true);
    }
  }

  public void SetDiscardEnabled(bool enabled) {
    HandDictionary[PhotonNetwork.LocalPlayer].SetDiscardEnabled(enabled);
  }

  public void Discard(Player target, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscard", RpcTarget.All, target, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscard(Player target, Card discard) {
    HandDictionary[target].RemoveCardFromHand(discard);
    OnDiscard?.Invoke(discard);
  }

  private void HandleSelectedCardChanged(Card selectedCard) {
    OnSelectedCardChanged?.Invoke(selectedCard);
    HandDictionary[PhotonNetwork.LocalPlayer].CloseLockModal();

    // Check for winning hands, hidden kongs, or locked pongs involving this card that could be turned into a kong. If there are any, open the lock modal.
    List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(selectedCard, true);
    LockableWrapper lockableKong = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHiddenKong(selectedCard);
    if (lockableKong != null) {
      lockableWrappers.Add(lockableKong);
    }
    if (lockableWrappers.Count > 0) {
      HandDictionary[PhotonNetwork.LocalPlayer].OpenLockModal(lockableWrappers);
    } 
  }

  public void ConsiderDiscard(Player sender, bool isTargetsTurn, Card discard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscardConsidered", RpcTarget.All, sender, isTargetsTurn, discard);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscardConsidered(Player target, bool isTargetsTurn, Card discard) {
    if (target == PhotonNetwork.LocalPlayer) {
      List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(discard, false);
      lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockablePongsAndKongs(discard));
      if (isTargetsTurn) {
        lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockableRuns(discard));
      }
      HandDictionary[PhotonNetwork.LocalPlayer].OpenLockModal(lockableWrappers);
    }
    OnDiscardConsidered?.Invoke(target);
  }

  public void LockCards(Player target, LockableWrapper wrapper) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleCardsLocked", RpcTarget.All, target, wrapper);
    }
  }

  [PunRPC]
  private void RpcClientHandleCardsLocked(Player target, LockableWrapper wrapper) {
    OnSelectedCardChanged?.Invoke(null);

    HandDictionary[target].LockCards(wrapper);

    if (wrapper.Discard != null) {
      OnDiscardUsed?.Invoke(wrapper.Discard);
    }
  }
}