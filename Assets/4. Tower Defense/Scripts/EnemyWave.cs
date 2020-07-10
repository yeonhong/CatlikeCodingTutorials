using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu(menuName = "TowerDefense/EnemyWave")]
	public class EnemyWave : ScriptableObject
	{
		[SerializeField]
		private EnemySpawnSequence[] spawnSequences = {
			new EnemySpawnSequence()
		};

		public State Begin() => new State(this);

		[System.Serializable]
		public struct State
		{
			private EnemyWave wave;
			private int index;
			private EnemySpawnSequence.State sequence;

			public State(EnemyWave wave) {
				this.wave = wave;
				index = 0;
				Debug.Assert(wave.spawnSequences.Length > 0, "Empty wave!");
				sequence = wave.spawnSequences[0].Begin();
			}

			public float Progress(float deltaTime) {
				deltaTime = sequence.Progress(deltaTime);
				while (deltaTime >= 0f) {
					if (++index >= wave.spawnSequences.Length) {
						return deltaTime;
					}
					sequence = wave.spawnSequences[index].Begin();
					deltaTime = sequence.Progress(deltaTime);
				}
				return -1f;
			}

		}
	}
}