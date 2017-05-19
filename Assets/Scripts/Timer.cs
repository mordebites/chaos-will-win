using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
	public const int bonusSeconds = 10;

	private const int maxMinutes = 1;
	private const int secsInMin = 60;

	private float timer = secsInMin - 1;
	private int seconds = 0;
	private int minutes = maxMinutes;
	private bool active = false;
	private String prevZeroSec = "";
	
	// Update is called once per frame
//	void Update () {
//		if (active) {
//			if (minutes > 0 || seconds > 0) {
//				timer -= Time.deltaTime;
//				if (timer > 0) {
//					minutes++;
//					timer -= secsInMin;
//				}
//				seconds = (int)((timer % secsInMin) + secsInMin);
//				if (seconds == 59) {
//					minutes--;
//				}
//			} else {
//				GameManager.instance.endMatch (GameManager.LOSE);
//			}
//
//			prevZeroSec = seconds >= 10 ? "" : "0";
//			GameObject.Find ("Timer").GetComponent<Text> ().text = "0" + minutes + ":" + prevZeroSec + seconds;
//			Debug.Log ("Timer: " + timer + " Minutes: " + minutes + " Seconds: " + seconds);
//		}
//	}

	//seems to work
	void Update() {
		if (active) {
			if (minutes > 0 || seconds > 0) {
				timer += Time.deltaTime;

				seconds = (int)(secsInMin - (timer % secsInMin));
				minutes = maxMinutes - ((int)timer / secsInMin);
			} else {
				GameManager.instance.endMatch (GameManager.LOSE);
			}

			prevZeroSec = seconds >= 10 ? "" : "0";
			GameObject.Find ("Timer").GetComponent<Text> ().text = "0" + minutes + ":" + prevZeroSec + seconds;
		}
	}

	public void startTimer() {
		active = true;
	}

	public void stopTimer() {
		active = false;
	}

	public void resetTimer() {
		active = false;

		//resets starting values
		timer = 0;
		seconds = 0;
		minutes = maxMinutes;
	}

	public void addSeconds(int secs) {
		if (active) {
			timer -= secs;
		}
	}
}
