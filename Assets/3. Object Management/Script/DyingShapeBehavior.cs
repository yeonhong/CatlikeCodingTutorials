﻿using UnityEngine;

namespace ObjectManagement
{
	public sealed class DyingShapeBehavior : ShapeBehavior
	{
		public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Dying;

		private Vector3 originalScale;
		private float duration, dyingAge;

		public override bool GameUpdate(Shape shape) {
			float dyingDuration = shape.Age - dyingAge;
			if (dyingDuration < duration) {
				float s = 1f - dyingDuration / duration;
				s = (3f - 2f * s) * s * s;
				shape.transform.localScale = s * originalScale;
				return true;
			}

			shape.Die();
			return true;
		}

		public void Initialize(Shape shape, float duration) {
			originalScale = shape.transform.localScale;
			this.duration = duration;
			dyingAge = shape.Age;
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(originalScale);
			writer.Write(duration);
			writer.Write(dyingAge);
		}

		public override void Load(GameDataReader reader) {
			originalScale = reader.ReadVector3();
			duration = reader.ReadFloat();
			dyingAge = reader.ReadFloat();
		}

		public override void Recycle() {
			ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
		}
	}
}