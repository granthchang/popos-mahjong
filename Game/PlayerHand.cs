using CardUtilities;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class PlayerHand {
  public List<Card> HiddenHand { get; private set; }
  public List<Set> LockedSets { get; private set; }
  public List<Card> Flowers { get; private set; }
  public HandDisplay HandDisplay;
  public event Action<Card> OnSelectedCardChanged;

  public PlayerHand(Player owner, HandDisplay handDisplay) {
    HandDisplay = handDisplay;
    Reset();
    HandDisplay.SetPlayer(owner);
    HandDisplay.ActivatePanel(true);
    HandDisplay.OnSelectedCardChanged += (c) => OnSelectedCardChanged?.Invoke(c);
  }

  public void Reset() {
    HiddenHand = new List<Card>();
    LockedSets = new List<Set>();
    Flowers = new List<Card>();
    HandDisplay.Reset();
  }

  public void SetDiscardEnabled(bool isEnabled) {
    HandDisplay.SetDiscardEnabled(isEnabled);
  }

  public void AddCardToHand(Card card) {
    HiddenHand.Add(card);
    HandDisplay.AddCardToHiddenHand(card);
  }

  public void RemoveCardFromHand(Card card) {
    HiddenHand.Remove(card);
    HandDisplay.RemoveCardFromHiddenHand(card);
  }

  public void AddSetToHand(Set set) {
    LockedSets.Add(set);
    HandDisplay.AddSetToLockedHand(set);
  }

  public void RemoveSetFromHand(Set set) {
    LockedSets.Remove(set);
    HandDisplay.RemoveSetFromLockedHand(set);
  }

  public void RevealFlower(Card card) {
    Flowers.Add(card);
    HandDisplay.RevealFlower(card);
  }

  public void OpenLockModal(List<LockableWrapper> wrappers) {
    HandDisplay.OpenLockModal(wrappers);
  }

  public void CloseLockModal() {
    HandDisplay.CloseLockModal();
  }

  public void LockCards(LockableWrapper wrapper, out bool ConvertedPongToKong) {
    ConvertedPongToKong = false;
    bool HasRemovedDiscard = wrapper.Discard == null;
    foreach (Set setToLock in wrapper.Sets) {
      // If this set is a kong of a card that this hand already has a pong of, remove the existing pong and replace it with a matching kong. Then remove one copy of the starting card from hidden hand.
      if (setToLock.Type == SetType.Kong) {
        Set setToRemove = null;
        foreach (Set lockedSet in LockedSets) {
          if (lockedSet.Type == SetType.Pong && lockedSet.StartingCard == setToLock.StartingCard) {
            setToRemove = lockedSet;
            break;
          }
        }
        if (setToRemove != null) {
          RemoveSetFromHand(setToRemove);
          AddSetToHand(setToLock);
          RemoveCardFromHand(setToLock.StartingCard);
          ConvertedPongToKong = true;
          break;
        }
      }
      // Otherwise, add this set to this hands locked sets and remove the used cards from the hidden hand.
      AddSetToHand(setToLock);
      List<Card> cardsToRemove = new List<Card>(setToLock.Cards);
      if (!HasRemovedDiscard && cardsToRemove.Contains(wrapper.Discard)) {
        cardsToRemove.Remove(wrapper.Discard);
        HasRemovedDiscard = true;
      }
      foreach (Card c in cardsToRemove) {
        RemoveCardFromHand(c);
      }
    }
  }

  public void SetLockedSetButtonEnabled(Set targetSet, bool enabled) {
    HandDisplay.SetLockedSetButtonEnabled(targetSet, enabled);
  }

  public void DisableAllLockedSetButtons() {
    HandDisplay.DisableAllLockedSetButtons();
  }

  public LockableWrapper GetLockableHiddenKong(Card targetCard) {
    if (targetCard != null) {
      // Find an existing pong of this card
      bool pongExists = false;
      foreach (Set lockedSet in LockedSets) {
        if (lockedSet.Type == SetType.Pong && lockedSet.StartingCard == targetCard) {
          pongExists = true;
        }
      }
      // Or find hidden kong of this card
      if (pongExists || CountCard(HiddenHand, targetCard) == 4) {
        Set kong = new Set(SetType.Kong, targetCard);
        return LockableWrapper.WrapSet(kong, null);
      }

    }
    return null;
  }

  public List<LockableWrapper> GetLockablePongsAndKongs(Card targetCard) {
    return LockableWrapper.WrapSetsSeparate(GetPongsAndKongs(HiddenHand, targetCard, false), targetCard);
  }

  public List<LockableWrapper> GetLockableRuns(Card targetCard) {
    return LockableWrapper.WrapSetsSeparate(GetRuns(HiddenHand, targetCard, false), targetCard);
  }

  public List<LockableWrapper> GetLockableHands(Card targetCard, bool findHidden) {
    // Create temp hand that includes the discard if needed.
    List<Card> fullHand = new List<Card>(HiddenHand);
    if (!findHidden) {
      if (targetCard == null || targetCard == Card.Unknown) {
        return new List<LockableWrapper>();
      }
      else {
        fullHand.Add(targetCard);
      }
    }
    fullHand.Sort();

    // Initialize list of hands to return. It may return empty.
    List<LockableWrapper> wrappersToReturn = new List<LockableWrapper>();
    Card discard = findHidden ? null : targetCard;

    // If forcing a winnable hand, just generate runs from the current hand.
    if (Constants.ForceCanAlwaysWinHand == 1) {
      Set handSet = new Set(SetType.Other, fullHand);
      wrappersToReturn.Add(LockableWrapper.WrapSet(handSet, discard));
      return wrappersToReturn;
    }

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
      Set eyes = new Set(SetType.Eye, eye);
      if (remainingCards.Count == 0) {
        wrappersToReturn.Add(LockableWrapper.WrapSet(eyes, discard));
      }
      else {
        foreach (List<Set> organizedHand in GetOrganizedHands(remainingCards)) {
          organizedHand.Add(eyes);
          wrappersToReturn.Add(LockableWrapper.WrapSetsTogether(organizedHand, discard));
        }
      }
      prevEye = eye;
    }
    return wrappersToReturn;
  }

  private static int CountCard(List<Card> hand, Card targetCard) {
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

  private static List<Set> GetPongsAndKongs(List<Card> hand, Card targetCard, bool findHidden) {
    List<Set> sets = new List<Set>();
    if (targetCard != null) {
      int duplicateCount = CountCard(hand, targetCard);
      if (duplicateCount >= (findHidden ? 3 : 2)) {
        sets.Add(new Set(SetType.Pong, targetCard));
        if (duplicateCount >= (findHidden ? 4 : 3)) {
          sets.Add(new Set(SetType.Kong, targetCard));
        }
      }
    }
    return sets;
  }

  private static List<Set> GetRuns(List<Card> hand, Card targetCard, bool findHidden) {
    List<Set> sets = new List<Set>();
    // Ensure targetCard is valid and that this hand contains it if we're finding hidden runs.
    if (targetCard != null && (!findHidden || hand.Contains(targetCard))) {
      if (!(targetCard.Suit == Suit.Circle || targetCard.Suit == Suit.Man || targetCard.Suit == Suit.Stick)) {
        return sets;
      }
      // Check for set {n-2, n-1, n}
      if (targetCard.Value >= 3) {
        Card c1 = new Card(targetCard.Suit, targetCard.Value - 2, 0);
        Card c2 = new Card(targetCard.Suit, targetCard.Value - 1, 0);
        if (hand.Contains(c1) && hand.Contains(c2)) {
          sets.Add(new Set(SetType.Run, c1));
        }
      }
      // Check for set {n-1, n, n+1}
      if (targetCard.Value >= 2 && targetCard.Value <= 8) {
        Card c1 = new Card(targetCard.Suit, targetCard.Value - 1, 0);
        Card c2 = new Card(targetCard.Suit, targetCard.Value + 1, 0);
        if (hand.Contains(c1) && hand.Contains(c2)) {
          sets.Add(new Set(SetType.Run, c1));
        }
      }
      // Check for set {n, n+1, n+2}
      if (targetCard.Value <= 7) {
        Card c1 = new Card(targetCard.Suit, targetCard.Value + 1, 0);
        Card c2 = new Card(targetCard.Suit, targetCard.Value + 2, 0);
        if (hand.Contains(c1) && hand.Contains(c2)) {
          sets.Add(new Set(SetType.Run, targetCard));
        }
      }
    }
    return sets;
  }

  private static List<List<Set>> GetOrganizedHands(List<Card> hand) {
    List<List<Set>> handsToReturn = new List<List<Set>>();

    List<Set> sets = GetPongsAndKongs(hand, hand[0], true);
    sets.AddRange(GetRuns(hand, hand[0], true));

    foreach (Set set in sets) {
      // When checking for an organized hand, only look for Runs and Pongs (ignore Kongs and Eyes)
      if (set.Type != SetType.Run && set.Type != SetType.Pong) {
        continue;
      }
      List<Card> remainingCards = new List<Card>(hand);
      foreach (Card c in set.Cards) {
        remainingCards.Remove(c);
      }
      if (remainingCards.Count == 0) {
        handsToReturn.Add(new List<Set>() { set });
      }
      else {
        foreach (List<Set> recursion in GetOrganizedHands(remainingCards)) {
          handsToReturn.Add(new List<Set>(recursion) { set });
        }
      }
    }
    return handsToReturn;
  }
}