using UnityEngine;

namespace PhysicsTutorial
{
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		private Rigidbody body;
		Vector3 desiredVelocity;

		private void Awake() {
			body = GetComponent<Rigidbody>();
		}

		private void Update() {
			Vector2 playerInput;
			playerInput.x = Input.GetAxis("Horizontal");
			playerInput.y = Input.GetAxis("Vertical");
			playerInput.Normalize();

			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		}

		private void FixedUpdate() {
			float maxSpeedChange = maxAcceleration * Time.deltaTime;

			var velocity = body.velocity;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

			body.velocity = velocity;
		}
	}
}