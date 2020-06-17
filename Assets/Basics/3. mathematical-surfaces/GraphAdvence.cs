using UnityEngine;
using UnityEngine.Assertions;

public class GraphAdvence : MonoBehaviour
{
	private enum GraphFunctionName : int
	{
		Sine,
		MultiSine,
		Sine2DFunction,
		MultiSine2DFunction,
		Ripple,
		Cylinder
	}

	[SerializeField] private Transform _pointPrefab = null;
	[SerializeField] [Range(10, 100)] private int _resolution = 10;
	[SerializeField] private GraphFunctionName graphName = GraphFunctionName.Sine;

	private Transform[] _points;
	private delegate Vector3 GraphFunction(float u, float v, float t);
	private static GraphFunction[] functions = {
		GetSine, GetMultiSine, Sine2DFunction, MultiSine2DFunction, Ripple,
		Cylinder
	};

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
		float r = 0.8f + Mathf.Sin(Mathf.PI * (6f * u + 2f * v + t)) * 0.2f;
		Vector3 p;
		p.x = r * Mathf.Sin(Mathf.PI * u);
		p.y = v;
		p.z = r * Mathf.Cos(Mathf.PI * u);
		return p;
	}
}