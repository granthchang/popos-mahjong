using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LockModal : ActivatablePanel {
  [SerializeField] private GameObject _lockOptionPrefab;
  public event Action OnLockModalClosed;
  private bool _isUsingDiscard = false;

  public void OpenLockModal(List<List<Card>> sets, Card discard) {
    Reset();
    _isUsingDiscard = (discard != null);
    foreach (List<Card> set in sets) {
      GameObject newOption = GameObject.Instantiate(_lockOptionPrefab, this.transform);
      LockOption lo = newOption.GetComponent<LockOption>();
      lo.SetLockOption(set);
      lo.OnClick += (s) => {
        RoundManager.Singleton.LockCards(s, discard);
        ActivatePanel(false);
      };
    }
    ActivatePanel(true);
  }

  public void CloseLockModal() {
    if (_isPanelActivated) {
      ActivatePanel(false);
      if (_isUsingDiscard) {
        RoundManager.Singleton.CancelConsiderDiscard();
      }
    }
  }

  public void Reset() {
    foreach (Transform child in this.transform) {
      if (child.GetComponent<LockOption>() != null) {
        GameObject.Destroy(child.gameObject);
      }
    }
  }
}