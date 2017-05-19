﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MemoryMatchGameManager : MonoBehaviour {
	private static MemoryMatchGameManager instance;
	private bool gameStarted;
	private bool gameStartup;
	private int score;
	private GameObject currentFoodToMatch;
	private int numDishes;
	private int difficultyLevel;
	private List<GameObject> activeFoods;
	private List<Food> matchedFoods;
	private bool stickerCanvasIsUp;
	private bool runningTutorial = false;
	private bool rotate = false;
	private float stopRotateTime;
	public Canvas reviewCanvas;

	public Canvas instructionPopup;
	public Transform foodToMatchSpawnPos;
	public Transform[] foodSpawnPos;
	public Transform[] foodParentPos;
	public float foodScale;
	public float foodToMatchScale;
	public List<GameObject> foods;
	public GameObject dish;
	public Transform dishParent;
	public List<GameObject> dishes;
	public Timer timer;
	public float timeLimit;
	public Text timerText;
	public Slider scoreGauge;
	public Canvas gameOverCanvas;
	public Canvas stickerPopupCanvas;
	public AudioClip correctSound;
	public GameObject subtitlePanel;
	public bool animIsPlaying = false;
	public AudioClip munchClip;
	public Transform target;
	public float speed;
	public AudioClip[] wrongMatchClips;

	public GameManager.MonsterType typeOfMonster;
	public MMMonster monsterObject;
	public GameObject[] monsterList;

	public GameObject[] tutorialDishes;
	public AudioClip instructions;
	public AudioClip letsPlay;
	public AudioClip nowYouTry;
	public bool hasSpawned;
	
	// Use this for initialization
	void Awake () {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);
		}

		difficultyLevel = GameManager.GetInstance ().GetLevel ("MemoryMatch");
		typeOfMonster = GameManager.GetMonsterType ();
		CreateMonster ();
		monsterObject.PlaySpawn ();
	}

	void Start () {
		if(GameManager.GetInstance().LagoonReview) {
			StartReview();
		}
		else {
			PregameSetup ();
		}
	}

	public void PregameSetup ()
	{
		if (GameManager.GetInstance ().LagoonTutorial [(int)Constants.BrainstormLagoonLevels.MEMORY_MATCH]) {
			StartCoroutine (RunTutorial ());
		}
		else {
			score = 0;

			switch (difficultyLevel) {
			case (1):
				numDishes = 3;
				break;
			case (2):
				numDishes = 4;
				break;
			case (3):
				numDishes = 6;
				break;
			default:
				numDishes = 6;
				break;
			}

			scoreGauge.maxValue = numDishes;

			if (timer != null) {
				timer = Instantiate (timer);
				timer.SetTimeLimit (timeLimit);
			}

			UpdateScoreGauge ();
			activeFoods = new List<GameObject> ();
			matchedFoods = new List<Food> ();
			StartGame ();
		}
	}

	public void StartReview() {
		Debug.Log ("starting memory match review");
//		reviewCanvas = GameManager.GetInstance().ChooseLagoonReviewGame();
		reviewCanvas.gameObject.SetActive(true);
	}

	// Update is called once per frame
	void Update () {
		if(runningTutorial) {
			if(score == 1) {
				StartCoroutine(TutorialTearDown());
			}
		}

		if(gameStarted){
			if(score >= numDishes || timer.TimeRemaining () <= 0) {
				if(!animIsPlaying) {
					StartCoroutine(RunEndGameAnimation());
				// GameOver();
				}
			}
		}
	}

	void FixedUpdate() {
		if(gameStarted) {
			timerText.text = "Time: " + timer.TimeRemaining();
		}

		if(hasSpawned && rotate && difficultyLevel > 1) {
			RotateDishes();
			if(Time.time > stopRotateTime) {
				rotate = false;
			}
		}
	}

	public static MemoryMatchGameManager GetInstance() {
		return instance;
	}

	IEnumerator RunTutorial () {
		print ("RunTutorial");
		runningTutorial = true;
		instructionPopup.gameObject.SetActive(true);
		currentFoodToMatch = tutorialDishes [0].GetComponent<DishBehavior> ().bottom.transform.FindChild ("Banana").gameObject;

		Animation anim = tutorialDishes[0].GetComponent<DishBehavior>().top.gameObject.GetComponent<Animation> ();
		yield return new WaitForSeconds(2f);

		// Dish lift.
		tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealLift"].name);
		tutorialDishes[1].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[1].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealLift"].name);
		tutorialDishes[2].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[2].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealLift"].name);
		yield return new WaitForSeconds(4f);

		// Dish close.
		tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealClose"].name);
		tutorialDishes[1].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[1].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealClose"].name);
		tutorialDishes[2].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[2].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealClose"].name);
		yield return new WaitForSeconds(2f);
		instructionPopup.gameObject.transform.FindChild ("panelbanana").gameObject.SetActive(true);

		tutorialDishes [0].GetComponent<DishBehavior> ().SetFood (tutorialDishes [0].GetComponent<DishBehavior> ().bottom.transform.FindChild ("Banana").GetComponent<Food>());
		tutorialDishes [1].GetComponent<DishBehavior> ().SetFood (tutorialDishes [1].GetComponent<DishBehavior> ().bottom.transform.FindChild ("Berry").GetComponent<Food>());
		tutorialDishes [2].GetComponent<DishBehavior> ().SetFood (tutorialDishes [2].GetComponent<DishBehavior> ().bottom.transform.FindChild ("Brocolli").GetComponent<Food>());

		yield return new WaitForSeconds(5f);
		Animator handAnim = instructionPopup.gameObject.transform.FindChild ("TutorialAnimation").gameObject.transform.FindChild ("Hand").gameObject.GetComponent<Animator>();
		handAnim.Play("mmhand_5_12");
		yield return new WaitForSeconds(4f);
		tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (tutorialDishes[0].GetComponent<DishBehavior>().top.GetComponent<Animation>()["DishTopRevealLift"].name);

		if (runningTutorial) {
			yield return new WaitForSeconds (instructions.length - 12f);
			subtitlePanel.GetComponent<SubtitlePanel> ().Display ("Now you try!", nowYouTry);
			tutorialDishes [0].GetComponent<DishBehavior> ().top.GetComponent<Animation> ().Play (tutorialDishes [0].GetComponent<DishBehavior> ().top.GetComponent<Animation> () ["DishTopRevealClose"].name);
			instructionPopup.gameObject.transform.FindChild ("TutorialAnimation").gameObject.transform.FindChild ("Hand").gameObject.SetActive (false);

			for (int i = 0; i < tutorialDishes.Length; ++i) {
				tutorialDishes [i].GetComponent<Collider2D> ().enabled = true;
			}
		}
//		StartGame ();
	}

	public void SkipReviewButton(GameObject button) {
		SkipReview ();
		Destroy (button);
	}

	public void SkipReview() {
		StopCoroutine (RunTutorial ());
		StartCoroutine (TutorialTearDown ());
	}

	IEnumerator TutorialTearDown() {
		print ("TutorialTearDown");
		score = 0;
		runningTutorial = false;
		subtitlePanel.GetComponent<SubtitlePanel>().Display("Perfect!", letsPlay);
		yield return new WaitForSeconds(2.0f);
		subtitlePanel.GetComponent<SubtitlePanel> ().Hide ();
		instructionPopup.gameObject.SetActive (false);
		GameManager.GetInstance ().LagoonTutorial [(int)Constants.BrainstormLagoonLevels.MEMORY_MATCH] = false;
		PregameSetup ();
	}

	public void StartGame() {
		scoreGauge.gameObject.SetActive (true);
		timerText.gameObject.SetActive (true);
		timerText.text = "Time: " + timer.TimeRemaining();
		UpdateScoreGauge ();

		gameStartup = true;

		SpawnDishes();
		hasSpawned = true;
		SelectFoods();

		List<GameObject> copy = new List<GameObject>(activeFoods);

		ChooseFoodToMatch();

		for(int i = 0; i < numDishes; ++i) {
//			GameObject newFood = SpawnFood(copy, true, foodSpawnPos[i], foodParentPos[i], foodScale);
			GameObject newFood = SpawnFood(copy, true, dishes[i].GetComponent<DishBehavior>().top.transform, dishes[i].GetComponent<DishBehavior>().bottom.transform, foodScale);
			dishes[i].GetComponent<DishBehavior>().SetFood(newFood.GetComponent<Food>());
		}

		StartCoroutine(RevealDishes());
	}

	void SpawnDishes() {
		//Mathf.Cos/Sin use radians, so the dishes are positioned 2pi/numDishes apart
		float dishPositionAngleDelta = (2*Mathf.PI)/numDishes;

		for(int i = 0; i < numDishes; ++i) {
			GameObject newDish = Instantiate(dish);
			newDish.transform.SetParent(dishParent);
			newDish.transform.localScale = new Vector3(1, 1, 1);
			newDish.transform.localPosition = new Vector3(200f*Mathf.Cos (dishPositionAngleDelta*i + Mathf.PI / 2), 200f*Mathf.Sin (dishPositionAngleDelta*i + Mathf.PI / 2) + 40, 0);
			dishes[i] = newDish;
		}
	}

	IEnumerator RevealDishes() {
		rotate = true;
		float rotateTimeDelta = Random.Range (3,6);
		stopRotateTime = Time.time + rotateTimeDelta;

		for(int i = 0; i < numDishes; ++i) {
			GameObject d = dishes[i];
			Animation animation = d.GetComponent<DishBehavior>().top.GetComponent<Animation>();
			d.GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (animation["DishTopRevealLift"].name);
		}
		yield return new WaitForSeconds(rotateTimeDelta);

		for(int i = 0; i < numDishes; ++i) {
			GameObject d = dishes[i];
			Animation animation = d.GetComponent<DishBehavior>().top.GetComponent<Animation>();
			d.GetComponent<DishBehavior>().top.GetComponent<Animation>().Play (animation["DishTopRevealClose"].name);
		} 

		gameStartup = false;

		if(!runningTutorial) {
			GameManager.GetInstance ().Countdown ();

			yield return new WaitForSeconds (4.0f);
			gameStarted = true;

			timer.StartTimer();
		}
	}

	void ResetDishes() {
		for(int i = 0; i < difficultyLevel*3; ++i) {
			GameObject dish = dishes[i];
			dish.GetComponent<DishBehavior>().Reset ();
		}
	}

	void SelectFoods() {
		int foodCount = 0;
		while(foodCount < numDishes){
			int randomIndex = Random.Range(0, foods.Count);
			GameObject newFood = foods[randomIndex];
			if(!activeFoods.Contains(newFood)){
				activeFoods.Add(newFood);
				++foodCount;
			}

		}
	}

	public void ChooseFoodToMatch() {
		if(!gameStartup) {
			++score;
			UpdateScoreGauge();
		}

		if(GameObject.Find ("ToMatchSpawnPos").transform.childCount > 0)
			Destroy(currentFoodToMatch);

		if(activeFoods.Count > 0) {
			currentFoodToMatch = SpawnFood(activeFoods, false, foodToMatchSpawnPos, foodToMatchSpawnPos, foodToMatchScale);
			currentFoodToMatch.GetComponent<SpriteRenderer> ().sortingLayerName = "UI";
			currentFoodToMatch.GetComponent<SpriteRenderer> ().sortingOrder = 5;
		}
	}

	GameObject SpawnFood(List<GameObject> foodsList, bool setAnchor, Transform spawnPos, Transform parent, float scale) {
		int randomIndex = Random.Range (0, foodsList.Count);
		GameObject newFood = Instantiate(foodsList[randomIndex]);
		newFood.name = foodsList[randomIndex].name;
		newFood.GetComponent<Food>().Spawn(spawnPos, parent, scale);
		if(setAnchor) {
			newFood.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
			newFood.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

		}
		foodsList.RemoveAt(randomIndex);
		return newFood;
	}

	public Food GetFoodToMatch() {
		return currentFoodToMatch.GetComponent<Food>();
	}

	public void AddToMatchedList(Food food) {
		matchedFoods.Add (food);
	}

	IEnumerator RunEndGameAnimation(){
		animIsPlaying = true;
		timer.StopTimer();

		for(int i = 0; i < numDishes; ++i) {
			GameObject d = dishes[i];
			Animation animation = d.GetComponent<DishBehavior>().bottom.GetComponent<Animation>();
			if(d.GetComponent<DishBehavior>().IsMatched ()) {
				Destroy(d.GetComponent<DishBehavior>().bottom.GetComponentInChildren<Food>().gameObject);
				SoundManager.GetInstance().PlaySFXClip(munchClip);
				d.GetComponent<DishBehavior>().bottom.GetComponent<Animation>().Play (animation["DishBottomShake"].name);
				monsterObject.PlayEat ();
				yield return new WaitForSeconds (1.2f);
			}
		}
		monsterObject.PlayDance ();
		yield return new WaitForSeconds (1f);
		GameOver ();
	}

	void GameOver() {
		gameStarted = false;
		timerText.gameObject.SetActive(false);
		scoreGauge.gameObject.SetActive(false);
		if (currentFoodToMatch)
			currentFoodToMatch.SetActive (false);

		if (score >= numDishes) {
			GameManager.GetInstance ().AddLagoonReviewGame ("MemoryMatchReviewGame");

			if (difficultyLevel == 1) {
				stickerPopupCanvas.gameObject.SetActive (true);
				GameManager.GetInstance ().ActivateBrainstormLagoonReview ();
				if (GameManager.GetInstance ().LagoonFirstSticker) {
					stickerPopupCanvas.transform.FindChild ("StickerbookButton").gameObject.SetActive (true);
					GameManager.GetInstance ().LagoonFirstSticker = false;
					Debug.Log ("This was Brainstorm Lagoon's first sticker");
				} else {
					Debug.Log ("This was not Brainstorm Lagoon's first sticker");
					stickerPopupCanvas.transform.FindChild ("BackButton").gameObject.SetActive (true);
				}
				GameManager.GetInstance ().ActivateSticker ("BrainstormLagoon", "Hippocampus");
				//GameManager.GetInstance ().LagoonTutorial [(int)Constants.BrainstormLagoonLevels.MEMORY_MATCH] = false;
			} else {
				DisplayGameOverPopup ();
			}
			GameManager.GetInstance ().LevelUp ("MemoryMatch");
		} else {
			if(difficultyLevel >= 1) {
				//GameManager.GetInstance ().LagoonTutorial [(int)Constants.BrainstormLagoonLevels.MEMORY_MATCH] = false;
				DisplayGameOverPopup();
			}
		}
	}

	public void DisplayGameOverPopup () {
		Debug.Log ("In DisplayGameOverPopup");
		gameOverCanvas.gameObject.SetActive(true);
		Text gameOverText = gameOverCanvas.GetComponentInChildren<Text> ();
		if (score > 1) {
			gameOverText.text = "Great job! You matched " + score + " healthy foods!";
		} else if (score == 1) {
			gameOverText.text = "Great job! You matched " + score + " healthy food!";
		} else {
			gameOverText.text = "You matched " + score + " healthy foods! Let's try again!";
		}
	}

	public bool isGameStarted() {
		return gameStarted;
	}

	public bool isRunningTutorial() {
		return runningTutorial;
	}

	void UpdateScoreGauge() {
		scoreGauge.value = score;
	}

	public void SubtractTime(float delta) {
		timer.SubtractTime(delta);
	}

	public void RotateDishes() {
		Vector3 zAxis = Vector3.forward; //<0, 0, 1>;

		for(int i = 0; i < numDishes; ++i) {
			GameObject d = dishes[i];
			Quaternion startRotation = d.transform.rotation;

			d.transform.RotateAround(target.position, zAxis, speed);
			d.transform.rotation = startRotation;
		}
	}

	void CreateMonster() {
		// Blue = 0, Green = 1, Red = 2, Yellow = 3

		switch (typeOfMonster) {
		case GameManager.MonsterType.Blue:
			InstantiateMonster (monsterList [(int)GameManager.MonsterType.Blue]);
			break;
		case GameManager.MonsterType.Green:
			InstantiateMonster (monsterList [(int)GameManager.MonsterType.Green]);
			break;
		case GameManager.MonsterType.Red:
			InstantiateMonster (monsterList [(int)GameManager.MonsterType.Red]);
			break;
		case GameManager.MonsterType.Yellow:
			InstantiateMonster (monsterList [(int)GameManager.MonsterType.Yellow]);
			break;
		}
	}

	void InstantiateMonster(GameObject monster) {
		GameObject monsterSpawn = Instantiate (
			monster, 
			Vector3.zero,
			Quaternion.identity) as GameObject;
		monsterObject = monsterSpawn.GetComponent<MMMonster> ();
	}
}
