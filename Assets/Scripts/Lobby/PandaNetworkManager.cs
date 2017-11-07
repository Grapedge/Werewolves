using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Panda network manager.
/// refence by unity
/// </summary>
public class PandaNetworkManager : NetworkLobbyManager {

	/// <summary>
	/// The message kicked.
	/// </summary>
	private static short MsgKicked = MsgType.Highest + 1;
	/// <summary>
	/// The instance.
	/// </summary>
	public static PandaNetworkManager instance;

	[Header("Unity UI Lobby")]
	[Tooltip("所有玩家准备后的等待时间")]
	public float prematchCountdown = 5.0f;

	[Space]
	[Header("UI Reference")]
	public RectTransform mainMenuPanel;
	public RectTransform lobbyPanel;
	public RectTransform topPanel;

	public Button backButton;
	public Button roomSettingButton;
	public LobbyRoomSetting roomSetting;
	public LobbyCountdownPanel countdownPanel;
	public Text ipAdress;

	public MessageBox messageBox;

	// 当前正在显示的界面
	private RectTransform currentPanel;

	[HideInInspector]
	public int _playerNumber = 0;

	private LobbyHook lobbyHooks;

	private void Start () {
		instance = this;
		backButton.gameObject.SetActive (false);
		ipAdress.gameObject.SetActive (false);
		roomSettingButton.gameObject.SetActive (false);
		GetComponent<Canvas>().enabled = true;
		DontDestroyOnLoad(gameObject);
		lobbyHooks = GetComponent<LobbyHook> ();
	}

	// 加载新场景时在客户端加载
	public override void OnLobbyClientSceneChanged (NetworkConnection conn) {
		if (SceneManager.GetSceneAt (0).name == lobbyScene) {
			// Lobby view to do here...
			topPanel.gameObject.SetActive (true);
			ChangeTo (lobbyPanel);
			if (conn.playerControllers[0].unetView.isClient) {
				backDelegate = StopHostClbk;
			} else {
				backDelegate = StopClientClbk;
			}
		} else {
			// 我们使用 Main scence 进行渲染
			topPanel.gameObject.SetActive (false);
			ChangeTo (null);
			if (NetworkServer.active) SetupGame ();
		}
	}

	// 用于切换功能界面
	public void ChangeTo (RectTransform newPanel) {
		if (currentPanel != null) {
			currentPanel.gameObject.SetActive(false);
		}
		if (newPanel != null) {
			newPanel.gameObject.SetActive(true);
		}

		currentPanel = newPanel;

		if (currentPanel != mainMenuPanel) {
			// 在服务器上显示 ip
			backButton.gameObject.SetActive(true);
		} else {
			backButton.gameObject.SetActive(false);
			//SetServerInfo("Offline", "None");
		}
	}

	public void DisplayIsConnecting () {
		var _this = this;
		messageBox.Display("连接中...", "取消", () => { _this.backDelegate(); });
	}

	public delegate void BackButtonDelegate();
	public BackButtonDelegate backDelegate;

	public void GoBackButton () {
		backDelegate();
		// To do here : set game state to normal=> not playing
	}





	//---------------------------------------------------服务器管理


	/// <summary>
	/// 添加本地玩家
	/// </summary>
	public void AddLocalPlayer () {
		TryToAddPlayer();
	}

	/// <summary>
	/// 移除一个角色
	/// </summary>
	/// <param name="player">Player.</param>
	//public void RemovePlayer(LobbyPlayer player) {
	//	player.RemovePlayer();
	//}

	/// <summary>
	/// 返回主菜单页面
	/// </summary>
	public void SimpleBackClbk () {
		ChangeTo(mainMenuPanel);
	}

	/// <summary>
	/// 停止主机模式
	/// </summary>
	public void StopHostClbk () {
		StopHost();		// 停止 host
		#if UNITY_EDITOR
		Debug.Log ("Host Stoped");
		#endif
		roomSettingButton.gameObject.SetActive (false);
		ipAdress.gameObject.SetActive (false);
		ChangeTo(mainMenuPanel);
	}

	/// <summary>
	/// 停止客户端连接
	/// </summary>
	public void StopClientClbk () {
		StopClient();		// stop client
		ChangeTo(mainMenuPanel);
	}

	/// <summary>
	/// 停止服务器
	/// </summary>
	public void StopServerClbk () {
		StopServer();
		ChangeTo (mainMenuPanel);
	}

	/// <summary>
	/// Kick message.
	/// </summary>
	class KickMsg : MessageBase { }
	/// <summary>
	/// Kicks the player.
	/// </summary>
	/// <param name="conn">Conn.</param>
	public void KickPlayer (NetworkConnection conn) {
		conn.Send (MsgKicked, new KickMsg ());
	}

	/// <summary>
	/// Kickeds the message handler.
	/// </summary>
	/// <param name="netMsg">Net message.</param>
	public void KickedMessageHandler(NetworkMessage netMsg) {
		messageBox.Display("被服务器移出房间", "关闭", null);
		netMsg.conn.Disconnect();
	}

	public void SetupGame () {
		// Debug.Log (GameManager.instance);
		GameManager mgr = FindObjectOfType<GameManager> ();
		if (mgr == null) {
			Debug.LogError ("Game Manager is not found.");
			return;
		}
		mgr.villagersCount = roomSetting.villageCount;
		mgr.prophetsCount = roomSetting.prophetCount;
		mgr.huntersCount = roomSetting.hunterCount;
		mgr.witchesCount = roomSetting.witchCount;
		mgr.wolvesCount = roomSetting.wolfCount;
		mgr.playersCount = roomSetting.totalCount;
	}


	//===========================================================结束

	public override void OnStartHost() {
		base.OnStartHost();

		ChangeTo(lobbyPanel);		// 显示玩家大厅界面
		roomSettingButton.gameObject.SetActive (true);		// 显示设置按钮
		ipAdress.text = "Host IP: " + Network.player.ipAddress;
		ipAdress.gameObject.SetActive (true);
		backDelegate = StopHostClbk;
		#if UNITY_EDITOR
		Debug.Log ("Hosting on " + networkAddress);
		#endif
	}

	// 当进入一个玩家时，玩家数量的修改
	public void OnPlayersNumberModified(int count) {
		
		_playerNumber += count;
		// 如果角色有多个playercontroller
		//int localPlayerCount = 0;
		//foreach (PlayerController p in ClientScene.localPlayers)
		//	localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;
	}

	// ----------------- 服务器回调 ------------------

	public void UpdateReadyButtonState (bool condition) {
		for (int i = 0; i < lobbySlots.Length; ++i) {
			LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

			if (p != null) {
				p.ToggleJoinButton(condition);
			}
		}
	}

	// 为了实现在没有足够的玩家时不能准备
	/// <summary>
	/// Raises the lobby server create lobby player event.
	/// </summary>
	/// <param name="conn">Conn.</param>
	/// <param name="playerControllerId">Player controller identifier.</param>
	public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
		GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

		LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
		newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);

		UpdateReadyButtonState (numPlayers + 1 >= minPlayers);
		return obj;
	}

	/// <summary>
	/// Raises the lobby server player removed event.
	/// </summary>
	/// <param name="conn">Conn.</param>
	/// <param name="playerControllerId">Player controller identifier.</param>
	public override void OnLobbyServerPlayerRemoved (NetworkConnection conn, short playerControllerId) {
		UpdateReadyButtonState (numPlayers + 1 >= minPlayers);
	}

	/// <summary>
	/// Raises the lobby server disconnect event.
	/// </summary>
	/// <param name="conn">Conn.</param>
	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		UpdateReadyButtonState (numPlayers >= minPlayers);
	}

	/// <summary>
	/// 将大厅状态替换为游戏状态
	/// </summary>
	/// <param name="lobbyPlayer">Lobby player.</param>
	/// <param name="gamePlayer">Game player.</param>
	public override bool OnLobbyServerSceneLoadedForPlayer (GameObject lobbyPlayer, GameObject gamePlayer) {
		//This hook allows you to apply state data from the lobby-player to the game-player
		//just subclass "LobbyHook" and add it to the lobby object.

		if (lobbyHooks)
			lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

		return true;
	}


	// --- Countdown management

	public override void OnLobbyServerPlayersReady () {
		bool allready = true;
		for(int i = 0; i < lobbySlots.Length; ++i) {
			if(lobbySlots[i] != null)
				allready &= lobbySlots[i].readyToBegin;
		}

		if(allready)
			StartCoroutine(ServerCountdownCoroutine());
	}

	public IEnumerator ServerCountdownCoroutine () {
		float remainingTime = prematchCountdown;
		int floorTime = Mathf.FloorToInt(remainingTime);

		while (remainingTime > 0) {
			yield return null;

			remainingTime -= Time.deltaTime;
			int newFloorTime = Mathf.FloorToInt (remainingTime);

			if (newFloorTime != floorTime) {
				floorTime = newFloorTime;

				for (int i = 0; i < lobbySlots.Length; ++i) {
					if (lobbySlots[i] != null) {
						(lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
					}
				}
			}
		}

		for (int i = 0; i < lobbySlots.Length; ++i) {
			if (lobbySlots[i] != null) {
				(lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
			}
		}

		ServerChangeScene (playScene);
	}

	// ----------------- Client callbacks ------------------

	public override void OnClientConnect (NetworkConnection conn) {
		base.OnClientConnect (conn);


		messageBox.gameObject.SetActive (false);

		conn.RegisterHandler (MsgKicked, KickedMessageHandler);

		// 只在非主机客户端运行
		if (!NetworkServer.active) {
			ChangeTo(lobbyPanel);
			backDelegate = StopClientClbk;
		}
	}


	public override void OnClientDisconnect (NetworkConnection conn) {
		base.OnClientDisconnect(conn);
		countdownPanel.gameObject.SetActive (false);
		ChangeTo(mainMenuPanel);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode) {
		ChangeTo(mainMenuPanel);
		messageBox.Display("客户端错误 : " + (errorCode == 6 ? "连接超时" : errorCode.ToString()), "关闭", null);
	}

	public override void OnLobbyClientAddPlayerFailed () {
		base.OnLobbyClientAddPlayerFailed ();
		StopClient ();
		messageBox.Display ("房间人数已满", "确认", () => { ChangeTo (mainMenuPanel); });
	}

	//========================== Value Set

	public void SetMaxPlayers (int max) {
		max = Mathf.Clamp (max, 1, 254);
		// 需要更新
		if (max > maxPlayers) {
			NetworkLobbyPlayer[] slots = new NetworkLobbyPlayer[max];
			for (int i = 0; i < lobbySlots.Length; i++) {
				slots [i] = lobbySlots [i];
			}
			lobbySlots = slots;
		}
		maxPlayers = max;
	}

	public void SetMaxPerPlayers (int max) {
		maxPlayersPerConnection = Mathf.Clamp (max, 1, 254);
	}

	public void SetMinPlayers (int min) {
		minPlayers = Mathf.Clamp (min, 0, 254);
	}
}
