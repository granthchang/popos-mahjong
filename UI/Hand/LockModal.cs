using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LockModal : ActivatablePanel {
  [SerializeField] private GameObject _lockOptionPrefab;
  public event Action OnLockModalClosed;
  private bool _isUsingDiscard = false;

  public void OpenLockModal(List<LockableWrapper> wrappers) {
    Reset();
    if (wrappers.Count > 0) {
      _isUsingDiscard = wrappers[0].Discard != null;
      foreach (LockableWrapper wrapper in wrappers) {
        GameObject newOption = GameObject.Instantiate(_lockOptionPrefab, this.transform);
        LockOption lo = newOption.GetComponent<LockOption>();
        lo.SetLockOption(wrapper);
        lo.OnClick += (wrapperToLock) => {
          if (RoundManager.Singleton != null) {
            RoundManager.Singleton.LockCards(wrapperToLock);
          }
          ActivatePanel(false);
        };
        lo.SetButtonEnabled(true);
      }
      ActivatePanel(true);
    }
  }

  public void CloseLockModal() {
    if (_isPanelActivated) {
      ActivatePanel(false);
      if (_isUsingDiscard) {
        if (RoundManager.Singleton != null) {
          RoundManager.Singleton.CancelConsiderDiscard();
        }
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