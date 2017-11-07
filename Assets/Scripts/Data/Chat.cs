using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chat.
/// 消息由玩家名称 + 玩家 id + 信息构成
/// skin 定义了消息以何种方式进行显示
/// 目前显示的方式包含了文本颜色和背景颜色
/// </summary>
public class Chat {

	public int senderId;
	public string name;
	public string message;
	public ChatSkin skin;

	public Chat () { }
	/// <summary>
	/// Initializes a new instance of the <see cref="Chat"/> class.
	/// </summary>
	/// <param name="behaviourId">Behaviour identifier. 发送者的身份标识</param>
	/// <param name="name">Name. 发送者的名字 </param>
	/// <param name="message">Message. 发送的信息 </param>
	/// <param name="skin">Skin. 使用的皮肤 </param>
	public Chat (int behaviourId, string name, string message, ChatSkin skin = null) {
		this.senderId = behaviourId;
		this.name = name;
		this.message = message;
		this.skin = skin;
	}
}

/// <summary>
/// Chat skin.
/// 皮肤系统，将会以同一皮肤展示同一职业玩家的信息
/// </summary>
public class ChatSkin {
	/// <summary>
	/// The color of the text.
	/// </summary>
	public Color textColor;
	/// <summary>
	/// The color of the background.
	/// </summary>
	public Color backColor;

	public ChatSkin () {
		this.textColor = Color.white;
		this.backColor = Color.black;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ChatSkin"/> class.
	/// </summary>
	/// <param name="text">文本颜色.</param>
	/// <param name="back">背景颜色.</param>
	public ChatSkin (Color text, Color back) {
		this.textColor = text;
		this.backColor = back;
	}

	/// <summary>
	/// 默认白色的配色方案
	/// </summary>
	public readonly static ChatSkin Default = new ChatSkin (new Color (50f / 255f, 50f / 255f, 50f / 255f), Color.white);
	/// <summary>
	/// 红色系的配色方案
	/// </summary>
	public readonly static ChatSkin Red = new ChatSkin (new Color (90f/255f, 10f/255f, 10f/255f), new Color (1f,186f/255f,186f/255f));
	/// <summary>
	/// 蓝色系使用的配色方案
	/// </summary>
	public readonly static ChatSkin Blue = new ChatSkin (new Color (8f/255f,30f/255f,128f/255f), new Color (210f/255f,230f/255f,230f/255f));
	/// <summary>
	/// 黄色系使用的配色方案
	/// </summary>
	public readonly static ChatSkin Yello = new ChatSkin (new Color (250f/255f,50f/255f,0f), new Color (1f,250f/255f,200f/255f));
	/// <summary>
	/// 绿色系的配色方案
	/// </summary>
	public readonly static ChatSkin Green = new ChatSkin (new Color (20f/255f,130f/255f,20f/255f), new Color (240f/255f,1f,200f/255f));
	/// <summary>
	/// 默认皮肤列表，用于分配颜色
	/// </summary>
	public readonly static ChatSkin[] DefaultSkins = new ChatSkin[] {
		Default, Red, Blue, Yello, Green
	};

}