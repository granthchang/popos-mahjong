using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndScreen : ActivatablePanel {
  [SerializeField] private TMP_Text _winnerTextObj;
  [SerializeField] private string _winnerText = "{player} won the game!";
  [SerializeField] private PlayerList _placementPlayerList;
  [SerializeField] private SyncedButton _playAgainButton;

  protected override void Awake() {
    base.Awake();
    GameManager.Singleton.OnGameFinished += HandleOnGameFinished;
    GameManager.Singleton.OnGameStopped += () => ActivatePanel(false);
    GameManager.Singleton.OnGameAboutToStart += () => ActivatePanel(false);
  }

  private void HandleOnGameFinished(List<Player> placements) {
    _winnerTextObj.text = _winnerText.Replace("{player}", placements[0].NickName);
    _placementPlayerList.RefreshPlayerList(placements);
    _playAgainButton.Reset();
    ActivatePanel(true);
  }
}