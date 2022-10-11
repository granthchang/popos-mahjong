using TMPro;
using UnityEngine;

public class EventLog : MonoBehaviour {
  [SerializeField] private TMP_Text _displayTextObj;

  private void Awake() {
    LobbyManager.Singleton.OnServerEvent += (s) => _displayTextObj.text = s;
  }
}