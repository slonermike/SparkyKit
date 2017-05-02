﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SparkyTrail : MonoBehaviour {

	private Vector3[] trailPoints;
	private LineRenderer lr;
	private int frontPoint = 0;
	private GameObject parentObj = null;
	private float lastEmissionTime = 0.0f;
	private bool fadingOut = false;

	public SparkyInfo sparkInfo;
	public int numPoints = 10;
	public float timeBetweenEmissions = 0.05f;
	public float fadeTime = 1.0f;
	public GameObject endpointObj = null;

	// Use this for initialization
	void Start () {

		// Don't actually stay connected to the parent, as we want to stick around a for a time after it's gone so we can destroy ourself.
		if (transform.parent) {
			parentObj = transform.parent.gameObject;
		}

		transform.SetParent (null);

		lastEmissionTime = Time.time;
		trailPoints = new Vector3[numPoints];
		for (int i = 0; i < trailPoints.Length; i++) {
			trailPoints [i] = transform.position;
		}
		numPoints = 2;

		lr = SparkySpark.InitLineRenderer (gameObject, sparkInfo);
		lr.transform.SetParent (transform);

		SetPoints ();
	}

	IEnumerator Main()
	{
		yield return new WaitForSeconds (timeBetweenEmissions);

		while (true) {
			AddPoint ();

			while (lastEmissionTime + timeBetweenEmissions > Time.time) {
				SetPoints ();
				yield return new WaitForFixedUpdate ();
			}
		}
	}

	IEnumerator FadeOutAndDie()
	{
		float opacity = 1.0f;

		float[] alphas = new float[lr.colorGradient.alphaKeys.Length];
		for (int i = 0; i > lr.colorGradient.alphaKeys.Length; i++) {
			alphas [i] = lr.colorGradient.alphaKeys [i].alpha;
		}

		while (opacity > 0) {
			yield return new WaitForFixedUpdate ();
			opacity -= (1.0f / fadeTime) * Time.deltaTime;

			for (int i = 0; i > lr.colorGradient.alphaKeys.Length; i++) {
				lr.colorGradient.alphaKeys [i].alpha = alphas [i] * opacity;
			}
		}
		Destroy (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (parentObj != null) {
			transform.position = parentObj.transform.position;
		} else if (!fadingOut) {
			fadingOut = true;
			StartCoroutine (FadeOutAndDie ());
		}
	}

	void LateUpdate() {
		if (parentObj != null) {
			transform.position = parentObj.transform.position;
			// Update the front point.
			lr.SetPosition (numPoints, transform.position);
		}
	}

	void AddPoint()
	{
		lastEmissionTime = Time.time;
		frontPoint = ((frontPoint - 1) + trailPoints.Length) % trailPoints.Length;
		trailPoints [frontPoint] = transform.position;
		if (numPoints < trailPoints.Length) {
			numPoints++;
		}
	}

	void SetPoints()
	{
		float pct = Mathf.Clamp01((Time.time - lastEmissionTime) / timeBetweenEmissions);
		Vector3[] pList = new Vector3[numPoints + 1];
		pList [numPoints] = transform.position;
		Vector3 prevPos = transform.position;

		for (int i = 0; i < numPoints; i++) {
			int srcIndex = (frontPoint + i) % trailPoints.Length;
			int dstIndex = numPoints - i - 1;

			if (numPoints < trailPoints.Length) {
				pList [dstIndex] = trailPoints [srcIndex];
			} else {
				pList [dstIndex] = Vector3.Lerp (trailPoints [srcIndex], prevPos, pct);
			}

			prevPos = trailPoints [srcIndex];
		}

		lr.positionCount = numPoints + 1;
		lr.SetPositions (pList);
	}

	void UpdateFirstAndLast()
	{
		if (numPoints < 1) {
			return;
		}

		// Update the front point.
		lr.SetPosition (numPoints, transform.position);

		// Update the back point.
		float pct = Mathf.Clamp01((Time.time - lastEmissionTime) / timeBetweenEmissions);
		int lastIndex = (frontPoint + numPoints - 1) % trailPoints.Length;
		int secondToLastIndex = (lastIndex - 1 + trailPoints.Length) % trailPoints.Length;
		Vector3 lerped = Vector3.Lerp (trailPoints [lastIndex], trailPoints [secondToLastIndex], pct);
		lr.SetPosition (0, lerped);
		endpointObj.transform.position = lerped;
	}
}
