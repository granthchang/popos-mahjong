using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatablePanel : MonoBehaviour {
  private CanvasGroup _canvasGroup;
  [Header("Activatable Panel")]
  [SerializeField] private bool _startActivated;

  protected virtual void Awake() {
    _canvasGroup = this.GetComponent<CanvasGroup>();
    ActivatePanel(_startActivated);
  }

  public virtual void ActivatePanel(bool isActivated) {
    if (isActivated) {
      _canvasGroup.alpha = 1f;
      _canvasGroup.blocksRaycasts = true;
    } else {
      _canvasGroup.alpha = 0f;
      _canvasGroup.blocksRaycasts = false;
    }
  }
}