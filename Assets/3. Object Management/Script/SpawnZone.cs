using UnityEngine;

namespace ObjectManagement
{
	public abstract class SpawnZone : PersistableObject
	{
		[System.Serializable]
		public struct SpawnConfiguration
		{
			public enum MovementDirection
			{
				Forward,
				Upward,
				Outward,
				Random
			}

			public ShapeFactory[] factories;
			public MovementDirection movementDirection;
			public FloatRange speed;
			public FloatRange angularSpeed;
			public FloatRange scale;
			public ColorRangeHSV color;
			public bool uniformColor;
		}

		[SerializeField] private SpawnConfiguration spawnConfig;

		public abstract Vector3 SpawnPoint { get; }
		
		public virtual Shape SpawnShape() {
			int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
			Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
			Transform t = shape.transform;
			t.localPosition = SpawnPoint;
			t.localRotation = Random.rotation;
			t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
			if (spawnConfig.uniformColor) {
				shape.SetColor(spawnConfig.color.RandomInRange);
			}
			else {
				for (int i = 0; i < shape.ColorCount; i++) {
					shape.SetColor(spawnConfig.color.RandomInRange, i);
				}
			}

			float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
			if (angularSpeed != 0f) {
				var rotation = shape.AddBehavior<RotationShapeBehavior>();
				rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
			}

			float speed = spawnConfig.speed.RandomValueInRange;
			if (speed != 0f) {
				Vector3 direction;
				switch (spawnConfig.movementDirection) {
					case SpawnConfiguration.MovementDirection.Upward:
						direction = transform.up;
						break;
					case SpawnConfiguration.MovementDirection.Outward:
						direction = (t.localPosition - transform.position).normalized;
						break;
					case SpawnConfiguration.MovementDirection.Random:
						direction = Random.onUnitSphere;
						break;
					default:
						direction = transform.forward;
						break;
				}

				var movement = shape.AddBehavior<MovementShapeBehavior>();
				movement.Velocity = direction * speed;
			}

			return shape;
		}
	}
}