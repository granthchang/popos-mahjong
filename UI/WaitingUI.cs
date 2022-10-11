using Photon.Pun;
using TMPro;
using UnityEngine;

public class WaitingUI : ActivatablePanel {
  [Header("Status Settings")]
  [SerializeField] private TMP_Text _statusTextObj;
  [SerializeField] private string _waitingStatusText = "Waiting for more players...";
  [SerializeField] private string _startingStatusText = "Game is starting soon...";

  [Header("Room Code Settings")]
  [SerializeField] private TMP_Text _roomCodeTextObj;
  [SerializeField] private string _roomCodeText = "Room code:";

  protected override void Awake() {
    base.Awake();
    RoomManager.Singleton.OnRoomManagerStarted += Reset;
    GameManager.Singleton.OnGameAboutToStart += () => _statusTextObj.text = _startingStatusText;
    GameManager.Singleton.OnGameStarted += () => ActivatePanel(false);
    GameManager.Singleton.OnGameStopped += Reset;
  }

  private  void Start() {
#if UNITY_EDITOR
    // Prevents PIE errors before DefaultSceneLoader loads the initial scene
    if (PhotonNetwork.CurrentRoom == null) {
      return;
    }
#endif

    _roomCodeTextObj.text = $"{_roomCodeText} {PhotonNetwork.CurrentRoom.Name}";
  }

  private void Reset() {
    _statusTextObj.text = _waitingStatusText;
    base.Awake();
  }
}