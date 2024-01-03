using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableDisplay : ActivatablePanel {
  [SerializeField] private Button _deckDisplay;
  private CardDisplay _lastDiscardDisplay;
  [SerializeField] private Transform _discardHistory;
  [SerializeField] private Button _discardButton;
  [SerializeField] private GameObject _cardPrefab;
  // private Card _lastDiscard;

  protected override void Awake() {
    base.Awake();
    RoundManager.Singleton.OnRoundStarted += () => {
      Reset();
      ActivatePanel(true);
    };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };
    PlayerManager.Singleton.OnSelectedCardChanged += HandleCardSelected;
    PlayerManager.Singleton.OnDiscardRequested += HandleDiscardRequested;
    PlayerManager.Singleton.OnDiscard += HandleDiscard;
    PlayerManager.Singleton.OnTurnStarted += HandleTurnStarted;
    PlayerManager.Singleton.OnDiscardConsidered += HandleDiscardConsidered;
    PlayerManager.Singleton.OnDiscardUsed += HandleDiscardUsed;
  }

  public void Reset() {
    _deckDisplay.interactable = false;
    Card.ClearCardsInTransform(_discardHistory);
  }

  private void HandleDiscardRequested(Player target) {
    if (_lastDiscardDisplay != null) {
      _lastDiscardDisplay.SetButtonEnabled(false);
    }
  }

  private void HandleDiscard(Card discard) {
    // If there is a previous discard, disable its button
    if (_lastDiscardDisplay != null) {
      _lastDiscardDisplay.SetButtonEnabled(false);
    }
    // Add the new discard to history
    GameObject c = GameObject.Instantiate(_cardPrefab, _discardHistory);
    CardDisplay cd = c.GetComponent<CardDisplay>();
    cd.SetCard(discard);
    cd.SetButtonEnabled(false);
    _lastDiscardDisplay = cd;
    cd.AddOnClickListener(() => {
      cd.SetButtonEnabled(false);
      RoundManager.Singleton.ConsiderDiscard();
    });
  }

  private void HandleTurnStarted(Player target, Card lastDiscard, bool canUseDiscard) {
    _deckDisplay.interactable = (target == PhotonNetwork.LocalPlayer);
    if (_lastDiscardDisplay != null) {
      _lastDiscardDisplay.SetButtonEnabled(canUseDiscard);
    }
  }

  private void HandleDiscardConsidered(Player target) {
    _deckDisplay.interactable = false;
    if (_lastDiscardDisplay != null) {
      _lastDiscardDisplay.SetButtonEnabled(false);
    }
  }

  private void HandleDiscardUsed(Card discard) {
    if (_lastDiscardDisplay != null) {
      GameObject.Destroy(_lastDiscardDisplay.gameObject);
    }
  }

  private void HandleCardSelected(Card selectedCard) {
    if (selectedCard == null) {
      _discardButton.interactable = false;
      _discardButton.onClick.RemoveAllListeners();
    } else {
      _discardButton.interactable = true;
      _discardButton.onClick.RemoveAllListeners();
      _discardButton.onClick.AddListener(() => {
        _discardButton.interactable = false;
        PlayerManager.Singleton.SetDiscardEnabled(false);
        RoundManager.Singleton.Discard(selectedCard);
      });
    }
  }
}
