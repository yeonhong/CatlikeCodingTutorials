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

		[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;

		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private int groundContactCount;
		private bool OnGround => groundContactCount > 0;
		private int jumpPhase;
		private float minGroundDotProduct;
		private Vector3 contactNormal;

		private void OnValidate() {
			minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		}

		private void Awake() {
			body = GetComponent<Rigidbody>();
			OnValidate();
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
				if (normal.y >= minGroundDotProduct) {
					groundContactCount += 1;
					contactNormal += normal;
				}
			}
		}

		private void FixedUpdate() {
			UpdateState();
			AdjustVelocity();

			if (desiredJump) {
				desiredJump = false;
				Jump();
			}

			body.velocity = velocity;

			ClearState();
		}

		private void ClearState() {
			groundContactCount = 0;
			contactNormal = Vector3.zero;
		}

		private void UpdateState() {
			velocity = body.velocity;
			if (OnGround) {
				jumpPhase = 0;
				if (groundContactCount > 1) {
					contactNormal.Normalize();
				}
			}
			else {
				contactNormal = Vector3.up;
			}
		}

		private void Jump() {
			if (OnGround || jumpPhase < maxAirJumps) {
				jumpPhase++;
				float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
				float alignedSpeed = Vector3.Dot(velocity, contactNormal);
				if (alignedSpeed > 0f) {
					jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
				}
				velocity += contactNormal * jumpSpeed;
			}
		}

		private Vector3 ProjectOnContactPlane(Vector3 vector) {
			return vector - contactNormal * Vector3.Dot(vector, contactNormal);
		}

		private void AdjustVelocity() {
			Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
			Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

			float currentX = Vector3.Dot(velocity, xAxis);
			float currentZ = Vector3.Dot(velocity, zAxis);

			float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
			float maxSpeedChange = acceleration * Time.deltaTime;

			float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
			float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

			velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		}
	}
}