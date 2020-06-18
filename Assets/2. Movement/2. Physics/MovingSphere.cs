using UnityEngine;

namespace PhysicsTutorial
{
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 1;

		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private bool onGround;
		private int jumpPhase;

		private void Awake() {
			body = GetComponent<Rigidbody>();
		}

		private void Update() {
			Vector2 playerInput;
			playerInput.x = Input.GetAxis("Horizontal");
			playerInput.y = Input.GetAxis("Vertical");
			playerInput.Normalize();

			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

			desiredJump |= Input.GetButtonDown("Jump");
		}

		private void OnCollisionEnter(Collision collision) {
			EvaluateCollision(collision);
		}

		private void OnCollisionStay(Collision collision) {
			EvaluateCollision(collision);
		}

		private void EvaluateCollision(Collision collision) {
			// 노멀이 직각일때, 평지에 서있다고 가정함.
			// 평면이 수평이면 법선이 위로 향하게되므로 Y성분은 정확히 1이어야합니다.이 경우에는지면에 닿아 있습니다. 
			// 그러나 여유있게 0.9 이상의 Y 구성 요소를 수용합니다.
			for (int i = 0; i < collision.contactCount; i++) {
				Vector3 normal = collision.GetContact(i).normal;
				onGround |= normal.y >= 0.9f;
			}
		}

		private void FixedUpdate() {
			UpdateState();

			float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
			float maxSpeedChange = acceleration * Time.deltaTime;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

			if (desiredJump) {
				desiredJump = false;
				Jump();
			}

			body.velocity = velocity;

			onGround = false;
		}

		void UpdateState() {
			velocity = body.velocity;
			if (onGround) {
				jumpPhase = 0;
			}
		}

		private void Jump() {
			if (onGround || jumpPhase < maxAirJumps) {
				jumpPhase++;
				float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
				if (velocity.y > 0f) {
					jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
				}
				velocity.y += jumpSpeed;
			}
		}
	}
}