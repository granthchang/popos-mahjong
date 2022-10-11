using CardUtilities;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundEndModal : ActivatablePanel {
  [Header("Text Settings")]
  [SerializeField] private TMP_Text _winnerTextObj;
  [SerializeField] private string _winnerText = "{player} won the hand!";
  [SerializeField] private TMP_Text _loserTextObj;
  [SerializeField] private string _loserText = "Thrown by {player}";

  [Header("Hand Display")]
  [SerializeField] private Transform _handDisplayObj;
  [SerializeField] private GameObject _cardPrefab;

  [Header("Fan Count")]
  [SerializeField] private TMP_InputField _inputField;
  [SerializeField] private Button _approveButton;
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

  private void OpenModal(Player winner, Player loser, List<CardUtilities.Card> hand) {
    _winnerTextObj.text = _winnerText.Replace("{player}", winner.NickName);
    _loserTextObj.text = _loserText.Replace("{player}", loser.NickName);
    DisplayHand(hand);
    ActivatePanel(true);
  }

  private void DisplayHand(List<CardUtilities.Card> hand) {
    // Clear existing list items
    foreach (Transform child in _handDisplayObj.transform) {
      GameObject.Destroy(child.gameObject);
    }
    // Populate the hand display with cards from hand
    foreach(CardUtilities.Card c in hand) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, _handDisplayObj.transform);
      newCard.GetComponent<CardDisplay>().SetCard(c);
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