using CardUtilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableDisplay : ActivatablePanel {
  [SerializeField] private Button _drawButton;

  protected override void Awake() {
    base.Awake();
    RoundManager.Singleton.OnRoundStarted += () => {
      Reset();
      ActivatePanel(true);
    };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };
    PlayerManager.Singleton.OnDrawStarted += HandleDrawStarted;
  }

  public void Reset() {
    _drawButton.interactable = false;
    Debug.Log("disabled draw");
  }

  private void HandleDrawStarted(Player target) {
    if (target == PhotonNetwork.LocalPlayer) {
      _drawButton.interactable = true;
      Debug.Log("enabled draw");
    }
  }
}
