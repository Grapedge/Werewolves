using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Server only
/// <summary>
/// 通过服务器管理向其他玩家发送信息等
/// </summary>
public class ServerManager : NetworkBehaviour {

	/// <summary>
	/// The instance.
	/// </summary>
	public static ServerManager instance;

	private void Awake () {
		instance = this;
	}
		
	/// <summary>
	/// 移除一个 List 所有值为 null 的元素
	/// </summary>
	/// <param name="list">List.</param>
	public void RemoveEmpty (List<uPomelos> list) {
		if (list == null)
			return;
		list.RemoveAll(item => item == null);
	}

	//============================================ 服务器通信函数=============================================

	/// <summary>
	/// 发送信息到客户端
	/// </summary>
	/// <param name="chat">信息.</param>
	/// <param name="recevier">接收玩家.</param>
	public void SendChat (Chat chat, uPomelos recevier) {
		if (recevier == null)
			return;
		recevier.RpcShowChat (chat);		// 客户端进行信息显示
	}

	/// <summary>
	/// 发送信息到客户端
	/// </summary>
	/// <param name="chat">信息.</param>
	/// <param name="recevier">接收列表.</param>
	public void SendChat (Chat chat, List<uPomelos> recevier) {
		if (recevier == null)
			return;
		RemoveEmpty (recevier);
		foreach (var p in recevier) {
			p.RpcShowChat (chat);		// 客户端进行信息显示
		}
	}

	/// <summary>
	/// 发送信息到客户端
	/// </summary>
	/// <param name="chat">信息.</param>
	/// <param name="recevier">接收玩家.</param>
	public void SendSystemChat (string msg, uPomelos recevier) {
		if (recevier == null)
			return;
		recevier.RpcShowChat (new Chat (-1, "System", msg, ChatSkin.Default));		// 客户端进行信息显示
	}

	/// <summary>
	/// 发送信息到客户端
	/// </summary>
	/// <param name="chat">信息.</param>
	/// <param name="recevier">接收列表.</param>
	public void SendSystemChat (string msg, List<uPomelos> recevier) {
		if (recevier == null)
			return;
		RemoveEmpty (recevier);
		foreach (var p in recevier) {
			p.RpcShowChat (new Chat (-1, "System", msg, ChatSkin.Default));		// 客户端进行信息显示
		}
	}

	//========================================================结束

	//=======================================================元素获取函数============

	/// <summary>
	/// 得到 Network 中的玩家对象
	/// </summary>
	/// <returns>The player.</returns>
	/// <param name="_netId">Net identifier.</param>
	/// <param name="players">Players.</param>
	public uPomelos GetPlayer (NetworkInstanceId _netId, List<uPomelos> players) {
		if (players == null)
			return null;
		RemoveEmpty (players);
		foreach (var p in players) {
			if (p.netId == _netId)
				return p;
		}
		return null;
	}

	/// <summary>
	/// 得到 Network 中的玩家对象
	/// </summary>
	/// <returns>The player.</returns>
	/// <param name="behaviourId">Behaviour identifier.</param>
	/// <param name="players">Players.</param>
	public uPomelos GetPlayer (int behaviourId, List<uPomelos> players) {
		if (players == null)
			return null;
		RemoveEmpty (players);
		foreach (var p in players) {
			if (p.behaviourId == behaviourId)
				return p;
		}
		return null;
	}

	/// <summary>
	/// 启用/禁用玩家输入
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
	/// <param name="players">Players.</param>
	public void TogglePlayerInput (bool active, UnityEngine.UI.InputField.ContentType type, uPomelos player) {
		if (player == null)
			return;
		if (player.isDie)
			player.RpcToggleInput (false, type);
		else
			player.RpcToggleInput (active, type);
	}
		
	/// <summary>
	/// 启用/禁用玩家输入
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
	/// <param name="players">Players.</param>
	public void TogglePlayerInput (bool active, UnityEngine.UI.InputField.ContentType type, List<uPomelos> players) {
		if (players == null)
			return;
		RemoveEmpty (players);
		foreach (var p in players) {
			if (p.isDie)
				p.RpcToggleInput (false, type);
			else
				p.RpcToggleInput (active, type);
		}
	}

	/// <summary>
	/// 设置信息发送者的接收者
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="receiver">Receiver.</param>
	public void SetReceivePlayer (uPomelos sender, List<uPomelos> receiver) {
		if (sender == null) {
			return;
		}
		if (receiver == null)
			receiver = new List<uPomelos> () { sender };
		sender.canReceiveChatList = receiver;
	}

	/// <summary>
	/// 设置信息发送者的接收者
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="receiver">Receiver.</param>
	public void SetReceivePlayer (List<uPomelos> sender, List<uPomelos> receiver) {
		if (sender == null) {
			return;
		}
		RemoveEmpty (sender);
		if (receiver == null) {
			foreach (var p in sender) {
				p.canReceiveChatList = new List<uPomelos> () { p };
			}
		} else {
			foreach (var p in sender) {
				p.canReceiveChatList = receiver;
			}
		}
	}

	// 检测玩家发言状态
	public bool CheckAllPlayerEndSpeech (List<uPomelos> players) {
		if (players == null || players.Count == 0)
			return false;
		bool allready = true;
		RemoveEmpty (players);
		foreach (var p in players) {
			if (!p.isDie)
				allready &= (p.isRobot || p.endSpeech);
		}
		return allready;
	}

	// 重置玩家发言状态
	public void ResetAllPlayerState (List<uPomelos> players) {
		RemoveEmpty (players);
		foreach (var p in players) {
			p.endSpeech = false;
			p.inputValue = -1;
		}
	}

	/// <summary>
	/// Gets the max count input player.
	/// </summary>
	/// <returns>The max count input player.</returns>
	/// <param name="players">Players.</param>
	public List<uPomelos> GetMaxCountInputPlayer (List<uPomelos> players, bool sheriffAdv = false) {
		if (players == null)
			return null;
		RemoveEmpty (players);

		int maxCount = -1;
		Dictionary<int, int> dic = new Dictionary<int, int> ();

		foreach (var p in players) {
			int input = p.inputValue;	// 得到 input value
			uPomelos up = GetPlayer (input, GameManager.instance.players);
			if (up == null || up.isDie) continue;
			if (!dic.ContainsKey (input)) dic.Add (input, 0);
			// 因为警长有1.5票 等价于 警长有3票，其他人2票
			if (sheriffAdv == true && p.isSheriff)
				++dic [input];		// 警长特殊票
			dic [input] += 2;		// 普通投票
			if (++dic [input] > maxCount)
				maxCount = dic [input];
		}
			
		List<uPomelos> res = new List<uPomelos> ();
		foreach (var d in dic) { if (d.Value == maxCount) { res.Add (GetPlayer (d.Key, GameManager.instance.lives)); } }
		RemoveEmpty (res);
		return res;
	}

	/// <summary>
	/// Sets the type of the player input.
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="players">Players.</param>
	public void SetPlayerInputType (UnityEngine.UI.InputField.ContentType type, uPomelos player) {
		if (player == null)
			return;
		player.RpcModifyInputType (type);
	}

	/// <summary>
	/// Sets the type of the player input.
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="players">Players.</param>
	public void SetPlayerInputType (UnityEngine.UI.InputField.ContentType type, List<uPomelos> players) {
		if (players == null)
			return;
		RemoveEmpty (players);
		foreach (var p in players) {
			p.RpcModifyInputType (type);
		}
	}
		
}
