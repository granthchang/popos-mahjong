using CardUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HandTestManager : MonoBehaviour {
  public static HandTestManager Singleton;
  private int _handSize = 0;

  private Hand _localHand;
  private Card _discard;

  [SerializeField] private HandDisplay _handDisplay;
  [SerializeField] private CardDisplay _discardDisplay;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  private void Start() {
    _localHand = new Hand();
    _handDisplay.Reset();
    SetDiscard(Card.Unknown);
  }

  public void AddCardToHand(Card card) {
    if (card.Suit == Suit.Flower) {
      _handDisplay.RevealFlower(card);
      return;
    }
    if (_handSize < 14) {
      _localHand.Add(card);
      _handDisplay.AddCard(card);
      _handSize++;
    }
  }

  public void ClearHand() {
    _localHand.Reset();
    _handDisplay.Reset();
    _handSize = 0;
  }

  public void SetDiscard(Card card) {
    _discard = card;
    _discardDisplay.SetCard(card);
  }

  public void CheckForGame() {
    if ((_discard == Card.Unknown && _handSize == 14) || (_discard != Card.Unknown && _handSize == 13)) {
      Debug.Log("Starting check...");
      
      // TODO: Algorithm to find all possible winning hands from this set of cards.
      
      Debug.Log($"Finished check: found {-1} possible winning hands");
    } else {
      Debug.Log("Cannot check: Invalid number of cards in hand.");
    }
  }
}
