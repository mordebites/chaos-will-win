using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float firstLevelStartDelay = 5f;
	public static GameManager instance = null;
	public BackgroundManager bgManager;
	public const int WIN = 0;
	public const int LOSE = 1;

	private Text levelText;
	private GameObject levelImage;
	private static int level = 0;
	private Timer timer = null;
	private bool paused = false;

	public void endMatch(int outcome) {
		if (outcome == LOSE) {
			bgManager.handleLoseText ();		
		}
		Invoke("Restart", 1.5f);
		timer.resetTimer ();
		DragAndDropHandler.instance.deactivate ();
	}

	public void pauseMatch() {
		paused = true;
		timer.stopTimer ();
		DragAndDropHandler.instance.deactivate ();
		bgManager.pauseScreen ();
	}

	public void resumeMatch() {
		paused = false;
		timer.startTimer ();
		DragAndDropHandler.instance.activate ();
		bgManager.resumeScreen ();
	}

	public void addBonusTime() {
		timer.addSeconds (Timer.bonusSeconds);
	}

	//currently not used
	public void GameOver() {
		levelText.text = "Oh no you lost! Meow :c";
		levelImage.SetActive (true);
		enabled = false;
	}
	
	//NON-PUBLIC FUNCTIONS
	void Awake () {
		if (instance == null) {
			instance = this;
		} else if(instance != this) {
			Destroy (gameObject);
		}

		bgManager = GetComponent<BackgroundManager> ();
		Debug.Log ("Awake! " + level);

		if (level >= 1) {
			GameObject.Find ("StartImage").SetActive (false);
		}
		//DontDestroyOnLoad (gameObject);
	}

	private void startButtonClick() {
		Debug.Log ("StartButtonClick!");
		closeStart ();
	}

	private void handleStart() {
		Debug.Log ("HandleStart!");
		bgManager.openStartScreen ();
		Button start = GameObject.Find ("StartButton").GetComponent<Button>();
		start.onClick.AddListener (startButtonClick);
	}

	private void closeStart() {
		Debug.Log ("CloseStart!");
		Button start = GameObject.Find ("StartButton").GetComponent<Button> ();
		start.onClick.RemoveListener (startButtonClick);
		bgManager.closeStartScreen ();
		InitGame ();
	}

	void InitGame() {
		Debug.Log ("InitGame!");
		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();
		setLevelImageColor ();
		levelText.text = "Level: " + level;
		Debug.Log (levelText.text);
		levelImage.SetActive (true);

		//sets up the board
		bgManager.BackgroundSetup ();
		DragAndDropHandler.instance.Reset ();

		timer = gameObject.AddComponent <Timer> () as Timer;
		if (level == 1) {
			Invoke ("HideLevelImage", firstLevelStartDelay);
		} else {
			GameObject.Find ("GuideText").SetActive (false);
			Invoke ("HideLevelImage", levelStartDelay);
		}
	}
	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
		level++;
		Debug.Log ("onlevelfinishedloading! " + level);
		if (level == 1) {
			handleStart ();
		} else {
			InitGame ();
		}
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable() {
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}
	
	private void Restart() {
		SceneManager.LoadScene (0);
	}

	private void setLevelImageColor() {
		Image img = levelImage.GetComponent<Image> ();
		img.color = bgManager.getLevelImageColor();
	}

	private void HideLevelImage() {
		levelImage.SetActive (false);
		Debug.Log ("Hide level image!");
		timer.startTimer ();

		Button pauseBtn = GameObject.Find ("PauseButton").GetComponent<Button>();
		pauseBtn.onClick.AddListener (delegate {
			if(paused) {
				resumeMatch();
			} else {
				pauseMatch();
			}
		});
	}
}
