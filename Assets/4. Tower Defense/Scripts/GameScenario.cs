using UnityEngine;

namespace TowerDefense
{
	[CreateAssetMenu]
	public class GameScenario : ScriptableObject
	{
		[SerializeField] private EnemyWave[] waves = { };

		public State Begin() => new State(this);

		[System.Serializable]
		public struct State
		{
			private GameScenario scenario;
			private int index;
			private EnemyWave.State wave;

			public State(GameScenario scenario) {
				this.scenario = scenario;
				index = 0;
				Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
				wave = scenario.waves[0].Begin();
			}

			public bool Progress() {
				float deltaTime = wave.Progress(Time.deltaTime);
				while (deltaTime >= 0f) {
					if (++index >= scenario.waves.Length) {
						return false;
					}
					wave = scenario.waves[index].Begin();
					deltaTime = wave.Progress(deltaTime);
				}
				return true;
			}
		}
	}
}