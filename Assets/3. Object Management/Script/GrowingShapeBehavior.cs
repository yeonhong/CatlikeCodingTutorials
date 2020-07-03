using UnityEngine;

namespace ObjectManagement
{
	public sealed class GrowingShapeBehavior : ShapeBehavior
	{
		public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Growing;

		private Vector3 originalScale;
		private float duration;

		public override bool GameUpdate(Shape shape) {
			if (shape.Age < duration) {
				float s = shape.Age / duration;
				s = (3f - 2f * s) * s * s; // Smooth Step Curve
				shape.transform.localScale = s * originalScale;
				return true;
			}
			shape.transform.localScale = originalScale;
			return false;
		}

		public void Initialize(Shape shape, float duration) {
			originalScale = shape.transform.localScale;
			this.duration = duration;
			shape.transform.localScale = Vector3.zero;
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(originalScale);
			writer.Write(duration);
		}

		public override void Load(GameDataReader reader) {
			originalScale = reader.ReadVector3();
			duration = reader.ReadFloat();
		}

		public override void Recycle() {
			ShapeBehaviorPool<GrowingShapeBehavior>.Reclaim(this);
		}
	}
}