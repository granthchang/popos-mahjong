using CardUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour {
  public Card Card { get; private set; }
  [SerializeField] private TMP_Text _suitTextObj;
  [SerializeField] private TMP_Text _valueTextObj;

  public void SetCard(CardUtilities.Card card) {
    Card = card;
    _suitTextObj.text = $"{card.Suit}";
    switch (card.Suit) {
      case Suit.None:
        _valueTextObj.text = "";
        _suitTextObj.text = "";
        this.GetComponent<Image>().color = Color.green;
        return;
      case Suit.Dragon:
        _valueTextObj.text = Constants.IntToDragon(card.Value);
        return;
      case Suit.Wind:
        _valueTextObj.text = Constants.IntToWind(card.Value);
        return;
      default:
        _valueTextObj.text = $"{card.Value}";
        return;
    }
  }
}