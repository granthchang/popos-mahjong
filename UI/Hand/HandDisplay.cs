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

  [SerializeField] bool _isLocalHand = false;

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
    _nextLockIndex = 1;
  }

  public void SetPlayer(Player player) {
    if (_playerAvatar) {
      _playerAvatar.SetItem(player, 0);
    }
  }

  public void AddCard(Card card) {
    GameObject newCard = GameObject.Instantiate(_cardPrefab, _hiddenHand.transform);
    CardDisplay cd = newCard.GetComponent<CardDisplay>();
    cd.SetCard(card);
    cd.AddOnClickListener(() => { SetDiscardEnabled(false); });
  }

  public void RevealFlower(Card card) {
    if (_flowerDisplay != null) {
      _flowerDisplay.AddFlower(card);
    }
  }

  public void SortHand() {
    Card.SortCardsInTransform(_hiddenHand);
  }

  public void SetDiscardEnabled(bool enabled) {
    foreach (Transform child in _hiddenHand) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        cd.SetButtonEnabled(enabled);
      }
    }
  }

  public void RemoveFromHand(Card discard) {
    foreach (Transform child in _hiddenHand) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        if ((_isLocalHand && cd.Card == discard) || (!_isLocalHand && cd.Card == Card.Unknown)) {
          GameObject.DestroyImmediate(child.gameObject);
          return;
        }
      }
    }
  }

  public void LockCards(List<Card> cardsToLock, Card cardFromDiscard) {
    foreach (Card c in cardsToLock) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, _lockedHand);
      newCard.GetComponent<CardDisplay>().SetCard(c);
      newCard.transform.SetSiblingIndex(_nextLockIndex);
      _nextLockIndex++;
    }
    if (_isLocalHand) {
      // TODO - remove specific cards from hand

      // Shift LockModal
      if (_lockModal != null) {
        float spacing = _lockedHand.GetComponent<HorizontalLayoutGroup>().spacing;
        Vector2 cardSize = _cardPrefab.GetComponent<RectTransform>().sizeDelta;
        float offset = (cardSize.x + spacing) * cardsToLock.Count;
        RectTransform modalTransform = _lockModal.GetComponent<RectTransform>();
        modalTransform.localPosition = modalTransform.localPosition + new Vector3(offset, 0, 0);
      }
    } else {
      int numToRemove = (cardFromDiscard == null) ? cardsToLock.Count : cardsToLock.Count - 1;
      for (int i = 0; i < numToRemove; i++) {
        foreach (Transform child in _hiddenHand) {
          CardDisplay cd = child.GetComponent<CardDisplay>();
          if (cd != null && cd.Card == Card.Unknown) {
            GameObject.DestroyImmediate(child.gameObject);
            break;
          }
        }
      }
    }
  }
}