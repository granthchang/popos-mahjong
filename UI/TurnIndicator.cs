using CardUtilities;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class TurnIndicator : ActivatablePanel {
  [SerializeField] private TMP_Text _indicatorText;
  [SerializeField] private string _isDrawingText = "{player} is drawing...";
  [SerializeField] private string _isDiscardingText = "{player} is discarding...";
  [SerializeField] private string _isDiscardConsideredText = "{player} is considering...";
  [SerializeField] private string _deckExhaustedText = "Deck exhausted. Cannot draw.";


  protected override void Awake() {
    base.Awake();

    RoundManager.Singleton.OnRoundStarted += () => { ActivatePanel(true); };
    RoundManager.Singleton.OnRoundFinished += (a, b, c) => { ActivatePanel(false); };
    RoundManager.Singleton.OnRoundStopped += () => { ActivatePanel(false); };

    PlayerManager.Singleton.OnTurnStarted += HandleTurnStarted;
    PlayerManager.Singleton.OnDiscardRequested += HandleDiscardRequested;
    PlayerManager.Singleton.OnDiscardConsidered += HandleDiscardConsidered;
  }

  private void HandleTurnStarted(Player target, Card lastDiscard, bool canUseDiscard, bool canDraw) {
    _indicatorText.text = canDraw ? _isDrawingText.Replace("{player}", target.NickName) : _deckExhaustedText;
  }

  private void HandleDiscardRequested(Player target) {
    _indicatorText.text = _isDiscardingText.Replace("{player}", target.NickName);
  }

  private void HandleDiscardConsidered(Player target) {
    _indicatorText.text = _isDiscardConsideredText.Replace("{player}", target.NickName);
  }
}
