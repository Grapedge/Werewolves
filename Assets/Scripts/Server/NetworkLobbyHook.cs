using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook {
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
		uPomelos player = gamePlayer.GetComponent<uPomelos>();

        // 在此处进行初始化
		player.behaviourName = lobby.playerName;		// 更新角色命名
		//player.behaviourId = m_Manager.AddPlayer (player);		// 添加至服务器列表
		// ServerManager.instance.SendSystemChat ("正在等待其他玩家...", player);
    }
}
