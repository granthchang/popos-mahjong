using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOption : MonoBehaviour {
  [SerializeField] private GameObject _cardPrefab;
  public event Action<List<Card>> OnClick;

  public void SetLockOption(List<Card> cards) {
    // Display cards
    foreach (Transform child in this.transform) {
      GameObject.Destroy(child.gameObject);
    }
    foreach (Card c in cards) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
      newCard.GetComponent<CardDisplay>().SetCard(c);
    }
    // Set size for button
    float spacing = this.GetComponent<HorizontalLayoutGroup>().spacing;
    Vector2 cardSize = _cardPrefab.GetComponent<RectTransform>().sizeDelta;
    float newX = (cardSize.x * cards.Count) + (spacing * (cards.Count - 1));
    this.GetComponent<RectTransform>().sizeDelta = new Vector2(newX, cardSize.y);
    // Set button event
    this.GetComponent<Button>().onClick.AddListener(() => OnClick.Invoke(cards));
  }
}