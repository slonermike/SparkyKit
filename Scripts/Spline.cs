/// <summary>
/// Spline
/// 2017 Slonersoft Games
/// Class to handle simple spline evaluation.
/// 
/// Set control points for the spline, and adjust curvature with the tension scalar.
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slonersoft {
	public class Spline {

		private Vector3[] controlPoints;
		public float tension;

		public Spline() {
			controlPoints = new Vector3[0];
			tension = 0.0f;
		}

		public Spline(Vector3[] points)
		{
			controlPoints = points;
			tension = 0.0f;
		}

		public Vector3 GetTangent(int pointIndex)
		{
			if (pointIndex < 0 || pointIndex >= controlPoints.Length) {
				Debug.LogError ("Tangent point index out of range: " + pointIndex);
				return Vector3.zero;
			}

			if (controlPoints.Length < 2) {
				return Vector3.zero;
			}

			Vector3 prevPoint;
			Vector3 nextPoint;
			if (pointIndex == 0) {
				prevPoint = controlPoints [0] + (controlPoints [0] - controlPoints [1]);
			} else {
				prevPoint = controlPoints [pointIndex - 1];
			}

			if (pointIndex == controlPoints.Length - 1) {
				nextPoint = controlPoints [pointIndex] + (controlPoints [pointIndex] - controlPoints [pointIndex - 1]);
			} else {
				nextPoint = controlPoints [pointIndex + 1];
			}

			Vector3 tangentOut = (nextPoint - prevPoint) * tension;
			return tangentOut;
		}

		private float InterpolateOneDimension(float u, float pk1, float pk2, float dp1, float dp2)
		{
			float u2 = u * u;
			float u3 = u * u2;		//u cubed

			//blending function expanded
			return (((2 * u3) - (3 * u2) + 1) * pk1) +
				(((-2 * u3) + (3 * u2)) * pk2) +
				((u3 - (2 * u2) + u) * dp1) +
				((u3 - u2) * dp2);
		}

		private Vector3 Evaluate(int ptIndex, float t)
		{
			if (controlPoints.Length == 0) {
				return Vector3.zero;
			}

			if (ptIndex >= controlPoints.Length-1) {
				return controlPoints [controlPoints.Length - 1];
			} else if(ptIndex < 0) {
				return controlPoints[0];
			} else {
				Vector3 pk1 = controlPoints[ptIndex];
				Vector3 pk2 = controlPoints[ptIndex+1];
				Vector3 slope1, slope2;

				slope1 = GetTangent(ptIndex);
				slope2 = GetTangent(ptIndex+1);

				float x = InterpolateOneDimension(t, pk1.x, pk2.x, slope1.x, slope2.x);
				float y = InterpolateOneDimension(t, pk1.y, pk2.y, slope1.y, slope2.y);
				float z = InterpolateOneDimension (t, pk1.z, pk2.z, slope1.z, slope2.z);
				return new Vector3 (x, y, z);
			}
		}

		public Vector3 Evaluate(float t)
		{
			t = Mathf.Clamp01 (t);

			float expandedT = t * (float)(controlPoints.Length-1);
			int index = Mathf.FloorToInt(expandedT);
			float u = expandedT - (float)index;

			index = Mathf.Clamp(index, 0, controlPoints.Length-1);
			return Evaluate(index, u);
		}

		public static Spline GeneratePath(Vector3 start, Vector3 finish, int numPasses = 1, float curveSpreadX = 0.5f, float curveSpreadY = 0.5f)
		{
			Vector3[] primaryCurvePoints = new Vector3[2];
			primaryCurvePoints [0] = start;
			primaryCurvePoints [1] = finish;
			Spline curve = new Spline (primaryCurvePoints);

			float length = Vector3.Distance (start, finish);
			if (length == 0f) {
				return curve;
			}

			Vector3 pathNormalized = finish - start;
			pathNormalized /= length;

			float rightDot = Mathf.Abs(Vector3.Dot (pathNormalized, Vector3.right));
			float upDot = Mathf.Abs(Vector3.Dot (pathNormalized, Vector3.up));

			Vector3 rVec = rightDot < upDot ? Vector3.right : Vector3.up;
			Vector3 fVec = Vector3.Cross (pathNormalized, rVec);

			int midPoints = 0;

			for (int i = 0; i < numPasses; i++) {
				midPoints += (midPoints + 1);
				int totalPoints = midPoints + 2;
				Vector3[] newPoints = new Vector3[totalPoints];
				newPoints [0] = start;
				newPoints [newPoints.Length-1] = finish;

				float segmentSize = length / (float)(midPoints + 1);

				Vector2 primaryCurveSpread = new Vector2(segmentSize * curveSpreadX, segmentSize * curveSpreadY);

				for (int j = 1; j < newPoints.Length-1; j++) {
					float pct = j / (float)(newPoints.Length - 1);
					Vector3 centerPoint = curve.Evaluate (pct);
					centerPoint += rVec * Random.Range (-primaryCurveSpread.x, primaryCurveSpread.x);
					centerPoint += fVec * Random.Range (-primaryCurveSpread.y, primaryCurveSpread.y);;
					newPoints [j] = centerPoint;
				}

				curve = new Spline (newPoints);
				curve.tension = 0.5f;
			}

			return curve;
		}
	}
}

