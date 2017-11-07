using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Message manager. 
/// 用于管理消息的类
/// 接收由客户端发送的消息
/// 将消息同步至各个客户端
/// </summary>
public class MessageManager : NetworkBehaviour {

	/// <summary>
	/// 消息管理器，只有一个将运行
	/// </summary>
	public static MessageManager instance;
	/// <summary>
	/// 交流唯一标识符，由服务器更新
	/// </summary>
	[SyncVar] public int behaviourID;
	/// <summary>
	/// 这个列表中的消息将会被更新至界面
	/// </summary>
	private List<MessageData> messageList;
	/// <summary>
	/// 所有的信息都会在此列表中，只有服务器会更新
	/// </summary>
	private List<MessageData> allMessageList;

	public NetworkManager m_Manager;

	private void Awake () {
		instance = this;
	}

	/// <summary>
	/// 在服务器的消息列表中添加
	/// </summary>
	/// <param name="msg">Message.</param>
	[Server] public void AddMessage (MessageData msg) {
		allMessageList.Add (msg);
	}

	/// <summary>
	/// 将消息扩散至各个客户端
	/// 只有 id 处于 cr 列表中才可以接收
	/// </summary>
	[ClientRpc] private void RpcSendMessage (List<int> cr, MessageData msg) {
		if (isServer || !cr.Contains (behaviourID))
			return;
		messageList.Add (msg);
	}

	/// <summary>
	/// 发送消息到服务器
	/// </summary>
	[Command] private void CmdSendMessage (string message) {
		MessageData msg = new MessageData (message, behaviourID);
		MessageManager.instance.AddMessage (msg);
	}

	/// <summary>
	/// 当列表中有新消息时执行
	/// </summary>
	private void OnMessageUpdate () {
	}

	/// <summary>
	/// 在每局之前我们会更新玩家 ID
	/// </summary>
	[Server] private void UpdateBehaviourID () {
		foreach (var con in NetworkServer.connections) {
			foreach (var obj in con.playerControllers) {
				MessageManager m = obj.gameObject.GetComponent<MessageManager> ();
				if (m != null) {
					m.behaviourID = con.connectionId;
					Debug.Log (m.behaviourID);
				}
			}
		}
	}

	void OnPlayerConnected() {
		Debug.Log ("true");
	}
}
