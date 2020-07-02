using UnityEngine;

namespace ObjectManagement
{
	public enum ShapeBehaviorType
	{
		Movement,
		Rotation
	}

	public abstract class ShapeBehavior : MonoBehaviour
	{
		public abstract ShapeBehaviorType BehaviorType { get; }
		public abstract void GameUpdate(Shape shape);
		public abstract void Save(GameDataWriter writer);
		public abstract void Load(GameDataReader reader);
	} 
}