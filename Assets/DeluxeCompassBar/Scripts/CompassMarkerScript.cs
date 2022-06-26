using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//
// CompassMarkerScript
// This script controls how Compass Markers behave, and allows The Compass to track
// objects for display in The Compass GUI.
//
// All specific decoration/behaviour flags are set to false by default, allowing for
// quick and easy basic operation.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class CompassMarkerScript : MonoBehaviour {

	[Tooltip("A custom icon for the marker and associated elements (Optional)")]
	public Sprite customMarkerIcon;
	[Tooltip("Set this if you want to use a built in icon type. Set to Custom if you want to set a Custom Marker Icon for direct tracking.")]
	public TheCompassScript.IconType myIconType = TheCompassScript.IconType.eCustom;
	[Tooltip("Turn Distance Text On and Off")]
	public bool showDistanceLabel = false;
	[Tooltip("Turn Up/Down Arrow graphic On and Off")]
	public bool showUpDownArrow = false;
	[Tooltip("Check this box to show hints when marker is outside compass bounds. Use sparingly.")]
	public bool showOffScreenHints = false;
	[Tooltip("Turn World Space 3D icon overlay On and Off")]
	public bool show3DOverlay = false;
	[Tooltip("Distance above game object position to show the 3D overlay (Default=0.75)")]
	public float vertical3DOverlayOffset = 0.75f;
	[Tooltip("Check this box if assigning script directly to an object in your scene")]
	public bool directTracking = false;

	// Internal Stuff
	private GameObject theCompass;
	private bool initialized = false;
	// UI Elements
	private GameObject myIcon;
	private GameObject myDistanceLabel;
	private float myDistanceLabelYStartPosition = 0.0f;
	private GameObject myUpDownArrow;
	private GameObject my3DOverlay;
	private GameObject myOffScreenHintLeft;
	private GameObject myOffScreenHintRight;
	private float compassGUIPosition = 0.0f;
	private float distanceToCompass = 0.0f;
	private Sprite updatedUpDownArrowSprite = null;

	void Awake(){

		theCompass = GameObject.FindGameObjectWithTag("TheCompass");
		if(!theCompass){
			Debug.LogError("Cannot find The Compass!");
		}

	}

	void Start(){

		if(directTracking){
			initTrackingOfMarker();
		}

	}
	
	void Update(){

		if(initialized){

			// Update all icon/text/arrow elements
			compassGUIPosition = theCompass.GetComponent<TheCompassScript>().getUpdatedGUIPositionForMarker(gameObject.transform, myIcon.GetComponent<RectTransform>().rect.width);

			if(compassGUIPosition !=  Mathf.Infinity && compassGUIPosition !=  Mathf.NegativeInfinity){

				// Hide the hints, if they exist
				if(showOffScreenHints && myOffScreenHintLeft && myOffScreenHintRight){
					myOffScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().hideImages();
					myOffScreenHintRight.GetComponent<CompassMarkerOffScreenHintScript>().hideImages();
				}

				// Show the rest, as required
				myIcon.GetComponent<CanvasRenderer>().SetAlpha(1.0f);

				Vector2 updatedMarkerPosition = myIcon.GetComponent<RectTransform>().anchoredPosition;
				updatedMarkerPosition.x = compassGUIPosition;
				myIcon.GetComponent<RectTransform>().anchoredPosition = updatedMarkerPosition;

				if(showDistanceLabel){

					distanceToCompass = theCompass.GetComponent<TheCompassScript>().getUpdatedDistanceToCompass(gameObject.transform);

					if(distanceToCompass != Mathf.Infinity){

						myDistanceLabel.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
						myDistanceLabel.GetComponent<Text>().text = "" + (int)distanceToCompass;
						updatedMarkerPosition = myDistanceLabel.GetComponent<RectTransform> ().anchoredPosition;
						updatedMarkerPosition.x = compassGUIPosition;
						myDistanceLabel.GetComponent<RectTransform>().anchoredPosition = updatedMarkerPosition;

					}else{

						myDistanceLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
						
					}
					
				}

				if(showUpDownArrow){

					// get arrow icon to switch to, if applicable.
					updatedUpDownArrowSprite = theCompass.GetComponent<TheCompassScript>().getUpdatedUpDownArrowSprite(gameObject.transform);

					if(updatedUpDownArrowSprite != null){

						myUpDownArrow.GetComponent<Image>().overrideSprite = updatedUpDownArrowSprite;
						myUpDownArrow.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
						updatedMarkerPosition = myUpDownArrow.GetComponent<RectTransform> ().anchoredPosition;
						updatedMarkerPosition.x = compassGUIPosition;
						myUpDownArrow.GetComponent<RectTransform>().anchoredPosition = updatedMarkerPosition;

						if(theCompass.GetComponent<TheCompassScript>().padUpDownArrows){
							Vector2 paddedArrowPos = myDistanceLabel.GetComponent<RectTransform>().anchoredPosition;
							paddedArrowPos.y = myDistanceLabelYStartPosition + myUpDownArrow.GetComponent<RectTransform>().rect.height;
							myDistanceLabel.GetComponent<RectTransform>().anchoredPosition = paddedArrowPos;
						}
						
					}else{

						myUpDownArrow.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

						if(theCompass.GetComponent<TheCompassScript>().padUpDownArrows){
							Vector2 unPaddedArrowPos = myDistanceLabel.GetComponent<RectTransform>().anchoredPosition;
							unPaddedArrowPos.y = myDistanceLabelYStartPosition;
							myDistanceLabel.GetComponent<RectTransform>().anchoredPosition = unPaddedArrowPos;
						}
						
					}

				}

			}else{

				myIcon.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

				if(showDistanceLabel && myDistanceLabel){
					myDistanceLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
				}

				if(showUpDownArrow && myUpDownArrow){
					myUpDownArrow.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
				}

				// Show corresponding off screen hint, if required
				if(showOffScreenHints && myOffScreenHintLeft && myOffScreenHintRight){

					if(compassGUIPosition ==  Mathf.Infinity){

						myOffScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().hideImages();
						myOffScreenHintRight.GetComponent<CompassMarkerOffScreenHintScript>().showImages();

					}else{

						myOffScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().showImages();
						myOffScreenHintRight.GetComponent<CompassMarkerOffScreenHintScript>().hideImages();

					}

				}

			}

		}//initialized

	}

	public void setMarkerIconType(TheCompassScript.IconType iconType){
		myIconType = iconType;
	}

	// If not using directTracking, you must explicitly call this function to start tracking the object.
	public void initTrackingOfMarker(){

		// An error to make it obvious why your custom icon is not showing up
		if(!customMarkerIcon && myIconType == TheCompassScript.IconType.eCustom){
			Debug.Log ("custommarkerIcon="+customMarkerIcon.name);
			Debug.LogError("Compass Marker Icon type is set to eCustom, but Sprite was not assigned.");
		}

		// Icon is boilerplate. Other elements are optional (see below)
		myIcon = Instantiate(Resources.Load("CompassMarkerIcon")) as GameObject;
		theCompass.GetComponent<TheCompassScript>().putIconOnCompass(myIcon.GetComponent<RectTransform>(), myIconType);
		// We make it invisible at first, so that the first frame is not drawn at the default compass position
		myIcon.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		
		if(show3DOverlay){
			add3DOverlay();
		}
		
		if(showDistanceLabel){
			addDistanceLabel();
			myDistanceLabelYStartPosition = myDistanceLabel.GetComponent<RectTransform>().anchoredPosition.y;
		}
		
		if(showUpDownArrow){
			addUpDownArrow();
		}
		
		if(showOffScreenHints){
			addOffScreenHints();
		}
		
		// If a custom icon was specified and icon type is still eCustom, then switch marker icon to use it
		if(customMarkerIcon != null && myIconType == TheCompassScript.IconType.eCustom){
			myIcon.GetComponent<Image>().overrideSprite = customMarkerIcon;
		}

		initialized = true;

	}

	public void add3DOverlay(){

		// Only create a new 3D Overlay if one doesn't already exist
		if(!my3DOverlay){

			my3DOverlay = Instantiate(Resources.Load ("CompassMarker3DOverlay")) as GameObject;
			Vector3 overlayPosition = transform.position;

			// If we're not using a custom icon, then apply the correct icon to the 3D overlay
			if(myIconType != TheCompassScript.IconType.eCustom){
				my3DOverlay.GetComponentInChildren<RectTransform>().GetComponent<CompassMarker3DOverlayScript>().updateIconSprite(theCompass.GetComponent<TheCompassScript>().getSpriteFromIconType(myIconType));
			}

			overlayPosition.y += vertical3DOverlayOffset;
			my3DOverlay.transform.position = overlayPosition;

			// Attach as child of object we're tracking
			my3DOverlay.transform.SetParent(transform, true);

			if(customMarkerIcon != null && myIconType == TheCompassScript.IconType.eCustom){
				my3DOverlay.GetComponentInChildren<RectTransform>().GetComponent<CompassMarker3DOverlayScript>().updateIconSprite(customMarkerIcon);
			}

		}
		
	}

	public void remove3DOverlay(){
		GameObject.Destroy(my3DOverlay);
	}

	public void toggle3DOverlay(){

		if(!my3DOverlay){

			show3DOverlay = true;
			add3DOverlay();

		}else{

			if(show3DOverlay){

				show3DOverlay = false;
				my3DOverlay.GetComponentInChildren<RectTransform>().GetComponent<CompassMarker3DOverlayScript>().hideImages();

			}else{

				show3DOverlay = true;
				my3DOverlay.GetComponentInChildren<RectTransform>().GetComponent<CompassMarker3DOverlayScript>().showImages();

			}

		}

	}

	public void addDistanceLabel(){

		if(!myDistanceLabel){

			myDistanceLabel = Instantiate(Resources.Load("CompassMarkerDistanceLabel")) as GameObject;
			// We make it invisible at first, so that the first frame is not drawn at the default compass position
			myDistanceLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
			theCompass.GetComponent<TheCompassScript>().putDistanceLabelOnCompass(myDistanceLabel.GetComponent<RectTransform>());

		}

	}

	public void removeDistanceLabel(){
		GameObject.Destroy (myDistanceLabel);
	}

	public void addUpDownArrow(){

		if(!myUpDownArrow){

			myUpDownArrow = Instantiate(Resources.Load("CompassMarkerUpDownArrow")) as GameObject;
			// We make it invisible at first, so that the first frame is not drawn at the default compass position
			myUpDownArrow.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
			theCompass.GetComponent<TheCompassScript>().putUpDownArrowOnCompass(myUpDownArrow.GetComponent<RectTransform>());

		}

	}

	public void addOffScreenHints(){

		if(!myOffScreenHintLeft && !myOffScreenHintRight){

			myOffScreenHintLeft = Instantiate(Resources.Load("CompassMarkerOffScreenHintLeft")) as GameObject;
			myOffScreenHintRight = Instantiate(Resources.Load("CompassMarkerOffScreenHintRight")) as GameObject;
			theCompass.GetComponent<TheCompassScript>().putOffscreenHintOnCompass(myOffScreenHintLeft.GetComponent<RectTransform>(), myIconType);
			theCompass.GetComponent<TheCompassScript>().putOffscreenHintOnCompass(myOffScreenHintRight.GetComponent<RectTransform>(), myIconType);

			// Apply theme/skin graphics from The Compass
			myOffScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().updateArrowSprite(theCompass.GetComponent<TheCompassScript>().offScreenArrowLeft);
			myOffScreenHintRight.GetComponent<CompassMarkerOffScreenHintScript>().updateArrowSprite(theCompass.GetComponent<TheCompassScript>().offScreenArrowRight);

			if(customMarkerIcon != null && myIconType == TheCompassScript.IconType.eCustom){
				myOffScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().updateIconSprite(customMarkerIcon);
				myOffScreenHintRight.GetComponent<CompassMarkerOffScreenHintScript>().updateIconSprite(customMarkerIcon);
			}

		}
		
	}

	public void removeUpDownArrow(){
		GameObject.Destroy(myUpDownArrow);
	}

	public void removeOffScreenHints(){
		GameObject.Destroy(myOffScreenHintLeft);
		GameObject.Destroy(myOffScreenHintRight);
	}

	// It's critical that we remove our tracking elements when Destroyed!
	// OnDisable() is called when MonoBehaviour is destroyed.
	void OnDisable(){
		
		GameObject.Destroy(myIcon);
		GameObject.Destroy(myDistanceLabel);
		GameObject.Destroy(my3DOverlay);
		GameObject.Destroy(myUpDownArrow);
		GameObject.Destroy(myOffScreenHintLeft);
		GameObject.Destroy(myOffScreenHintRight);
		
	}

}
