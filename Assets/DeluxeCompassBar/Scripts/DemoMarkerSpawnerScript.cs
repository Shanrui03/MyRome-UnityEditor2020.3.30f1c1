using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoMarkerSpawnerScript : MonoBehaviour {

	public GameObject monster;
	public GameObject generic;
	public GameObject important;
	private GameObject theCompass;
	private GameObject compassCamera;
	private GameObject worldPoles;
	private BoxCollider[] spawnZones;
	private bool showHelp = true;
	private List<GameObject> spawnedObjects;
	private GUIStyle debugFontStyle;
	// Ignore this
	public bool screenshotMode = false;

	void Start(){
	
		theCompass = GameObject.FindGameObjectWithTag("TheCompass");
		if(!theCompass){
			Debug.LogError("Cannot Find TheCompass!");
		}

		compassCamera = GameObject.FindGameObjectWithTag("CompassCamera");
		if(!compassCamera){
			Debug.LogError("Cannot Find Compass Camera!");
		}

		worldPoles = GameObject.FindGameObjectWithTag("CompassWorldPoles");
		if(!worldPoles){
			Debug.LogError("Cannot Find CompassWorldPoles!");
		}

		spawnZones = gameObject.GetComponentsInChildren<BoxCollider>();

		spawnedObjects = new List<GameObject>();

		debugFontStyle = new GUIStyle();
		debugFontStyle.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		debugFontStyle.fontSize = 14;

	}
	
	void Update(){

		// Spawn a Generic Marker //
		// Note: The GenericMarker prefab already has CompassMarkerScript attached and configured, so
		// we don't need to do any special configuratino here. It's worth noting that since this kind
		// of Compass Marker is configured separately, we can optionally ignore overall theme/skin rules
		// set in The Compass. This is good for super special things that should stand out.
		// See Important Marker below for example on how to dynamically add/configure/initialize a Compass Marker.
		if(Input.GetKeyUp(KeyCode.G)){

			GameObject tempGeneric = GameObject.Instantiate(generic) as GameObject;
			Vector3 genericPosition = Vector3.zero;
			BoxCollider bc = spawnZones[Random.Range(0,spawnZones.Length)];
			genericPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			genericPosition.y = bc.bounds.center.y;
			genericPosition.z = bc.bounds.center.z + Random.Range(-bc.bounds.extents.z,bc.bounds.extents.z);
			tempGeneric.transform.position = genericPosition;
			spawnedObjects.Add(tempGeneric);
			
		}

		// Spawn an Important Marker //
		if(Input.GetKeyUp(KeyCode.I)){

			GameObject tempImportant = GameObject.Instantiate(important) as GameObject;
			Vector3 importantPosition = Vector3.zero;
			BoxCollider bc = spawnZones[Random.Range(0,spawnZones.Length)];
			importantPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			importantPosition.y = bc.bounds.center.y;
			importantPosition.z = bc.bounds.center.z;
			tempImportant.transform.position = importantPosition;
			spawnedObjects.Add(tempImportant);

			// Attach a Compass Marker script to the new Important Game Object so we can track it
			tempImportant.AddComponent<CompassMarkerScript>();

			if(tempImportant.GetComponent<CompassMarkerScript>()){

				// Set our icon type to be the important icon from our theme/skin
				tempImportant.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eImportant);
				// Here we yield to the theme/skin settings in The Compass
				tempImportant.GetComponent<CompassMarkerScript>().showUpDownArrow = theCompass.GetComponent<TheCompassScript>().drawUpDownArrows;
				tempImportant.GetComponent<CompassMarkerScript>().showDistanceLabel = theCompass.GetComponent<TheCompassScript>().drawDistanceText;
				// For important markers, I always want 3D overlays.
				tempImportant.GetComponent<CompassMarkerScript>().show3DOverlay = true;
				tempImportant.GetComponent<CompassMarkerScript>().vertical3DOverlayOffset = 3.2f;
				// Finally, initialize the Compass Marker so tracking of our new object begins
				tempImportant.GetComponent<CompassMarkerScript>().initTrackingOfMarker();
				
			}

		}


		// Spawn a Monster //
		// See Important marker above for how we configure a Compass Marker
		if(Input.GetKeyUp(KeyCode.M)){

			GameObject tempMonster = GameObject.Instantiate(monster) as GameObject;
			Vector3 monsterPosition = Vector3.zero;
			BoxCollider bc = spawnZones[Random.Range(0,spawnZones.Length)];
			monsterPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			monsterPosition.y = bc.bounds.center.y;
			monsterPosition.z = bc.bounds.center.z;
			tempMonster.transform.position = monsterPosition;
			tempMonster.GetComponent<DemoMonsterScript>().setPatrolBounds(Mathf.Min(bc.bounds.extents.z,bc.bounds.extents.x));
			spawnedObjects.Add(tempMonster);

			tempMonster.AddComponent<CompassMarkerScript>();
			if(tempMonster.GetComponent<CompassMarkerScript>()){
				// Note that we assign the Monster icon instead of the Important icon
				tempMonster.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eMonster);
				tempMonster.GetComponent<CompassMarkerScript>().showUpDownArrow = theCompass.GetComponent<TheCompassScript>().drawUpDownArrows;
				tempMonster.GetComponent<CompassMarkerScript>().showDistanceLabel = theCompass.GetComponent<TheCompassScript>().drawDistanceText;
				tempMonster.GetComponent<CompassMarkerScript>().show3DOverlay = true;
				tempMonster.GetComponent<CompassMarkerScript>().vertical3DOverlayOffset = 2.2f;
				tempMonster.GetComponent<CompassMarkerScript>().initTrackingOfMarker();
			}
			
		}

		// Toggle 3D Overlay on nearest Compass Marker //
		if(Input.GetKeyUp(KeyCode.O)){
			
			if(spawnedObjects.Count > 0){
				
				float distanceToNearestMarker = Mathf.Infinity;
				GameObject closestMarker = null;
				
				foreach(GameObject cm in spawnedObjects){

					if(cm.GetComponent<CompassMarkerScript>()){

						float tempDistance = Vector3.Distance(compassCamera.transform.position, cm.transform.position);

						if(tempDistance < distanceToNearestMarker){
							distanceToNearestMarker = tempDistance;
							closestMarker = cm;
						}

					}

				}
				
				if(closestMarker){
					closestMarker.GetComponent<CompassMarkerScript>().vertical3DOverlayOffset = 2.5f;
					closestMarker.GetComponent<CompassMarkerScript>().toggle3DOverlay();
				}
				
			}

		}

		// Randomize the orientation of the world poles to demonstrate that they can be arbitrary //
		if(Input.GetKeyUp(KeyCode.P)){
			worldPoles.transform.rotation = Quaternion.Euler(new Vector3(0.0f,Random.Range(0.0f,360.0f),0.0f));
		}

		// To stop tracking nearest Compass Marker, just remove CompassMarkerScript from the game object//
		if(Input.GetKeyUp(KeyCode.R)){

			if(spawnedObjects.Count > 0){
				
				float distanceToNearestMarker = Mathf.Infinity;
				GameObject closestMarker = null;
				
				foreach(GameObject cm in spawnedObjects){
					
					if(cm.GetComponent<CompassMarkerScript>()){
						
						float tempDistance = Vector3.Distance(compassCamera.transform.position, cm.transform.position);
						
						if(tempDistance < distanceToNearestMarker){
							distanceToNearestMarker = tempDistance;
							closestMarker = cm;
						}
						
					}
					
				}
				
				if(closestMarker){
					Destroy(closestMarker.GetComponent<CompassMarkerScript>());
				}
				
			}
			

		}

		// Stop tracking nearest Compass Marker, and remove associated Game Object from world //
		if(Input.GetKeyUp(KeyCode.T)){

			if(spawnedObjects.Count > 0){
				
				float distanceToNearestMarker = Mathf.Infinity;
				GameObject closestMarker = null;
				
				foreach(GameObject cm in spawnedObjects){
					
					if(cm.GetComponent<CompassMarkerScript>()){
						
						float tempDistance = Vector3.Distance(compassCamera.transform.position, cm.transform.position);
						
						if(tempDistance < distanceToNearestMarker){
							distanceToNearestMarker = tempDistance;
							closestMarker = cm;
						}
						
					}
					
				}
				
				if(closestMarker){
					spawnedObjects.Remove(closestMarker); // demo overhead, ignore
					GameObject.Destroy(closestMarker);
				}
				
			}



		}

		// Clear Scene of all Markers, Generic, Important, and Monsters //
		if(Input.GetKeyUp(KeyCode.C)){

			// Just destroy the game objects, Compass Marker stuff is handled automatically
			foreach(GameObject g in spawnedObjects){
				GameObject.Destroy(g);
			}

			spawnedObjects.Clear(); // demo overhead, ignore

		}

		if(Input.GetKeyUp(KeyCode.H)){
			showHelp = !showHelp;
		}

		if(Input.GetKeyUp(KeyCode.Escape)){
			Application.Quit();
		}

		// For making screenshots only, you can ignore this completely if you wish :)
		if(Input.GetKeyUp(KeyCode.Y) && screenshotMode){

			foreach(GameObject g in spawnedObjects){
				GameObject.Destroy(g);
			}
			spawnedObjects.Clear();

			showHelp = false;

			GameObject tempImportant = GameObject.Instantiate(important) as GameObject;
			Vector3 importantPosition = Vector3.zero;
			BoxCollider bc = spawnZones[0];
			importantPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			importantPosition.y = bc.bounds.center.y;
			importantPosition.z = bc.bounds.center.z;
			tempImportant.transform.position = importantPosition;
			spawnedObjects.Add(tempImportant);
			tempImportant.AddComponent<CompassMarkerScript>();
			if(tempImportant.GetComponent<CompassMarkerScript>()){
				tempImportant.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eImportant);
				tempImportant.GetComponent<CompassMarkerScript>().showUpDownArrow = true;
				tempImportant.GetComponent<CompassMarkerScript>().showDistanceLabel = true;
				tempImportant.GetComponent<CompassMarkerScript>().show3DOverlay = true;
				tempImportant.GetComponent<CompassMarkerScript>().vertical3DOverlayOffset = 3.2f;
				tempImportant.GetComponent<CompassMarkerScript>().initTrackingOfMarker();
			}

			GameObject tempMonster = GameObject.Instantiate(monster) as GameObject;
			Vector3 monsterPosition = Vector3.zero;
			bc = spawnZones[1];
			monsterPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			monsterPosition.y = bc.bounds.center.y;
			monsterPosition.z = bc.bounds.center.z;
			tempMonster.transform.position = monsterPosition;
			tempMonster.GetComponent<DemoMonsterScript>().setPatrolBounds(0.0f);
			spawnedObjects.Add(tempMonster);
			tempMonster.AddComponent<CompassMarkerScript>();
			if(tempMonster.GetComponent<CompassMarkerScript>()){
				tempMonster.GetComponent<CompassMarkerScript>().setMarkerIconType(TheCompassScript.IconType.eMonster);
				tempMonster.GetComponent<CompassMarkerScript>().showUpDownArrow = true;
				tempMonster.GetComponent<CompassMarkerScript>().showDistanceLabel = true;
				tempMonster.GetComponent<CompassMarkerScript>().show3DOverlay = true;
				tempMonster.GetComponent<CompassMarkerScript>().vertical3DOverlayOffset = 2.2f;
				tempMonster.GetComponent<CompassMarkerScript>().initTrackingOfMarker();
			}

			GameObject tempGeneric = GameObject.Instantiate(generic) as GameObject;
			Vector3 genericPosition = Vector3.zero;
			bc = spawnZones[2];
			genericPosition.x = bc.bounds.center.x + Random.Range(-bc.bounds.extents.x,bc.bounds.extents.x);
			genericPosition.y = bc.bounds.center.y;
			genericPosition.z = bc.bounds.center.z + Random.Range(-bc.bounds.extents.z,bc.bounds.extents.z);
			tempGeneric.transform.position = genericPosition;
			spawnedObjects.Add(tempGeneric);

			for(int i = 0; i < spawnZones.Length; i++){
				spawnZones[i].gameObject.GetComponent<Renderer>().enabled = false;
			}

		}

	}

	void OnGUI(){

		debugFontStyle.normal.textColor = Color.yellow;
		GUI.skin.label = debugFontStyle;

		if(!showHelp){

			GUI.Label (new Rect(16,0,400,128),"[H] Show Help");

		}else{

			GUI.Label (new Rect(16,20,400,128),">>>Spawned Object Count:" + spawnedObjects.Count + "<<<");
			GUI.Label (new Rect(16,0,500,128),"[H] Hide Help");
			GUI.Label (new Rect(16,40,500,128),"[G] Spawn Generic Marker");
			GUI.Label (new Rect(16,60,500,128),"[I] Spawn Important Marker");
			GUI.Label (new Rect(16,80,500,128),"[M] Spawn Moving Marker (e.g. monster)");
			GUI.Label (new Rect(16,100,500,128),"[O] Toggle 3D Overlay on nearest Compass Marker");
			GUI.Label (new Rect(16,120,500,128),"[P] Rotate World Poles to random orientation");
			GUI.Label (new Rect(16,140,500,128),"[R] Remove Compass Marker from nearest Spawned Object");
			GUI.Label (new Rect(16,160,500,128),"[T] Remove nearest Spawned Object and Associated Compass Marker");
			GUI.Label (new Rect(16,180,500,128),"[C] Clear all objects from level");

		}

	}

}
