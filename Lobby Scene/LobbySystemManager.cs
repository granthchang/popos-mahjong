// Author: Grant Chang
// Date: 14 July 2021

using UnityEngine;

/// <summary>
/// LobbySystemManager is the script called when quitting the application. This option is only
/// accessible from the Lobby.
/// </summary>
public class LobbySystemManager : MonoBehaviour
{
    public void QuitGame()
	{
		LobbyNetworkManager.singleton.Disconnect();
		Application.Quit();
	}
}
