using CardUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour {
  public Card Card { get; private set; }
  [SerializeField] private Image _tileBackground;
  [SerializeField] private Image _tileEngraving;

  [SerializeField] private TMP_Text _suitTextObj;
  [SerializeField] private TMP_Text _valueTextObj;

  public void SetCard(CardUtilities.Card card) {
    Card = card;
    _suitTextObj.text = $"{card.Suit}";
    if (_tileEngraving != null) {
      _tileEngraving.sprite = StyleManager.StyleSettings.GetSpriteFromCard(card);
      _tileEngraving.enabled = true;
    }
    if (_tileBackground != null) {
      _tileBackground.sprite = StyleManager.StyleSettings.TileBackground;
      _tileBackground.color = StyleManager.StyleSettings.TileFrontFill;
    }
    switch (card.Suit) {
      case Suit.None:
        _valueTextObj.text = "";
        _suitTextObj.text = "";
        if (_tileBackground != null) {
          _tileBackground.color = StyleManager.StyleSettings.TileBackFill;
        }
        if (_tileEngraving != null) {
          _tileEngraving.enabled = false;
        }
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