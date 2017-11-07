using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIReference {

	/// <summary>
	/// The instance.
	/// </summary>
	public static LobbyUIReference instance;

	[Header("主菜单起始界面")]
	public RectTransform mainMenuPanel;
	[Header("大厅等待界面")]
	public RectTransform lobbyPanel;
	// 当前正在显示的界面
	private RectTransform currentPanel;

	private void Awake () {
		instance = this;
	}
}
