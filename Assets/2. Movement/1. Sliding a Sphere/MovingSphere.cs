using UnityEngine;

public class MovingSphere : MonoBehaviour
{
	[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
	[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
	private Vector3 velocity = Vector3.zero;

	private void Update() {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput.Normalize();

		Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		float maxSpeedChange = maxAcceleration * Time.deltaTime;

		velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

		Vector3 displacement = velocity * Time.deltaTime;
		transform.localPosition += displacement;
	}
}