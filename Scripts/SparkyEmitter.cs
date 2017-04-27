using UnityEngine;
using System.Collections;

public class SparkyEmitter : MonoBehaviour {

	private float startTime;
	private float nextEmissionTime;

	public LineRenderer[] sparkPrefabs;
	public float emitterLifetime = 0.0f;
	[Range(0.0f, 360.0f)] public float emissionAngle = 360.0f;
	public Vector2 timeBetweenEmissions = new Vector2(0.1f, 0.2f);
	public Vector2 simultaneousEmissions = new Vector2(1.0f, 5.0f);
	public Vector2 sparkLifetime = new Vector2(0.3f, 0.5f);
	public Vector2 sparkAppearTime = new Vector2(0.2f, 0.25f);
	public Vector2 sparkDisappearTime = new Vector2(0.4f, 0.5f);
	public float sparkLengthMultiplier = 1.0f;
	public Vector2 startVelocityRange = new Vector2(1.5f, 2.0f);
	public float drag = 0.1f;
	public Vector2 directionalAcceleration;

	// Use this for initialization
	void Start () {
		nextEmissionTime = Time.time;
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		ProcessEmission ();

		// Disable me after my lifetime has elapsed.
		if (emitterLifetime > 0.0f && Time.time > startTime + emitterLifetime) {
			this.enabled = false;
		}
	}

	void ProcessEmission() {
		if (Time.time < nextEmissionTime) {
			return;
		}

		int numEmissions = Mathf.FloorToInt (Random.Range (simultaneousEmissions.x, simultaneousEmissions.y));
		for (int i = 0; i < numEmissions; i++) {
			CreateSpark ();
		}

		nextEmissionTime = Time.time + Random.Range(timeBetweenEmissions.x, timeBetweenEmissions.y);
	}

	void CreateSpark() {

		float halfAngle = emissionAngle * 0.5f;
		float angle = Random.Range (-halfAngle, halfAngle);
		Vector2 dir = Quaternion.AngleAxis (angle, Vector3.forward) * transform.right;

		if (sparkPrefabs.Length == 0) {
			Debug.LogError ("Spark emitter has no spark prefabs.  Destroying self.");
			GameObject.Destroy (gameObject);
			return;
		}

		GameObject original = sparkPrefabs [Random.Range (0, sparkPrefabs.Length)].gameObject;
		GameObject newObj = Instantiate (original, transform.position, transform.rotation) as GameObject;
		SparkySpark spark = newObj.GetComponent<SparkySpark> ();

		if (spark == null) {
			Debug.LogError ("Spark component missing from created spark.");
			return;
		}

		float myVel = Random.Range (startVelocityRange.x, startVelocityRange.y);
		float myLifetime = Random.Range (sparkLifetime.x, sparkLifetime.y);
		float myAppearTime = Random.Range (sparkAppearTime.x, sparkAppearTime.y);
		float myDisappearTime = Random.Range (sparkDisappearTime.x, sparkDisappearTime.y);
		spark.SetupSpark (dir * myVel, myLifetime, directionalAcceleration, drag, myAppearTime, myDisappearTime, sparkLengthMultiplier);
	}
}
