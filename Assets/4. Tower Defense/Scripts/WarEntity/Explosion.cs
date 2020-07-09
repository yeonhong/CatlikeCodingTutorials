using UnityEngine;

namespace TowerDefense
{
	public class Explosion : WarEntity
	{
		[SerializeField, Range(0f, 1f)] private float duration = 0.5f;
		[SerializeField] private AnimationCurve opacityCurve = default;
		[SerializeField] private AnimationCurve scaleCurve = default;

		private static int colorPropertyID = Shader.PropertyToID("_Color");
		private static MaterialPropertyBlock propertyBlock;

		private float age;
		private float scale;
		private MeshRenderer meshRenderer;

		private void Awake() {
			meshRenderer = GetComponent<MeshRenderer>();
			Debug.Assert(meshRenderer != null, "Explosion without renderer!");
		}

		public void Initialize(Vector3 position, float blastRadius, float damage = 0f) {
			if (damage > 0f) {
				TargetPoint.FillBuffer(position, blastRadius);
				for (int i = 0; i < TargetPoint.BufferedCount; i++) {
					TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
				}
			}

			transform.localPosition = position;
			scale = 2f * blastRadius;
		}

		public override bool GameUpdate() {
			age += Time.deltaTime;
			if (age >= duration) {
				OriginFactory.Reclaim(this);
				return false;
			}

			ExplosionEffect();
			return true;
		}

		private void ExplosionEffect() {
			if (propertyBlock == null) {
				propertyBlock = new MaterialPropertyBlock();
			}
			float t = age / duration;
			Color c = Color.clear;
			c.a = opacityCurve.Evaluate(t);
			propertyBlock.SetColor(colorPropertyID, c);
			meshRenderer.SetPropertyBlock(propertyBlock);
			transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));
		}
	}
}