﻿using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu(menuName = "TowerDefense/EnemyAnimationConfig")]
	public class EnemyAnimationConfig : ScriptableObject
	{
		[SerializeField]
		private AnimationClip move = default, intro = default, outro = default;

		public AnimationClip Move => move;
		public AnimationClip Intro => intro;
		public AnimationClip Outro => outro;
	}
}