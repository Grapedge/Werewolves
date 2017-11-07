using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初始化状态
/// </summary>
public class StartState : PlayerState {

	// 状态将执行：分配角色->开局提示->进入狼人状态

	public override void Enter (GameManager target) {
		base.Enter (target);
		StartTip ();
		AssignPlayer ();
		target.ChangeState (GameManager.State.Wolf);		// 进入狼人状态
	}

	private void AssignPlayer () {
		// 分配角色
		List<uPomelos> players = target.players;
		List<int> map = new List<int> ();		// 用于计算映射
		for (int i = 0; i < players.Count; i++) map.Add (i);

		string idTip = "游戏开始，你的身份为：";
		// 分配平民
		target.villagers.Clear ();
		for (int i = 0; i < target.villagersCount; i++) {
			int idx = map[Random.Range (0, map.Count)]; map.Remove (idx);	// 获得一个角色 id
			players[idx].behaviourClass = uPomelos.PlayerClasses.Villager;	// 更新职业
			target.villagers.Add (players[idx]);							// 加入列表
			serverManager.SendSystemChat (idTip + "村民，你的名字是：" + players[idx].playerName, players[idx]);
		}
			
		// 分配狼人
		target.wolves.Clear ();
		for (int i = 0; i < target.wolvesCount; i++) {
			int idx = map[Random.Range (0, map.Count)]; map.Remove (idx);	// 获得一个角色 id
			players[idx].behaviourClass = uPomelos.PlayerClasses.Wolf;		// 更新职业
			target.wolves.Add (players[idx]);							// 加入列表
			serverManager.SendSystemChat (idTip + "狼人，你的名字是：" + players[idx].playerName, players[idx]);
		}

		// 分配女巫
		target.witches.Clear ();
		for (int i = 0; i < target.witchesCount; i++) {
			int idx = map[Random.Range (0, map.Count)]; map.Remove (idx);	// 获得一个角色 id
			players[idx].behaviourClass = uPomelos.PlayerClasses.Witch;		// 更新职业
			players[idx].antidoteCount = 1;									// 更新解药数量
			players [idx].poisonCount = 1;									// 更新毒药数量
			target.witches.Add (players[idx]);							// 加入列表
			serverManager.SendSystemChat (idTip + "女巫，你的名字是：" + players[idx].playerName, players[idx]);
		}

		// 分配预言家
		target.prophets.Clear ();
		for (int i = 0; i < target.prophetsCount; i++) {
			int idx = map[Random.Range (0, map.Count)]; map.Remove (idx);	// 获得一个角色 id
			players[idx].behaviourClass = uPomelos.PlayerClasses.Prophet;		// 更新职业
			target.prophets.Add (players[idx]);							// 加入列表
			serverManager.SendSystemChat (idTip + "预言家，你的名字是：" + players[idx].playerName, players[idx]);
		}

		// 分配猎人
		target.hunters.Clear ();
		for (int i = 0; i < target.huntersCount; i++) {
			int idx = map[Random.Range (0, map.Count)]; map.Remove (idx);	// 获得一个角色 id
			players[idx].behaviourClass = uPomelos.PlayerClasses.Hunter;		// 更新职业
			target.hunters.Add (players[idx]);							// 加入列表
			serverManager.SendSystemChat (idTip + "猎人，你的名字是：" + players[idx].playerName, players[idx]);
		}

		target.hasSheriff = true;		// 有警长
		foreach (var p in players) p.isSheriff = false;
	}

	private void StartTip () {
		ResetPlayer (target.players);
		// 开局提示
		// 显示游戏人数设置
		string startTip = "对战设置为";
		startTip += target.villagersCount + "名村民，";
		startTip += target.witchesCount + "名女巫，";
		startTip += target.prophetsCount + "名预言家，";
		startTip += target.huntersCount + "名猎人，";
		startTip += target.wolvesCount + "名狼人。";
		//foreach (var p in target.players)
			//
		serverManager.SendSystemChat (startTip, target.players);
	}
}