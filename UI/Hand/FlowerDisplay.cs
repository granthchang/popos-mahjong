using CardUtilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlowerDisplay : ActivatablePanel {
  [SerializeField] private GameObject _cardPrefab;
  [SerializeField] private TMP_Text _flowerButtonTextObj;
  [SerializeField] private Image _flowerButtonCircle;
  private int _flowerCount = 0;

  public void Reset() {
    _flowerCount = 0;
    foreach (Transform child in this.transform) {
      GameObject.Destroy(child.gameObject);
    }
    if (_flowerButtonCircle != null) {
      _flowerButtonCircle.color = StyleManager.StyleSettings.TileYellow;
    }
  }

  public void AddFlower(Card card) {
    _flowerCount++;
    GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
    newCard.GetComponent<CardDisplay>().SetCard(card);
    Card.SortCardsInTransform(this.transform);
    if (_flowerButtonTextObj != null) {
      _flowerButtonTextObj.text = _flowerCount.ToString();
    }
  }
}