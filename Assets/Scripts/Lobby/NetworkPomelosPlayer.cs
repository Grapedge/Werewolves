#if ENABLE_UNET
/*
using System;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkPomelosPlayer")]
public class NetworkPomelosPlayer : NetworkBehaviour {

	byte m_Slot;
	bool m_ReadyToBegin;

	public byte slot { get { return m_Slot; } set { m_Slot = value; }}
	public bool readyToBegin { get { return m_ReadyToBegin; } set { m_ReadyToBegin = value; } }

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
		
	public override void OnStartClient() {
		var lobby = NetworkManager.singleton as NetworkPomelosManager;
		if (lobby) {
			lobby.lobbySlots[m_Slot] = this;
			m_ReadyToBegin = false;
			OnClientEnterLobby();
		} else {
			Debug.LogError("Pomelos Player 需要 Pomelos Manager 来调用函数，确保它。");
		}
	}

	public void SendReadyToBeginMessage() {
		if (LogFilter.logDebug) {
			Debug.Log ("NetworkLobbyPlayer SendReadyToBeginMessage");
		}

		var lobby = NetworkManager.singleton as NetworkPomelosManager;
		if (lobby) {
			
			var msg = new LobbyReadyToBeginMessage ();
			msg.slotId = (byte)playerControllerId;
			msg.readyState = true;
			Debug.Log ("Send");
			lobby.client.Send (MsgType.LobbyReadyToBegin, msg);
		}
	}

	public void SendNotReadyToBeginMessage() {
		if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendReadyToBeginMessage"); }
		var lobby = NetworkManager.singleton as NetworkPomelosManager;
		if (lobby) {
			var msg = new LobbyReadyToBeginMessage ();
			msg.slotId = (byte)playerControllerId;
			msg.readyState = false;
			lobby.client.Send (MsgType.LobbyReadyToBegin, msg);
		}
	}

	public void SendSceneLoadedMessage() {
		if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendSceneLoadedMessage"); }

		var lobby = NetworkManager.singleton as NetworkPomelosManager;
		if (lobby) {
			var msg = new IntegerMessage (playerControllerId);
			lobby.client.Send (MsgType.LobbySceneLoaded, msg);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		var lobby = NetworkManager.singleton as NetworkPomelosManager;
		if (lobby) {
			// dont even try this in the startup scene
			// Should we check if the LoadSceneMode is Single or Additive??
			// Can the lobby scene be loaded Additively??
			string loadedSceneName = scene.name;
			if (loadedSceneName == lobby.lobbyScene) {
				return;
			}
		}

		if (isLocalPlayer) {
			SendSceneLoadedMessage ();
		}
	}

	public void RemovePlayer() {
		if (isLocalPlayer && !m_ReadyToBegin) {
			if (LogFilter.logDebug) {
				Debug.Log ("NetworkLobbyPlayer RemovePlayer");
			}

			ClientScene.RemovePlayer (GetComponent<NetworkIdentity> ().playerControllerId);
		}
	}

	// ------------------------ callbacks ------------------------

	public virtual void OnClientEnterLobby()
	{
	}

	public virtual void OnClientExitLobby()
	{
	}

	public virtual void OnClientReady(bool readyState)
	{
	}

	// ------------------------ Custom Serialization ------------------------

	public override bool OnSerialize(NetworkWriter writer, bool initialState) {
		// dirty flag
		writer.WritePackedUInt32(1);
	
		writer.Write(m_Slot);
		writer.Write(m_ReadyToBegin);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState) {
		var dirty = reader.ReadPackedUInt32();
		if (dirty == 0)
			return;
	
		m_Slot = reader.ReadByte();
		m_ReadyToBegin = reader.ReadBoolean();
	}

}
*/

#endif // ENABLE_UNET