using UnityEngine;
using System.Collections;

[System.Serializable]
public class SparkyInfo {

	public SparkyInfo()
	{
	}

	public SparkyInfo(SparkyInfo src) {
		this.width = src.width;
		this.taper = src.taper;
		this.trailColor = src.trailColor;
		this.lineMaterial = src.lineMaterial;
	}

	public float width = 0.1f;

	[Range(-1f,1f)]
	[Tooltip("0 = no taper, 1 = taper to zero tail, -1 = taper to zero tip")]
	public float taper = 0.0f;

	[Tooltip("Left is tail, right is tip.")]
	public Gradient trailColor;

	public Material lineMaterial;
}

public class SparkySpark : MonoBehaviour {

	public SparkyInfo info;

	private float lifetime;
	private Vector2 velocity;
	private float lengthPerVel = 0.5f;
	private LineRenderer lr;
	private Vector2 acceleration;
	private float drag;
	private float curSize = 0.0f;
	private float appearTime = 0.0f;
	private float disappearTime = 0.0f;

	// Copy the instance values into the line renderer.
	//
	// TODO: we can probably just create the line renderer here.
	//
	public static LineRenderer InitLineRenderer(GameObject onObj, SparkyInfo info)
	{
		LineRenderer lr = onObj.AddComponent<LineRenderer> ();

		if (lr == null) {
			Debug.LogError ("LineRenderer already exists on SparkyKit object: " + onObj.name);
			lr = onObj.GetComponent<LineRenderer> ();
		}

		lr.positionCount = 2;
		lr.colorGradient = info.trailColor;

		if (info.lineMaterial == null) {
			Debug.LogError ("No material provided for SparkyKit object: " + onObj);
		} else {
			lr.material = new Material (info.lineMaterial);
		}

		lr.startWidth = info.width;
		lr.endWidth = info.width;

		// TODO: These are backwards.  Fix it!
		if (info.taper > 0) {
			lr.startWidth = lr.endWidth * (1f - info.taper);
		} else if (info.taper < 0) {
			lr.endWidth = lr.startWidth * (1f + info.taper);
		}

		return lr;
	}

	public void SetupSpark(Vector2 vel, float life_sec, Vector2 accelVec, float linearDrag, float _appearTime, float _disappearTime, float lengthMult) {
		lr = InitLineRenderer (gameObject, info);
		velocity = vel;
		acceleration = accelVec;
		lifetime = life_sec;
		drag = linearDrag;
		lengthPerVel = lengthMult;

		// If appear time is zero, start at full size.
		disappearTime = _disappearTime;
		appearTime = _appearTime;
		if (appearTime <= 0.0f) {
			curSize = 1.0f;
		}

		UpdateLineRenderer ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		lifetime -= Time.deltaTime;

		Vector3 movement = velocity * Time.deltaTime;
		transform.position = transform.position + movement;

		UpdateLineRenderer ();

		float dragThisFrame = Mathf.Min (drag * Time.deltaTime, 1.0f);
		velocity = (velocity * (1.0f-(dragThisFrame))) + (acceleration * Time.deltaTime);

		// Start at size zero and by appearTime seconds, it should be full size.
		if (IsDisappearing()) {
			ProcessDisappearance ();
		} else if (IsAppearing()) {
			ProcessAppearance ();
		}
	}

	bool IsAppearing() {
		return lifetime > 0.0f && curSize < 1.0f;
	}

	bool IsDisappearing() {
		return lifetime <= 0.0f;
	}

	void ProcessAppearance() {
		curSize = Mathf.Min(1.0f, curSize + ((1.0f / appearTime) * Time.deltaTime));
	}

	// Lifetime is up--start disappearing, or just destroy yourself.
	//
	void ProcessDisappearance() {
		if (disappearTime == 0.0f || curSize <= 0.0f) {
			GameObject.Destroy (gameObject);
			return;
		}

		curSize = Mathf.Max(0.0f, curSize - ((1.0f / disappearTime) * Time.deltaTime));
	}

	void UpdateLineRenderer() {
		Vector3 start;
		Vector3 end;

		// When disappearing, treat the endpoint as the anchor, so the start point looks like a tail.
		if (IsDisappearing ()) {
			end = transform.position + (Vector3)(lengthPerVel * velocity);
			start = Vector3.Lerp (end, transform.position, curSize);
		} else {
			start = transform.position;
			end = transform.position + (Vector3)(lengthPerVel * velocity * curSize);
		}

		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
	}
}
