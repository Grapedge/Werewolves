using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// Lobby player.
/// 处理大厅中角色的颜色，命名等信息
/// </summary>
public class LobbyPlayer : NetworkLobbyPlayer {
	// 启用的颜色
	static Color[] Colors = new Color[] { 
		new Color (1f, 84f / 255f, 107f / 255f),		// 西瓜红
		new Color (1f, 65f / 255f, 55f / 255f),			// 红色
		new Color (48f / 255f, 203f / 255f, 44f / 255f),		// 青色
		new Color (1f / 255f, 115f / 255f, 218f / 255f),		// 蓝色
		new Color (61f / 255f, 153f / 255f, 113f / 255f), 		// 绿色
		new Color (251f / 255f, 218f / 255f, 0f)			// 黄色
	};

	public Button colorButton;		// 颜色按钮
	public InputField nameInput;		// 名字输入
	public Button readyButton;		// 准备按钮
	public Button waitingPlayerButton;		// 当玩家人数不足时显示此按钮
	public Button removePlayerButton;		// 移除玩家按钮

	public GameObject localIcone;		// 本地玩家图标 (host)
	public GameObject remoteIcone;		// 远程玩家图标 (client)

	[SyncVar (hook = "OnNameChange")]
	public string playerName;
	// 我不保证之后还会不会用到这个颜色设置，毕竟只是为了好看
	[SyncVar (hook = "OnColorChange")]
	public Color playerColor = Color.white;

	public Color OddRowColor = new Color(250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f);
	public Color EvenRowColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f);

	static Color JoinColor = new Color(255.0f/255.0f, 0.0f, 101.0f/255.0f,1.0f);
	static Color NotReadyColor = new Color(34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f);
	static Color ReadyColor = new Color(68.0f / 255.0f, 68.0f / 255.0f, 100.0f / 255.0f, 1.0f);
	static Color TransparentColor = new Color(0, 0, 0, 0);

	// 在新玩家进入大厅时在所有玩家对象上执行
	public override void OnClientEnterLobby () {
		base.OnClientEnterLobby ();

		if (PandaNetworkManager.instance != null) PandaNetworkManager.instance.OnPlayersNumberModified (1);
		// 将新玩家加入列表
		LobbyPlayerList.instance.AddPlayer (this);

		// 设置此玩家信息
		if (isLocalPlayer) {
			SetupLocalPlayer ();
		} else {
			SetupOtherPlayer ();
		}

		// 建立玩家数据和 UI 界面
		OnNameChange (playerName);
		OnColorChange (playerColor);
	}

	public override void OnStartAuthority () {
		base.OnStartAuthority();

		readyButton.transform.GetChild(0).GetComponent<Text>().color = Color.white;
		SetupLocalPlayer();
	}

	/// <summary>
	/// 设置准备按钮颜色
	/// </summary>
	/// <param name="c">C.</param>
	private void ChangeReadyButtonColor (Color c) {
		ColorBlock b = readyButton.colors;
		b.normalColor = c;
		b.pressedColor = c;
		b.highlightedColor = c;
		b.disabledColor = c;
		readyButton.colors = b;
	}

	/// <summary>
	/// 建立其他玩家，加入的玩家对象不是自身
	/// </summary>
	private void SetupOtherPlayer () {
		nameInput.interactable = false;
		removePlayerButton.interactable = NetworkServer.active;		// 保证只有服务器可以踢人

		ChangeReadyButtonColor (NotReadyColor);
		readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
		readyButton.interactable = false;

		OnClientReady (false);
	}

	/// <summary>
	/// 建立自身玩家
	/// </summary>
	private void SetupLocalPlayer () {
		nameInput.interactable = true;
		remoteIcone.gameObject.SetActive(false);
		localIcone.gameObject.SetActive(true);

		// CheckRemoveButton ();

		if (playerColor == Color.white)
			CmdColorChange ();

		ChangeReadyButtonColor (JoinColor);

		readyButton.transform.GetChild(0).GetComponent<Text>().text = "准备";
		readyButton.interactable = true;

		if (playerName == "")
			CmdNameChanged ("Player");

		colorButton.interactable = true;
		nameInput.interactable = true;

		nameInput.onEndEdit.RemoveAllListeners();
		nameInput.onEndEdit.AddListener(OnNameChanged);

		colorButton.onClick.RemoveAllListeners();
		colorButton.onClick.AddListener(OnColorClicked);

		readyButton.onClick.RemoveAllListeners();
		readyButton.onClick.AddListener(OnReadyClicked);
	}

	public override void OnClientReady (bool readyState) {
		if (readyState) {
			ChangeReadyButtonColor (TransparentColor);

			Text textComponent = readyButton.transform.GetChild (0).GetComponent<Text> ();
			textComponent.text = "等待"; 
			textComponent.color = ReadyColor;
			readyButton.interactable = false;
			colorButton.interactable = false;
			nameInput.interactable = false;
		} else {
			ChangeReadyButtonColor(isLocalPlayer ? JoinColor : NotReadyColor);

			Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
			textComponent.text = isLocalPlayer ? "准备" : "...";
			textComponent.color = Color.white;
			readyButton.interactable = isLocalPlayer;
			colorButton.interactable = isLocalPlayer;
			nameInput.interactable = isLocalPlayer;
		}
	}

	public void OnPlayerListChanged (int idx) {
		GetComponent<Image> ().color = (idx % 2 == 0) ? EvenRowColor : OddRowColor;
	}

	//==========================同步变量回调
	public void OnNameChange (string newName) {
		playerName = newName;
		nameInput.text = playerName;
	}

	public void OnColorChange (Color newColor) {
		playerColor = newColor;
		colorButton.GetComponent<Image> ().color = newColor;
	}

	//========================UI Handler
	public void OnColorClicked () {
		CmdColorChange ();
	}

	public void OnReadyClicked () {
		SendReadyToBeginMessage ();
	}

	public void OnNameChanged (string str) {
		CmdNameChanged (str);
	}

	public void OnRemovePlayerClick () {
		if (!isLocalPlayer && isServer) {
			PandaNetworkManager.instance.KickPlayer (connectionToClient);
		}
	}

	/// <summary>
	/// 修改准备按钮的状态
	/// </summary>
	/// <param name="enabled">If set to <c>true</c> enabled.</param>
	public void ToggleJoinButton (bool enabled) {
		readyButton.gameObject.SetActive(enabled);
		waitingPlayerButton.gameObject.SetActive(!enabled);
	}

	[ClientRpc]
	public void RpcUpdateCountdown (int countdown) {
		PandaNetworkManager.instance.countdownPanel.UIText.text = "" + countdown;
		PandaNetworkManager.instance.countdownPanel.gameObject.SetActive (countdown != 0);
	}

	//===================================Command

	[Command] public void CmdColorChange () {
		int size = Colors.Length;
		playerColor = Colors [Random.Range (0, size)];
	}

	[Command] public void CmdNameChanged (string name) {
		playerName = name;
	}

	public void OnDestroy () {
		LobbyPlayerList.instance.RemovePlayer (this);
		if (PandaNetworkManager.instance != null)
			PandaNetworkManager.instance.OnPlayersNumberModified (-1);
	}

}

