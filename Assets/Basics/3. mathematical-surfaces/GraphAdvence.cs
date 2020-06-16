using UnityEngine;
using UnityEngine.Assertions;

public class GraphAdvence : MonoBehaviour
{
	private enum GraphFunctionName : int
	{
		Sine,
		MultiSine,
		Sine2DFunction,
		MultiSine2DFunction
	}

	[SerializeField] private Transform _pointPrefab = null;
	[SerializeField] [Range(10, 100)] private int _resolution = 10;
	[SerializeField] private GraphFunctionName graphName = GraphFunctionName.Sine;

	private Transform[] _points;
	private delegate float GraphFunction(float x, float z, float t);
	private static GraphFunction[] functions = {
		GetSine, GetMultiSine, Sine2DFunction, MultiSine2DFunction
	};

	private void Awake() {
		Assert.IsNotNull(_pointPrefab);

		var maxStep = 2f / _resolution;
		var initScale = Vector3.one * maxStep;
		var initPosition = Vector3.zero;

		_points = new Transform[_resolution * _resolution];

		for (int i = 0, z = 0; z < _resolution; z++) {

			initPosition.z = (z + 0.5f) * maxStep - 1f;

			for (int x = 0; x < _resolution; x++, i++) {
				var point = Instantiate(_pointPrefab);

				initPosition.x = (x + 0.5f) * maxStep - 1f;

				point.localPosition = initPosition;
				point.localScale = initScale;
				point.SetParent(transform, false);
				_points[i] = point;
			}
		}
	}

	private void Update() {
		float t = Time.time;
		GraphFunction graphFunction = functions[(int)graphName];

		for (int i = 0; i < _points.Length; i++) {
			var point = _points[i];
			var position = point.localPosition;
			position.y = graphFunction(position.x, position.z, t);
			point.localPosition = position;
		}
	}

	private static float GetSine(float x, float z, float t) {
		return Mathf.Sin(Mathf.PI * (x + t));
	}

	private static float GetMultiSine(float x, float z, float t) {
		float y = GetSine(x, z, t);

		//사인파에 더 많은 복잡성을 추가하는 가장 간단한 방법은 주파수가 두 배인 다른 파형을 추가하는 것입니다.이는 사인 함수의 인수에 2를 곱하여 수행되는 것보다 두 배 빠르게 변경됨을 의미합니다.동시에이 함수의 결과는 절반으로 줄어 듭니다. 이는 사인파의 모양을 절반 크기로 동일하게 유지합니다
		y += Mathf.Sin(2f * Mathf.PI * (x + 2f * t)) / 2f;

		//함수의 양과 음의 극단이 모두 1과 - 1이므로이 새로운 함수의 최대 값과 최소값은 1.5와 - 1.5입니다. 
		//−1–1 범위에 머 무르려면 전체를 1.5로 나누어야합니다
		y *= 2f / 3f;
		return y;
	}

	static float Sine2DFunction(float x, float z, float t) {
		float y = Mathf.Sin(Mathf.PI * (x + t));
		y += Mathf.Sin(Mathf.PI * (z + t));
		y *= 0.5f;
		return y;
	}

	static float MultiSine2DFunction(float x, float z, float t) {
		float y = 4f * Mathf.Sin(Mathf.PI * (x + z + t * 0.5f));
		y += Mathf.Sin(Mathf.PI * (x + t));
		y += Mathf.Sin(2f * Mathf.PI * (z + 2f * t)) * 0.5f;
		y *= 1f / 5.5f;
		return y;
	}
}