﻿using UnityEngine;
using System.Collections;

/**
 * \class DishBehavior
 * \brief Defines the behavior of the dishes in the MemoryMatch game.
 * 
 * This script defines what a dish does when it is clicked on as well as
 * determines if a match has been made.
 */

public class DishBehavior : MonoBehaviour {
	
	private Food myFood; /*!< The food belonging to this dish. */
	private static bool isGuessing; /*!< Flag that keeps track of whether the player made a guess. */

	public GameObject top; /*!< Reference to the top part of the dish. */
	public GameObject bottom; /*!< Reference to the bottom part of the dish. */

	/** \cond */
	void Start () {
		isGuessing = false;
	}
	/** \endcond */

	/**
	 * \brief Set the food of this dish.
	 * @param food: a GameObject with a Food component attached
	 */
	public void SetFood(Food food) {
		myFood = gameObject.GetComponentsInChildren<Food>()[0];
	}

	/**
	 * \brief OnMouseDown is called when the player clicks (or taps) one of the dishes.
	 * 
	 * Check if this dish's myFood matches the foodToMatch. If it does match, deactivate
	 * the top part of the dish permenanatly, otherwise, cover the food again.
	 * @return WaitForSeconds for a delay.
	 */
	IEnumerator OnMouseDown() {
		Animation animation = top.GetComponent<Animation>();
		if(!isGuessing && MemoryMatchGameManager.GetInstance().isGameStarted()) {
			isGuessing = true;
			//Reveal the food underneath the dish by setting the sprite renderer to disabled.
			top.GetComponent<Animation>().Play (animation["DishTopRevealLift"].name);
			myFood.GetComponent<Subtitle>().Display(MemoryMatchGameManager.GetInstance().subtitlePanel, myFood.gameObject);
			if(MemoryMatchGameManager.GetInstance().GetFoodToMatch().name != myFood.name) {
				int randomClipIndex = Random.Range (0, MemoryMatchGameManager.GetInstance().wrongMatchClips.Length);
				SoundManager.GetInstance().PlayVoiceOverClip(MemoryMatchGameManager.GetInstance().wrongMatchClips[randomClipIndex]);

				yield return new WaitForSeconds(2f);
				top.GetComponent<Animation>().Play (animation["DishTopRevealClose"].name);
			}
			else {
				top.GetComponent<Animation>().Play (animation["DishTopRevealLift"].name);
				SoundManager.GetInstance().PlaySFXClip(MemoryMatchGameManager.GetInstance().correctSound);
				MemoryMatchGameManager.GetInstance().AddToMatchedList(myFood);
				yield return new WaitForSeconds(1.5f);
				top.GetComponent<SpriteRenderer>().enabled = false;

				MemoryMatchGameManager.GetInstance().ChooseFoodToMatch();
			}
			//The player can now guess again.
			isGuessing = false;
			//return true;
		}
	}
	
}
