using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
	private const float DEGREE_PER_HOUR = 360f / 12f;
	private const float DEGREE_PER_MIN = 360f / 60f;
	private const float DEGREE_PER_SEC = 360f / 60f;

	[SerializeField] private Transform _hoursTransform = null;
	[SerializeField] private Transform _minutesTransform = null;
	[SerializeField] private Transform _secondsTransform = null;
	[SerializeField] private bool _continuous = false;

	private void Update() {
		if (_continuous) {
			UpdateContinuous();
		}
		else {
			UpdateDiscate();
		}
	}

	private void UpdateContinuous() {
		TimeSpan time = DateTime.Now.TimeOfDay;
		SetArmRotation((float)time.TotalHours, (float)time.TotalMinutes, (float)time.TotalSeconds);
	}

	private void UpdateDiscate() {
		DateTime time = DateTime.Now;
		SetArmRotation(time.Hour, time.Minute, time.Second);
	}

	private void SetArmRotation(float hour, float min, float sec) {
		if (_hoursTransform != null) {
			_hoursTransform.localRotation = Quaternion.Euler(0f, hour * DEGREE_PER_HOUR, 0f);
		}

		if (_minutesTransform != null) {
			_minutesTransform.localRotation = Quaternion.Euler(0f, min * DEGREE_PER_MIN, 0f);
		}

		if (_secondsTransform != null) {
			_secondsTransform.localRotation = Quaternion.Euler(0f, sec * DEGREE_PER_SEC, 0f);
		}
	}
}
