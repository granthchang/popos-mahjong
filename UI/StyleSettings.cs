using CardUtilities;
using UnityEngine;

[CreateAssetMenu(menuName = "Style Settings")]
public class StyleSettings : ScriptableObject {
  [Header("Tile Sprites")]
  public Sprite[] FlowerSprites;
  public Sprite[] DragonSprites;
  public Sprite[] WindSprites;
  public Sprite[] CircleSprites;
  public Sprite[] ManSprites;
  public Sprite[] StickSprites;
  public Sprite TileBackground;
  [Header("Tile Colors")]
  public Color TileFrontFill = Color.white;
  public Color TileBackFill = Color.green;
  public Color TileOutline = Color.black;
  public Color TileRed = Color.red;
  public Color TileGreen = Color.green;
  public Color TileBlue = Color.blue;
  public Color TileYellow = Color.yellow;

  public StyleSettings() {
  }

  public Sprite GetSpriteFromCard(Card card) {
    int index = (card.ID % 100) / 10;
    switch (card.Suit) {
      case Suit.Flower:
        int value = (card.ID % 100) / 10;
        int id = (card.ID % 100) % 10;
        index = (value - 1) * 2 + (id - 1);
        return FlowerSprites[index];
      case Suit.Dragon:
        return DragonSprites[index - 1];
      case Suit.Wind:
        return WindSprites[index - 1];
      case Suit.Circle:
        return CircleSprites[index - 1];
      case Suit.Man:
        return ManSprites[index - 1];
      case Suit.Stick:
        return StickSprites[index - 1];
      default:
        return null;
    }
  }
}
