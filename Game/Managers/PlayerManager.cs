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

  public void SendCards(List<Card> cards, Player target) {
    if (PhotonNetwork.IsMasterClient) {
      List<Card> unknownCards = new List<Card>();
      for (int i = 0; i < cards.Count; i++) {
        unknownCards.Add(new Card());
      }
      foreach (Player p in PhotonNetwork.PlayerList) {
        if (p == target) {
          photonView.RPC("RpcClientHandleCardsReceived", p, cards, target);
        } else {
          photonView.RPC("RpcClientHandleCardsReceived", p, unknownCards, target);
        }
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleCardsReceived(List<Card> cards, Player target) {
    foreach (Card c in cards) {
      _handDictionary[target].AddCard(c);
    }
  }

  public void RevealFlower(Card card, Player revealer) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleFlowerRevealed", RpcTarget.All, card, revealer);
    }
  }

  [PunRPC]
  private void RpcClientHandleFlowerRevealed(Card card, Player revealer) {
    _handDictionary[revealer].RevealFlower(card);
  }
}