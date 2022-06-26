using UnityEngine;
using System.Collections;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoMonsterScript : MonoBehaviour {

	private Vector3 startPos = Vector3.zero;
	private Vector3 minPos = Vector3.zero;
	private Vector3 maxPos = Vector3.zero;
	private float moveSpeed = 0.25f;

	void Update(){

		// Basic "patrol" for the sake of demonstration
		transform.position = Vector3.Lerp(minPos, maxPos, Mathf.PingPong(Time.time*moveSpeed, 0.5f));

	}

	// Set patrol bounds, randomly patrol in either x or z
	public void setPatrolBounds(float newRange){

		startPos = transform.position;
		minPos = startPos;
		maxPos = startPos;

		if(Random.Range(0,10) < 5){
			minPos.z -= newRange;
			maxPos.z += newRange;
		}else{
			minPos.x -= newRange;
			maxPos.x += newRange;
		}

	}

}
