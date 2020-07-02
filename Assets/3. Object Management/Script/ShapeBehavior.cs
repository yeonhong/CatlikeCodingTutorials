using UnityEngine;

namespace ObjectManagement
{
	public enum ShapeBehaviorType
	{
		Movement,
		Rotation,
		Oscillation
	}

	public static class ShapeBehaviorTypeMethods
	{
		public static ShapeBehavior GetInstance(this ShapeBehaviorType type) {
			switch (type) {
				case ShapeBehaviorType.Movement:
					return ShapeBehaviorPool<MovementShapeBehavior>.Get();
				case ShapeBehaviorType.Rotation:
					return ShapeBehaviorPool<RotationShapeBehavior>.Get();
				case ShapeBehaviorType.Oscillation:
					return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
			}
			UnityEngine.Debug.Log("Forgot to support " + type);
			return null;
		}
	}

	public abstract class ShapeBehavior
#if UNITY_EDITOR
		: ScriptableObject
#endif
	{
#if UNITY_EDITOR
		public bool IsReclaimed { get; set; }

		private void OnEnable() {
			if (IsReclaimed) {
				Recycle();
			}
		}
#endif

		public abstract ShapeBehaviorType BehaviorType { get; }
		public abstract void GameUpdate(Shape shape);
		public abstract void Save(GameDataWriter writer);
		public abstract void Load(GameDataReader reader);
		public abstract void Recycle();
	}
}