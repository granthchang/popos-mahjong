// Author: Grant Chang
// Date: 16 August 2021

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manager for the Information Panel. Updates to show player and room information important to
/// the game.
/// </summary>
public class InformationUi : MonoBehaviourPunCallbacks {
  #region Events / Fields / References

  [Header("Game Info")]
  [SerializeField] private TMP_Text _roomCode;
  [SerializeField] private TMP_Text _prevailingWind;

  [Header("Player Information")]
  [SerializeField] private TMP_Text[] _playerInfo;

  #endregion
  #region Constructors / Initializers

  public override void OnEnable() {
    RoomManager.Singleton.OnPlayerListUpdated += OnPlayerListUpdated;
  }

  private void Start() {
    _roomCode.text = $"Room Code: <b>{ PhotonNetwork.CurrentRoom.Name}</b>";
  }

  #endregion
  #region Update Information

  private void OnPlayerListUpdated(List<Player> players) {
    int index = 0;
    foreach (Player p in players) {
      _playerInfo[index * 4].text = p.NickName;
      _playerInfo[index * 4 + 1].text = "" + (index + 1);
      _playerInfo[index * 4 + 2].text = TurnToString(index);
      _playerInfo[index * 4 + 3].text = "" + 2000;
      index++;
    }
    for (int i = index * 4; i < _playerInfo.Length; i++) {
      _playerInfo[i].text = "";
    }
  }

  #endregion
  #region Helper Methods

  private static string TurnToString(int i) {
    switch (i) {
      case 0:
        return "East";
      case 1:
        return "South";
      case 2:
        return "West";
      case 3:
        return "North";
      default:
        return "ERROR";
    }
  }

  #endregion
}