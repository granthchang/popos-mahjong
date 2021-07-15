// Author: Grant Chang
// Date: 14 July 2021

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MenuPanel toggles the Menu Panel open and closed.
/// </summary>
public class MenuPanel : MonoBehaviour
{
	[SerializeField] private Animator menuAnimator;
	[SerializeField] private Animator overlayAnimator;
	[SerializeField] private Image overlay;

	private bool isOpen;

	/// <summary>
	/// Toggles the Menu Panel open and closed
	/// </summary>
	public void ToggleOpen()
	{
		if (menuAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
			return;

		if (!isOpen)
		{
			menuAnimator.Play("Open");
			overlayAnimator.Play("Fade In");
			overlay.raycastTarget = true;
		}
		else
		{
			menuAnimator.Play("Close");
			overlayAnimator.Play("Fade Out");
			overlay.raycastTarget = false;
		}

		isOpen = !isOpen;
	}
}
