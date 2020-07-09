using UnityEngine;

namespace TowerDefense
{
	public abstract class GameBehavior : MonoBehaviour
	{
		public virtual bool GameUpdate() => true;
	} 
}