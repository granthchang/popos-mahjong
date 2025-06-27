using CardUtilities;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HandDisplay : ActivatablePanel {
  [SerializeField] private PlayerListItem _playerAvatar;
  [SerializeField] private FlowerDisplay _flowerDisplay;
  [SerializeField] private LockModal _lockModal;
  [SerializeField] private Transform _lockedHand;
  [SerializeField] private Transform _hiddenHand;
  [SerializeField] private GameObject _cardPrefab;
  [SerializeField] private GameObject _setPrefab;

  [SerializeField] bool _isLocalHand = false;

  public event Action<Card> OnSelectedCardChanged;
  private CardDisplay _selectedCardDisplay;

  public void Reset() {
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

  public void SetCardSelectionEnabled(bool enabled) {
    if (!enabled) {
      CloseLockModal();
    }
    foreach (Transform child in _hiddenHand) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        cd.SetButtonEnabled(enabled);
      }
    }
  }

  public void AddCardToHiddenHand(Card card) {
    GameObject newCard = GameObject.Instantiate(_cardPrefab, _hiddenHand.transform);
    CardDisplay cd = newCard.GetComponent<CardDisplay>();
    cd.SetCard(card);
    cd.AddOnClickListener(() => {
      if (_selectedCardDisplay != null) {
        _selectedCardDisplay.SetButtonEnabled(true);
      }
      _selectedCardDisplay = cd;
      cd.SetButtonEnabled(false);
      OnSelectedCardChanged?.Invoke(cd.Card);
    });
  }

  public void RemoveCardFromHiddenHand(Card cardToRemove) {
    foreach (Transform child in _hiddenHand) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        if ((_isLocalHand && cd.Card == cardToRemove) || (!_isLocalHand && cd.Card == Card.Unknown)) {
          if (cd = _selectedCardDisplay) {
            _selectedCardDisplay = null;
          }
          GameObject.DestroyImmediate(child.gameObject);
          return;
        }
      }
    }
  }

  public void AddSetToLockedHand(Set set) {
    GameObject newSet = GameObject.Instantiate(_setPrefab, _lockedHand);
    newSet.GetComponent<SetDisplay>().SetSet(set);
  }

  public void RemoveSetFromLockedHand(Set set) {
    foreach (Transform child in _lockedHand) {
      SetDisplay setDisplay = child.GetComponent<SetDisplay>();
      if (setDisplay != null && setDisplay.Set.Type == SetType.Pong && setDisplay.Set.StartingCard == set.StartingCard) {
        GameObject.Destroy(child.gameObject);
        return;
      }
    }
  }

  public void SetLockedSetButtonEnabled(Set targetSet, bool enabled) {
    foreach (Transform child in _lockedHand) {
      SetDisplay setDisplay = child.GetComponent<SetDisplay>();
      if (setDisplay != null && setDisplay.Set.Type == targetSet.Type && setDisplay.Set.StartingCard == targetSet.StartingCard) {
        setDisplay.RemoveAllOnClickListeners();
        if (enabled) {
          setDisplay.AddOnClickListener(() => {
            setDisplay.SetButtonEnabled(false);
            RoundManager.Singleton.ConsiderKong(setDisplay.Set.StartingCard);
          });
        }
        setDisplay.SetButtonEnabled(enabled);
        return;
      }
    }
  }

  public void RemoveFromLockedHand(Card cardToRemove) {
    foreach (Transform child in _lockedHand) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        if (cd.Card == cardToRemove) {
          GameObject.DestroyImmediate(child.gameObject);
          return;
        }
      }
    }
  }

  public void RevealFlower(Card card) {
    if (_flowerDisplay != null) {
      _flowerDisplay.AddFlower(card);
    }
  }

  public void SortHand() {
    Card.SortCardsInTransform(_hiddenHand);
  }

  public void OpenLockModal(List<LockableWrapper> wrappers) {
    _lockModal.OpenLockModal(wrappers);
  }

  public void CloseLockModal() {
    _lockModal.CloseLockModal();
  }
}