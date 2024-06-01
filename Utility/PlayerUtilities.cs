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

  public static int UpdatePlayerData(Player player, int scoreDiff, bool shouldAdvanceFlower) {
    // Update player score. If the player doesn't have enough to pay the full cost, only subtract that amount.
    int score = (int)player.CustomProperties[Constants.ScoreKey];
    int amountTaken;
    if (score < -scoreDiff) {
      amountTaken = score;
      score = 0;
    } else {
      amountTaken = -scoreDiff;
      score += scoreDiff;
    }
    Hashtable hash = new Hashtable();
    hash.Add(Constants.ScoreKey, score);

    // Update player's flower number
    if (shouldAdvanceFlower) {
      int flower = (int)player.CustomProperties[Constants.FlowerKey] - 1;
      if (flower <= 0) {
        flower = PhotonNetwork.CurrentRoom.PlayerCount;
      }
      hash.Add(Constants.FlowerKey, flower);
    }

    // Commit changes and return the amount taken from this player 
    player.SetCustomProperties(hash);
    return amountTaken;
  }
}