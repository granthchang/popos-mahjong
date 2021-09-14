// Author: Grant Chang
// Date: 17 August 2021

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// WaitingUi controls the list of players when waiting for enough players for
/// the game to start
/// </summary>
public class WaitingListUi : MonoBehaviour {
  #region Events / Fields / References

  private const string _waitingText = "Waiting for more players...";
  private const string _startingText = "Game is starting soon...";
  private const string _playerPlaceholderText = "...";

  [SerializeField] private TMP_Text _statusText;
  [SerializeField] private TMP_Text _roomCode;
  [SerializeField] private TMP_Text[] _playerNames;

  #endregion
  #region Constructors / Initializers

  private void OnEnable() {
    RoomManager.Singleton.OnPlayerListUpdated += OnPlayerListUpdated;
  }

  void Start() {
    _roomCode.text = $"Room Code: <b>{PhotonNetwork.CurrentRoom.Name}</b>";
  }

  #endregion
  #region Update Information

  private void OnPlayerListUpdated(List<Player> players) {
    int index = 0;
    foreach (Player p in players) {
      _playerNames[index].text = p.NickName;
      index++;
    }
    for (int i = index; i < _playerNames.Length; i++) {
      _playerNames[i].text = _playerPlaceholderText;
    }
    if (index >= Constants.PlayersNeeded) {
      _statusText.text = _startingText;
    } else {
      _statusText.text = _waitingText;
    }
  }

  #endregion
}
