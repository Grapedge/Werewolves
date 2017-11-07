using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uPomelosCanvasManager : MonoBehaviour {

	[Header ("Chat Refence")]
	[SerializeField] private RectTransform chatContent;
	[SerializeField] private GameObject chatPrefab;
	[SerializeField] private ScrollRect scrollRect;

	[Header ("Input Refence")]
	[SerializeField] private InputField chatInput;
	[SerializeField] private Button sendButton;

	[Header ("Runtime Refence")]
	[SerializeField] private Button endChatButton;

	public uPomelos player = null;		// 玩家控制

	/// <summary>
	/// 在本地画布上添加一条信息，使用它的属性
	/// </summary>
	/// <param name="chat">Chat.</param>
	public void AddChat (Chat chat) {
		string name = chat.name;
		string msg = chat.message;
		if (chat.senderId != -1) msg = name + "：" + msg;

		// attach the componment.
		GameObject clone = Instantiate (chatPrefab, chatContent) as GameObject;
		Text text = clone.GetComponentInChildren<Text> ();

		// 如果信息具有一个皮肤，更新此预制组件
		if (chat.skin != null) {
			clone.GetComponentInChildren<Image> ().color = chat.skin.backColor;
			text.color = chat.skin.textColor;
		}

		text.text = msg;		
		StartCoroutine (UpdateScrollRect ());
	}

	/// <summary>
	/// 延时更新画布信息
	/// </summary>
	/// <returns>The scroll rect.</returns>
	private IEnumerator UpdateScrollRect () {
		yield return new WaitForSeconds (0.1f);
		scrollRect.verticalNormalizedPosition = 0f;
	}

	/// <summary>
	/// 修改信息输入系统的启用态
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
	public void ToggleChatInput (bool active) {
		chatInput.interactable = active;
		sendButton.interactable = active;
		endChatButton.interactable = active;
	}

	/// <summary>
	/// 修改信息输入框的输入类型
	/// 注意，这将会清空输入框的内容
	/// </summary>
	/// <param name="type">Type.</param>
	public void ModifyInputType (InputField.ContentType type) {
		chatInput.text = "";
		chatInput.contentType = type;
		chatInput.lineType = InputField.LineType.MultiLineSubmit;
	}

	/// <summary>
	/// 发送按钮点击时
	/// </summary>
	public void OnSendClick () {
		if (player != null && chatInput.text.Length > 0) {
			player.CmdSendChat (chatInput.text);
			chatInput.text = "";
			chatInput.ActivateInputField ();
		}
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update () {
		// 判断如果按下 return 键或 enter 键发送消息
		if (sendButton.interactable && Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter)) {
			OnSendClick ();
		}
	}

	/// <summary>
	/// 结束发言状态设置
	/// </summary>
	public void OnEndSpeechClick () {
		player.CmdSpeechState (true);
	}

}
