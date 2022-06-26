using UnityEngine;
using System.Collections;

//
// DemoQuestMarkerScript
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only.
// A simple demo script to show how to make a basic quest marker that when
// entered, leads to another quest marker.
// Copyright 2016 While Fun Games
// http://whilefun.com
//
// To make your own Quest Markers:
//-------------------------------
//1) Create a new Game Object (e.g. a Cube), and check "Is Trigger" checkbox for it's Box Collider
//2) Attached DemoQuestMarkerScript and assign "Custom Quest Marker Icon" texture you want the Quest Marker to have
//4) Save this as a prefab
//5) Drag a few copies of the new prefab into the scene
//6) In the inspector, check "Start Active" for the first marker in the quest
//7) In the inspector, set "Next Quest Marker" for each quest marker object to be the next quest marker in the sequence
//8) Repeat step 7 for the rest except the last Quest Marker
//9) If desired, disable the Mesh Renderers on quest marker objects 2 to n (for effect only, see script)
//10) Run the scene, and walk into the first quest marker. Quest marker 2 will now activate. Repeat until the quest is done.
//
public class DemoQuestMarkerScript : MonoBehaviour {

	[Tooltip("A custom icon for this Demo Quest Marker")]
	public Sprite customQuestMarkerIcon;
	// For quest markers, perhaps you want to track the next quest item in the Inspector
	[Tooltip("The Quest Marker to activate after this one is reached. Select something in your scene.")]
	public GameObject nextQuestMarker;
	// When setting these up in the inspector, perhaps mark the 1st quest in the series.
	[Tooltip("If this Quest Marker should be tracked on Start")]
	public bool startActive = false;

	private GameObject theCompass;
	private bool questMarkerVisited = false;

	void Start(){
	
		theCompass = GameObject.FindGameObjectWithTag("TheCompass");
		if(!theCompass){
			Debug.LogError("DemoQuestMarkerScript::Cannot Find TheCompass!");
		}

		// Activate the quest if we are supposed to start active!
		if(startActive){
			activateQuest();
		}

	}

	// When player reaches quest marker, activate the next marker in this quest line
	void OnTriggerEnter(){

		if(!questMarkerVisited){
			questMarkerVisited = true;
			activateNextQuest();
		}

	}

	private void activateQuest(){

		// Add COmpass Marker, configure it, and initialize it
		gameObject.AddComponent<CompassMarkerScript>();
		gameObject.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eCustom);
		gameObject.GetComponent<CompassMarkerScript>().customMarkerIcon = customQuestMarkerIcon;
		gameObject.GetComponent<CompassMarkerScript>().showUpDownArrow = true;
		gameObject.GetComponent<CompassMarkerScript>().showDistanceLabel = true;
		// Since we know there is only going to be one active quest marker at a time, it's a good time to use off screen hints
		gameObject.GetComponent<CompassMarkerScript>().showOffScreenHints = true;
		gameObject.GetComponent<CompassMarkerScript>().initTrackingOfMarker();

		// Want it to be hidden until activated for effect. You can disable the MeshRenderers in the Inspector for Quests 2...n
		MeshRenderer[] cmr = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer mr in cmr){
			mr.GetComponent<Renderer>().enabled = true;
		}

	}

	private void activateNextQuest(){

		// Stop tracking me
		Destroy(gameObject.GetComponent<CompassMarkerScript>());

		// Hide all meshes but planes, and make them visually different than before
		MeshRenderer[] cmr = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer mr in cmr){
			if(mr.gameObject.name != "Plane"){
				mr.GetComponent<Renderer>().enabled = false;
			}else if(mr.gameObject.name == "Plane"){
				mr.GetComponent<Renderer>().material.color = Color.green;
			}
		}

		// Activate next quest marker if there is one
		if(nextQuestMarker){
			nextQuestMarker.GetComponent<DemoQuestMarkerScript>().activateQuest();
		}else{
			// When there is no nextQuestMarker, we know we've reached the last marker. Our Quest is complete!
			Debug.Log("Quest Complete!");
		}

	}

}
