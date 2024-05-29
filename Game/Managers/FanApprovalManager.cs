using CardUtilities;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class FanApprovalManager : MonoBehaviourPunCallbacks {
  public static FanApprovalManager Singleton;

  public event Action<Player, Player, List<Set>> OnFanApprovalsStarted;
  public event Action OnFanApprovalsStopped;
  public event Action<int> OnAllFansApproved;

  public event Action<int> OnFansUpdated;
  public event Action<int> OnApprovalsUpdated;
  public event Action<bool> OnApproveButtonInteractableChanged;

  private int fans = 0;
  private int approvals = 0;

  private void Awake() {
    if (Singleton != null && Singleton != this) {
      this.gameObject.SetActive(false);
    }
    Singleton = this;
  }

  public void StartFanApprovals(Player winner, Player loser, List<Set> hand) {
    if (PhotonNetwork.IsMasterClient) {
      fans = 0;
      approvals = 0;
      photonView.RPC("RpcClientHandleFanApprovalsStarted", RpcTarget.All, winner, loser, LockableWrapper.WrapSetsTogether(hand, null));
      photonView.RPC("RpcClientHandleUpdateFans", RpcTarget.All, fans);
      photonView.RPC("RpcClientHandleApproveButtonInteractableChanged", RpcTarget.All, true);
      photonView.RPC("RpcClientHandleApproveFans", RpcTarget.All, approvals);
    }
  }

  public void StopFanApprovals() {
    fans = 0;
    approvals = 0;
    OnFanApprovalsStopped?.Invoke();
  }

  [PunRPC]
  private void RpcClientHandleFanApprovalsStarted(Player winner, Player loser, LockableWrapper hand) {
    OnFanApprovalsStarted?.Invoke(winner, loser, hand.Sets);
  }

  public void UpdateFans(int newFans) {
    photonView.RPC("RpcMasterHandleUpdateFans", RpcTarget.MasterClient, newFans);
  }

  [PunRPC]
  private void RpcMasterHandleUpdateFans(int newFans) {
    if (newFans != fans) {
      fans = newFans;
      photonView.RPC("RpcClientHandleUpdateFans", RpcTarget.All, newFans);
      approvals = 0;
      photonView.RPC("RpcClientHandleApproveButtonInteractableChanged", RpcTarget.All, true);
      photonView.RPC("RpcClientHandleApproveFans", RpcTarget.All, approvals);
    }
  }

  [PunRPC]
  private void RpcClientHandleUpdateFans(int newFans) {
    OnFansUpdated?.Invoke(newFans);
  }

  public void ApproveFans(int approvedFans) {
    photonView.RPC("RpcMasterHandleApproveFans", RpcTarget.MasterClient, approvedFans, PhotonNetwork.LocalPlayer);
  }

  [PunRPC]
  private void RpcMasterHandleApproveFans(int approvedFans, Player sender) {
    if (approvedFans == fans) {
      approvals++;
      photonView.RPC("RpcClientHandleApproveButtonInteractableChanged", sender, false);
      photonView.RPC("RpcClientHandleApproveFans", RpcTarget.All, approvals);

      if (approvals == PhotonNetwork.CurrentRoom.PlayerCount) {
        photonView.RPC("RpcClientHandleAllFansApproved", RpcTarget.All, fans);
      }
    }
  }

  [PunRPC]
  private void RpcClientHandleApproveFans(int approvalCount) {
    OnApprovalsUpdated?.Invoke(approvalCount);
  }

  [PunRPC]
  private void RpcClientHandleApproveButtonInteractableChanged(bool isInteractable) {
    OnApproveButtonInteractableChanged?.Invoke(isInteractable);
  }

  [PunRPC]
  private void RpcClientHandleAllFansApproved(int fans) {
    OnAllFansApproved?.Invoke(fans);
  }
}