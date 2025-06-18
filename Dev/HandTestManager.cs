using CardUtilities;
using System.Collections.Generic;
using UnityEngine;

public class HandTestManager : MonoBehaviour {
  public static HandTestManager Singleton;
  private PlayerHand _hand;
  private Card _discard;
  [SerializeField] private CardDisplay _discardDisplay;
  [SerializeField] private HandDisplay _handDisplay;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  private void Start() {
    _hand = new PlayerHand(null, _handDisplay);
    SetDiscard(Card.Unknown);
  }

  public void AddCardToHand(Card card) {
    if (card.Suit == Suit.Flower) {
      _hand.RevealFlower(card);
      return;
    }
    _hand.AddCardToHand(card);
  }

  public void ClearHand() {
    _hand.Reset();
  }

  public void SetDiscard(Card card) {
    _discard = card;
    _discardDisplay.SetCard(card);
  }

  public void CheckForGame() {    
    List<LockableWrapper> winningHands = _hand.GetLockableHands(_discard, false);
    _hand.OpenLockModal(winningHands);
  }

  public void FindPongs() {
    List<LockableWrapper> wrappers = new List<LockableWrapper>();
    if (_discard != null && _discard != Card.Unknown) {
      wrappers = _hand.GetLockablePongsAndKongs(_discard);
    }
    _hand.OpenLockModal(wrappers);
  }

  public void FindRuns() {
    List<LockableWrapper> sets = new List<LockableWrapper>();
    if (_discard != null && _discard != Card.Unknown) {
      sets = _hand.GetLockableRuns(_discard);
    }
    _hand.OpenLockModal(sets);
  }
}
