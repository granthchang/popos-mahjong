using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableDisplay : ActivatablePanel {
  [SerializeField] private Button _deckButton;
  [SerializeField] private Button _lastDiscardButton;
  [SerializeField] private Transform _discardHistory;
  [SerializeField] private GameObject _cardPrefab;
  private Card _lastDiscard;

  protected override void Awake() {
    base.Awake();
    RoundManager.Singleton.OnRoundStarted += () => {
      Reset();
      ActivatePanel(true);
    };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };
    PlayerManager.Singleton.OnDiscard += HandleDiscard;
    PlayerManager.Singleton.OnTurnStarted += HandleTurnStarted;
  }

  public void Reset() {
    _deckButton.interactable = false;
    Card.ClearCardsInTransform(_discardHistory);
    _lastDiscardButton.gameObject.SetActive(false);
    _lastDiscard = null;
  }

  private void HandleDiscard(Card discard) {
    // If this is first discard, make it visible.
    if (_lastDiscard == null) {
      _lastDiscardButton.gameObject.SetActive(true);
    }
    // Otherwise, add the last discard to discard history.
    else {
      GameObject c = GameObject.Instantiate(_cardPrefab, _discardHistory);
      c.GetComponent<CardDisplay>().SetCard(_lastDiscard);
    }
    _lastDiscardButton.GetComponent<CardDisplay>().SetCard(discard);
    _lastDiscard = discard;
  }

  private void HandleTurnStarted(Player target, Card lastDiscard) {
    if (target == PhotonNetwork.LocalPlayer) {
      _deckButton.interactable = true;
    }
  }
}
