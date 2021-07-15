// Author: Grant Chang
// Date: 14 July 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Manager for the Information Panel. Updates to show player and room information important to
/// the game.
/// </summary>
public class InfoPanelManager : MonoBehaviourPunCallbacks
{
    #region Inspector Elements

    [Header("Game Info")]
    [SerializeField] private TMP_Text roomCode;

    [Header("Player Information")]
    [SerializeField] private TMP_Text[] playerInfo;
    
    #endregion
    #region Monobehaviour

    /// <summary>
    /// When loading into a new room, show the room code.
    /// </summary>
    private void Start() {
        roomCode.text = "Room Code: <b>" + PhotonNetwork.CurrentRoom.Name + "</b>";
        GameManager.singleton.OnPlayerListUpdated += RefreshPlayerInfo;
    }

    #endregion
    #region Event Handlers

    /// <summary>
    /// Refreshes player table to show accurate player information.
    /// </summary>
    public void RefreshPlayerInfo() {
        // Clear panel first
        foreach (TMP_Text t in playerInfo)
            t.text = "";

        // Repopulate panel with updated player info
        int playerIndex = 0;
        foreach (Player entry in GameManager.singleton.playerList)
        {
            playerInfo[playerIndex * 4].text = entry.NickName;
            playerInfo[playerIndex * 4 + 1].text = "";
            playerInfo[playerIndex * 4 + 2].text = "";
            playerInfo[playerIndex * 4 + 3].text = "";
            playerIndex++;
        }
    }

    #endregion
}