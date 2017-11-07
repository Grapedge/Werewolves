using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class LobbyMainMenu : MonoBehaviour {
	public PandaNetworkManager lobbyManager;

	public RectTransform lobbyPanel;
	public RectTransform roomSettingBox;

	public InputField ipInput;

	public void OnEnable () {
		ipInput.onEndEdit.RemoveAllListeners();
		ipInput.onEndEdit.AddListener(onEndEditIP);
	}

	public void OnClickSetting () {
		// lobbyManager.StartHost ();
		roomSettingBox.gameObject.SetActive (true);
	}

	public void OnClickJoin () {
		lobbyManager.ChangeTo (lobbyPanel);

		lobbyManager.networkAddress = ipInput.text;
		lobbyManager.StartClient ();

		lobbyManager.backDelegate = lobbyManager.StopClientClbk;
		lobbyManager.DisplayIsConnecting ();

	}
		
	public void onEndEditIP (string text) {
		if (Input.GetKeyDown (KeyCode.Return)) {
			OnClickJoin ();
		}
	}
}
