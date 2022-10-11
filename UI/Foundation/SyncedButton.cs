using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class InspectorEvent : UnityEvent { }

public class SyncedButton : MonoBehaviourPunCallbacks {
  [SerializeField] private Button _button;
  [SerializeField] private InspectorEvent _onAllClicked;
  [SerializeField] private Transform _clickCountObj;
  [SerializeField] private GameObject _clickDisplayPrefab;
  private List<Player> _clicks;

  private void Start() {
    Reset();
  }

  public void Reset() {
    _clicks = new List<Player>();
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleClicksChanged", RpcTarget.All, _clicks.Count);
      photonView.RPC("RpcClientHandleButtonInteractableChanged", RpcTarget.All, true);
    }
  }

  public override void OnPlayerEnteredRoom(Player newPlayer) {
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleClicksChanged", newPlayer, _clicks.Count);
      photonView.RPC("RpcClientHandleButtonInteractableChanged", newPlayer, true);
    }
  }

  public override void OnPlayerLeftRoom(Player otherPlayer) {
    _clicks.Remove(otherPlayer);
    photonView.RPC("RpcClientHandleClicksChanged", RpcTarget.All, _clicks.Count);
    if (PhotonNetwork.IsMasterClient) {
      if (_clicks.Count == PhotonNetwork.CurrentRoom.PlayerCount) {
        photonView.RPC("RpcClientHandleAllClicked", RpcTarget.All);
      }
    }
  }

  public void OnClick() {
    photonView.RPC("RpcClientHandleClick", RpcTarget.All, PhotonNetwork.LocalPlayer);
  }

  public void OnUnclick() {
    photonView.RPC("RpcClientHandleUnclick", RpcTarget.All, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcClientHandleClick(Player sender) {
    if (!_clicks.Contains(sender)) {
      _clicks.Add(sender); 
    }
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleButtonInteractableChanged", sender, false);
      photonView.RPC("RpcClientHandleClicksChanged", RpcTarget.All, _clicks.Count);
      if (_clicks.Count == PhotonNetwork.CurrentRoom.PlayerCount) {
        photonView.RPC("RpcClientHandleAllClicked", RpcTarget.All);
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleUnclick(Player sender) {
    if (_clicks.Contains(sender)) {
      _clicks.Remove(sender);
    }
    if (PhotonNetwork.IsMasterClient) {
      photonView.RPC("RpcClientHandleButtonInteractableChanged", sender, true);
      photonView.RPC("RpcClientHandleClicksChanged", RpcTarget.All, _clicks.Count);
    }
  }

  [PunRPC]
  private void RpcClientHandleButtonInteractableChanged(bool isInteractable) {
    _button.interactable = isInteractable;
  }

  [PunRPC]
  private void RpcClientHandleClicksChanged(int count) {
    if (_clickCountObj != null && _clickDisplayPrefab != null) {
      foreach (Transform child in _clickCountObj) {
        GameObject.Destroy(child.gameObject);
      }
      for (int i = 0; i < count; i++) {
        GameObject listItem = GameObject.Instantiate(_clickDisplayPrefab, _clickCountObj);
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleAllClicked() {
    _onAllClicked?.Invoke();
  }
}