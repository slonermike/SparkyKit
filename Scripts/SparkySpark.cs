using UnityEngine;
using System.Collections;

public class SparkySpark : MonoBehaviour {

	private float lifetime;
	private Vector2 velocity;
	private float lengthPerVel = 0.5f;
	private LineRenderer lr;
	private Vector2 acceleration;
	private float drag;
	private float curSize = 0.0f;
	private float appearTime = 0.0f;
	private float disappearTime = 0.0f;

	public void SetupSpark(Vector2 vel, float life_sec, Vector2 accelVec, float linearDrag, float _appearTime, float _disappearTime, float lengthMult) {
		velocity = vel;
		acceleration = accelVec;
		lifetime = life_sec;
		lr = GetComponent<LineRenderer> ();
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
