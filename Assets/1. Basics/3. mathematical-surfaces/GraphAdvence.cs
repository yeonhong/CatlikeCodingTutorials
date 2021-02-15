using System;
using UnityEngine;
using UnityEngine.Assertions;

public class GraphAdvence : MonoBehaviour
{
	public enum GraphFunctionName : int
	{
		Sine,
		MultiSine,
		Sine2DFunction,
		MultiSine2DFunction,
		Ripple,
		Cylinder,
		Sphere,
		Torus
	}

	[SerializeField] private Transform _pointPrefab = null;
	[SerializeField] [Range(10, 100)] private int _resolution = 10;
	[SerializeField] private GraphFunctionName graphName = GraphFunctionName.Sine;

	private Transform[] _points;
	private float duration;
	private bool transitioning;
	private GraphFunctionName transitionFunction;

	private delegate Vector3 GraphFunction(float u, float v, float t);
	private static GraphFunction[] functions = {
		GetSine, GetMultiSine, Sine2DFunction, MultiSine2DFunction, Ripple,
		Cylinder, Sphere, Torus,
	};

	public enum TransitionMode { Cycle, Random }

	[SerializeField]
	private TransitionMode transitionMode = TransitionMode.Cycle;

	[SerializeField, Min(0f)]
	private float functionDuration = 1f, transitionDuration = 1f;

	private void Awake() {
		Assert.IsNotNull(_pointPrefab);

		var maxStep = 2f / _resolution;
		var initScale = Vector3.one * maxStep;

		_points = new Transform[_resolution * _resolution];

		for (int i = 0; i < _points.Length; i++) {
			Transform point = Instantiate(_pointPrefab);
			point.localScale = initScale;
			point.SetParent(transform, false);
			_points[i] = point;
		}
	}

	private void Update() {
		duration += Time.deltaTime;
		if (transitioning) {
			if (duration >= transitionDuration) {
				duration -= transitionDuration;
				transitioning = false;
			}
		}
		else if (duration >= functionDuration) {
			duration -= functionDuration;
			transitioning = true;
			transitionFunction = graphName;
			PickNextFunction();
		}

		if (transitioning) {
			UpdateFunctionTransition();
		} else {
			UpdateFunction();
		}
	}

	private void UpdateFunctionTransition() {
		float progress = duration / transitionDuration;

		float t = Time.time;
		GraphFunction f = functions[(int)graphName];
		float step = 2f / _resolution;

		for (int i = 0, z = 0; z < _resolution; z++) {
			float v = (z + 0.5f) * step - 1f;
			for (int x = 0; x < _resolution; x++, i++) {
				float u = (x + 0.5f) * step - 1f;
				_points[i].localPosition = Morph(u, v, t, transitionFunction, graphName, progress);
			}
		}
	}

	private void PickNextFunction() {
		graphName = transitionMode == TransitionMode.Cycle ?
			GetNextFunctionName(graphName) :
			GetRandomFunctionNameOtherThan(graphName);
	}

	public static GraphFunctionName GetNextFunctionName(GraphFunctionName name) {
		return (int)name < functions.Length - 1 ? name + 1 : 0;
	}

	public static GraphFunctionName GetRandomFunctionNameOtherThan(GraphFunctionName name) {
		var choice = (GraphFunctionName)UnityEngine.Random.Range(1, functions.Length);
		return choice == name ? 0 : choice;
	}

	public static Vector3 Morph(float u, float v, float t, GraphFunctionName from, GraphFunctionName to, float progress) {
		return Vector3.LerpUnclamped(functions[(int)from](u, v, t), functions[(int)to](u, v, t), progress);
	}

	private void UpdateFunction() {
		float t = Time.time;
		GraphFunction f = functions[(int)graphName];
		float step = 2f / _resolution;

		for (int i = 0, z = 0; z < _resolution; z++) {
			float v = (z + 0.5f) * step - 1f;
			for (int x = 0; x < _resolution; x++, i++) {
				float u = (x + 0.5f) * step - 1f;
				_points[i].localPosition = f(u, v, t);
			}
		}
	}

	private static Vector3 GetSine(float x, float z, float t) {
		Vector3 ret;
		ret.x = x;
		ret.y = Mathf.Sin(Mathf.PI * (x + t));
		ret.z = z;
		return ret;
	}

	private static Vector3 GetMultiSine(float x, float z, float t) {
		Vector3 ret;
		ret.x = x;
		ret.y = Mathf.Sin(Mathf.PI * (x + t));
		ret.y += Mathf.Sin(2f * Mathf.PI * (x + 2f * t)) / 2f;
		ret.y *= 2f / 3f;
		ret.z = z;
		return ret;
	}

	private static Vector3 Sine2DFunction(float x, float z, float t) {
		Vector3 ret;
		ret.x = x;
		ret.y = Mathf.Sin(Mathf.PI * (x + t));
		ret.y += Mathf.Sin(Mathf.PI * (z + t));
		ret.y *= 0.5f;
		ret.z = z;
		return ret;
	}

	private static Vector3 MultiSine2DFunction(float x, float z, float t) {
		Vector3 ret;
		ret.x = x;
		ret.y = 4f * Mathf.Sin(Mathf.PI * (x + z + t * 0.5f));
		ret.y += Mathf.Sin(Mathf.PI * (x + t));
		ret.y += Mathf.Sin(2f * Mathf.PI * (z + 2f * t)) * 0.5f;
		ret.y *= 1f / 5.5f;
		ret.z = z;
		return ret;
	}

	private static Vector3 Ripple(float x, float z, float t) {
		Vector3 ret;
		ret.x = x;
		float distance = Mathf.Sqrt(x * x + z * z);
		ret.y = Mathf.Sin(Mathf.PI * (4f * distance - t));
		ret.y /= 1f + 10f * distance;
		ret.z = z;
		return ret;
	}

	private static Vector3 Cylinder(float u, float v, float t) {
		//float r = 1f + Mathf.Sin(6f * Mathf.PI * u) * 0.2f;
		//float r = 1f + Mathf.Sin(2f * Mathf.PI * v) * 0.2f;
		float radius = 0.8f + Mathf.Sin(Mathf.PI * (6f * u + 2f * v + t)) * 0.2f;
		Vector3 p;
		p.x = radius * Mathf.Sin(Mathf.PI * u);
		p.y = v;
		p.z = radius * Mathf.Cos(Mathf.PI * u);
		return p;
	}

	private static Vector3 SphereOrigin(float u, float v, float t) {
		Vector3 p;
		float s = Mathf.Cos(Mathf.PI * 0.5f * v);
		p.x = s * Mathf.Sin(Mathf.PI * u);
		p.y = Mathf.Sin(Mathf.PI * 0.5f * v);
		p.z = s * Mathf.Cos(Mathf.PI * u);
		return p;
	}

	private static Vector3 Sphere(float u, float v, float t) {
		Vector3 p;
		float r = 0.8f + Mathf.Sin(Mathf.PI * (6f * u + t)) * 0.1f;
		r += Mathf.Sin(Mathf.PI * (4f * v + t)) * 0.1f;
		float s = r * Mathf.Cos(Mathf.PI * 0.5f * v);
		p.x = s * Mathf.Sin(Mathf.PI * u);
		p.y = r * Mathf.Sin(Mathf.PI * 0.5f * v);
		p.z = s * Mathf.Cos(Mathf.PI * u);
		return p;
	}

	private static Vector3 Torus(float u, float v, float t) {
		Vector3 p;
		//float r1 = 1f;
		//float r2 = 0.5f;
		float r1 = 0.65f + Mathf.Sin(Mathf.PI * (6f * u + t)) * 0.1f;
		float r2 = 0.2f + Mathf.Sin(Mathf.PI * (4f * v + t)) * 0.05f;
		float s = r2 * Mathf.Cos(Mathf.PI * v) + r1;
		p.x = s * Mathf.Sin(Mathf.PI * u);
		p.y = r2 * Mathf.Sin(Mathf.PI * v);
		p.z = s * Mathf.Cos(Mathf.PI * u);
		return p;
	}
}