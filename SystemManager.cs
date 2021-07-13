using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public void ExitGame()
	{
		NetworkManager.singleton.Disconnect();
		Application.Quit();
	}
}
