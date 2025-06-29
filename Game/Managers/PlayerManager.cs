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

  public event Action<Player, Card, bool, bool> OnTurnStarted;
  public event Action<Player> OnDiscardRequested;
  public event Action<Card, bool> OnSelectedCardChanged;
  public event Action<Card> OnDiscard;
  public event Action<Player> OnDiscardConsidered;
  public event Action<Card> OnDiscardUsed;

  public Dictionary<Player, PlayerHand> HandDictionary { get; private set; }

  private Set _lastConvertedKong = null;
  private bool _isSelectingDiscard = false;

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
    _lastConvertedKong = null;
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
    Card cardToAdd = card;
    if (target == PhotonNetwork.LocalPlayer && Constants.ForceDrawCard >= 0) {
      cardToAdd = new Card(Constants.ForceDrawCard);
    }
    HandDictionary[target].AddCardToHand(cardToAdd);
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

  public void StartTurn(Player turnPlayer, Card lastDiscard, Player discarder, bool canDraw, bool isFirstTurn) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleTurnStarted", RpcTarget.All, turnPlayer, lastDiscard, discarder, canDraw, isFirstTurn);
    }
  }

  [PunRPC]
  private void RpcClientHandleTurnStarted(Player turnPlayer, Card lastDiscard, Player discarder, bool canDraw, bool isFirstTurn) {
    // On first turn, all players check for Disconnect. If so, enable card selection.
    if (isFirstTurn) {
      if (HandDictionary[PhotonNetwork.LocalPlayer].GetLockableDisconnect() != null) {
        _isSelectingDiscard = false;
        HandDictionary[PhotonNetwork.LocalPlayer].SetCardSelectionEnabled(true);
      }
      OnTurnStarted?.Invoke(turnPlayer, lastDiscard, false, canDraw);
    }
    // If not first turn, check if you can use the discard.
    else {
      bool canUseDiscard = false;
      if (lastDiscard != null && discarder != PhotonNetwork.LocalPlayer) {
        List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(lastDiscard, false);
        lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockablePongsAndKongs(lastDiscard));
        if (turnPlayer == PhotonNetwork.LocalPlayer) {
          lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockableRuns(lastDiscard));
        }
        canUseDiscard = lockableWrappers.Count > 0;
      }
      OnTurnStarted?.Invoke(turnPlayer, lastDiscard, canUseDiscard, canDraw);
    }
  }

  public void RequestDiscard(Player requestedPlayer) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleDiscardRequested", RpcTarget.All, requestedPlayer);
    }
  }

  [PunRPC]
  private void RpcClientHandleDiscardRequested(Player requestedPlayer) {
    OnDiscardRequested?.Invoke(requestedPlayer);
    // If discard is requested for local player, enable discard for the local hand
    if (requestedPlayer == PhotonNetwork.LocalPlayer) {
      _isSelectingDiscard = true;
      HandDictionary[requestedPlayer].SetCardSelectionEnabled(true);
    }
    // Otherwise, if there is a converted kong, check if the local player can win off of it
    else if (_lastConvertedKong != null) {
      List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(_lastConvertedKong.StartingCard, false);
      if (lockableWrappers.Count > 0) {
        HandDictionary[requestedPlayer].SetLockedSetButtonEnabled(_lastConvertedKong, true);
      }
    }
  }

  public void SetCardSelectionEnabled(bool enabled) {
    HandDictionary[PhotonNetwork.LocalPlayer].SetCardSelectionEnabled(enabled);
  }

  private void HandleSelectedCardChanged(Card selectedCard) {
    OnSelectedCardChanged?.Invoke(selectedCard, _isSelectingDiscard);
    HandDictionary[PhotonNetwork.LocalPlayer].CloseLockModal(); // TODO: remove this call so that we don't get extra Cancel Consider calls

    List<LockableWrapper> lockableWrappers = new List<LockableWrapper>();
    // If using selecting for discard, we should check for winning hands and hidden kongs
    if (_isSelectingDiscard) {
      lockableWrappers.AddRange(HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(selectedCard, true));
      LockableWrapper lockableKong = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHiddenKong(selectedCard);
      if (lockableKong != null) {
        lockableWrappers.Add(lockableKong);
      }
    }
    // If not using a discard, only look for disconnect
    else {
      LockableWrapper disconnect = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableDisconnect();
      if (disconnect != null) {
        lockableWrappers.Add(disconnect);
      }
    }
    // Present the lock modal if there is anything lockable
    if (lockableWrappers.Count > 0) {
      HandDictionary[PhotonNetwork.LocalPlayer].OpenLockModal(lockableWrappers);
    }
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

    // If there are any locked kongs that could have been used to win, disable them because a card has been discarded now.
    if (_lastConvertedKong != null) {
      foreach (KeyValuePair<Player, PlayerHand> pair in HandDictionary) {
        if (pair.Key != PhotonNetwork.LocalPlayer) {
          pair.Value.SetLockedSetButtonEnabled(_lastConvertedKong, false);
        }
      }
      _lastConvertedKong = null;
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

  public void ConsiderKong(Player sender, Player turnPlayer, Card kongCard) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleKongConsidered", RpcTarget.All, sender, turnPlayer, kongCard);
    }
  }

  [PunRPC]
  private void RpcClientHandleKongConsidered(Player sender, Player turnPlayer, Card kongCard) {
    // If this is the player considering, open the lock modal.
    if (sender == PhotonNetwork.LocalPlayer) {
      List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(kongCard, false);
      HandDictionary[PhotonNetwork.LocalPlayer].OpenLockModal(lockableWrappers);
    }
    // If this is the player who revealed the kong, disable their discard until sender has decided whether they'll use it.
    else if (turnPlayer == PhotonNetwork.LocalPlayer) {
      HandDictionary[turnPlayer].SetCardSelectionEnabled(false);
    }
    OnDiscardConsidered?.Invoke(sender);
  }

  public void LockCards(Player target, LockableWrapper wrapper) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleCardsLocked", RpcTarget.All, target, wrapper);
    }
  }

  [PunRPC]
  private void RpcClientHandleCardsLocked(Player target, LockableWrapper wrapper) {
    OnSelectedCardChanged?.Invoke(null, false);
    HandDictionary[target].LockCards(wrapper, out bool ConvertedPongToKong);

    // If using the discard, fire event to remove it.
    if (wrapper.Discard != null) {
      OnDiscardUsed?.Invoke(wrapper.Discard);
    }
    // If not using the discard, check a pong was converted to a kong. If so, check if the local player can use that card to win.
    else if (ConvertedPongToKong && (target != PhotonNetwork.LocalPlayer)) {
      Card lockedCard = wrapper.Sets[0].Cards[0];
      List<LockableWrapper> lockableWrappers = HandDictionary[PhotonNetwork.LocalPlayer].GetLockableHands(lockedCard, false);
      if (lockableWrappers.Count > 0) {
        _lastConvertedKong = wrapper.Sets[0];
        HandDictionary[target].SetLockedSetButtonEnabled(wrapper.Sets[0], true);
      }
    }
  }
}