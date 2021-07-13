using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
	[SerializeField] private Animator menuAnimator;
	[SerializeField] private Animator overlayAnimator;
	[SerializeField] private Image overlay;
	private bool isOpen;

	/// <summary>
	/// Toggles the menu panel open and closed
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
