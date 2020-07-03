namespace ObjectManagement
{
	public sealed class LifecycleShapeBehavior : ShapeBehavior
	{
		public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.LifeCycle;

		private float adultDuration, dyingDuration, dyingAge;

		public override bool GameUpdate(Shape shape) {
			if (shape.Age >= dyingAge) {
				if (dyingDuration <= 0f) {
					shape.Die();
					return true;
				}
				shape.AddBehavior<DyingShapeBehavior>().Initialize(
					shape, dyingDuration + dyingAge - shape.Age
				);
				return false;
			}
			return true;
		}

		public void Initialize(Shape shape, float growingDuration, float adultDuration, float dyingDuration) {
			this.adultDuration = adultDuration;
			this.dyingDuration = dyingDuration;
			dyingAge = growingDuration + adultDuration;

			if (growingDuration > 0f) {
				shape.AddBehavior<GrowingShapeBehavior>().Initialize(
					shape, growingDuration
				);
			}
		}

		public override void Save(GameDataWriter writer) {
			writer.Write(adultDuration);
			writer.Write(dyingDuration);
			writer.Write(dyingAge);
		}

		public override void Load(GameDataReader reader) {
			adultDuration = reader.ReadFloat();
			dyingDuration = reader.ReadFloat();
			dyingAge = reader.ReadFloat();
		}

		public override void Recycle() {
			ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
		}
	}
}