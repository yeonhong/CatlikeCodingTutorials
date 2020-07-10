using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu(menuName = "TowerDefense/EnemyAnimationConfig")]
	public class EnemyAnimationConfig : ScriptableObject
	{
		[SerializeField] private AnimationClip move = default;
		public AnimationClip Move => move;
	}
}