using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyPlayerList : MonoBehaviour {
	public static LobbyPlayerList instance = null;

	public RectTransform playerListContentTransform;

	protected VerticalLayoutGroup layout;
	protected List<LobbyPlayer> players = new List<LobbyPlayer>();

	public void OnEnable () {
		instance = this;
		layout = playerListContentTransform.GetComponent<VerticalLayoutGroup> ();
	}

	private void Update () {
		// 强制重新计算布局，防止布局不正确
		if (layout) layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
	}

	/// <summary>
	/// Adds the player.
	/// </summary>
	/// <param name="p">P.</param>
	public void AddPlayer (LobbyPlayer p) {
		if (players.Contains (p))
			return;

		players.Add (p);

		p.transform.SetParent(playerListContentTransform, false);

		PlayerListModified ();
	}

	public void RemovePlayer (LobbyPlayer p) {
		players.Remove (p);
		PlayerListModified ();
	}

	public void PlayerListModified () {
		int i = 0;
		foreach (LobbyPlayer p in players) {
			p.OnPlayerListChanged (i);
			++i;
		}
	}
}
	