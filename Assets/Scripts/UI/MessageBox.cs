using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageBox : MonoBehaviour {
	public Text infoText;
	public Text buttonText;
	public Button singleButton;

	/// <summary>
	/// 显示消息
	/// </summary>
	/// <param name="info">显示信息.</param>
	/// <param name="buttonInfo">按钮信息.</param>
	/// <param name="buttonClbk">点击事件.</param>
	public void Display(string info, string buttonInfo, UnityEngine.Events.UnityAction buttonClbk) {
		infoText.text = info;

		buttonText.text = buttonInfo;

		singleButton.onClick.RemoveAllListeners();

		if (buttonClbk != null) {
			singleButton.onClick.AddListener(buttonClbk);
		}

		singleButton.onClick.AddListener(() => { gameObject.SetActive(false); });

		gameObject.SetActive(true);
	}
}