using CardUtilities;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnIndicator : ActivatablePanel {
  [SerializeField] private TMP_Text _indicatorText;
  [SerializeField] private string _isDrawingText = "{player} is drawing...";
  [SerializeField] private string _isDiscardingText = "{player} is discarding...";

  protected override void Awake() {
    base.Awake();
    
    RoundManager.Singleton.OnRoundStarted += () => { ActivatePanel(true); };
    RoundManager.Singleton.OnRoundFinished += (a, b, c) => { ActivatePanel(false); };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };

    PlayerManager.Singleton.OnTurnStarted += HandleTurnStarted;
    PlayerManager.Singleton.OnDiscardRequested += HandleDiscardRequested;
  }

  private void HandleTurnStarted(Player target, Card lastDiscard) {
    _indicatorText.text = _isDrawingText.Replace("{player}", target.NickName);
  }

  private void HandleDiscardRequested(Player target) {
    _indicatorText.text = _isDiscardingText.Replace("{player}", target.NickName);
  }
}
