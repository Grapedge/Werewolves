using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomSetting : MonoBehaviour {

	public InputField villagerCountInput;
	public InputField witchCountInput;
	public InputField hunterCountInput;
	public InputField prophetCountInput;
	public InputField wolfCountInput;
	public MessageBox messageBox;

	public int villageCount {
		get {
			int count;
			int.TryParse (villagerCountInput.text, out count);
			return count;
		}
	}

	public int witchCount {
		get {
			int count;
			int.TryParse (witchCountInput.text, out count);
			return count;
		}
	}

	public int hunterCount {
		get {
			int count;
			int.TryParse (hunterCountInput.text, out count);
			return count;
		}
	}

	public int prophetCount {
		get {
			int count;
			int.TryParse (prophetCountInput.text, out count);
			return count;
		}
	}

	public int wolfCount {
		get {
			int count;
			int.TryParse (wolfCountInput.text, out count);
			return count;
		}
	}

	[HideInInspector]public int totalCount = 0;

	public void OnConfirmClick () {
		totalCount = villageCount + witchCount + prophetCount + hunterCount + wolfCount;
		if (totalCount <= 0 || totalCount > 254) {
			messageBox.Display ("玩家总数量应介于 1 - 254 之间", "我错了", null);
			return;
		}
		PandaNetworkManager manager = PandaNetworkManager.instance;
		if (totalCount < manager._playerNumber) {
			messageBox.Display ("设置的玩家数量小于房间内的玩家数量，请先移除一些玩家在进行操作", "我错了", null);
			return;
		}
		manager.SetMaxPlayers (totalCount);
		manager.SetMinPlayers (totalCount);
		manager.SetMaxPerPlayers (1);
		if (!UnityEngine.Networking.NetworkServer.active) {
			manager.StartHost ();
		}
		manager.UpdateReadyButtonState (PandaNetworkManager.instance.numPlayers + 1 >= PandaNetworkManager.instance.minPlayers);
		gameObject.SetActive (false);
	}

	public void OnCancelClick () {
		gameObject.SetActive (false);
	}
}
