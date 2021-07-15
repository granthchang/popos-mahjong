// Author: Grant Chang
// Date: 14 July 2021

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// GameManager oversees communicates changes in the game state to other clients' GameManagers.
/// The MasterClient's GameManager is the "central" GameManager that keeps the other GameManagers
/// up to date and synchronized.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    #region Singleton

    public static GameManager singleton;
    private void Awake()
    {
        if (singleton != null && singleton != this)
            this.gameObject.SetActive(false);
        singleton = this;
    }

    #endregion
    #region Information
    
    /// <summary>
    /// Indicates the state of play that the GameManager is in. Depending on the current state,
    /// different actions may be handled differently.
    /// </summary>
    public State CurrentState { get; private set; }
    public enum State {Setup, GameStarted, GameEnded};
    public event Action<State> OnStateChanged;


    /// <summary>
    /// A list of all players in turn order. If the game hasn't started, they organized by 
    /// connection time.
    /// </summary>
    public List<Player> playerList { get; private set; }
    public event Action OnPlayerListUpdated;

    #endregion
    #region Monobehaviour / Pun Callbacks

    private void Start()
    {
        CurrentState = State.Setup;
        playerList = new List<Player>();

        // If this is the master client when this method is called, it should be the only one
        // in the room
        if (PhotonNetwork.IsMasterClient) {
            playerList.Add(PhotonNetwork.LocalPlayer);           
            SyncPlayerLists();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient) {
            playerList.Add(newPlayer);
            SyncPlayerLists();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("Player " + otherPlayer.UserId + " disconnected.");
        
        List<Player> newList = new List<Player>();

        foreach (Player p in playerList) {
            if (!p.Equals(otherPlayer))
                newList.Add(p);
        }

        playerList = newList;
        SyncPlayerLists();
    }

    #endregion
    #region MasterClient Functions
    
    /// <summary>
    /// Can only be called by the MasterClient. Syncs up all clients' playerlists to match the
    /// MasterClient list.
    /// </summary>
    private void SyncPlayerLists() {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Local GameManager does not have authority to perform this action");
            return;
        }

        photonView.RPC("ClearPlayerList", RpcTarget.Others);
        foreach(Player p in playerList) {
            photonView.RPC("AddPlayer", RpcTarget.Others, p);
        }

        photonView.RPC("TriggerPlayerListUpdate", RpcTarget.Others);
        OnPlayerListUpdated?.Invoke();
    }

    #endregion
    #region Client Pun RPCs

    [PunRPC]
    private void ClearPlayerList() {
        playerList.Clear();
    }

    [PunRPC]
    private void AddPlayer(Player player) {
        playerList.Add(player);
        Debug.Log("Local GameManager recieved player: \n" + player);
    }

    [PunRPC]
    private void TriggerPlayerListUpdate() {
        OnPlayerListUpdated?.Invoke();
    }

    #endregion
}