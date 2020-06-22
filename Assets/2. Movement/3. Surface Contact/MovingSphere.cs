using UnityEngine;

namespace SurfaceContract
{
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 1;
		[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;
		[SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;
		[SerializeField, Min(0f)] private float probeDistance = 1f;
		[SerializeField] private LayerMask probeMask = -1;

		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private int groundContactCount;
		private bool OnGround => groundContactCount > 0;
		private int jumpPhase;
		private float minGroundDotProduct;
		private Vector3 contactNormal;
		private int stepsSinceLastGrounded, stepsSinceLastJump;

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

			GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white);
		}

		private void OnCollisionEnter(Collision collision) {
			EvaluateCollision(collision);
		}

		private void OnCollisionStay(Collision collision) {
			EvaluateCollision(collision);
		}

		private void EvaluateCollision(Collision collision) {
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
			stepsSinceLastGrounded++;
			stepsSinceLastJump++;

			velocity = body.velocity;
			if (OnGround || SnapToGround()) {
				stepsSinceLastGrounded = 0;
				jumpPhase = 0;
				if (groundContactCount > 1) {
					contactNormal.Normalize();
				}
			}
			else {
				contactNormal = Vector3.up;
			}
		}

		private bool SnapToGround() {
			if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
				return false;
			}

			float speed = velocity.magnitude;
			if (speed > maxSnapSpeed) {
				return false;
			}

			if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit,
				probeDistance, probeMask)) {
				return false;
			}

			if (hit.normal.y < minGroundDotProduct) {
				return false;
			}

			groundContactCount = 1;
			contactNormal = hit.normal;

			float dot = Vector3.Dot(velocity, hit.normal);
			if (dot > 0f) {
				velocity = (velocity - hit.normal * dot).normalized * speed;
			}

			return true;
		}

		private void Jump() {
			if (OnGround || jumpPhase < maxAirJumps) {
				stepsSinceLastJump = 0;
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