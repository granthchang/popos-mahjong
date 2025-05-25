using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

[CreateAssetMenu(menuName = "Room Settings")]
public class RoomSettings : ScriptableObject {
  public int RoomSize = 4;
  public int TimeToStart = 5;
  public int StartingScore = 2000;
  public int MinimumFans = 3;
  public int Cycles = 4;
  public int MaxBrokePlayers = 2;

  public Hashtable ToCustomProperties() {
    Hashtable hash = new Hashtable();
    hash.Add(Constants.RoomSizeKey, RoomSize);
    hash.Add(Constants.TimeToStartKey, TimeToStart);
    hash.Add(Constants.StartingScoreKey, StartingScore);
    hash.Add(Constants.MinimumFansKey, MinimumFans);
    hash.Add(Constants.MaxCyclesKey, Cycles);
    hash.Add(Constants.MaxBrokePlayersKey, MaxBrokePlayers);
    return hash;
  }

  public RoomSettings() { }

  public void UpdateSettings(Hashtable propertiesThatChanged) {
    if (propertiesThatChanged.ContainsKey(Constants.RoomSizeKey)) {
      RoomSize = (int)propertiesThatChanged[Constants.RoomSizeKey];
    }
    if (propertiesThatChanged.ContainsKey(Constants.TimeToStartKey)) {
      TimeToStart = (int)propertiesThatChanged[Constants.TimeToStartKey];
    }
    if (propertiesThatChanged.ContainsKey(Constants.StartingScoreKey)) {
      StartingScore = (int)propertiesThatChanged[Constants.StartingScoreKey];
    }
    if (propertiesThatChanged.ContainsKey(Constants.MinimumFansKey)) {
      MinimumFans = (int)propertiesThatChanged[Constants.MinimumFansKey];
    }
    if (propertiesThatChanged.ContainsKey(Constants.MaxCyclesKey)) {
      Cycles = (int)propertiesThatChanged[Constants.MaxCyclesKey];
    }
    if (propertiesThatChanged.ContainsKey(Constants.MaxBrokePlayersKey)) {
      MaxBrokePlayers = (int)propertiesThatChanged[Constants.MaxBrokePlayersKey];
    }
  }
}