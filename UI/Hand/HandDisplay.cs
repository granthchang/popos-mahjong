using CardUtilities;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandDisplay : ActivatablePanel {
  [SerializeField] private PlayerListItem _playerAvatar;
  [SerializeField] private FlowerDisplay _flowerDisplay;
  [SerializeField] private LockModal _lockModal;
  [SerializeField] private Transform _lockedHand;
  [SerializeField] private Transform _hiddenHand;
  [SerializeField] private GameObject _cardPrefab;

  public event Action<List<Card>> OnSetLocked;
  private int _nextLockIndex = 1;

  public void Reset() {
    if (_playerAvatar) {
      _playerAvatar.ResetItem();
    }
    if (_flowerDisplay != null) {
      _flowerDisplay.ActivatePanel(false);
      _flowerDisplay.Reset();
    }
    if (_lockModal != null) {
      _lockModal.ActivatePanel(false);
      _lockModal.Reset();
    }
    Card.ClearCardsInTransform(_lockedHand);
    Card.ClearCardsInTransform(_hiddenHand);
  }
  
  public void SetPlayer(Player player) {
    if (_playerAvatar) {
      _playerAvatar.SetItem(player, 0);
    }
  }

  public void AddCard(Card card) {
    GameObject newCard = GameObject.Instantiate(_cardPrefab, _hiddenHand.transform);
    newCard.GetComponent<CardDisplay>().SetCard(card);
  }

  public void RevealFlower(Card card) {
    if (_flowerDisplay != null) {
      _flowerDisplay.AddFlower(card);
    }
  }

  public void SortHand() {
    Card.SortCardsInTransform(_hiddenHand);
  }

  private void HandleCardsLocked(List<Card> cards) {
    foreach (Card c in cards) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, _lockedHand);
      newCard.GetComponent<CardDisplay>().SetCard(c);
      newCard.transform.SetSiblingIndex(_nextLockIndex);
      _nextLockIndex++;
    }
    if (_lockModal != null) {
      float spacing = _lockedHand.GetComponent<HorizontalLayoutGroup>().spacing;
      Vector2 cardSize = _cardPrefab.GetComponent<RectTransform>().sizeDelta;
      float offset = (cardSize.x + spacing) * cards.Count;
      RectTransform modalTransform = _lockModal.GetComponent<RectTransform>();
      modalTransform.localPosition = modalTransform.localPosition + new Vector3(offset, 0, 0);
    }
  }

  // Test case
  private void Update() {
    // D to deal hand
    if (Input.GetKeyUp(KeyCode.D)) {
      Deck deck = new Deck();
      deck.Shuffle();
      for (int i = 0; i < 13; i++) {
        Card c = deck.Draw();
        AddCard(c);
        if (c.Suit == Suit.Flower) {
          i--;
        }
      }
    }

    // H to send hidden cards
    if (Input.GetKeyUp(KeyCode.H)) {
      AddCard(new Card(0));
    }
    // L to send hidden cards
    if (Input.GetKeyUp(KeyCode.L)) {
      List<Card> set0 = new List<Card>();
      set0.Add(new Card(Suit.Circle, 2, 1));
      set0.Add(new Card(Suit.Circle, 3, 1));
      set0.Add(new Card(Suit.Circle, 4, 1));
      HandleCardsLocked(set0);
    }

    // M to open lock modal
    if (Input.GetKeyUp(KeyCode.M) && _lockModal != null) {
      List<Card> set0 = new List<Card>();
      set0.Add(new Card(Suit.Circle, 2, 1));
      set0.Add(new Card(Suit.Circle, 3, 1));
      set0.Add(new Card(Suit.Circle, 4, 1));
      List<Card> set1 = new List<Card>();
      set1.Add(new Card(Suit.Stick, 6, 1));
      set1.Add(new Card(Suit.Stick, 7, 1));
      set1.Add(new Card(Suit.Stick, 8, 1));
      List<Card> set2 = new List<Card>();
      set2.Add(new Card(Suit.Wind, 4, 1));
      set2.Add(new Card(Suit.Wind, 4, 1));
      set2.Add(new Card(Suit.Wind, 4, 1));
      set2.Add(new Card(Suit.Wind, 4, 1));

      List<List<Card>> sets = new List<List<Card>>();
      sets.Add(set0);
      sets.Add(set1);
      sets.Add(set2);

      _lockModal.OnOptionSelected += HandleCardsLocked;
      _lockModal.OpenLockModal(sets);
    }
  }
}