// Author: Grant Chang
// Date: 23 August 2021

using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaitingListAnimator is called in animation events to toggle the Waiting UI canvas.
/// </summary>
public class WaitingListAnimator : MonoBehaviour {
  #region Events / Fields / References

  [SerializeField] private Canvas _canvas;
  [SerializeField] private Animator _animator;

  #endregion
  #region Constructors / Initializers

  private void OnEnable() {
    GameManager.Singleton.OnGameStarted += () => { ToggleUi(false); };
    GameManager.Singleton.OnGameStopped += () => { ToggleUi(true); };
  }

  private void Start() {
    ToggleUi(true);
  }

  #endregion
  #region Event Handlers

  public void EnableCanvas() {
    _canvas.enabled = true;
  }

  public void DisableCanvas() {
    _canvas.enabled = false;
  }

  #endregion
  #region Helper Methods

  private void ToggleUi(bool b) {
    if (b) {
      _animator.Play("Open");
    } else {
      _animator.Play("Close");
    }
  }

  #endregion
}
