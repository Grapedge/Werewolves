using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossKeyInf : MonoBehaviour {

	public GameObject notepad;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
		notepad.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F9)) {
			notepad.SetActive (!notepad.activeSelf);
		}
	}
}
