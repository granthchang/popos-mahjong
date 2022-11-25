using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOption : MonoBehaviour {
  [SerializeField] private GameObject _cardPrefab;
  public event Action<List<Card>> OnClick;
  [SerializeField] private Button _optionButton;
  private List<Card> _set;

  public void SetLockOption(List<Card> set) {
    _set = set;
    // Display cards
    Card.ClearCardsInTransform(this.transform);
    foreach (Card c in set) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
      newCard.GetComponent<CardDisplay>().SetCard(c);
      newCard.GetComponent<CardDisplay>().SetButtonEnabled(false);
    }
    _optionButton.onClick.RemoveAllListeners();
    _optionButton.onClick.AddListener(HandleOptionClicked);
  }

  private void HandleOptionClicked() {
    Debug.Log("Lock seen by LockOption");
    OnClick.Invoke(_set);
    RoundManager.Singleton.LockCards(_set);
  }
}