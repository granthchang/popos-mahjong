using CardUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LockModal : ActivatablePanel {
  [SerializeField] private GameObject _lockOptionPrefab;
  public event Action<List<Card>> OnOptionSelected;

  public void OpenLockModal(List<List<Card>> sets) {
    foreach (List<Card> set in sets) {
      GameObject newOption = GameObject.Instantiate(_lockOptionPrefab, this.transform);
      LockOption lo = newOption.GetComponent<LockOption>();
      lo.SetLockOption(set);
      lo.OnClick += OnOptionSelected;
      lo.OnClick += (s) => {
        Reset();
        ActivatePanel(false);
      };
    }
    ActivatePanel(true);
  }

  public void Reset() {
    foreach (Transform child in this.transform) {
      GameObject.Destroy(child.gameObject);
    }
    OnOptionSelected = null;
  }
}