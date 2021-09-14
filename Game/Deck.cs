// Author: Grant Chang
// Date: 24 August 2021

using System.Collections.Generic;

public class Deck {
  #region Events / Fields / References

  private LinkedList<Card> _cards;
  public int Size { get; private set; }

  #endregion
  #region Constructors / Initializers

  public Deck() {
    // TODO: create a new deck

    // 1. Put all cards in a List<Card>
    // 2. Shuffle List<Card>
    // 3. Copy deck into _cards

    Size = 144;
  }

  #endregion
  #region Draw Cards

  public Card DrawFront() {
    Card c = _cards.First.Value;
    _cards.RemoveFirst();
    Size--;
    return c;
  }

  public Card DrawBack() {
    Card c = _cards.Last.Value;
    _cards.RemoveLast();
    Size--;
    return c;
  }

  #endregion
  #region Helper Methods

  private List<Card> GenerateCards() {
    throw new System.NotImplementedException();
    // TODO: generate a list of cards
  }

  private void ShuffleCards(List<Card> cards) {
    throw new System.NotImplementedException();
    // TODO: shuffle cards
  }

  private void SaveCards(List<Card> cards) {
    throw new System.NotImplementedException();
    // TODO: insert cards into _cards
  }

  #endregion
}
