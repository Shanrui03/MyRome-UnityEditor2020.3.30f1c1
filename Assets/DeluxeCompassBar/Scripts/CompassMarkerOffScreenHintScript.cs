using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//
// CompassMarkerOffScreenHintScript
// This script controls behaviour of off screen hint arrows.
//
// It is strongly recommended that you use these one at a time.
// Alternatively, you could make Y position on canvas a function of 
// distance to compass.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class CompassMarkerOffScreenHintScript : MonoBehaviour {

	private RectTransform markerIconArrow;
	private RectTransform markerIconChild;

	void Awake(){

		RectTransform[] rtChildren = GetComponentsInChildren<RectTransform>();

		foreach(RectTransform rt in rtChildren){

			if(rt.transform.gameObject.name == "OffScreenMarkerIcon"){
				markerIconChild = rt;
				break;
			}

		}

		markerIconArrow = gameObject.GetComponent<RectTransform>();

		if(!markerIconChild || !markerIconArrow){
			Debug.LogError("Cannot find one of more of the Off Screen Hint icon Images!");
		}

	}

	void Start(){
	
	}
	
	void Update(){
	
	}

	public void updateIconSprite(Sprite updatedSprite){
		markerIconChild.GetComponent<Image>().overrideSprite = updatedSprite;
	}

	public void updateArrowSprite(Sprite updatedSprite){
		markerIconArrow.GetComponent<Image>().overrideSprite = updatedSprite;
	}

	public void hideImages(){
		markerIconArrow.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		markerIconChild.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
	}

	public void showImages(){
		markerIconArrow.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
		markerIconChild.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
	}

}
