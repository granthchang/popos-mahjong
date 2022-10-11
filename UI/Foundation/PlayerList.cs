using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour {
  [SerializeField] private GameObject _listItemPrefab;
  [SerializeField] private bool _updateManually = false;

  private void Awake() {
    if (!_updateManually) {
      RoomManager.Singleton.OnPlayerListUpdated += RefreshPlayerList;
      GameManager.Singleton.OnPlayerListUpdated += RefreshPlayerList;
    }
  }

  private void RefreshPlayerList() {
    if (GameManager.Singleton.PlayerList != null) {
      RefreshPlayerList(GameManager.Singleton.PlayerList);
    } else {
      RefreshPlayerList(new List<Player>(PhotonNetwork.PlayerList));
    }
  }

  public void RefreshPlayerList(List<Player> playerList) {
    // Clear existing list items
    foreach (Transform child in this.transform) {
      if (child.gameObject.GetComponent<PlayerListItem>() != null) {
        GameObject.Destroy(child.gameObject);
      }
    }
    // Add new PlayerListItems. Fill the remaining player slots with fallback.
    IEnumerator enumerator = playerList.GetEnumerator();
    for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++) {
      GameObject listItem = GameObject.Instantiate(_listItemPrefab, this.transform);
      if (enumerator.MoveNext()) {
        listItem.GetComponent<PlayerListItem>().SetItem((Player)enumerator.Current, i);
      } else {
        listItem.GetComponent<PlayerListItem>().ResetItem();
      }
    }
  }
}