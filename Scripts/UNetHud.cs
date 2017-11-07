using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]   
public class UNetHud : MonoBehaviour {
	public NetworkManager m_Manager;

	private void Awake () {
		m_Manager = GetComponent<NetworkManager> ();
	}

	// 只作为主机
	public void StartServer () {
		m_Manager.StartServer ();
	}

	// 同时作为主机和客户端
	public void StartHost () {
		m_Manager.StartHost ();
	}

	// 作为客户端
	public void StartClient () {
		m_Manager.StartClient ();
	}

	// 停止连接
	public void StopHost () {
		m_Manager.StopHost ();
	}

	private void SetNetworkAddress (string address) {
		m_Manager.networkAddress = address;
	}
}

