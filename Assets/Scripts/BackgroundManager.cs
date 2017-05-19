using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour {

	//private classes
	[Serializable]
	private class Dot {
		public Color color;
		public GameObject img;
		public int maxAmount;
		public int count;

		public Dot(Color pColor, GameObject pImg, int pMaxAmount) {
			color = pColor;
			img = pImg;
			maxAmount = pMaxAmount;
			count = 0;
		}
	}
	[Serializable]
	private class RectHolder {
		public bool complete = false;
		public Image rec;
		public int count = 0;

		public RectHolder(Image rectangle) {
			rec = rectangle;
		}
	}
	
	//FIELDS
	//constants
	private const int minRows = 1; 
	private const int maxRows = 3;
	private const int minCols = 3;
	private const int maxCols = 5;
	private const String winString = "YOU WIN!";
	private const String loseString = "YOU LOSE 3:";
	private const int firstHorizontalBound = 1136;
	private const int secondHorizontalBound = 960;
	private const int VerticalBound = 720;
	private const float opacityPause = 0.4f;
	private readonly Vector3 dotScaleVector = new Vector3 (0.5f, 0.5f, 1f);

	//public static fields
	public static BackgroundManager instance;

	//public fields
	public GameObject rectImg;
	public GameObject dotImg;
	public Sprite resumeSprite;
	public Sprite pauseSprite;

	//private static fields
	private static int rows = 2;
	private static int columns = 4;

	//private fields
	private RectTransform canvasRect;
	private int horDotPerRect = 3;
	private int vertDotPerRect = 2;
	private Vector3 scaleVector = new Vector3 (1f, 1f, 1f);
	private List<Color> colors = new List<Color>();
	private Vector2 pivotVector =  new Vector2 (0.5f, 0.5f);
	private GameObject finalText;

	//flat colors palette
//	private String[] hexColors = { "#00aba9", "#fba026", "#1abc9c",
//		"#3d8eb9", "#1F5189", "#9365b8",
//		"#553982", "#ffe681", "#fac51c",
//		"#41a85f", "#f37934", "#da4167",
//		"#b8312f", "#75706b", "#d1d5d8"};

	//pastel palette
	private String[] hexColors = {"#ffccda", "#feeae9", "#ffcbb8",
								"#ffe4b7", "#faf1a4", "#ecebe6",
								"#dbe9c6", "#b8d7a8", "#c1e4dd",
								"#c6edf7", "#afdafb", "#8ca4d4",
								"#b6b2d7", "#ccb2d5", "#c2a1da"};

	private List<Dot> dots = new List<Dot>();
	private RectHolder[,] rectangles;
	private Transform dotHolder;
	private int setDots = 0;

	private float xToAvoid;
	private float yToAvoid;
	private int totalDotCount;
	private int swaps = 0;
	private Slider colorSlider;
	private Color[] dotColors;
	private Color[] rectColors;

	
	//FUNCTIONS

	//function called by the GameManager to open the start screen 
	public void openStartScreen() {
		//GameObject.Find ("StartImage").transform.SetAsLastSibling;
		colorSlider = GameObject.Find ("ColorSlider").GetComponent<Slider> ();

		//handles the slider for the number of colours used in the match
		colorSlider.onValueChanged.AddListener (delegate {
			sliderHandler (colorSlider);
		});
	}

	//function called by the GameManager to close the start screen
	public void closeStartScreen() {
		//GameObject.Find ("StartImage").transform.SetAsFirstSibling ();
		int value = sliderHandler (colorSlider);
		GameObject.Find ("StartImage").SetActive(false);
		Debug.Log ("Start image set inactive");

		colorSlider.onValueChanged.RemoveListener (delegate {
			sliderHandler (colorSlider);
		});

		//sets the number of rows and columns of the board, based on
		//the number of colours chosen by the player 
		bool found = false;
		int divisor = 5;
		while (!found && divisor >= 3) {
			if (value % divisor == 0) {
				columns = divisor;
				rows = value / divisor;
				found = true;
			} else {
				divisor--;
			}
		} 
		Debug.Log ("Rows " + rows + " " + "columns: " + columns);
	}

	//function called by the GameManager to pause the match
	public void pauseScreen() {
		//increases the opacity of the pause image
		Image imgComp = GameObject.Find ("PauseImage").GetComponent<Image> ();
		imgComp.color = new Color (imgComp.color.r, imgComp.color.g, imgComp.color.b, opacityPause);

		//sets all rectangles and cats to white so the player cannot plan further action during the pause
		for (int i = 0; i < columns * rows; i++) {
			Image imgCompRect = GameObject.Find("Rect" + (i+1)).GetComponent<Image>();
			imgCompRect.color = Color.white;
		}

		for (int i = 0; i < totalDotCount; i++) {
			Image imgCompDot = GameObject.Find ("Dot" + (i + 1)).GetComponent<Image> ();
			imgCompDot.color = Color.white;
		}

		Button btn = GameObject.Find ("PauseButton").GetComponent<Button> ();
		btn.GetComponent<Image> ().sprite = resumeSprite;
	}

	//function called by the GameManager to resume the match
	public void resumeScreen() {
		//lowers the opacity of the pause image
		Image imgComp = GameObject.Find ("PauseImage").GetComponent<Image> ();
		imgComp.color = new Color (imgComp.color.r, imgComp.color.g, imgComp.color.b, 0f);

		//sets all rectangles and cats to the colour they showed before the pause
		for (int i = 0; i < rows * columns; i++) {
			Image imgRect = GameObject.Find ("Rect" + (i + 1)).GetComponent<Image> ();
			imgRect.color = rectColors [i];
		}

		for (int i = 0; i < totalDotCount; i++) {
			Image imgCompDot = GameObject.Find ("Dot" + (i + 1)).GetComponent<Image> ();
			imgCompDot.color = dotColors [i];
		}

		Button btn = GameObject.Find ("PauseButton").GetComponent<Button> ();
		btn.GetComponent<Image> ().sprite = pauseSprite;
	}

	//function used to perform the setup of the game board before every match
	public void BackgroundSetup () {

		setDots = 0;
		swaps = 0;
		dots = new List<Dot> ();
		rectangles = new RectHolder[rows, columns];
		colors = new List<Color>();
		finalText = GameObject.Find ("GameHolder/FinalText");

		//parses all colours expressed in hexadecimal format to obtain Color objects
		for (int i = 0; i < hexColors.Length; i++) {
			Color temp;
			ColorUtility.TryParseHtmlString (hexColors [i], out temp);
			colors.Add (temp);
		}
			
		canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform> ();

		int canvasWidth = (int) canvasRect.rect.width;
		int canvasHeight = (int) canvasRect.rect.height;

		rectColors = new Color[columns * rows];
		InitRects (canvasWidth, canvasHeight);
//		Color swapsColor = findContrastingColor (rectangles [0, 0].rec.color);
//		GameObject.Find ("Swaps").GetComponent<Text> ().color = swapsColor;
//		Color scoreColor = findContrastingColor (rectangles [0, columns - 1].rec.color);
//		GameObject.Find ("Score").GetComponent<Text> ().color = scoreColor;

		//sets up the texts for the number of swaps performed and the current score
		RectTransform swapTransform = GameObject.Find ("Swaps").GetComponent<RectTransform> ();
		swapTransform.localPosition = new Vector2 (-canvasWidth/2, swapTransform.localPosition.y);
		RectTransform scoreTransform = GameObject.Find ("Score").GetComponent<RectTransform> ();
		scoreTransform.localPosition = new Vector2(canvasWidth/2, scoreTransform.localPosition.y);

		//sets up the GameObject that will contain all cats (i.e. dots)
		dotHolder = GameObject.Find ("Dots").transform;
		dotHolder.DetachChildren ();
		dotHolder.localPosition = new Vector3 ((float) - (canvasWidth/2), (float)(canvasHeight/2), -2f);
		dotHolder.position = new Vector3 (dotHolder.position.x, dotHolder.position.y, -2f);

		//tries to avoid overcrowded rectangles by considering the size of the screen,
		//still to improve
		if (Screen.width <= firstHorizontalBound && columns > minCols) {
			horDotPerRect--;
			if (Screen.width <= secondHorizontalBound && columns == maxCols) {
				horDotPerRect--;
				vertDotPerRect++;
			}
		}
		if (rows == minRows || (rows < maxRows && columns > minCols)) {
			vertDotPerRect++;	
		}
			
		dotColors = new Color[horDotPerRect * columns * vertDotPerRect * rows];

		//makes the HUD smaller when there are too many rows of cats, 
		//which could overlap the text information
		if (rows == maxRows) {
			Transform hudTransform = GameObject.Find ("HUD").transform;
			for (int i = 0; i < hudTransform.childCount; i++) {
				Vector3 childScale = hudTransform.GetChild (i).localScale;
				hudTransform.GetChild (i).localScale = childScale * 4 / 5;
			}
		}

		//xStep and yStep mark the distance between two cats and
		//the steps of the cycles that scan the game board
		float xStep = (canvasRect.rect.width / ((horDotPerRect + 1) * columns));
		xToAvoid = canvasRect.rect.width / columns;
		float yStep = (canvasRect.rect.height / ((vertDotPerRect + 1) * rows));
		yToAvoid = (canvasRect.rect.height / rows);

		int rectCol = 0;
		int rectRow = 0;
		totalDotCount = 0;
		int dotColIndex = 0;

		List<Dot> tempDots = new List<Dot>();
		for (float y = yStep; y < canvasHeight; y = y + yStep) {
			rectCol = 0;
			//if the y coordinate does NOT fall on the horizontal line between two background rectangles
			if (!Mathf.Approximately (y, (yToAvoid * (rectRow + 1)))) {

				for (float x = xStep; x < canvasWidth; x = x + xStep) {
					//if the x coordinate does NOT fall on the vertical line between two background rectangles
					if (!Mathf.Approximately (x, (xToAvoid * (rectCol + 1)))) {

						//this part helps avoiding that the last rectangle contains cats
						//of the same colour at the beginning of the match;
						//to do so, the colour of the last rectangle is 4 times more
						//likely to be selected for cats
						int dotIndex = Random.Range (0, dots.Count+2);
						if (dotIndex > dots.Count - 1) {
							dotIndex = dots.Count - 1;
						}
						
						bool set = false;
						int checkValue = 0;
						while (!set && checkValue < rows*columns) {;
							Dot dot = dots [dotIndex];
							if (dot.color != rectangles [rectRow, rectCol].rec.color || dots.Count == 1) {
								InstantiateImage (dot.img, dotHolder, "Dot"+(totalDotCount+1), new Vector3 ((float)x, (float)-y, 0f),
									dotScaleVector, dot.color);

								set = true;
								dot.count++;
								totalDotCount++;

								dotColors [dotColIndex] = dot.color;
								dotColIndex++;

								//removes a color with no dots left to place in the game
								if (dot.count == horDotPerRect*vertDotPerRect) {
									dots.RemoveAt (dotIndex);
								}
								//refills original dot list
								if (tempDots.Count != 0) {
									foreach(Dot tempDot in tempDots) {
										dots.Add (tempDot);
									}
									tempDots.RemoveAll (returnTrue);
								}
								if (dot.color == rectangles [rectRow, rectCol].rec.color) {
									rectangles [rectRow, rectCol].count++;
									setDots++;
								}
							} else {
								tempDots.Add(dots[dotIndex]);
								dots.RemoveAt (dotIndex);
								dotIndex = Random.Range (0, dots.Count);
								checkValue++;
							}
						}
					//if the x coordinate DOES fall on the horizontal line between two background rectangles
					} else {
						rectCol++;
					}
				}
			//if the y coordinate DOES fall on the horizontal line between two background rectangles
			} else {
				rectRow++;
			}
		}
		GameObject.Find ("Score").GetComponent<Text> ().text = "Score: " + setDots + "/" + totalDotCount + " ";
	}

	public void checkDotPosition(GameObject dot1, GameObject dot2) {
		float x1 = dot1.transform.localPosition.x;
		float y1 = dot1.transform.localPosition.y;
		float x2 = dot2.transform.localPosition.x;
		float y2 = dot2.transform.localPosition.y;

		int row1 = (int)(-y1 / yToAvoid);
		int col1 = (int)(x1 / xToAvoid);
		int row2 = (int)(-y2 / yToAvoid);
		int col2 = (int)(x2 / xToAvoid);

		RectHolder holder1 = rectangles [row1, col1];
		RectHolder holder2 = rectangles [row2, col2];

		//if the cats are not swapped inside the same rectangle
		if (holder1.rec.color != holder2.rec.color) {
			if (holder1.rec.color == dot1.GetComponent<Image> ().color) {
				holder1.count++;
				setDots++;
			} else if (holder2.rec.color == dot1.GetComponent<Image> ().color) {
				holder2.count--;
				setDots--;
			}

			if (holder2.rec.color == dot2.GetComponent<Image> ().color) {
				holder2.count++;
				setDots++;
			} else if (holder1.rec.color == dot2.GetComponent<Image> ().color) {
				holder1.count--;
				setDots--;
			}
		}

		swaps++;

		GameObject.Find ("Score").GetComponent<Text> ().text = "Score: " + setDots + "/" + totalDotCount + " ";
		GameObject.Find ("Swaps").GetComponent<Text> ().text = " Swaps: " + swaps;

		if (setDots == totalDotCount) {
			finalText.GetComponent<Text> ().text = winString;
			handleFinalText ();
			GameManager.instance.endMatch (GameManager.WIN);
		}

		if (holder1.count == horDotPerRect * vertDotPerRect && !holder1.complete) {
			Debug.Log ("Holder1 reached max!");
			//the rectangle expands for 1 second when completed 
			StartCoroutine(ExpandRect(holder1.rec, row1, col1));
			holder1.complete = true;
			GameManager.instance.addBonusTime ();
		}
		if (holder2.count == horDotPerRect * vertDotPerRect && !holder2.complete) {
			Debug.Log ("Holder2 reached max!");
			//the rectangle expands for 1 second when completed
			StartCoroutine(ExpandRect(holder2.rec, row2, col2));
			holder2.complete = true;
			GameManager.instance.addBonusTime ();
		}
	}

	//function used to randomly generate a colour for the screen shown before the match
	public Color getLevelImageColor() {
		Color temp;
		int rdm = Random.Range (0, hexColors.Length);
		ColorUtility.TryParseHtmlString (hexColors [rdm], out temp);
		return temp;
	}
		
	public void handleLoseText() {
		finalText.GetComponent<Text> ().text = loseString;
		handleFinalText ();
	}

	
	//NON-PUBLIC FUNCTIONS
	
	private int sliderHandler(Slider slider) {
		GameObject text = GameObject.Find(slider.name + "/Handle Slide Area/Handle/Text");
		int realValue = (int) slider.value;
		//the values on the slider are not the actual possible values for the
		//colours, this part fixes this difference
		if (realValue == 10) {
			realValue += 2;
		} else if (realValue > 6) {
			realValue++;
		}

		text.GetComponent<Text> ().text = realValue.ToString();
		return realValue;
	}

	private void InitRects(int canvasWidth, int canvasHeight) {
		Transform rectTrans = GameObject.Find ("Rectangles").transform;
		rectTrans.localPosition = new Vector3 ((float)-(canvasWidth / 2), (float)(canvasHeight / 2), 0f);
		int count = 0;
		for(int y = 0; y < rows; y++) {
			for(int x = 0; x < columns; x++) {
				int rdm = Random.Range (0, colors.Count);
				GameObject rect = InstantiateImage (rectImg, rectTrans, "Rect"+(count+1), new Vector3 (0f, 0f, 0f), scaleVector, colors [rdm]);
				rectColors [count] = colors [rdm];
				dots.Add(new Dot (colors [rdm], dotImg, horDotPerRect*vertDotPerRect));
				colors.RemoveAt (rdm);
				rectangles [y, x] = new RectHolder (rect.GetComponent<Image> ());
				count++;

				HandleRectPosition (rect.GetComponent<RectTransform> (), canvasWidth, canvasHeight, x, y);
			}
		}
	}

	private void handleFinalText() {
		finalText.transform.SetAsLastSibling ();
		Invoke ("hideFinalText", 1.5f);
	}

	private void hideFinalText () {
		finalText.transform.SetAsFirstSibling ();
	}	

//	void OnGUI() {
//		//usa matrice
//		imgTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, (float) (canvasRect.rect.width/2));
//		imgTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (float) (canvasRect.rect.height/2));
//	}

	//function used to expand a rectagle that has been completed by the player
	IEnumerator ExpandRect(Image rect, int row, int col) {
		int number = (row * columns) + (col + 1);
		if (GameObject.Find ("Rect" + number) != null) {
			RectTransform tr = GameObject.Find("Rect" + number).GetComponent<RectTransform> ();
			tr.SetAsLastSibling ();
			tr.localScale = new Vector3 (1.05f, 1.15f, 1f);
			yield return new WaitForSeconds (0.8f);
			tr.localScale = scaleVector;
		}
	} 
	
	
	
	//currently not used
	private Color findContrastingColor(Color color) {
		float h = 75;
		float s;
		float v;
		Color.RGBToHSV (color, out h, out s, out v);

		//if either the color hue is itself dark (blue, purple and so forth)
		//or the brightness is low, increases the contrasting color's brightness
		if (h >= 0.55f || v <= 0.5f) {
			v = 0.85f;
		}

		//finds the hue opposite to the starting one on the color wheel
		h = (h + 0.5f) % 1f;
		//the saturation is set to the max value between itself and a set value
		s = Mathf.Max (s, 0.75f);

		Color contrasting = Color.HSVToRGB (h, s, v);
		return contrasting;
	}
}
