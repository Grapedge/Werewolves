/// <summary>
/// 定义发送的消息内容
/// </summary>
public class MessageData {

	/// <summary>
	/// 发送的消息内容
	/// </summary>
	public string message;
	/// <summary>
	/// 发送者 ID
	/// </summary>
	public int senderID;

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageData"/> class.
	/// </summary>
	/// <param name="msg">Message.</param>
	/// <param name="id">Sender Identifier, -1 means system.</param>
	public MessageData (string msg, int id = -1) {
		message = msg;
		senderID = id;
	}
}
