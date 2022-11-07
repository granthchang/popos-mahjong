using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnIndicator : MonoBehaviour {
  [SerializeField] private TMP_Text _indicatorText;
  [SerializeField] private string _isDrawingText = "{player} is drawing...";
  [SerializeField] private string _isDiscardingText = "{player} is discarding...";

  private void Awake() {
    PlayerManager.Singleton.OnDrawStarted += HandleDrawStarted;
    PlayerManager.Singleton.OnDiscardRequested += HandleDiscardRequested;
  }

  private void HandleDrawStarted(Player target) {
    _indicatorText.text = _isDrawingText.Replace("{player}", target.NickName);
  }

  private void HandleDiscardRequested(Player target) {
    _indicatorText.text = _isDiscardingText.Replace("{player}", target.NickName);
  }

}
