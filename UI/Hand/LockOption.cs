using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOption : MonoBehaviour {
  public event Action<LockableWrapper> OnClick;
  [SerializeField] private GameObject _setPrefab;
  [SerializeField] private Button _optionButton;
  private LockableWrapper _wrapper;

  public void SetLockOption(LockableWrapper wrapper) {
    _wrapper = wrapper;
    Card.ClearCardsInTransform(this.transform);
    foreach (Set set in wrapper.Sets) {
      GameObject newCard = GameObject.Instantiate(_setPrefab, this.transform);
      newCard.GetComponent<SetDisplay>().SetSet(set);
    }
    _optionButton.onClick.RemoveAllListeners();
    _optionButton.onClick.AddListener( () => OnClick.Invoke(_wrapper));
  }

  public void SetButtonEnabled(bool enabled) {
    _optionButton.interactable = enabled;
  }
}