using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//==================================1105发布，测试
//==================================1106补丁：优化代码 

public class PlayerState : uState<GameManager> {

	/// <summary>
	/// 系统每次发送消息的时间间隔
	/// </summary>
	protected readonly float waitTime = 1.5f;		

	/// <summary>
	/// 使用机器人托管后的随机时间间隔
	/// </summary>
	protected readonly float robotRandomTime = 8.5f;		

	protected ServerManager serverManager = null;

	/// <summary>
	/// 初始化玩家：启用输入系统
	/// </summary>
	/// <param name="players">Players.</param>
	/// <param name="type">Type.</param>
	protected void SetupPlayer (List<uPomelos> players, UnityEngine.UI.InputField.ContentType type) {
		// 禁用所有玩家输入系统
		serverManager.TogglePlayerInput (false, type, target.players);
		// 设置玩家发言的接收列表为本身
		serverManager.SetReceivePlayer (players, players);
		// 启用玩家输入系统
		serverManager.TogglePlayerInput (true, type, players);
	}

	protected void ResetPlayer (List<uPomelos> players) {
		// 设置为无人接收信息
		serverManager.SetReceivePlayer (players, null);
		// 禁用玩家输入系统
		serverManager.TogglePlayerInput (false, UnityEngine.UI.InputField.ContentType.Standard, players);
	}

	protected void SetupPlayer (uPomelos player, UnityEngine.UI.InputField.ContentType type) {
		// 禁用所有玩家输入系统
		serverManager.TogglePlayerInput (false, type, target.players);
		// 设置玩家发言的接收列表为本身
		serverManager.SetReceivePlayer (player, target.players);
		// 启用玩家输入系统
		serverManager.TogglePlayerInput (true, type, player);
	}

	protected void ResetPlayer (uPomelos player) {
		// 设置为无人接收信息
		serverManager.SetReceivePlayer (player, null);
		// 禁用玩家输入系统
		serverManager.TogglePlayerInput (false, UnityEngine.UI.InputField.ContentType.Standard, player);
	}

	public override void Enter (GameManager target) {
		base.Enter (target);
		serverManager = ServerManager.instance;
	}

	/// <summary>
	/// 以系统身份发送消息
	/// </summary>
	/// <param name="msg">Message.</param>
	protected IEnumerator SendChat (string msg, List<uPomelos> receiver) {
		yield return new WaitForSeconds (waitTime);
		serverManager.SendSystemChat (msg, receiver);
	}

	/// <summary>
	/// 以系统身份发送消息
	/// </summary>
	/// <param name="msg">Message.</param>
	protected IEnumerator SendChat (string msg, uPomelos receiver) {
		yield return new WaitForSeconds (waitTime);
		serverManager.SendSystemChat (msg, receiver);
	}

	/// <summary>
	/// 显示存活玩家列表
	/// </summary>
	/// <returns>The live.</returns>
	protected IEnumerator ShowLive (List<uPomelos> players) {
		// 存活玩家
		string livePlayer = "当前存活的玩家有：";
		foreach (var p in target.lives) {
			livePlayer += p.playerName + "，";
		}

		yield return target.StartCoroutine (SendChat (livePlayer, players));
	}

	/// <summary>
	/// 等待玩家发言完毕
	/// </summary>
	/// <returns>The for speech.</returns>
	protected IEnumerator WaitForSpeech (uPomelos player, float facTime) {
		for (float timer = 0f; !player.endSpeech && timer <= facTime; timer += Time.deltaTime) yield return null;
	}

	/// <summary>
	/// 等待玩家发言完成，注意这是同时发言
	/// </summary>
	/// <returns>The for speech.</returns>
	/// <param name="players">检测的玩家.</param>
	/// <param name="facTime">发言时间.</param>
	/// <param name="checkTime">检查时间.</param>
	protected IEnumerator WaitForSpeech (List<uPomelos> players, float facTime, float checkTime = 1f) {
		float lastTime = 0f;
		for (float timer = 0f; timer < facTime; timer += Time.deltaTime) {
			if (timer - lastTime >= checkTime) {
				if (serverManager.CheckAllPlayerEndSpeech (players)) break;
				lastTime = timer;
			}
			yield return null;
		}
	}
}

//==============================================================狼人
//                     所有状态使用协程实现。
//==============================================================



public class WolfState : PlayerState {

	// 狼人阶段：提示信息=>狼人讨论=>狼人投票

	private readonly float discussTime = 45f;
	private readonly float voteTime = 25f;

	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (WolfExcute ());
	}

	private IEnumerator WolfExcute () {
		// 显示提示信息
		yield return target.StartCoroutine (ShowTip ());
		// 开始讨论
		yield return target.StartCoroutine (Discuss ());
		// 开始投票
		yield return target.StartCoroutine (Vote ());

		target.ChangeState (GameManager.State.Witch);
	}

	private IEnumerator ShowTip () {
		ResetPlayer (target.players);

		yield return target.StartCoroutine (SendChat ("进入黑夜阶段，狼人将进行操作。", target.players));

		// 获得存活狼人列表
		string liveWolf = "当前存活的狼人玩家有：";
		foreach (var p in target.wolves) {
			liveWolf += p.playerName + "，";
		}
		yield return target.StartCoroutine (SendChat (liveWolf, target.wolves));

		// 显示存活玩家
		yield return target.StartCoroutine (ShowLive (target.wolves));
	}

	private IEnumerator Discuss () {
		yield return target.StartCoroutine (SendChat ("你们有 " + discussTime.ToString () + " s 的时间讨论本回合要杀害的玩家。", target.wolves));

		SetupPlayer (target.wolves,  UnityEngine.UI.InputField.ContentType.Standard);
	
		float facTime = target.wolves.Count > 0 ? discussTime : Random.Range (0f, robotRandomTime);

		// 等待讨论
		yield return target.StartCoroutine (WaitForSpeech (target.wolves, facTime));
		// 讨论结束。
		ResetPlayer (target.wolves);
	}

	private IEnumerator Vote () {
		yield return target.StartCoroutine (SendChat ("你将有" + voteTime.ToString () + "s 的时间输入你要杀害的玩家编号，输入错误视为空刀，平票随机。", target.wolves));

		SetupPlayer (target.wolves, UnityEngine.UI.InputField.ContentType.IntegerNumber);
		// 投票环节无人接收消息
		serverManager.SetReceivePlayer (target.wolves, null);

		float facTime = target.wolves.Count > 0 ? voteTime : Random.Range (0f, robotRandomTime);

		// 投票时只能输入数字
		serverManager.SetPlayerInputType (UnityEngine.UI.InputField.ContentType.IntegerNumber, target.wolves);

		// 等待狼人玩家输入
		yield return target.StartCoroutine (WaitForSpeech (target.wolves, facTime));

		ResetPlayer (target.wolves);

		// 得到投票列表
		target.StartCoroutine (CountWolfKill ());
	}

	/// <summary>
	/// 统计狼人的杀人信息
	/// </summary>
	private IEnumerator CountWolfKill () {
		List<uPomelos> kill = serverManager.GetMaxCountInputPlayer (target.wolves);
		// 空刀
		if (kill.Count < 1) {
			yield return target.StartCoroutine (SendChat ("本回合你们进行了空刀，你们没有杀害玩家。", target.wolves));
		} else {
			uPomelos die = kill [Random.Range (0, kill.Count)];
			yield return target.StartCoroutine (SendChat ("你们在本回合中你们杀害了" + die.behaviourId.ToString () + "号玩家", target.wolves));
			if (!target.victims.Contains (die)) target.victims.Add (die);		// 将玩家添加入死亡列表
		}
	}
		
}


//==================================================女巫

public class WitchState : PlayerState {

	// 女巫阶段：提示信息=>是否救人=>是否毒人

	private readonly float saveTime = 25f;
	private readonly float killTime = 25f;
	private bool save = false;

	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (WitchExcute ());
	}

	private IEnumerator WitchExcute () {
		// 显示提示信息
		yield return target.StartCoroutine (ShowTip ());
		// 是否救人
		yield return target.StartCoroutine (Save ());
		// 是否杀人
		yield return target.StartCoroutine (Kill ());
		target.ChangeState (GameManager.State.Prophet);
	}

	private IEnumerator ShowTip () {
		yield return target.StartCoroutine (SendChat  ("狼人操作完毕，女巫将进行操作。", target.players));

		yield return target.StartCoroutine (ShowLive (target.witches));

		string diePlayer;

		// 是否有解药
		bool hasAbtidote = true;

		foreach (var p in target.witches) {
			hasAbtidote &= (p.antidoteCount > 0);
		}

		if (!hasAbtidote) {		// 没有解药
			diePlayer = "由于所有女巫已经没有解药，你们不能知道今晚死亡的玩家。";
		} else if (target.victims.Count > 0) {
			diePlayer = "今晚被杀的玩家是：" + target.victims [0].playerName;
		} else {
			diePlayer = "今晚没有玩家被杀。";
		}

		yield return target.StartCoroutine (SendChat (diePlayer, target.witches));
	}

	private IEnumerator Save () {
		// 回合等待
		yield return new WaitForSeconds (waitTime);

		foreach (var p in target.witches) {
			serverManager.SendSystemChat ("你们是否救该玩家？输入 1 使用解药。您当前拥有 " + p.antidoteCount.ToString () + " 瓶解药。", p);
		}

		SetupPlayer (target.witches, UnityEngine.UI.InputField.ContentType.IntegerNumber);

		// 投票类环节无人接收信息。
		serverManager.SetReceivePlayer (target.witches, null);


		// 等待一定时间，判断所有女巫玩家是否过麦
		float facTime = target.witches.Count > 0 ? saveTime : Random.Range (0f, robotRandomTime);

		yield return target.StartCoroutine (WaitForSpeech (target.witches, facTime));

		ResetPlayer (target.witches);

		// 判断是否救人
		save = false;

		if (target.victims.Count > 0) {
			foreach (var p in target.witches) {
				// 有药水
				if (p.inputValue == 1 && p.antidoteCount > 0) {
					--p.antidoteCount;  		// 减去一瓶药水
					// 移除死亡的玩家
					target.victims.Remove (target.victims [0]);
					serverManager.SendSystemChat ("您使用一瓶解药救了该玩家。", p);
					save = true;
					break;
				}
			}
		}

		serverManager.SendSystemChat (save ? "此玩家已被某个女巫拯救。" : "没有女巫使用解药。", target.witches);
	}

	private IEnumerator Kill () {
		// 回合等待
		yield return new WaitForSeconds (waitTime);
		if (save == false) {
			
			foreach (var p in target.witches) {
				serverManager.SendSystemChat ("输入要投毒的玩家编号使用毒药，不输入或输入错误视为不使用。你拥有 " + p.poisonCount.ToString () + " 瓶毒药。", p);
			}

			SetupPlayer (target.witches, UnityEngine.UI.InputField.ContentType.IntegerNumber);

			// 投票类环节无人接收信息。
			serverManager.SetReceivePlayer (target.witches, null);

			// 等待一定时间，判断所有女巫玩家是否过麦
			float facTime = target.witches.Count > 0 ? killTime : Random.Range (0f, robotRandomTime);

			yield return target.StartCoroutine (WaitForSpeech (target.witches, facTime));

			ResetPlayer (target.witches);

			uPomelos kill = null;
			foreach (var p in target.witches) {
				// 有药水
				kill = serverManager.GetPlayer (p.inputValue, target.players);
				if (kill != null && kill.isDie == false && p.poisonCount > 0) {
					p.poisonCount--;  		// 减去一瓶药水
					serverManager.SendSystemChat ("您使用一瓶毒药杀死了该玩家。", p);
					kill.killBy = 2;
					if (kill.behaviourClass == uPomelos.PlayerClasses.Hunter) {
						serverManager.SendSystemChat ("女巫将毒药投给了你，这表示你死亡后将不能使用技能。", kill);
					}
					target.victims.Add (kill);
					break;
				}
			}

			serverManager.SendSystemChat ((kill == null ? "没有女巫使用毒药。" : "某个女巫使用毒药杀死了 " + kill.playerName + " 玩家"), target.witches);

		} else {
			serverManager.SendSystemChat ("由于有女巫使用了解药，所以今晚无法使用毒药。", target.witches);
		}
	}

}


//==============================================================预言家

public class ProphetState : PlayerState {

	// 预言家阶段：提示信息=>预言家投票

	private readonly float voteTime = 25f;

	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (ProphetExcute ());
	}

	private IEnumerator ProphetExcute () {
		// 显示提示信息
		yield return target.StartCoroutine (ShowTip ());		// 提示
		// 开始投票
		yield return target.StartCoroutine (Vote ());		// 投票
		target.ChangeState (GameManager.State.Sheriff);
	}

	private IEnumerator ShowTip () {
		// 回合等待
		yield return target.StartCoroutine (SendChat ("女巫操作结束，预言家将进行操作。", target.players));

		// 存活玩家
		yield return target.StartCoroutine (ShowLive (target.prophets));

	}

	private IEnumerator Vote () {
		// 回合等待
		yield return target.StartCoroutine (SendChat ("你将有" + voteTime.ToString () + "s 的时间输入你要检验的玩家编号，输入错误为不查验，平票随机。", target.prophets));

		SetupPlayer (target.prophets, UnityEngine.UI.InputField.ContentType.IntegerNumber);

		// 投票环节无人接收消息
		serverManager.SetReceivePlayer (target.prophets, null);

		float facTime = target.prophets.Count > 0 ? voteTime : Random.Range (0f, robotRandomTime);

		yield return target.StartCoroutine (WaitForSpeech (target.prophets, facTime));

		ResetPlayer (target.prophets);

		target.StartCoroutine (CountProphetCheck ());

	}

	/// <summary>
	/// 统计预言家的验人信息
	/// </summary>
	private IEnumerator CountProphetCheck () {
		List<uPomelos> check = serverManager.GetMaxCountInputPlayer (target.prophets);
		// 没有检验的玩家
		if (check.Count < 1) {
			yield return target.StartCoroutine (SendChat ("本回合你们没有查验玩家。", target.prophets));
		} else {
			uPomelos test = check [Random.Range (0, check.Count)];
			yield return target.StartCoroutine (SendChat ("你们检验的玩家 " + test.playerName + " 的身份是 " + (test.behaviourClass == uPomelos.PlayerClasses.Wolf ? "坏人" : "好人"), target.prophets));
		}
	}

}



//======================================================= 竞选警长
public class SheriffState : PlayerState {

	// 预言家阶段：提示信息=>竞选警长玩家投票=>竞选警长玩家发言=>玩家投票=>再次发言

	private readonly float thinkTime = 30f;
	private readonly float speechTime = 240f;

	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (SheriffExcute ());
	}

	private IEnumerator SheriffExcute () {
		// 显示提示
		yield return target.StartCoroutine (ShowTip ());		// 提示

		// 如果有警徽
		if (target.hasSheriff == true) { 
			// 如果没有警长
			if (target.sherif == null) {
				// 参与警长竞选 
				yield return target.StartCoroutine (Join ());		// 投票
			}
		}

		DieState.vote = false;		// 表示公布死亡状态时以公布状态显示
		target.ChangeState (GameManager.State.Die);
	}

	private IEnumerator ShowTip () {
		// 回合等待
		yield return target.StartCoroutine (SendChat ("预言家操作结束，进入白天阶段。", target.players));

		// 存活玩家
		yield return target.StartCoroutine (ShowLive (target.players));

	}

	private List<uPomelos> sheriffs;

	private IEnumerator Join () {
		// 回合等待
		yield return target.StartCoroutine (SendChat ("你将有" + thinkTime.ToString () + "s 的时间决定是否参与警长竞选，如果参加请输入 1 。", target.lives));

		// 只有活人可以参加竞选
		SetupPlayer (target.lives, UnityEngine.UI.InputField.ContentType.IntegerNumber);
		// 投票环节无人接收消息
		serverManager.SetReceivePlayer (target.lives, null);

		yield return target.StartCoroutine (WaitForSpeech (target.lives, thinkTime));
		                                    
		ResetPlayer (target.lives);

		// === 统计警长
		sheriffs = new List<uPomelos> ();
		string tex = "参与警长竞选的玩家有：";
		foreach (var p in target.lives) {
			if (p.inputValue == 1) {
				sheriffs.Add (p);
				tex += p.playerName + "，";
			}
		}
		yield return target.StartCoroutine (SendChat (tex, target.players));
		yield return target.StartCoroutine (Campaign ());		// 竞选
	}

	/// <summary>
	/// 统计预言家的验人信息
	/// </summary>
	private IEnumerator Campaign () {
		
		if (sheriffs.Count == 1) {
			// 直接当选
			target.hasSheriff = true;
			target.sherif = sheriffs[0];
			sheriffs [0].isSheriff = true;
			yield return target.StartCoroutine (SendChat ("由于只有一人上警，玩家" + sheriffs[0].playerName + "直接当选警长。", target.players));
		}

		else if (sheriffs.Count == 0) {
			// 没有警长
			target.hasSheriff = false;
			target.sherif = null;
			yield return target.StartCoroutine (SendChat ("由于没有玩家上警，本局没有警长。", target.players));
		}

		else if (sheriffs.Count > 1) {
			// 选举 * 1
			yield return target.StartCoroutine (SpeechAndVote ());

			if (sheriffs.Count > 1) {
				yield return target.StartCoroutine (SendChat ("一些警长出现平票，未参与警长竞选的玩家将重新投票", target.players));
				// 选举 * 2
				yield return target.StartCoroutine (SpeechAndVote ());
			}

			if (sheriffs.Count > 1 ||sheriffs.Count == 0) {
				target.hasSheriff = false;
				target.sherif = null;
				yield return target.StartCoroutine (SendChat ("由于再次平票，本局没有警长。", target.players));
			} 

			else {
				target.hasSheriff = true;
				target.sherif = sheriffs [0];
				sheriffs [0].isSheriff = true;
				yield return target.StartCoroutine (SendChat ("玩家" + sheriffs[0].playerName + "在竞选中成为警长。", target.players));
			}
		}
	}

	private IEnumerator SpeechAndVote () {
		yield return target.StartCoroutine (SendChat ("接下来将按编号进行竞选发言。", target.players));
		foreach (var p in sheriffs) {
			yield return target.StartCoroutine (SendChat ("玩家 " + p.playerName + " 进行竞选发言。", target.players));
			SetupPlayer (p, UnityEngine.UI.InputField.ContentType.Standard);
			serverManager.SendSystemChat ("你有" + speechTime.ToString () + "s 的时间进行警长竞选的发言，点击结束按钮结束发言。", p);
			Debug.Log (p.endSpeech);
			yield return target.StartCoroutine (WaitForSpeech (p, speechTime));
			ResetPlayer (p);
		}

		List<uPomelos> unCampaign = new List<uPomelos> ();
		foreach (var p in target.lives) {
			if (!sheriffs.Contains (p)) {
				unCampaign.Add (p);
			}
		}

		yield return new WaitForSeconds (waitTime);
		serverManager.SendSystemChat ("未参与警长竞选的玩家正在进行投票。", sheriffs);
		serverManager.SendSystemChat ("你有" + thinkTime.ToString () + "s 的时间输入选择的警长编号，输入错误为不投票。", unCampaign);

		SetupPlayer (unCampaign, UnityEngine.UI.InputField.ContentType.IntegerNumber);

		serverManager.SetReceivePlayer (unCampaign, null);

		yield return target.StartCoroutine (WaitForSpeech (unCampaign, unCampaign.Count > 0 ? thinkTime : Random.Range (0f, robotRandomTime)));

		ResetPlayer (target.players);

		sheriffs = serverManager.GetMaxCountInputPlayer (unCampaign);
	}

}

//===============================公布死亡状态

public class DieState : PlayerState {
	public static bool vote;
	//= 死亡公布状态=>死亡玩家=>依次留遗言
	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (DieExcute ());
	}

	private IEnumerator CheckWin () {
		if (target.lives.Count == 0) {
			yield return target.StartCoroutine (SendChat ("所有玩家死亡，平局。", target.players));
			yield return new WaitForSeconds (6f);
			target.ChangeToLobbyScence ();
		}
		else if (target.wolves.Count == 0) {
			yield return target.StartCoroutine (SendChat ("游戏结束，好人胜利。", target.players));
			yield return new WaitForSeconds (6f);
			target.ChangeToLobbyScence ();
		} else {
			bool alldie = false;
			alldie = target.villagers.Count == 0 && target.hunters.Count == 0 && target.witches.Count == 0 && target.prophets.Count == 0;
			if (alldie) {
				yield return target.StartCoroutine (SendChat ("游戏结束，狼人胜利。", target.players));
				yield return new WaitForSeconds (6f);
				target.ChangeToLobbyScence ();
			}

			if (vote)
				target.ChangeState (GameManager.State.Wolf);
			else
				target.ChangeState (GameManager.State.Vote);
		}
	}

	private IEnumerator DieExcute () {
		yield return target.StartCoroutine (Publish (vote));
	}

	private IEnumerator Publish (bool vote) {
		if (!vote) {
			if (target.victims.Count == 0) {
				// 没有玩家死亡
				yield return target.StartCoroutine (SendChat ("昨晚是平安夜，没有玩家死亡。", target.players));
			}
		}
		string dieText = "这些玩家将死亡：";
		target.victims.Sort ((a, b) => {
			return a.behaviourId < b.behaviourId ? 1 : 0;
		});

		foreach (var p in target.victims) {
			dieText += p.playerName + "，";
		}

		yield return target.StartCoroutine (SendChat (dieText, target.players));

		foreach (var p in target.victims) {
			yield return target.StartCoroutine (LastWord (p));
			MakeDie (p);
		}

		target.victims.Clear ();		// 清空死亡列表

		target.StartCoroutine (CheckWin ());
	}

	private void MakeDie (uPomelos vic) {
		vic.isDie = true;
		if (target.lives.Contains (vic)) {
			target.lives.Remove (vic);
		}
		if (target.villagers.Contains (vic)) {
			target.villagers.Remove (vic);
		}
		else if (target.hunters.Contains (vic)) {
			target.hunters.Remove (vic);
		}
		else if (target.prophets.Contains (vic)) {
			target.prophets.Remove (vic);
		}
		else if (target.witches.Contains (vic)) {
			target.witches.Remove (vic);
		}
		else if (target.wolves.Contains (vic)) {
			target.wolves.Remove (vic);
		}
	}

	private float lastWdTime = 180f;

	private IEnumerator LastWord (uPomelos vic) {
		// 猎人死亡
		if (vic.behaviourClass == uPomelos.PlayerClasses.Hunter && vic.killBy != 2) {
			yield return target.StartCoroutine (ShootPlayer (vic));
		}
			
		yield return target.StartCoroutine (SendChat ("你需要留下遗言，按下结束停止发言。", vic));
		SetupPlayer (vic, UnityEngine.UI.InputField.ContentType.Standard);

		float facTime = vic.isRobot ? 3f : lastWdTime;

		yield return target.StartCoroutine (WaitForSpeech (vic, facTime));

		ResetPlayer (target.players);

		if (vic.isSheriff == true) {
			yield return MoveSheriff (vic);
		}

	}

	private float moveTime = 20f;

	private IEnumerator MoveSheriff (uPomelos player) {
		yield return target.StartCoroutine (SendChat ("你有 " + moveTime.ToString () + "s 的时间选择撕毁警徽或移交警徽，输入 -1 撕毁警徽，输入玩家编号进行移交。", player));
		SetupPlayer (player, UnityEngine.UI.InputField.ContentType.IntegerNumber);
		player.isSheriff = false;		// 移除警长身份
		target.sherif = null;
		serverManager.SetReceivePlayer (player, null);

		yield return target.StartCoroutine (WaitForSpeech (player, moveTime));

		ResetPlayer (target.players);

		int v = player.inputValue;
		uPomelos tmp = serverManager.GetPlayer (v, target.lives);

		if (tmp == null || tmp.isDie) {
			// 撕毁警徽
			target.hasSheriff = false;
			target.sherif = null;
			yield return target.StartCoroutine (SendChat ("玩家 " + player.playerName + " 撕毁了警徽。", target.players));
		} else {
			tmp.isSheriff = true;
			target.hasSheriff = true;
			yield return target.StartCoroutine (SendChat ("玩家将警徽移交给了 " + tmp.playerName + "。", target.players));
		}
	}

	private float skillTime = 20f;

	private IEnumerator ShootPlayer (uPomelos hunter) {
		yield return target.StartCoroutine (SendChat ("你拥有 " + skillTime.ToString () + "s 的时间输入你要杀死的玩家编号，输入错误视为不发动技能。", hunter));

		SetupPlayer (hunter, UnityEngine.UI.InputField.ContentType.IntegerNumber);

		yield return target.StartCoroutine (WaitForSpeech (hunter, skillTime));

		ResetPlayer (target.players);

		uPomelos kill = serverManager.GetPlayer (hunter.inputValue, target.players);
		if (kill == null || kill.isDie) {
			// 空刀
			yield return target.StartCoroutine (SendChat ("玩家选择不发动英雄技能。", target.players));
		} else {
			yield return target.StartCoroutine (SendChat ("猎人使用技能击杀了玩家：" + kill.playerName, target.players));
			target.victims.Add (kill);
		}
	}


}

//============================投票
public class VoteState : PlayerState {

	//==  警长决定警左警右发言。
	//==  玩家依次发言。
	//==  玩家投票
	//==  死亡遗言
	private float chatTime = 300f;

	public override void Enter (GameManager target) {
		base.Enter (target);
		target.StartCoroutine (VoteExcute ());
	}

	private int firstIdx = 0;
	private int offset = 1;
	private int startBehaviour = 0;

	List<uPomelos> maxCount;

	private IEnumerator VoteExcute () {
		// 警长决定发言顺序
		yield return target.StartCoroutine (HandleBySheriff ());
		// 依次发言
		yield return target.StartCoroutine (Chat (target.lives));
		// 同时投票
		yield return target.StartCoroutine (Voting ());

		if (maxCount == null || maxCount.Count == 0) {
			yield return target.StartCoroutine (SendChat ("由于无人投票，今天是平安日。", target.players));
			target.ChangeState (GameManager.State.Wolf);
		} else if (maxCount.Count > 1) {
			yield return target.StartCoroutine (SendChat ("由于有玩家平票，因此重新进行投票。", target.players));

			yield return target.StartCoroutine (Voting ());

			if (maxCount == null || maxCount.Count > 1) {
				serverManager.SendSystemChat ("由于无人投票或再次平票，今天是平安日。", target.players);
				target.ChangeState (GameManager.State.Wolf);
			}

		} else if (maxCount .Count == 1) {
			target.victims.Add (maxCount [0]);
			DieState.vote = true;		// 设置为投票后的死亡态
			target.ChangeState (GameManager.State.Die);
		}
	}

	private float thinkTime = 20f;

	private IEnumerator HandleBySheriff () {
		
		uPomelos sheriff = target.sherif;

		firstIdx = 0;
		offset = 1;
		startBehaviour = target.lives.Count - 1;

		if (sheriff != null) {
			yield return target.StartCoroutine (SendChat ("你有 " + thinkTime.ToString () + "s 的时间决定发言顺序，输入 1 从警右发言，输入 -1 从警左发言。", sheriff));

			SetupPlayer (sheriff, UnityEngine.UI.InputField.ContentType.IntegerNumber);

			serverManager.SetReceivePlayer (sheriff, null);

			yield return target.StartCoroutine (WaitForSpeech (sheriff, thinkTime));

			ResetPlayer (target.players);

			int v = sheriff.inputValue;

			startBehaviour = target.lives.IndexOf (sheriff);

			if (v == -1) {
				// 警左发言
				yield return target.StartCoroutine (SendChat ("警长决定由警左开始发言。", target.players));
				firstIdx = (startBehaviour - 1 + target.lives.Count) % target.lives.Count;
				offset = -1;
			} else {
				//警右发言
				yield return target.StartCoroutine (SendChat ("警长决定由警右开始发言。", target.players));
				firstIdx = ((startBehaviour + 1) % target.lives.Count);
				offset = 1;
			}
		}
	}

	private IEnumerator Chat (List<uPomelos> players) {
		
		yield return target.StartCoroutine (SendChat ("玩家将依次发言，每个玩家有至多 " + chatTime + "s 的时间。", target.players));

		for (int i = firstIdx; ; i = (i + offset + players.Count) % players.Count) {
			if (players [i] == null || players [i].isDie)
				continue;
			yield return target.StartCoroutine (SendChat ("玩家" + players [i].playerName + "开始发言。", target.players));

			SetupPlayer (players[i], UnityEngine.UI.InputField.ContentType.Standard);
			yield return WaitForSpeech (players [i], chatTime);

			ResetPlayer (players[i]);
			if (i == startBehaviour)
				break;
		}

	}

	private float voteTime = 30f;
	private IEnumerator Voting () {
		// 发言结束开始投票
		yield return target.StartCoroutine (SendChat ("请开始投票，输入你要投票的玩家编号。你们有 "+ voteTime.ToString () + "s 的时间进行选择。", target.players));

		SetupPlayer (target.lives, UnityEngine.UI.InputField.ContentType.IntegerNumber);

		serverManager.SetReceivePlayer (target.lives, null);		// 投票环节无人可看

		yield return target.StartCoroutine (WaitForSpeech (target.lives, thinkTime));

		ResetPlayer (target.players);

		maxCount = serverManager.GetMaxCountInputPlayer (target.lives, true);		// 这里警长有1.5票

		// Debug.LogError (maxCount);
	}
}