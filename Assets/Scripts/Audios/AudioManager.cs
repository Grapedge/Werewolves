using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public AudioClip[] clips;
	private AudioSource audioSource;
	bool isPausing;

	//private int curPlay;

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource> ();
		if (clips.Length == 0)
			return;
		audioSource.clip = clips [Random.Range (0, clips.Length)];
		audioSource.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F6) || !audioSource.isPlaying && isPausing == false) {
			audioSource.clip = clips [Random.Range (0, clips.Length)];
			audioSource.Play ();
		}

		if (Input.GetKeyDown (KeyCode.F5)) {
			if (isPausing == false) {
				isPausing = true;
				audioSource.Pause ();
			} else {
				isPausing = false;
				audioSource.Play ();
			}
		}
	}
}
