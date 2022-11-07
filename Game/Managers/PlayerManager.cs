using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
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
      Debug.Log($"{p.NickName} : {h.name}");
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

  // // Testing
  // private void Update() {
  //   // When L is pressed, Lock set for an opposing player
  //   if (Input.GetKeyDown(KeyCode.L)) {
  //     List<Card> set0 = new List<Card>();
  //     set0.Add(new Card(Suit.Circle, 2, 1));
  //     set0.Add(new Card(Suit.Circle, 3, 1));
  //     set0.Add(new Card(Suit.Circle, 4, 1));

  //     _handDisplays2p[1].LockCards(set0, new Card(Suit.Circle, 3, 1));
  //   }
  // }
}