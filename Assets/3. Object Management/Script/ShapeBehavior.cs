using UnityEngine;

namespace ObjectManagement
{
	public enum ShapeBehaviorType
	{
		Movement,
		Rotation
	}

	public abstract class ShapeBehavior
#if	UNITY_EDITOR
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