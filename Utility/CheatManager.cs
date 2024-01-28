using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CardUtilities;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class CheatManager : MonoBehaviour {
  [SerializeField] private bool _2AddsPongToHand = true;

  private List<Card> _winningHand;

  private void Start() {
    _winningHand = new List<Card>();
    for (int i = 1; i <= 4; i++) {
      for (int j = 0; j < 3; j++) {
        _winningHand.Add(new Card(Suit.Circle, i, 1));
      }
    }
    _winningHand.Add(new Card(Suit.Circle, 5, 1));
    _winningHand.Add(new Card(Suit.Circle, 5, 1));
  }

  void Update() {
    if (PhotonNetwork.IsMasterClient) {
      // Pressing 2 will add three of the card on top of the deck to your hand.
      if (Input.GetKeyDown(KeyCode.Alpha2) && _2AddsPongToHand) {
        Debug.Log("cheat 2: added 3 of a kind to local hand");
        List<Card> cards = new List<Card>();
        cards.Add(RoundManager.Singleton._deck.Peek());
        cards.Add(RoundManager.Singleton._deck.Peek());
        cards.Add(RoundManager.Singleton._deck.Peek());
        PlayerManager.Singleton.SendCards(PhotonNetwork.LocalPlayer, cards);
        return;
      }
    }
  }
}
