using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Nucleon : MonoBehaviour
{
	public float attractionForce = 10f;
	private Rigidbody body;

	private void Awake() {
		body = GetComponent<Rigidbody>();
	}

	private void FixedUpdate() {
		body.AddForce(transform.localPosition * -attractionForce);
	}
}