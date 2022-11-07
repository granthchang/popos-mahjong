using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class PlayerUtilities {
  public static void ClearPlayerProperties(Player player) {
    Hashtable customProps = new Hashtable();
    customProps.Add(Constants.FlowerKey, -1);
    customProps.Add(Constants.ScoreKey, -1);
    player.SetCustomProperties(customProps);
  }

  public static int ChangePlayerScore(Player player, int difference) {
    int score = (int)player.CustomProperties[Constants.ScoreKey];
    int amountTaken = 0;
    // If the player doesn't have enough to pay the full cost
    if (score < -difference) {
      amountTaken = score;
      score = 0;
    } else {
      amountTaken = -difference;
      score += difference;
    }
    Hashtable hash = new Hashtable();
    hash.Add(Constants.ScoreKey, score);
    player.SetCustomProperties(hash);
    return amountTaken;
  }

  public static void AdvancePlayerFlower(Player player) {
    int flower = (int)player.CustomProperties[Constants.FlowerKey];
    flower--;
    if (flower <= 0) {
      flower = PhotonNetwork.CurrentRoom.PlayerCount;
    }
    Hashtable hash = new Hashtable();
    hash.Add(Constants.FlowerKey, flower);
    player.SetCustomProperties(hash);
  }  
}