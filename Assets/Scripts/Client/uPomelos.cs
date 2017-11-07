using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 用于执行服务器端的操作。
/// 唯一标识符 netId
/// </summary>
public class uPomelos : NetworkBehaviour {

	// 玩家职业
	public enum PlayerClasses {
		Villager,
		Witch,
		Prophet,
		Wolf,
		Hunter
	}

	//=====================================玩家信息

	/// <summary>
	/// 玩家 Id.
	/// </summary>
	[SyncVar] public int behaviourId = -1;
	/// <summary>
	/// 玩家命名
	/// </summary>
	[SyncVar] public string behaviourName = "Player";
	/// <summary>
	/// 玩家职业，状态中进行分配
	/// </summary>
	[SyncVar] public PlayerClasses behaviourClass;

	//=====================================状态
	//=======================公共
	[SyncVar] public bool isDie = false;		// 是否死亡
	[SyncVar] public bool endSpeech = false;	// 结束发言

	//======================女巫
	[SyncVar] public int antidoteCount = 0;		// 解药
	[SyncVar] public int poisonCount = 0;		// 毒药

	//=====================警长
	[SyncVar] public bool isSheriff = false;			// 是否是警长
	[SyncVar] public int inputValue;		// 玩家输入的数值
	[SyncVar] public int killBy = 0;		// 当 killBy = 2 时表示被女巫杀死
	[SyncVar] public bool isRobot = false;			// 启用机器人系统
	[SyncVar] public bool hasStart = false;			// 是否初始化完毕

	// 该对象发送消息后可以接收消息的列表
	public List<uPomelos> canReceiveChatList = null;		// 只在服务器端更新

	//======================================UI
	private uPomelosCanvasManager m_CanvasManager = null;

	public override void OnStartLocalPlayer () {
		base.OnStartLocalPlayer ();
		m_CanvasManager = FindObjectOfType<uPomelosCanvasManager> ();

		if (m_CanvasManager == null) 
			Debug.LogWarning ("Canvas Manager is not found. Something will error.");

		m_CanvasManager.player = this;
	}

	private void Start () {
		CmdStart ();
	}

	//==================================属性

	/// <summary>
	/// 得到玩家命名，以 behaviorName + id 的命名方式
	/// </summary>
	/// <value>The name of the player.</value>
	public string playerName {
		get {
			return behaviourName + "(" + behaviourId.ToString () + ")";
		}
	}
		
	//======================================命令

	//======为了防止一些错误的操作，在每个函数
	//=====前进行判断是否为本地或服务器========
	//======================================命令

	[Command] private void CmdStart () {
		hasStart = true;		// 告诉服务器我们完成初始化了
	}

	/// <summary>
	/// Cmds：发送信息至服务端并扩散至客户端
	/// </summary>
	/// <param name="msg">Message.</param>
	[Command] public void CmdSendChat (string msg) {
		if (!isServer || ServerManager.instance == null) return;
		// 玩家每次输入我们尝试将其转换为数字
		int parsed = 0; 
		parsed = int.TryParse (msg, out parsed) ? parsed : -1;
		inputValue = parsed;
													// id			name	msg							chat skin											receivers.
		ServerManager.instance.SendChat (new Chat (behaviourId, playerName, msg, ChatSkin.DefaultSkins [Random.Range (0, ChatSkin.DefaultSkins.Length)]), canReceiveChatList);
	}

	/// <summary>
	/// Cmds: 修改发言状态
	/// </summary>
	[Command] public void CmdSpeechState (bool isEnd) {
		if (!isServer) return;
		endSpeech = isEnd;
	}

	/// <summary>
	/// Cmds the modify input value.
	/// </summary>
	/// <param name="value">Value.</param>
	[Command] public void CmdModifyInputValue (int value) {
		inputValue = value;
	}

	//======================================服务器回调

	/// <summary>
	/// Rpc：将消息发送至 UI 绘制界面
	/// </summary>
	[ClientRpc] public void RpcShowChat (Chat chat) {
		if (!isLocalPlayer) return;
		if (m_CanvasManager != null) {
			m_CanvasManager.AddChat (chat);
		}
	}

	/// <summary>
	/// 设置输入框的状态
	/// 启用/禁用：发送按钮，结束按钮，消息框，设置发言状态
	/// 禁用/启用发送按钮
	/// </summary>
	[ClientRpc] public void RpcToggleInput (bool active, UnityEngine.UI.InputField.ContentType type) {
		if (!isLocalPlayer)
			return;
		if (m_CanvasManager != null) {
			m_CanvasManager.ToggleChatInput (active);
			m_CanvasManager.ModifyInputType (type);
			CmdSpeechState (false);
			if (active == true) CmdModifyInputValue (-1);		// 如果是开启输入框则重置输入值
		}
	}

	/// <summary>
	/// Rpcs：修改输入框类型
	/// 用于一些只能输入特殊字符的环节
	/// </summary>
	/// <param name="type">Type.</param>
	[ClientRpc] public void RpcModifyInputType (UnityEngine.UI.InputField.ContentType type) {
		if (!isLocalPlayer) return;
		if (m_CanvasManager != null) {
			m_CanvasManager.ModifyInputType (type);
		}
	}


}
