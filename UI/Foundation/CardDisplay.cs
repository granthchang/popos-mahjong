using CardUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour {
  public Card Card { get; private set; }
  [SerializeField] private Image _tileHover;
  [SerializeField] private Image _tileBackground;
  [SerializeField] private Image _tileEngraving;
  [SerializeField] private Button _cardButton;
  [SerializeField] private bool _overrideDefaultButtonFunction;

  public void SetCard(CardUtilities.Card card) {
    Card = card;
    if (card.Suit == Suit.None) {
      _tileBackground.color = StyleManager.StyleSettings.TileBackFill;
      _tileEngraving.enabled = false;
    }
    else {
      if (_tileEngraving != null) {
        _tileEngraving.sprite = StyleManager.StyleSettings.GetSpriteFromCard(card);
        _tileEngraving.enabled = true;
      }
      if (_tileBackground != null) {
        _tileBackground.sprite = StyleManager.StyleSettings.TileBackground;
        _tileBackground.color = StyleManager.StyleSettings.TileFrontFill;
      }
    }
  }

  public void SetButtonEnabled(bool enabled) {
    _cardButton.interactable = enabled;
    _tileHover.raycastTarget = enabled;
  }

  public void AddOnClickListener(UnityEngine.Events.UnityAction call) {
    _cardButton.onClick.AddListener(call);
  }
}