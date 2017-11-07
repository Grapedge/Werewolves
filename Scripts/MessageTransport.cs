using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MessageTransport : NetworkBehaviour {

	/// <summary>
	/// 发送消息到服务器
	/// </summary>
	[Command] private void CmdSendMessage (string message) {
		MessageData msg = new MessageData (message, MessageManager.instance.behaviourID);
		MessageManager.instance.AddMessage (msg);
	}
}