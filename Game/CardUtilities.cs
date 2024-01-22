using System;
using System.Collections.Generic;
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

  // Representation of a hand
  public class Hand {
    private List<Card> _cards;
    
    public Hand() {
      Reset();
    }

    public void Reset() {
      _cards = new List<Card>();
    }

    public void Add(Card card) {
      _cards.Add(card);
    }

    public void Remove(Card card) {
      if (_cards.Contains(card)) {
        _cards.Remove(card);
      }
    }

    public bool HasHiddenKong(Card targetCard) {
      int duplicateCount = 0;
      if (targetCard != null) {
        foreach (Card c in _cards) {
          if (c == targetCard) {
            duplicateCount++;
          }
        }
      }
      return duplicateCount >= 4;
    }

    public List<List<Card>> GetPossiblePongsAndKongs(Card targetCard) {
      List<List<Card>> usableSets = new List<List<Card>>();
      if (targetCard != null) {
        int duplicateCount = 0;
        foreach (Card c in _cards) {
          if (c == targetCard) {
            duplicateCount++;
          }
        }
        if (duplicateCount >= 2) {
          List<Card> pong = new List<Card>();
          pong.Add(new Card(targetCard.ID));
          pong.Add(new Card(targetCard.ID));
          pong.Add(new Card(targetCard.ID));
          usableSets.Add(pong);
          if (duplicateCount >= 3) {
            List<Card> kong = new List<Card>(pong);
            kong.Add(new Card(targetCard.ID));
            usableSets.Add(kong);
          }
        }
      }
      return usableSets;
    }

    public List<List<Card>> GetAllPossibleSets(Card targetCard) {
      // Check for pongs or kongs
      List<List<Card>> usableSets = GetPossiblePongsAndKongs(targetCard);
      if (targetCard != null) {
        if (!(targetCard.Suit == Suit.Circle || targetCard.Suit == Suit.Man || targetCard.Suit == Suit.Stick)) {
          return usableSets;
        }
        // Check for set {n-2, n-1, n}
        if (targetCard.Value >= 3) {
          Card c1 = new Card(targetCard.Suit, targetCard.Value - 2, 0);
          Card c2 = new Card(targetCard.Suit, targetCard.Value - 1, 0);
          if (_cards.Contains(c1) && _cards.Contains(c2)) {
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
          if (_cards.Contains(c1) && _cards.Contains(c2)) {
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
          if (_cards.Contains(c1) && _cards.Contains(c2)) {
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
  }
}