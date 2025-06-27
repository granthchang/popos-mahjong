using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableDisplay : ActivatablePanel {
  [SerializeField] private Button _deckDisplay;
  [SerializeField] private TMP_Text _deckButtonTextObj;
  [SerializeField] private SyncedButton _endRoundButton;
  private CardDisplay _lastDiscardDisplay;
  [SerializeField] private Transform _discardHistory;
  [SerializeField] private Button _discardButton;
  [SerializeField] private GameObject _cardPrefab;

  protected override void Awake() {
    base.Awake();
    RoundManager.Singleton.OnRoundStarted += () => {
      Reset();
      ActivatePanel(true);
    };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };
    RoundManager.Singleton.OnDeckSizeChanged += (newSize) => { _deckButtonTextObj.text = newSize.ToString(); };
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

  private void HandleTurnStarted(Player target, Card lastDiscard, bool canUseDiscard, bool canDraw) {
    _deckDisplay.interactable = (canDraw && target == PhotonNetwork.LocalPlayer);
    _endRoundButton.gameObject.SetActive(!canDraw);
    _endRoundButton.Reset();
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

  private void HandleCardSelected(Card selectedCard, bool isDiscarding) {
    if (selectedCard == null || !isDiscarding) {
      _discardButton.interactable = false;
      _discardButton.onClick.RemoveAllListeners();
    } else {
      _discardButton.interactable = true;
      _discardButton.onClick.RemoveAllListeners();
      _discardButton.onClick.AddListener(() => {
        _discardButton.interactable = false;
        PlayerManager.Singleton.SetCardSelectionEnabled(false);
        RoundManager.Singleton.Discard(selectedCard);
      });
    }
  }
}
