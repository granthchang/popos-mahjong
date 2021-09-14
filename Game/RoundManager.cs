// Author: Grant Chang
// Date: 24 August 2021

using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour {
  private Deck _deck;
  private List<Player> _players;

  public void StartRound(List<Player> players) {
    _players = players;
    _deck = new Deck();

    // TODO: deal cards to players
  }
}
