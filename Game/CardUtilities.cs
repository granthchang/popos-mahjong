using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CardUtilities {
  public enum Suit {
    None = 0,
    Flower = 1,
    Dragon = 2,
    Wind = 3,
    Circle = 4,
    Man = 5,
    Stick = 6,
  }

  // Representation of a single mahjong card.
  public class Card : IComparable<Card> {
    public static Card Unknown = new Card(0);

    public Suit Suit { get; private set; }
    public int Value { get; private set; }
    public int ID { get; private set; }

    public static int SuitToInt(Suit suit) {
      switch (suit) {
        default:
          return 0;
        case Suit.Flower:
          return 1;
        case Suit.Dragon:
          return 2;
        case Suit.Wind:
          return 3;
        case Suit.Circle:
          return 4;
        case Suit.Man:
          return 5;
        case Suit.Stick:
          return 6;
      }
    }

    public static Suit IntToSuit(int suitId) {
      switch (suitId) {
        default:
          return Suit.None;
        case 1:
          return Suit.Flower;
        case 2:
          return Suit.Dragon;
        case 3:
          return Suit.Wind;
        case 4:
          return Suit.Circle;
        case 5:
          return Suit.Man;
        case 6:
          return Suit.Stick;
      }
    }

    public Card() {
      Suit = Suit.None;
      Value = 0;
      ID = 000;
    }

    public Card(Suit suit, int value, int copyIndex) {
      Suit = suit;
      Value = value;
      ID += SuitToInt(suit) * 100;
      ID += value * 10;
      ID += copyIndex;
    }

    public Card(int id) {
      Suit = IntToSuit(id / 100);
      Value = (id % 100) / 10;
      ID = id;
    }

    public override string ToString() {
      return $"{Suit},{Value},{ID}";
    }

    public int CompareTo(Card card) {
      if (ReferenceEquals(card, null) || this.ID / 10 < card.ID / 10) {
        return -1;
      }
      if (this.ID / 10 == card.ID / 10) {
        return 0;
      }
      return 1;
    }

    public override bool Equals(object obj) {
      if (obj is Card) {
        return this.CompareTo((Card)obj) == 0;
      }
      return false;
    }

    public override int GetHashCode() {
      return this.ID;
    }

    public static bool operator ==(Card left, Card right) {
      if(ReferenceEquals(left, null)) {
        return ReferenceEquals(right, null);
      }
      return left.CompareTo(right) == 0;
    }

    public static bool operator !=(Card left, Card right) {
      if (ReferenceEquals(left, null)) {
        return !ReferenceEquals(right, null);
      }
      return left.CompareTo(right) != 0;
    }

    public static void SortCardsInTransform(Transform transform) {
      for (int i = 0; i < transform.childCount - 1; i++) {
        for (int j = 0; j < transform.childCount - 1; j++) {
          Card left = transform.GetChild(j).GetComponent<CardDisplay>().Card;
          Card right = transform.GetChild(j + 1).GetComponent<CardDisplay>().Card;
          if (left.CompareTo(right) == 1) {
            transform.GetChild(j + 1).SetSiblingIndex(j);
          }
        }
      }
    }

    public static void ClearCardsInTransform(Transform transform) {
      foreach (Transform child in transform) {
        if (child.GetComponent<CardDisplay>() != null) {
          GameObject.Destroy(child.gameObject);
        }
      }
    }
  }

  public static class Hand {

    /// <summary>
    /// Retuns the number of duplicates of this card that this hand contains.
    /// </summary>
    public static int CountCard(Card targetCard, List<Card> hand) {
      int duplicateCount = 0;
      if (targetCard != null) {
        foreach (Card c in hand) {
          if (c == targetCard) {
            duplicateCount++;
          }
        }
      }
      return duplicateCount;
    }

    /// <summary>
    /// Returns a list of all pongs and kongs this hand can take containing the target card.
    /// </summary>
    public static List<List<Card>> GetPongsAndKongs(Card targetCard, List<Card> hand, bool requireTargetCard) {
      List<List<Card>> usableSets = new List<List<Card>>();
      if (targetCard != null) {
        int duplicateCount = CountCard(targetCard, hand);
        if (duplicateCount >= (requireTargetCard ? 3 : 2)) {
          List<Card> pong = new List<Card>();
          pong.Add(new Card(targetCard.ID));
          pong.Add(new Card(targetCard.ID));
          pong.Add(new Card(targetCard.ID));
          usableSets.Add(pong);
          if (duplicateCount >= (requireTargetCard ? 4 : 3)) {
            List<Card> kong = new List<Card>(pong);
            kong.Add(new Card(targetCard.ID));
            usableSets.Add(kong);
          }
        }
      }
      return usableSets;
    }

    /// <summary>
    /// Returns a list of all runs this hand can take containing the target card.
    /// </summary>
    public static List<List<Card>> GetRuns(Card targetCard, List<Card> hand, bool requireTargetCard) {
      List<List<Card>> usableSets = new List<List<Card>>();
      if (targetCard != null && (!requireTargetCard || hand.Contains(targetCard))) {
        if (!(targetCard.Suit == Suit.Circle || targetCard.Suit == Suit.Man || targetCard.Suit == Suit.Stick)) {
          return usableSets;
        }
        // Check for set {n-2, n-1, n}
        if (targetCard.Value >= 3) {
          Card c1 = new Card(targetCard.Suit, targetCard.Value - 2, 0);
          Card c2 = new Card(targetCard.Suit, targetCard.Value - 1, 0);
          if (hand.Contains(c1) && hand.Contains(c2)) {
            List<Card> set = new List<Card>();
            set.Add(c1);
            set.Add(c2);
            set.Add(targetCard);
            usableSets.Add(set);
          }
        }
        // Check for set {n-1, n, n+1}
        if (targetCard.Value >= 2 && targetCard.Value <= 8) {
          Card c1 = new Card(targetCard.Suit, targetCard.Value - 1, 0);
          Card c2 = new Card(targetCard.Suit, targetCard.Value + 1, 0);
          if (hand.Contains(c1) && hand.Contains(c2)) {
            List<Card> set = new List<Card>();
            set.Add(c1);
            set.Add(targetCard);
            set.Add(c2);
            usableSets.Add(set);
          }
        }
        // Check for set {n, n+1, n+2}
        if (targetCard.Value <= 7) {
          Card c1 = new Card(targetCard.Suit, targetCard.Value + 1, 0);
          Card c2 = new Card(targetCard.Suit, targetCard.Value + 2, 0);
          if (hand.Contains(c1) && hand.Contains(c2)) {
            List<Card> set = new List<Card>();
            set.Add(targetCard);
            set.Add(c1);
            set.Add(c2);
            usableSets.Add(set);
          }
        }
      }
      return usableSets;
    }

    /// <summary>
    /// Returns a list of all ways to organize this hand into a valid winning hand using the target card. If there are no valid ways, returns an empty list.
    /// </summary>
    public static List<List<Card>> GetWinningHands(Card targetCard, List<Card> hand) {
      // Create initial list of hands to return. It may return empty.
      List<List<Card>> handsToReturn = new List<List<Card>>();

      List<Card> fullHand = new List<Card>(hand);
      if (targetCard != null && targetCard != Card.Unknown) {
        fullHand.Add(targetCard);
      }
      fullHand.Sort();

      // Check all possible hands where this card was the eye
      Card prevEye = null;
      foreach (Card eye in fullHand) {
        if (eye == prevEye) {
          continue;
        }
        List<Card> remainingCards = new List<Card>(fullHand);
        for (int i = 0; i < 2; i++) {
          if (!remainingCards.Remove(eye)) {
            continue;
          }
        }
        // Start recursive function on the remaining cards to get possible organizations
        if (remainingCards.Count == 0) {
          handsToReturn.Add(new List<Card>() { eye, eye });
        } else {
          foreach (List<Card> recursion in GetOrganizedHands(remainingCards)) {
            List<Card> organizedHand = new List<Card>(recursion) {
              eye,
              eye
            };
            handsToReturn.Add(organizedHand);
          }
        }
        prevEye = eye;
      }
      return handsToReturn;
    }

    /// <summary>
    /// Recursive helper fuction that returns a list of all valid ways to organize the given cards into sets of 3. If there are no valid ways, returns an empty list.
    /// </summary>
    private static List<List<Card>> GetOrganizedHands(List<Card> hand) {
      List<List<Card>> handsToReturn = new List<List<Card>>();

      List<List<Card>> usableSets = Hand.GetPongsAndKongs(hand[0], hand, true);
      usableSets.AddRange(GetRuns(hand[0], hand, true));

      foreach (List<Card> set in usableSets) {
        // Sets can only be made of 3 cards (ignore kongs)
        if (set.Count != 3) {
          continue;
        }
        List<Card> remainingCards = new List<Card>(hand);
        foreach (Card c in set) {
          remainingCards.Remove(c);
        }
        if (remainingCards.Count == 0) {
          handsToReturn.Add(set);
        } else {
          foreach (List<Card> recursion in GetOrganizedHands(remainingCards)) {
            List<Card> organizedHand = new List<Card>(recursion);
            organizedHand.AddRange(set);
            handsToReturn.Add(organizedHand);
          }
        }
      }

      return handsToReturn;
    }
  }

  // Representation of the deck.
  public class Deck {
    private LinkedList<Card> _cardList;
    public int Size { get; private set; }

    public Deck() {
      _cardList = new LinkedList<Card>();
      ResetDeck();
    }

    private void ResetDeck() {
      lock (_cardList) {
        Size = 0;
        _cardList.Clear();
        // Add Flowers
        for (int value = 1; value <= 4; value++) {
          for (int copy = 1; copy <= 2; copy++) {
            _cardList.AddLast(new Card(Suit.Flower, value, copy));
            Size++;
          }
        }
        // Add Dragons
        for (int value = 1; value <= 3; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Dragon, value, copy));
            Size++;
          }
        }
        // Add Winds
        for (int value = 1; value <= 4; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Wind, value, copy));
            Size++;
          }
        }
        // Add Circles
        for (int value = 1; value <= 9; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Circle, value, copy));
            Size++;
          }
        }
        // Add Man
        for (int value = 1; value <= 9; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Man, value, copy));
            Size++;
          }
        }
        // Add Sticks
        for (int value = 1; value <= 9; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Stick, value, copy));
            Size++;
          }
        }
      }
    }

    public void Shuffle() {
      lock (_cardList) {
        // Create array from _cardList
        Card[] arr = new Card[Size];
        int i = 0;
        foreach (Card c in _cardList) {
          arr[i] = c;
          i++;
        }
        // Shuffle array
        int n = Size;
        while (n > 1) {
          n--;
          int k = (int)UnityEngine.Random.Range(0, n + 1);
          Card temp = arr[k];
          arr[k] = arr[n];
          arr[n] = temp;
        }
        // Place cards back in deck
        _cardList.Clear();
        foreach (Card c in arr) {
          _cardList.AddLast(c);
        }
      }
    }

    public Card Peek() {
      return _cardList.First.Value;
    }

    public Card Draw() {
      lock (_cardList) {
        Card first = null;
        if (Size > 0) {
          first = _cardList.First.Value;
          _cardList.RemoveFirst();
          Size--;
        }
        return first;
      }
    }
  }
}