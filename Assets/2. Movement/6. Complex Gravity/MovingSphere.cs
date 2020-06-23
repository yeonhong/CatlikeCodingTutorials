﻿using UnityEngine;

namespace ComplexGravityTutorial
{
	public class MovingSphere : MonoBehaviour
	{
		[SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
		[SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;
		[SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
		[SerializeField, Range(0, 5)] private int maxAirJumps = 1;
		[SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f, maxStairsAngle = 50f;
		[SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;
		[SerializeField, Min(0f)] private float probeDistance = 1f;
		[SerializeField] private LayerMask probeMask = -1, stairsMask = -1;
		[SerializeField] private Transform playerInputSpace = default;
		
		private Rigidbody body;
		private Vector3 velocity, desiredVelocity;
		private bool desiredJump;
		private int jumpPhase;
		private float minGroundDotProduct, minStairsDotProduct;
		private Vector3 contactNormal, steepNormal;
		private int groundContactCount, steepContactCount;
		private int stepsSinceLastGrounded, stepsSinceLastJump;

		private bool OnGround => groundContactCount > 0;
		private bool OnSteep => steepContactCount > 0;

		private Vector3 upAxis, rightAxis, forwardAxis;

		private void OnValidate() {
			minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
			minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
		}

		private void Awake() {
			body = GetComponent<Rigidbody>();
			body.useGravity = false;
			OnValidate();
		}

		private void Update() {
			Vector2 playerInput;
			playerInput.x = Input.GetAxis("Horizontal");
			playerInput.y = Input.GetAxis("Vertical");
			playerInput.Normalize();

			if (playerInputSpace) {
				rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
				forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
			}
			else {
				rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
				forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
			}

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
			float minDot = GetMinDot(collision.gameObject.layer);
			for (int i = 0; i < collision.contactCount; i++) {
				Vector3 normal = collision.GetContact(i).normal;
				float upDot = Vector3.Dot(upAxis, normal);
				if (upDot >= minDot) {
					groundContactCount += 1;
					contactNormal += normal;
				}
				else if (upDot > -0.01f) { // 가파른 곳인지
					steepContactCount += 1;
					steepNormal += normal;
				}
			}
		}

		private void FixedUpdate() {
			Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

			UpdateState();
			AdjustVelocity();

			if (desiredJump) {
				desiredJump = false;
				Jump(gravity);
			}

			velocity += gravity * Time.deltaTime;
			body.velocity = velocity;

			ClearState();
		}

		private void ClearState() {
			groundContactCount = steepContactCount = 0;
			contactNormal = steepNormal = Vector3.zero;
		}

		private void UpdateState() {
			stepsSinceLastGrounded++;
			stepsSinceLastJump++;

			velocity = body.velocity;
			if (OnGround || SnapToGround() || CheckSteepContacts()) {
				stepsSinceLastGrounded = 0;
				if (stepsSinceLastJump > 1) {
					jumpPhase = 0;
				}
				if (groundContactCount > 1) {
					contactNormal.Normalize();
				}
			}
			else {
				contactNormal = upAxis;
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

			if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit,
				probeDistance, probeMask)) {
				return false;
			}

			float upDot = Vector3.Dot(upAxis, hit.normal);
			if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
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

		private void Jump(Vector3 gravity) {
			Vector3 jumpDirection = Vector3.zero;

			if (OnGround) {
				jumpDirection = contactNormal;
			}
			else if (OnSteep) {
				jumpDirection = steepNormal;
				jumpPhase = 0;
			}
			else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
				if (jumpPhase == 0) {
					jumpPhase = 1;
				}
				jumpDirection = contactNormal;
			}
			else {
				return;
			}

			stepsSinceLastJump = 0;
			jumpPhase++;
			float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
			jumpDirection = (jumpDirection + upAxis).normalized;
			float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
			if (alignedSpeed > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			velocity += jumpDirection * jumpSpeed;
		}

		Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
			return (direction - normal * Vector3.Dot(direction, normal)).normalized;
		}

		private void AdjustVelocity() {
			Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal).normalized;
			Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal).normalized;

			float currentX = Vector3.Dot(velocity, xAxis);
			float currentZ = Vector3.Dot(velocity, zAxis);

			float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
			float maxSpeedChange = acceleration * Time.deltaTime;

			float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
			float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

			velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		}

		private float GetMinDot(int layer) {
			return (stairsMask & (1 << layer)) == 0 ?
				minGroundDotProduct : minStairsDotProduct;
		}

		private bool CheckSteepContacts() {
			if (steepContactCount > 1) {
				steepNormal.Normalize();
				float upDot = Vector3.Dot(upAxis, steepNormal);
				if (upDot >= minGroundDotProduct) {
					groundContactCount = 1;
					contactNormal = steepNormal;
					return true;
				}
			}
			return false;
		}
	}
}