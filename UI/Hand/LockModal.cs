using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LockModal : ActivatablePanel {
  [SerializeField] private GameObject _lockOptionPrefab;
  public event Action<List<Card>> OnOptionSelected;

  public void OpenLockModal(List<List<Card>> sets) {
    Reset();
    foreach (List<Card> set in sets) {
      GameObject newOption = GameObject.Instantiate(_lockOptionPrefab, this.transform);
      LockOption lo = newOption.GetComponent<LockOption>();
      lo.SetLockOption(set);
      lo.OnClick += (s) => {
        Debug.Log("Lock seen by LockModal");
        OnOptionSelected(s);
        ActivatePanel(false);
      };
    }
    ActivatePanel(true);
  }

  public void Reset() {
    foreach (Transform child in this.transform) {
      GameObject.Destroy(child.gameObject);
    }
  }
}