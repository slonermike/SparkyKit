using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SparkyTrail : MonoBehaviour {

	private LinkedList<Vector3> pointList = new LinkedList<Vector3> ();
	private LineRenderer lr;
	private GameObject parentObj = null;
	private float lastEmissionTime = 0.0f;
	private bool fadingOut = false;

	public SparkyInfo sparkInfo;

	[Tooltip("Time that passes between points being laid.")]
	public float timeBetweenEmissions = 0.05f;

	[Tooltip("Time it takes to fade out once the parent is destroyed.")]
	public float fadeTime = 1.0f;
	
	[Tooltip("Number of total points laid by the trail. 2 = straight line")]
	[Range(2,32)]
	public int numPoints = 2;

	[Tooltip("Number of spline points per point laid by the trail.")]
	[Range(1,16)]
	public int splinePrecision = 1;

	public GameObject endpointObj = null;
	public float idleSpeed = 0f;

	// Use this for initialization
	void Start () {

		// Don't actually stay connected to the parent, as we want to stick around a for a time after it's gone so we can destroy ourself.
		if (transform.parent) {
			parentObj = transform.parent.gameObject;
		}

		transform.SetParent (null);

		lastEmissionTime = Time.time;
		pointList.AddFirst (transform.position);

		lr = SparkySpark.InitLineRenderer (gameObject, sparkInfo);
		lr.transform.SetParent (transform);

		SetPoints ();
	}

	void OnEnable()
	{
		// If we're re-enabling it, get rid of the old points and reposition at the parent object.
		if (parentObj != null) {
			transform.position = parentObj.transform.position;
			pointList.Clear ();
			pointList.AddFirst (transform.position);
		}
	}

	IEnumerator FadeOutAndDie()
	{
		float opacity = 1.0f;

		float startAlpha = lr.startColor.a;
		float endAlpha = lr.endColor.a;

		while (opacity > 0) {
			yield return new WaitForEndOfFrame ();

			for (int i = 0; i < lr.colorGradient.alphaKeys.Length; i++) {
				lr.startColor = new Color(lr.startColor.r, lr.startColor.g, lr.startColor.b, opacity * startAlpha);
				lr.endColor = new Color(lr.endColor.r, lr.endColor.g, lr.endColor.b, opacity * endAlpha);
			}

			opacity -= (1.0f / fadeTime) * Time.deltaTime;
		}

		Destroy (gameObject);
	}

	void FixedUpdate() {

		bool addPoint = true;
		// Fill the list before timing them out.
		if (pointList.Count < numPoints) {
			float timePassed = Time.time - lastEmissionTime;
			if (timePassed < timeBetweenEmissions) {
				addPoint = false;
			}
		}

		if (parentObj != null) {
			transform.position = parentObj.transform.position;
		}

		SetPoints ();

		// Add the point after because setpoints will use the current position anyway.
		if (addPoint) {
			AddPoint ();
			lastEmissionTime = Time.time;
		}

		if (parentObj == null && !fadingOut) {
			fadingOut = true;
			StartCoroutine (FadeOutAndDie ());
		}
	}

	void AddPoint()
	{
		pointList.AddLast (transform.position);

		if (pointList.Count > numPoints) {
			pointList.RemoveFirst ();
		}
	}

	void SetPoints()
	{
		if (pointList.Count == 0) {
			Debug.LogError ("pointList should never have zero points!");
			return;
		}

		if (pointList.Count == 1) {
			lr.positionCount = 2;
			lr.SetPosition (0, pointList.First.Value);
			lr.SetPosition (1, transform.position);
		}

		lr.enabled = pointList.Count > 1;
		if (!lr.enabled) {
			return;
		}

		Vector3[] pList = new Vector3[pointList.Count + 1];
		pList [pointList.Count] = transform.position;
		Vector3 prevPos = transform.position;

		LinkedListNode<Vector3> lln = pointList.First;
		int index = 0;
		do {
			if (parentObj != null) {
				lln.Value += parentObj.transform.forward.normalized * idleSpeed;
			}

			pList [index] = lln.Value;
			prevPos = lln.Value;
			index++;
			lln = lln.Next;
		} while (lln != null);

		if (splinePrecision <= 1) {
			lr.positionCount = pList.Length;
			lr.SetPositions (pList);
		} else {
			Slonersoft.Spline sp = new Slonersoft.Spline (pList);
			sp.tension = 0.4f;

			int totalPoints = (splinePrecision * pList.Length) + 1;
			lr.positionCount = totalPoints;
			for (int i = 0; i < totalPoints; i++) {
				float t = (float)i / (float)(totalPoints - 1);
				Vector3 pos = sp.Evaluate (t);
				lr.SetPosition (i, pos);
			}
		}
	}
}
