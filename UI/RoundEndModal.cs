using CardUtilities;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RoundEndModal : ActivatablePanel {
  [Header("Text Settings")]
  [SerializeField] private TMP_Text _primaryTextObj;
  [SerializeField] private TMP_Text _secondaryTextObj;
  [SerializeField] private string _winnerText = "{player} won the hand!";
  [SerializeField] private string _loserText = "Thrown by {player}";
  [SerializeField] private string _selfDrawnText = "Self-drawn";
  [SerializeField] private string _deckExhaustedPrimaryText = "Deck exhausted";
  [SerializeField] private string _deckExhaustedSeconaryText = "No one won the hand";

  [Header("Hand Display")]
  [SerializeField] private Transform _handDisplayObj;
  [SerializeField] private GameObject _setPrefab;
  [SerializeField] private GameObject _emptyDeckObj;

  [Header("Fan Count")]
  [SerializeField] private GameObject _fanCountObj;
  [SerializeField] private TMP_InputField _inputField;
  [SerializeField] private Button _approveButton;
  [SerializeField] private TMP_Text _approveButtonTextObj;
  [SerializeField] private string _approveFanText = "Approve";
  [SerializeField] private string _deckExhaustedContinueText = "Continue";

  [SerializeField] private Transform _approvalList;
  [SerializeField] private GameObject _approvalListItem;

  protected override void Awake() {
    base.Awake();
    FanApprovalManager.Singleton.OnFanApprovalsStarted += OpenModal;
    FanApprovalManager.Singleton.OnFanApprovalsStopped += () => { ActivatePanel(false); };
    FanApprovalManager.Singleton.OnAllFansApproved += (n) => { ActivatePanel(false); };

    FanApprovalManager.Singleton.OnFansUpdated += HandleFansUpdated;
    FanApprovalManager.Singleton.OnApprovalsUpdated += HandleApprovalsUpdated;
    FanApprovalManager.Singleton.OnApproveButtonInteractableChanged += HandleApproveButtonInteractableChanged;
  }

  private void OpenModal(Player winner, Player loser, List<Set> hand) {
    if (winner == null) {
      _primaryTextObj.text = _deckExhaustedPrimaryText;
      _secondaryTextObj.text = _deckExhaustedSeconaryText;
    }
    else {
      _primaryTextObj.text = _winnerText.Replace("{player}", winner.NickName);
      _secondaryTextObj.text = (loser == null) ? _selfDrawnText: _loserText.Replace("{player}", loser.NickName); 
    }
    _fanCountObj.SetActive(winner != null);
    _approveButtonTextObj.text = (winner == null) ? _deckExhaustedContinueText : _approveFanText;
    DisplayHand(hand);
    ActivatePanel(true);
  }

  private void DisplayHand(List<Set> hand) {
    _handDisplayObj.gameObject.SetActive(hand.Count != 0);
    _emptyDeckObj.SetActive(hand.Count == 0);
    Card.ClearCardsInTransform(_handDisplayObj.transform);
    foreach (Set s in hand) {
      GameObject newSet = GameObject.Instantiate(_setPrefab, _handDisplayObj.transform);
      newSet.GetComponent<SetDisplay>().SetSet(s);
    }
  }

  private void HandleFansUpdated(int fans) {
    _inputField.text = fans.ToString();
  }

  private void HandleApprovalsUpdated(int approvals) {
    // Clear existing approval
    foreach (Transform child in _approvalList) {
      GameObject.Destroy(child.gameObject);
    }
    // Repopulate with the appropriate amount
    for (int i = 0; i < approvals; i++) {
      GameObject listItem = GameObject.Instantiate(_approvalListItem, _approvalList);
    }
  }

  public void ApprovalFans() {
    int fans = int.Parse(_inputField.text);
    FanApprovalManager.Singleton.ApproveFans(fans);
  }

  public void UpdateFans(string fanString) {
    int newFans = int.Parse(fanString);
    FanApprovalManager.Singleton.UpdateFans(newFans);
  }

  private void HandleApproveButtonInteractableChanged(bool isInteractable) {
    _approveButton.interactable = isInteractable;
  }
}