using UnityEngine;

public class Graph : MonoBehaviour
{
	[SerializeField] private Transform _pointPrefab = null;
	[SerializeField] [Range(10,100)] private int _resolution = 10;

	private Transform[] _points;

	void Awake() {
		float step = 2f / _resolution;
		_points = new Transform[_resolution];
		var scale = Vector3.one * step;
		var position = Vector3.zero;
		for (int i = 0; i < _points.Length; i++) {
			Transform point = Instantiate(_pointPrefab);

			position.x = (i + 0.5f) * step - 1f;

			point.localPosition = position;
			point.localScale = scale;

			point.SetParent(transform, false);
			_points[i] = point;
		}
	}

	private void Update() {
		for (int i = 0; i < _points.Length; i++) {
			var point = _points[i];
			var position = point.localPosition;

			position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));

			point.localPosition = position;
		}
	}
}