using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PropertyManager : MonoBehaviourPunCallbacks {
  public static PropertyManager Singleton;
  private Dictionary<Player, bool> _playersUpdatedDictionary;
  private Action _callbackAction;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  public void UpdatePropertiesWithCallback(Action initialAction, Action callbackAction) {
    if (PhotonNetwork.IsMasterClient) {
      _playersUpdatedDictionary = new Dictionary<Player, bool>();
      foreach (Player p in PhotonNetwork.PlayerList) {
        _playersUpdatedDictionary.Add(p, false);
      }
      _callbackAction = callbackAction;
      initialAction();
    }
  }

  public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
    if (PhotonNetwork.IsMasterClient) {
      if (_playersUpdatedDictionary != null && _callbackAction != null) {
        _playersUpdatedDictionary[targetPlayer] = true;
        foreach (KeyValuePair<Player, bool> pair in _playersUpdatedDictionary) {
          if (!pair.Value) {
            return;
          }
        }
        _callbackAction();
        StopCallback();
      }
    }
  }

  public void StopCallback() {
    if (PhotonNetwork.IsMasterClient) {
      _playersUpdatedDictionary = null;
      _callbackAction = null;
    }
  }
}