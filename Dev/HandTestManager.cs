using CardUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HandTestManager : MonoBehaviour {
  public static HandTestManager Singleton;
  private int _handSize = 0;

  private List<Card> _localHand;
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
    _localHand = new List<Card>();
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
    _localHand = new List<Card>();
    _handDisplay.Reset();
    _handSize = 0;
  }

  public void SetDiscard(Card card) {
    _discard = card;
    _discardDisplay.SetCard(card);
  }

  public void CheckForGame() {    
    List<List<Card>> winningHands = Hand.GetWinningHands(_discard, _localHand);
    _handDisplay.OpenLockModal(winningHands, _discard);
  }

  public void FindPongs() {
    List<List<Card>> sets = new List<List<Card>>();
    if (_discard != null && _discard != Card.Unknown) {
      sets = Hand.GetPongsAndKongs(_discard, _localHand, false);
    }
    _handDisplay.OpenLockModal(sets, _discard);
  }

  public void FindRuns() {
    List<List<Card>> sets = new List<List<Card>>();
    if (_discard != null && _discard != Card.Unknown) {
      sets = Hand.GetRuns(_discard, _localHand, false);
    }
    _handDisplay.OpenLockModal(sets, _discard);
  }
}
