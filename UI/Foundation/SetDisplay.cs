using System.Collections;
using System.Collections.Generic;
using CardUtilities;
using UnityEngine;

public class SetDisplay : MonoBehaviour {
  public Set Set {get; private set; }
  [SerializeField] private GameObject _cardPrefab;

  public void SetSet(Set set) {
    Set = set;
    Card.ClearCardsInTransform(this.transform);
    foreach (Card c in set.Cards) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
      newCard.GetComponent<CardDisplay>().SetCard(c);
      newCard.GetComponent<CardDisplay>().SetButtonEnabled(false);
    }
  }
}