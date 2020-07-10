using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu]
	public class EnemyAnimationConfig : ScriptableObject
	{
		[SerializeField] private AnimationClip move = default;
		public AnimationClip Move => move;
	}
}