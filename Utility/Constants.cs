public static class Constants {
  // Room custom properties
  public static readonly string RoomSizeKey = "RoomSize";
  public static readonly string TimeToStartKey = "TimeToStart";
  public static readonly string StartingScoreKey = "StartingScore";
  public static readonly string MinimumFansKey = "MinimumFans";
  public static readonly string MaxCyclesKey = "MaxCycles";
  public static readonly string MaxBrokePlayersKey = "MaxBrokePlayers";

  // Player custom properties
  public static readonly string ScoreKey = "Score";
  public static readonly string FlowerKey = "Flower";

  // Utility functions
  public static string IntToWind(int n) {
    switch (n) {
      case 1: return "East";
      case 2: return "South";
      case 3: return "West";
      case 4: return "North";
      default: return "Error";
    }
  }

  public static int WindToInt(string wind) {
    switch (wind) {
      case "East": return 1;
      case "South": return 2;
      case "West": return 3;
      case "North": return 4;
      default: return 0;
    }
  }

  public static string IntToDragon(int n) {
    switch (n) {
      case 1: return "Green";
      case 2: return "Red";
      case 3: return "White";
      default: return "Error";
    }
  }

  public static int DragonToInt(string dragon) {
    switch (dragon) {
      case "Green": return 1;
      case "Red": return 2;
      case "White": return 3;
      default: return 0;
    }
  }

  // Scoring functions
  public static int GetCostForFans(int fans) {
    switch (fans) {
      case 0: return 0;
      case 1: return 2;
      case 2: return 4;
      case 3: return 8;
      case 4: return 16;
      case 5: return 32;
      case 6: return 42;
      case 7: return 52;
      case 8: return 64;
      case 9: return 74;
      case 10: return 84;
      case 11: return 128;
      case 12: return 138;
      case 13: return 148;
      case 14: return 256;
      case 15: return 266;
      case 16: return 276;
      default: return 512;
    }
  }
}