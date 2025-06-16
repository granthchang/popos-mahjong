using CardUtilities;
using System.Collections.Generic;
using UnityEngine;

public class SetDisplay : MonoBehaviour {
  public Set Set { get; private set; }
  [SerializeField] private GameObject _cardPrefab;

  private List<CardDisplay> _cardDisplays;

  public void SetSet(Set set) {
    Set = set;
    Card.ClearCardsInTransform(this.transform);
    _cardDisplays = new List<CardDisplay>();
    foreach (Card c in set.Cards) {
      GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
      CardDisplay newCardDisplay = newCard.GetComponent<CardDisplay>();
      newCardDisplay.SetCard(c);
      newCardDisplay.SetButtonEnabled(false);
      _cardDisplays.Add(newCardDisplay);
    }
  }

  public void SetButtonEnabled(bool enabled) {
    foreach (CardDisplay cardDisplay in _cardDisplays) {
      cardDisplay.SetButtonEnabled(enabled);
    }
  }
  
  public void AddOnClickListener(UnityEngine.Events.UnityAction call) {
    foreach (CardDisplay cardDisplay in _cardDisplays) {
      cardDisplay.AddOnClickListener(call);
    }
  }

  public void RemoveAllOnClickListeners() {
    foreach (CardDisplay cardDisplay in _cardDisplays) {
      cardDisplay.RemoveAllOnClickListeners();
    }
  }
}