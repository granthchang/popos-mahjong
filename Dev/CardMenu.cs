using CardUtilities;
using UnityEngine;
using UnityEngine.UI;

public class CardMenu : MonoBehaviour {
  [SerializeField] private GameObject _cardPrefab;
  [SerializeField] private Toggle _toggleSelectCards;
  [SerializeField] private Toggle _toggleSelectDiscard;

  private void Start() {
    Card.ClearCardsInTransform(this.transform);
    // Add facedown card
    AddCard(Card.Unknown);
    // Add flowers
    for (int value = 1; value <= 4; value++) {
      for (int copy = 1; copy <= 2; copy++) {
        AddCard(new Card(Suit.Flower, value, copy));
      }
    }
    // Add Dragons
    for (int value = 1; value <= 3; value++) {
      AddCard(new Card(Suit.Dragon, value, 1));
    }
    // Add Winds
    for (int value = 1; value <= 4; value++) {
      AddCard(new Card(Suit.Wind, value, 1));
    }
    // Add Circles
    for (int value = 1; value <= 9; value++) {
      AddCard(new Card(Suit.Circle, value, 1));
    }
    // Add Man
    for (int value = 1; value <= 9; value++) {
      AddCard(new Card(Suit.Man, value, 1));
    }
    // Add Sticks
    for (int value = 1; value <= 9; value++) {
      AddCard(new Card(Suit.Stick, value, 1));
    }

    SetModeSelectDiscard(false);
  }

  public void AddCard(Card card) {
    GameObject newCard = GameObject.Instantiate(_cardPrefab, this.transform);
    CardDisplay cd = newCard.GetComponent<CardDisplay>();
    cd.SetCard(card);
    cd.SetButtonEnabled(true);
  }

  public void SetModeSelectDiscard(bool isSelectingDiscard) {
    foreach (Transform child in this.transform) {
      CardDisplay cd = child.GetComponent<CardDisplay>();
      if (cd != null) {
        cd.RemoveAllOnClickListeners();
        if (isSelectingDiscard) {
          cd.AddOnClickListener(() => {
            HandTestManager.Singleton.SetDiscard(cd.Card);
          });
        } else {
          cd.AddOnClickListener(() => {
            HandTestManager.Singleton.AddCardToHand(cd.Card);
          });
        }
      }
    }
  }
}
