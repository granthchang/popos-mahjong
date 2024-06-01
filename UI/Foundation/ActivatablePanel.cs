using UnityEngine;

public class ActivatablePanel : MonoBehaviour {
  private CanvasGroup _canvasGroup;
  [Header("Activatable Panel")]
  [SerializeField] private bool _startActivated;
  protected bool _isPanelActivated;

  protected virtual void Awake() {
    _canvasGroup = this.GetComponent<CanvasGroup>();
    ActivatePanel(_startActivated);
  }

  public virtual void ActivatePanel(bool isActivated) {
    if (isActivated) {
      _canvasGroup.alpha = 1f;
      _canvasGroup.blocksRaycasts = true;
      _isPanelActivated = true;
    } else {
      _canvasGroup.alpha = 0f;
      _canvasGroup.blocksRaycasts = false;
      _isPanelActivated = false;
    }
  }
}