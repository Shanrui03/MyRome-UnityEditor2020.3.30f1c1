using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//
// CompassMarker3DOverlayScript
// This script controls behaviour of 3D overlays on marked game objects
//
// You can adjust maxCamDistance to change how far away these are visible
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class CompassMarker3DOverlayScript : MonoBehaviour {

	// Change this to determine distance to start alpha falloff of overlay graphic
	// Larger values mean the markers are visible from farther away
	private float maxCamDistance = 25.0f;

	private Vector3 horizontalRelativePosition = Vector3.zero;
	private Vector3 relativePosition = Vector3.zero;
	private Quaternion updatedRotation = Quaternion.identity;
	private RectTransform childMarkerRectTransform;
	private float distanceToCamera = 0.0f;
	private bool overlayVisible = true;
	
	void Awake(){
		
		childMarkerRectTransform = gameObject.GetComponentInChildren<RectTransform>();
		if(!childMarkerRectTransform){
			Debug.LogError("CompassMarker3DOverlayScript:: Cannot find Child Image!");
		}
		
	}
	
	void Update(){

		if(overlayVisible){

			// Always face camera
			horizontalRelativePosition.x = Camera.main.transform.position.x;
			horizontalRelativePosition.y = transform.position.y;
			horizontalRelativePosition.z = Camera.main.transform.position.z;
			relativePosition = horizontalRelativePosition - transform.position;
			updatedRotation = Quaternion.LookRotation(relativePosition);
			transform.rotation = updatedRotation;
			// Fade alpha in when we get closer to camera
			distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
			childMarkerRectTransform.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Max(0.25f,1.0f - (distanceToCamera/maxCamDistance)));
		
		}
		
	}

	public void updateIconSprite(Sprite updatedSprite){
		gameObject.GetComponent<Image>().overrideSprite = updatedSprite;
	}

	public void showImages(){
		overlayVisible = true;
	}

	public void hideImages(){

		if(overlayVisible){

			overlayVisible = false;
			childMarkerRectTransform.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

		}

	}

}
