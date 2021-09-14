// Author: Grant Chang
// Date: 17 August 2021

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MenuAnimator toggles the Menu open and closed.
/// </summary>
public class MenuAnimator : MonoBehaviour {
  [SerializeField] private Animator _menuAnimator;
  [SerializeField] private Animator _overlayAnimator;

  private bool _isOpen;

  private void Start() {
    this.GetComponent<Canvas>().enabled = false;
  }

  public void ToggleUi() {
    // Detect if in the middle of an animation.
    // if (_menuAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) { }
    if (!_isOpen) {
      _menuAnimator.Play("Open");
      _overlayAnimator.Play("Fade In");
    } else {
      _menuAnimator.Play("Close");
      _overlayAnimator.Play("Fade Out");
    }
    _isOpen = !_isOpen;

  }
}
