using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//
// The Compass Script
// This script controls central compass behaviour and CompassMarker management
//
// Place TheCompass prefab in your scene, and attach CompassCamera prefab as a child
// of your player or equivalent game avatar. Compass Camera should have the same 
// position and rotation as your main camera. For example, make it a child of the 
// Standard "First Person Controller" Game Object. The CompassCamera is used to 
// determine position relative to marked objects. Any offset will affect the
// accuracy of distance/position calculations.
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class TheCompassScript : MonoBehaviour {

	// You can replace these textures with your own graphics in the Inspector
	[Tooltip("The main Compass GUI graphic")]
	public Sprite compassGUIBackground;
	[Tooltip("Turn Bookend graphic On and Off")]
	public bool drawCompassBookends = true;
	public Sprite compassGUIBookEndLeft;
	public Sprite compassGUIBookEndRight;
	[Tooltip("Turn Ticks graphic On and Off")]
	public bool drawCompassTicks = true;
	public Sprite compassGUITicks;
	public Sprite compassGUILabelNorth;
	public Sprite compassGUILabelSouth;
	public Sprite compassGUILabelEast;
	public Sprite compassGUILabelWest;
	[Tooltip("Turn Up/Down arrow graphic On and Off")]
	public bool drawUpDownArrows = true;
	[Tooltip("Turn Up/Down Arrow graphic padding On and Off. When Off, vertical position of Distance text is static.")]
	public bool padUpDownArrows = true;
	[Tooltip("The vertical pixel offset of up/down arrows")]
	public float upDownArrowOffset = 0.0f;
	public Sprite compassMarkerUpArrow;
	public Sprite compassMarkerDownArrow;

	// Distance Text
	[Tooltip("Turn Distance Text On and Off")]
	public bool drawDistanceText = true;
	[Tooltip("The vertical pixel offset of the distance text")]
	public float distanceTextOffset = 0.0f;
	public Color compassGUITextColor = Color.white;
	[Tooltip("Turn Distance Text Outline On and Off")]
	public bool outlineDistanceText = true;
	public Color compassGUITextOutlineColor = Color.black;
	[Tooltip("Default font size (Scales with screen size changes)")]
	public int compassBaseFontSize = 12;
	// How far up/down a marker needs to be in order to be flagged with up/down arrow on the compass
	[Tooltip("Min vertical distance before drawing up/down arrows above Compass Marker")]
	public float verticalMarkerDistanceThreshold = 2.0f;
	// The min/max distance are the bounds in which we draw the distance to marker over top of the marker
	[Tooltip("Min distance to draw distance indicator above Compass Marker")]
	public float minMarkerDistance = 2.0f;
	[Tooltip("Max distance to draw distance indicator above Compass Marker")]
	public float maxMarkerDistance = 50.0f;
	[Tooltip("Arrow icons to point toward a Marker when hints are enabled on it")]
	public Sprite offScreenArrowLeft;
	public Sprite offScreenArrowRight;

	// Internal stuff
	private float currentDirectionInDegrees = 0.0f;
	private GameObject worldPoles;
	private float deltaFromWorldPoles = 0.0f;
	private float directionInDegrees = 0.0f;
	// GUI stuff
	private float compassGUINorthPosition = 0.0f;
	private float compassGUISouthPosition = 0.0f;
	private float compassGUIEastPosition = 0.0f;
	private float compassGUIWestPosition = 0.0f;
	private float distanceBetweenDirectionsOnCompass = 0.0f;
	// UI Canvas Stuff
	private RectTransform compassBackground;
	private RectTransform compassBackgroundMask;
	private RectTransform compassTicksRect;
	private Vector2 tempTicksPos = Vector2.zero;
	private RectTransform northLabel;
	private RectTransform southLabel;
	private RectTransform eastLabel;
	private RectTransform westLabel;
	private RectTransform compassMarkerContainer;
	private RectTransform compassCanvasParent;
	private RectTransform compassBookendLeft;
	private RectTransform compassBookendRight;
	private GameObject compassCamera;
	private Vector2 compassBackgroundAnchoredPosition = Vector2.zero;

	// To add more icon types for Compass Markers:
	// 1) Create the graphic using provided template
	// 2) Add a new IconType enum value for the new icon type (e.g. eMyIconType)
	// 3) Add a new public Sprite below (e.g. markerIconMyNewType)
	// 4) Add case for new enum value in getSpriteFromIconType() function below
	public enum IconType {eGeneric, eImportant, eMonster, eCustom};
	public Sprite markerIconGeneric;
	public Sprite markerIconImportant;
	public Sprite markerIconMonster;

	void Awake(){
		
		worldPoles = GameObject.FindGameObjectWithTag("CompassWorldPoles");
		if(!worldPoles){
			Debug.LogError("Cannot find CompassWorldPoles object. Is the WorldPoles tag missing or prefab missing?");
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}

		RectTransform[] rtc = gameObject.GetComponentsInChildren<RectTransform>();
		foreach(RectTransform rt in rtc){

			// This is where the compass markers live. It is z-fixed behind the 
			// bookends so that markers always go behind them
			if(rt.gameObject.name == "CompassMarkerContainer"){
				compassMarkerContainer = rt;
			}else if(rt.gameObject.name == "CompassCanvas"){
				compassCanvasParent = rt;
			}else if(rt.gameObject.name == "CompassBackground"){
				compassBackground = rt;
			}else if(rt.gameObject.name == "CompassBackgroundMask"){
				compassBackgroundMask = rt;
			}else if(rt.gameObject.name == "CompassTicks"){
				compassTicksRect = rt;
			}else if(rt.gameObject.name == "NorthLabel"){
				northLabel = rt;
			}else if(rt.gameObject.name == "SouthLabel"){
				southLabel = rt;
			}else if(rt.gameObject.name == "EastLabel"){
				eastLabel = rt;
			}else if(rt.gameObject.name == "WestLabel"){
				westLabel = rt;
			}else if(rt.gameObject.name == "BookendLeft"){
				compassBookendLeft = rt;
			}else if(rt.gameObject.name == "BookendRight"){
				compassBookendRight = rt;
			}
			
		}

		// If this error occurs, you probably broke the prefab, as it's missing some required elements
		if(!compassMarkerContainer || !compassCanvasParent || !compassBackground || !compassBackgroundMask || !compassTicksRect || !northLabel || !southLabel || !eastLabel || !westLabel || !compassBookendLeft || !compassBookendRight){
			Debug.LogError("TheCompassScript:: Could not find one or more of the child UI Canvas elements of The Compass. Did you break the Prefab?");
		}

		// Compass Camera will be used to calculate where our compass is.
		// We don't make The Compass direct child of player because it can cause subtle 
		// texture movements as camera and canvas will move/adjust at the same time.
		compassCamera = GameObject.FindGameObjectWithTag("CompassCamera");
		if(!compassCamera){
			Debug.LogError("TheCompassScript:: Could not find Compass Camera!");
		}
		
	}

	void Start(){

		// Assign all our specified graphics - i.e. "Apply the Compass Theme"
		compassBackground.GetComponent<Image>().overrideSprite = compassGUIBackground;
		compassBackgroundMask.GetComponent<Image>().overrideSprite = compassGUIBackground;
		northLabel.GetComponent<Image>().overrideSprite = compassGUILabelNorth;
		southLabel.GetComponent<Image>().overrideSprite = compassGUILabelSouth;
		eastLabel.GetComponent<Image>().overrideSprite = compassGUILabelEast;
		westLabel.GetComponent<Image>().overrideSprite = compassGUILabelWest;

		// Note: If your implementation will always exclude Ticks graphic, you can delete the ticks completely rather than just hiding them
		// Same with Bookends. If the theme/skin you want to use does not require bookends, you can delete them completely as well.
		if(drawCompassTicks){
			compassTicksRect.GetComponent<Image>().overrideSprite = compassGUITicks;
		}else{
			compassTicksRect.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}
		if(drawCompassBookends){
			compassBookendLeft.GetComponent<Image>().overrideSprite = compassGUIBookEndLeft;
			compassBookendRight.GetComponent<Image>().overrideSprite = compassGUIBookEndRight;
		}else{
			compassBookendLeft.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
			compassBookendRight.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}

		// Save our position on start, as this affects where markers/arrows/etc. are placed on the canvas
		compassBackgroundAnchoredPosition.x = compassBackground.anchoredPosition.x;
		compassBackgroundAnchoredPosition.y = compassBackground.anchoredPosition.y;

	}
	
	void Update(){

		// Determine our direction relative to WorldPoles
		currentDirectionInDegrees = compassCamera.transform.rotation.eulerAngles.y;
		deltaFromWorldPoles = currentDirectionInDegrees - worldPoles.GetComponent<Transform>().rotation.eulerAngles.y;

		if(deltaFromWorldPoles < 0){
			directionInDegrees = (360+deltaFromWorldPoles);
		}else if(deltaFromWorldPoles > 0){
			directionInDegrees = deltaFromWorldPoles;
		}else{
			directionInDegrees = 0.0f;
		}

		distanceBetweenDirectionsOnCompass = 1.5f*compassBackground.rect.width;

		if(directionInDegrees >= 180){
			compassGUINorthPosition = compassBackground.anchoredPosition.x + ((360-directionInDegrees)/180)*distanceBetweenDirectionsOnCompass;
			compassGUISouthPosition = compassBackground.anchoredPosition.x + ((180-directionInDegrees)/180)*distanceBetweenDirectionsOnCompass;
			compassGUIEastPosition = compassBackground.anchoredPosition.x + ((90-directionInDegrees)/180)*distanceBetweenDirectionsOnCompass;
			compassGUIWestPosition = compassBackground.anchoredPosition.x + ((270-directionInDegrees)/180)*distanceBetweenDirectionsOnCompass;
		}else{
			compassGUINorthPosition = compassBackground.anchoredPosition.x - (directionInDegrees/180)*distanceBetweenDirectionsOnCompass;
			compassGUISouthPosition = compassBackground.anchoredPosition.x - ((directionInDegrees-180)/180)*distanceBetweenDirectionsOnCompass;
			compassGUIEastPosition = compassBackground.anchoredPosition.x - ((directionInDegrees-90)/180)*distanceBetweenDirectionsOnCompass;
			compassGUIWestPosition = compassBackground.anchoredPosition.x - ((directionInDegrees-270)/180)*distanceBetweenDirectionsOnCompass;
		}
		
		// Update direction label positions
		Vector2 tempLabelPosition = Vector2.zero;

		// Only show if in bounds, otherwise just make it invisible
		if(compassGUINorthPosition > (compassBackground.anchoredPosition.x - compassBackground.rect.width/2 + northLabel.rect.width/2) && compassGUINorthPosition < (compassBackground.anchoredPosition.x + compassBackground.rect.width/2 - northLabel.rect.width/2)){
			northLabel.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			tempLabelPosition = northLabel.anchoredPosition;
			tempLabelPosition.x = compassGUINorthPosition;
			northLabel.anchoredPosition = tempLabelPosition;
		}else{
			northLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}
		
		if(compassGUISouthPosition > (compassBackground.anchoredPosition.x - compassBackground.rect.width/2 + southLabel.rect.width/2) && compassGUISouthPosition < (compassBackground.anchoredPosition.x + compassBackground.rect.width/2 - southLabel.rect.width/2)){
			southLabel.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			tempLabelPosition = southLabel.anchoredPosition;
			tempLabelPosition.x = compassGUISouthPosition;
			southLabel.anchoredPosition = tempLabelPosition;
		}else{
			southLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}
		
		if(compassGUIEastPosition > (compassBackground.anchoredPosition.x - compassBackground.rect.width/2 + eastLabel.rect.width/2) && compassGUIEastPosition < (compassBackground.anchoredPosition.x + compassBackground.rect.width/2 - eastLabel.rect.width/2)){
			eastLabel.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			tempLabelPosition = eastLabel.anchoredPosition;
			tempLabelPosition.x = compassGUIEastPosition;
			eastLabel.anchoredPosition = tempLabelPosition;
		}else{
			eastLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}
		
		if(compassGUIWestPosition > (compassBackground.anchoredPosition.x - compassBackground.rect.width/2 + westLabel.rect.width/2) && compassGUIWestPosition < (compassBackground.anchoredPosition.x + compassBackground.rect.size.x/2 - westLabel.rect.width/2)){
			westLabel.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			tempLabelPosition = westLabel.anchoredPosition;
			tempLabelPosition.x = compassGUIWestPosition;
			westLabel.anchoredPosition = tempLabelPosition;
		}else{
			westLabel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		}

		// Move Ticks based on North position, and  mod by 256 so that we don't scroll to far (BG images are 512px wide)
		tempTicksPos.x = (compassGUINorthPosition % 256);
		compassTicksRect.anchoredPosition = tempTicksPos;

	}

	// Adds a marker icon to compass
	// Specify an icon type if desired, otherwise it is assumed you are using a custom icon
	// Note: This function is different than the others as it places the UI element as a child of 
	// the CompassMarkerContrainer, which is not offset from the main canvas at all :)
	public void putIconOnCompass(RectTransform iconToAdd, IconType iconType=IconType.eCustom){

		iconToAdd.SetParent(compassMarkerContainer, false);
		Vector2 tempMarkerIconPosition = Vector2.zero;
		tempMarkerIconPosition.y = 0.0f;
		iconToAdd.anchoredPosition = tempMarkerIconPosition;

		// If we're not using a custom icon, then apply the correct icon to this UI element
		if(iconType != IconType.eCustom){
			iconToAdd.GetComponent<Image>().overrideSprite = getSpriteFromIconType(iconType);
		}

	}
	
	// Adds distance label to compass, and applies all formatting options specified
	public void putDistanceLabelOnCompass(RectTransform distanceLabelToAdd){

		distanceLabelToAdd.SetParent(compassMarkerContainer, false);
		Vector2 tempDistanceLabelPosition = Vector2.zero;
		tempDistanceLabelPosition.y = distanceTextOffset;
		distanceLabelToAdd.anchoredPosition = tempDistanceLabelPosition;

		// Update font size, color, and outline color (or remove outline completely)
		distanceLabelToAdd.GetComponent<Text>().color = compassGUITextColor;
		distanceLabelToAdd.GetComponent<Text>().fontSize = compassBaseFontSize;

		if(outlineDistanceText && distanceLabelToAdd.GetComponent<Outline>()){
			distanceLabelToAdd.GetComponent<Outline>().effectColor = compassGUITextOutlineColor;
		}else{
			//This assumes you will not try to dynamically add/remove outline effects at runtime
			Destroy(distanceLabelToAdd.GetComponent<Outline>());
		}

	}

	// Add up/down arrows to indicate vertical position difference
	public void putUpDownArrowOnCompass(RectTransform upDownArrowToAdd){
		
		upDownArrowToAdd.SetParent(compassMarkerContainer, false);
		Vector2 tempDistanceLabelPosition = Vector2.zero;
		tempDistanceLabelPosition.y = upDownArrowOffset;
		upDownArrowToAdd.anchoredPosition = tempDistanceLabelPosition;
		
	}

	// Add off screen hints to compass. Note these are designed for single item use only. They become unreadable
	// very quickly if more than one set of hints is visible off the same side of the screen at the same time.
	public void putOffscreenHintOnCompass(RectTransform offScreenHintLeft, IconType iconType=IconType.eCustom){

		offScreenHintLeft.SetParent(compassCanvasParent, false);

		// If we're not using a custom icon, then apply the correct icon to this UI element
		if(iconType != IconType.eCustom){
			offScreenHintLeft.GetComponent<CompassMarkerOffScreenHintScript>().updateIconSprite(getSpriteFromIconType(iconType));
		}

	}

	// Returns correct Sprite for iconType specified
	public Sprite getSpriteFromIconType(IconType iconType){
		
		Sprite spriteForIconType = markerIconGeneric;
		
		switch(iconType){
			
		case IconType.eGeneric:
			spriteForIconType = markerIconGeneric;
			break;
			
		case IconType.eImportant:
			spriteForIconType = markerIconImportant;
			break;
			
		case IconType.eMonster:
			spriteForIconType = markerIconMonster;
			break;
			
		/*
		// Your new Marker Icon case here:
		case IconType.eMyIconType:
			spriteForIconType = markerIconMyNewType;
			break;
		*/
			
		// Default to the generic icon
		default:
			spriteForIconType = markerIconGeneric;
			break;
			
		}
		
		return spriteForIconType;
		
	}

	// Calculate and returns updated Compass GUI position
	// Returns Mathf.INFINITY if marker is out of bounds RIGHT of the GUI (and should not be drawn)
	// Returns Mathf.NEGATIVEINFINITY if marker is out of bounds LEFT of the GUI (and should not be drawn)
	public float getUpdatedGUIPositionForMarker(Transform markerTransform, float widthOfUIElement){

		Vector3 relative = compassCamera.transform.InverseTransformPoint(markerTransform.position);
		float tempAngle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
		float newGUIPosition = 0.0f;
		newGUIPosition = ((compassBackground.anchoredPosition.x) + (tempAngle/180)*distanceBetweenDirectionsOnCompass);

		// If out of bounds, set to infinity +/-
		if(newGUIPosition > (compassBackground.anchoredPosition.x + compassBackground.rect.width/2 - widthOfUIElement/2)){
			newGUIPosition = Mathf.Infinity;
		}else if(newGUIPosition < (compassBackground.anchoredPosition.x - compassBackground.rect.width/2 + widthOfUIElement/2)){
			newGUIPosition = Mathf.NegativeInfinity;
		}

		return newGUIPosition;

	}

	// Calculates and returns vertical positional difference between specified marker transform and compass transform
	// Used for determining which, if any, up/down arrow icons to display.
	// Returns up or down arrow Sprite, or null if no sprite should be displayed
	public Sprite getUpdatedUpDownArrowSprite(Transform markerTransform){

		Sprite updatedSprite = null;

		if(markerTransform.position.y > (compassCamera.transform.position.y + verticalMarkerDistanceThreshold)){
			updatedSprite = compassMarkerUpArrow;
		}else if(markerTransform.position.y < (compassCamera.transform.position.y - verticalMarkerDistanceThreshold)){
			updatedSprite = compassMarkerDownArrow;
		}

		return updatedSprite;

	}


	// Calculates and returns distance between compass and specified transform
	// If distance if outside bounds specified, Mathf.INFINITY is returned, in which
	// case, the text should be hidden as it is not inside the valid range
	public float getUpdatedDistanceToCompass(Transform markerTransform){

		float compassDistance = Vector3.Distance(compassCamera.transform.position, markerTransform.position);

		if(compassDistance < minMarkerDistance || compassDistance > maxMarkerDistance){
			compassDistance = Mathf.Infinity;
		}

		return compassDistance;

	}

}
