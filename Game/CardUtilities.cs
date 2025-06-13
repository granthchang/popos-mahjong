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

    public Card() {
      Suit = Suit.None;
      Value = 0;
      ID = 000;
    }

    public Card(Suit suit, int value, int copyIndex) {
      Suit = suit;
      Value = value;
      ID += (int)suit * 100;
      ID += value * 10;
      ID += copyIndex;
    }

    public Card(int id) {
      Suit = (Suit)(id / 100);
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
        if (child.GetComponent<CardDisplay>() != null || child.GetComponent<SetDisplay>() != null) {
          GameObject.Destroy(child.gameObject);
        }
      }
    }
  }

  // Representation of the deck.
  public class Deck {
    private LinkedList<Card> _cardList;
    public int Size { get; private set; }
    public event Action<int> OnSizeChanged;

    public Deck() {
      _cardList = new LinkedList<Card>();
      ResetDeck();
    }

    private void ResetDeck() {
      lock (_cardList) {
        Size = 0;
        _cardList.Clear();
        if (Constants.ForceAllStickDeck <= 0) {
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
        }
        // Add Sticks
        for (int value = 1; value <= 9; value++) {
          for (int copy = 1; copy <= 4; copy++) {
            _cardList.AddLast(new Card(Suit.Stick, value, copy));
            Size++;
          }
        }
        OnSizeChanged?.Invoke(Size);
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
          OnSizeChanged?.Invoke(Size);
        }
        return first;
      }
    }
  }

  public enum SetType {
    Run = 0, // three consecutive numbers of the same suit (e.g., 3-4-5 of sticks)
    Pong = 1, // three identical cards (e.g., East-East-East)
    Kong = 2, // four identical cards (e.g., 2-2-2-2 of circles)
    Eye = 3, // two identical cards (e.g., Red-Red)
    Other = 4 // abnormal hands that do not follow typical set organization (e.g., 13 Angels)
  }

  // Representation of a set of cards that can be locked together. This may be a run, pong, kong, or an eye.
  public class Set {
    public SetType Type { get; private set; }
    public List<Card> Cards { get; private set; }
    public Card StartingCard => Cards[0];

    public Set(SetType type, Card startingCard) {
      Type = type;
      Cards = GetCardListFromStartingCard(startingCard, type);
    }

    public Set(int ID) {
      Type = (SetType)(ID / 1000);
      Cards = GetCardListFromStartingCard(new Card(ID % 1000), Type);
    }

    public Set(SetType type, List<Card> cards) {
      Type = type;
      Cards = new List<Card>(cards);
    }

    public int GetID() {
      return (int)Type * 1000 + StartingCard.ID;
    }

    public bool Contains(Card targetCard) {
      foreach (Card c in Cards) {
        if (c == targetCard) {
          return true;
        }
      }
      return false;
    }

    public override string ToString() {
      return $"{Type},{StartingCard}";
    }

    private static List<Card> GetCardListFromStartingCard(Card startingCard, SetType type) {
      List<Card> cards = new List<Card>();
      if (type == SetType.Run) {
        cards.Add(startingCard);
        cards.Add(new Card(startingCard.Suit, startingCard.Value + 1, 1));
        cards.Add(new Card(startingCard.Suit, startingCard.Value + 2, 1));
      }
      else {
        int cardCount = 0;
        switch (type) {
          case SetType.Eye:
            cardCount = 2;
            break;
          case SetType.Pong:
            cardCount = 3;
            break;
          case SetType.Kong:
            cardCount = 4;
            break;
          case SetType.Other:
            Debug.LogError("Cannot create set of type Other from a starting card. Please use a full Card List.");
            return null;
        }
        for (int i = 0; i < cardCount; i++) {
          cards.Add(startingCard);
        }
      }
      return cards;
    }
  }

  public class LockableWrapper {
    public List<Set> Sets { get; private set; }
    public Card Discard { get; private set; }

    public LockableWrapper(List<Set> sets, Card discard) {
      Sets = sets;
      Discard = discard;
    }

    public LockableWrapper(Set set, Card discard) {
      Sets = new List<Set>() { set };
      Discard = discard;
    }

    public List<Card> GetCards() {
      List<Card> cardsToReturn = new List<Card>();
      foreach (Set set in Sets) {
        foreach (Card card in set.Cards) {
          cardsToReturn.Add(card);
        }
      }
      return cardsToReturn;
    }

    /// <summary>
    /// Wraps a single Set into a single LockableWrapper. Returns one LockableWrapper
    /// </summary>
    public static LockableWrapper WrapSet(Set set, Card discard) {
      return new LockableWrapper(set, discard);
    }

    /// <summary>
    /// Wraps multiple Sets into a single LockableWrapper. Returns one Lockable Wrapper
    /// </summary>
    public static LockableWrapper WrapSetsTogether(List<Set> sets, Card discard) {
      return new LockableWrapper(sets, discard);
    }

    /// <summary>
    /// Wraps multiple Sets into multiple LockableWrappers. Returns one LockableWrapper per Set.
    /// </summary>
    public static List<LockableWrapper> WrapSetsSeparate(List<Set> sets, Card discard) {
      List<LockableWrapper> wrappersToReturn = new List<LockableWrapper>();
      foreach (Set set in sets) {
        wrappersToReturn.Add(WrapSet(set, discard));
      }
      return wrappersToReturn;
    }
  }
}