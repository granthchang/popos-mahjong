using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

/// <summary>
/// Representation of a player in the game.
/// </summary>
public class PlayerObject
{
    public Player Player { get; private set; }

    public string Name { get; private set; }

    /// <summary>
    /// Creates a new playerobject with from the provided photon player
    /// </summary>
    public PlayerObject(Player player) {
        Name = player.NickName;
    }


    /* -------------------- OPERATOR OVERLOADING -------------------- */
    // public static bool operator== (PlayerObject left, PlayerObject right) {
    //     if (ReferenceEquals(left, null))
    //         return ReferenceEquals(right, null);
    //     if (ReferenceEquals(right, null))
    //         return ReferenceEquals(left, null);
    //     return left.Equals(right);
    // }

    // public static bool operator!= (PlayerObject left, PlayerObject right) {
    //     if (ReferenceEquals(left, null))
    //         return !ReferenceEquals(right, null);
    //     if (ReferenceEquals(right, null))
    //         return !ReferenceEquals(left, null);
    //     return !left.Equals(right);
    // }

    // public override bool Equals(object obj)
    // {
    //     return this.Player.UserId == ((PlayerObject)obj).Player.UserId;
    // }





    public int flower { get; private set; }
    public int score { get; private set; }
    public enum Wind { East, West, North, South };
    public Wind wind { get; private set; }

    /// <summary>
    /// Creates a default player with name of "NO NAME", first flower, east wind, and score of 2000.
    /// </summary>
    public PlayerObject() : this("NO NAME", 1, Wind.East, 2000) {}

    /// <summary>
    /// Creates a new player with the specifed parameters.
    /// </summary>
    public PlayerObject(string name, int flower, Wind wind, int score) {
        this.Name = name;
        this.flower = flower;
        this.wind = wind;
        this.score = score;
    }

    /// <summary>
    /// Lowers player score by the provided amount. If the player score is lower than the provided
    /// amount, score goes to 0. Returns the amount that the player's score decreased.
    /// </summary>
    public int LowerScore(int amount) {
        int returnValue;

        if (amount >= score) {
            returnValue = score;
            score = 0;
        }
        else {
            score -= amount;
            returnValue = amount;
        }
        return returnValue;
    }

    /// <summary>
    /// Increases the player score by the provided amount.
    /// </summary>
    public void IncreaseScore(int amount) {
        score += amount;
    }
}
