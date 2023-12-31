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
    PlayerManager.Singleton.OnCardSelected += HandleCardSelected;
    PlayerManager.Singleton.OnDiscard += HandleDiscard;
    PlayerManager.Singleton.OnCanUseDiscardChecked += HandleCanUseDiscardChecked;
    PlayerManager.Singleton.OnTurnStarted += HandleTurnStarted;
    PlayerManager.Singleton.OnDiscardConsidered += HandleDiscardConsidered;
    PlayerManager.Singleton.OnDiscardUsed += HandleDiscardUsed;
  }

  public void Reset() {
    _deckDisplay.interactable = false;
    Card.ClearCardsInTransform(_discardHistory);
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
    // cd.RemoveOnClickListeners();
    cd.AddOnClickListener(() => {
      cd.SetButtonEnabled(false);
      RoundManager.Singleton.ConsiderDiscard();
    });
  }

  private void HandleCanUseDiscardChecked(bool canUse) {
    _lastDiscardDisplay.gameObject.SetActive(true);
    _lastDiscardDisplay.SetButtonEnabled(canUse);
  }

  private void HandleTurnStarted(Player target, Card lastDiscard) {
    if (target == PhotonNetwork.LocalPlayer) {
      _deckDisplay.interactable = true;
    }
  }

  private void HandleDiscardConsidered(Player target) {
    _deckDisplay.interactable = false;
    _lastDiscardDisplay.SetButtonEnabled(false);
  }

  private void HandleDiscardUsed(Card discard) {
    GameObject.Destroy(_lastDiscardDisplay.gameObject);
  }

  private void HandleCardSelected(Card selectedCard) {
    _discardButton.interactable = true;
    _discardButton.onClick.RemoveAllListeners();
    _discardButton.onClick.AddListener(() => {
      _discardButton.interactable = false;
      PlayerManager.Singleton.SetDiscardEnabled(false);
      RoundManager.Singleton.Discard(selectedCard);
    });
  }
}
