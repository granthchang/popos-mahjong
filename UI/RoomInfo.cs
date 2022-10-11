using Photon.Pun;
using TMPro;
using UnityEngine;

public class RoomInfo : MonoBehaviour {

  [SerializeField] private TMP_Text _minFansTextObj;
  [SerializeField] private string _minFansText = "Minimum fans:";
  [SerializeField] private TMP_Text _windTextObj;
  [SerializeField] private string _windText = "Current Wind:";
  [SerializeField] private TMP_Text _roomCodeObj;
  [SerializeField] private string _roomCodeText = "Room Code:";

  private void Awake() {
    GameManager.Singleton.OnMinFansSet += (n) => _minFansTextObj.text = $"{_minFansText} <b>{n}</b>";
    GameManager.Singleton.OnGameStarted += () => _windTextObj.enabled = true;
    GameManager.Singleton.OnGameStopped += () => _windTextObj.enabled = false;
    GameManager.Singleton.OnGameAboutToStart += () => _windTextObj.enabled = false;
    GameManager.Singleton.OnCurrentWindUpdated += (n) => _windTextObj.text = $"{_windText} <b>{Constants.IntToWind(n)}</b>";
  }

  private void Start() {
#if UNITY_EDITOR
    // Prevents PIE errors before DefaultSceneLoader loads the initial scene
    if (PhotonNetwork.CurrentRoom == null) {
      return;
    }
#endif
    if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(Constants.MinimumFansKey)) {
      _minFansTextObj.text = $"{_minFansText} <b>{PhotonNetwork.CurrentRoom.CustomProperties[Constants.MinimumFansKey]}</b>";
    }
    _windTextObj.enabled = false;
    _roomCodeObj.text = $"{_roomCodeText} <b>{PhotonNetwork.CurrentRoom.Name}</b>";
  }
}