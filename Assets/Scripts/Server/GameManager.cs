using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Server only.
public class GameManager : NetworkBehaviour {
	//======================================状态
	public enum State {
		Start,		// 开始
		Wolf,		// 狼人
		Witch,		// 女巫
		Prophet,	// 预言家
		Hunter,		// 猎人
		Sheriff,	// 警长竞选
		Die,		// 公布死亡
		Vote,		// 处决
	}
	//======================================结束

	public static GameManager instance;

	public List<uPomelos> players = new List<uPomelos> ();		// 玩家列表
	public List<uPomelos> lives = new List<uPomelos> ();		// 存活列表
	public List<uPomelos> villagers = new List<uPomelos> ();	// 平民列表
	public List<uPomelos> witches = new List<uPomelos> ();		// 女巫列表
	public List<uPomelos> prophets = new List<uPomelos> ();	// 预言家列表
	public List<uPomelos> hunters = new List<uPomelos> (); 	// 猎人列表
	public List<uPomelos> wolves = new List<uPomelos> ();		// 狼人列表

	public List<uPomelos> victims = new List<uPomelos> (); 		// 受害者

	public uPomelos sherif = null;

	public bool hasSheriff;				// 是否有警长

	public int playersCount;
	public int villagersCount;
	public int witchesCount;
	public int prophetsCount;
	public int huntersCount;
	public int wolvesCount;

	public float waitTime = 30f;

	private ServerManager m_ServerManager;
	private StateManager<GameManager> m_StateManager;
	private Dictionary<State, uState<GameManager>> dic;

	public ServerManager serverManager { get { return m_ServerManager == null ? FindObjectOfType<ServerManager> () : m_ServerManager; }}

	private void Awake () {
		instance = this;
	}

	//
	public void AddPlayer (uPomelos player) {
		players.Add (player);
		lives.Add (player);
		player.behaviourId = players.Count;
	}

	private void Start () {
		// 在这里注册所有状态
		RegisterStateHandler (State.Start, new StartState ());
		RegisterStateHandler (State.Wolf, new WolfState ());
		RegisterStateHandler (State.Witch, new WitchState ());
		RegisterStateHandler (State.Prophet, new ProphetState ());
		RegisterStateHandler (State.Sheriff, new SheriffState ());
		RegisterStateHandler (State.Die, new DieState ());
		//
		RegisterStateHandler (State.Vote, new VoteState ());
		//RegisterStateHandler (State.Witch,
		m_StateManager = new StateManager<GameManager> (this);
		m_ServerManager = ServerManager.instance;
		StartCoroutine (SetupNewGame ());
	}

	private void Update () {
		m_StateManager.Update ();		// 更新状态机
	}

	/// <summary>
	/// Setups the new game.
	/// </summary>
	/// <returns>The new game.</returns>
	private IEnumerator SetupNewGame () {
		// 为了防止一些玩家连接超时，在这里特判一下
		for (float timer = 0; timer < 30f; timer += Time.deltaTime) {
			if (FindObjectsOfType<uPomelos> ().Length == playersCount)
				break;
			yield return null;
		}
		// 添加所有角色
		uPomelos[] pls = FindObjectsOfType<uPomelos> ();
		foreach (var p in pls) {
			AddPlayer (p);
		}
		// 因为 Client Rpc 需要在所有客户端初始化后才能调用，在这里判断是否客户端已经初始化。
		float checkTime = 1f, lsTime = 0f;	// 为了减少资源占用。
		for (float timer = 0; timer < 30f; timer += Time.deltaTime) {
			bool allready = true;
			if (timer - lsTime >= checkTime) { 
				foreach (var p in players) {
					allready &= p.hasStart;
				}
				if (allready)
					break;
				lsTime = timer;
			}
			yield return null;
		}


		if (players.Count < playersCount) {
			// 有玩家未加入
			m_ServerManager.SendSystemChat ("一些玩家连接失败，他们将加入游戏但不进行操作。", players);
			int rmCount = playersCount - players.Count;
			for (int i = 0; i < rmCount; i++) {
				uPomelos dis = new uPomelos ();
				dis.isRobot = true;
				AddPlayer (dis);
			}
		}
		StartNewGame ();
	}

	// todo here..................
	/// <summary>
	/// 开始游戏
	/// </summary>
	private void StartNewGame () {
		ChangeState (State.Start);
	}


	//===============================================状态机
	/// <summary>
	/// Registers the state handler.
	/// </summary>
	/// <param name="state">State.</param>
	/// <param name="ust">Ust.</param>
	public void RegisterStateHandler (State state, uState<GameManager> ust) {
		if (dic == null)
			dic = new Dictionary<State, uState<GameManager>> ();
		dic.Add (state, ust);
	}
	/// <summary>
	/// Changes the state.
	/// </summary>
	/// <param name="state">State.</param>
	public void ChangeState (State state) {
		if (!dic.ContainsKey (state)) {
			Debug.LogError ("State " + state.ToString () + "is not registered. FSM will not work for it.");
			return;
		}
		m_StateManager.ChangeState (dic [state]);
	}

	public void ChangeToLobbyScence () {
		PandaNetworkManager manager = FindObjectOfType <PandaNetworkManager> ();
		manager._playerNumber = 0;
		manager.ServerReturnToLobby ();
	}
}
