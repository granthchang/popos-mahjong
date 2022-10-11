using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour {
  [SerializeField] private TMP_Text _nameTextObj;
  [SerializeField] private string _fallbackNameText = "...";
  [SerializeField] private TMP_Text _flowerTextObj;
  [SerializeField] private string _fallbackFlowerText = "...";
  [SerializeField] private TMP_Text _windTextObj;
  [SerializeField] private string _fallbackWindText = "...";
  [SerializeField] private TMP_Text _scoreTextObj;
  [SerializeField] private string _fallbackScoreText = "...";
  [SerializeField] private TMP_Text _indexTextObj;
  [SerializeField] private string _fallbackIndexText = "...";

  public void SetItem(Player player, int index) {
    if (_nameTextObj != null) {
      _nameTextObj.text = player.NickName;
    }

    if (player.CustomProperties.ContainsKey(Constants.FlowerKey)) {
      int flowerValue = (int)player.CustomProperties[Constants.FlowerKey];

      if (_flowerTextObj != null) {
        _flowerTextObj.text = flowerValue != -1 ? flowerValue.ToString() : _fallbackFlowerText;
      }
      if (_windTextObj != null) {
        _windTextObj.text = flowerValue != -1 ? Constants.IntToWind(flowerValue) : _fallbackWindText;
      }
    }

    if (player.CustomProperties.ContainsKey(Constants.ScoreKey)) {
      int scoreValue = (int)player.CustomProperties[Constants.ScoreKey];

      if (_scoreTextObj != null) {
        _scoreTextObj.text = scoreValue != -1 ? scoreValue.ToString() : _fallbackScoreText;
      }
    }

    if (_indexTextObj != null) {
      _indexTextObj.text = (index + 1).ToString();
    }
  }

  public void ResetItem() {
    if (_nameTextObj != null) {
      _nameTextObj.text = _fallbackNameText;
    }
    if (_flowerTextObj != null) {
      _flowerTextObj.text = _fallbackFlowerText;
    }
    if (_windTextObj != null) {
      _windTextObj.text = _fallbackWindText;
    }
    if (_scoreTextObj != null) {
      _scoreTextObj.text = _fallbackScoreText;
    }
    if (_indexTextObj != null) {
      _indexTextObj.text = _fallbackIndexText;
    }
  }
}