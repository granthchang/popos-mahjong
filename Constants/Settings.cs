using UnityEngine;

namespace MahjongConstants {
  [CreateAssetMenu(menuName = "Settings")]
  public abstract class Settings : ScriptableObject {
    public readonly int StartingScore;
    public readonly int MinimumFans;
  }
}
