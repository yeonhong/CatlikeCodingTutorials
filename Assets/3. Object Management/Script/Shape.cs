﻿using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
	public class Shape : PersistableObject
	{
		private int shapeId = int.MinValue;
		public int MaterialId { get; private set; }
		private Color color;

		private static int colorPropertyId = Shader.PropertyToID("_Color");
		private static MaterialPropertyBlock sharedPropertyBlock;

		private List<ShapeBehavior> behaviorList = new List<ShapeBehavior>();

		public int ShapeId {
			get => shapeId;
			set {
				if (shapeId == int.MinValue && value != int.MinValue) {
					shapeId = value;
				}
				else {
					Debug.LogError("Not allowed to change shapeId.");
				}
			}
		}

		[SerializeField] private MeshRenderer[] meshRenderers = null;

		private Color[] colors;
		public int ColorCount => colors.Length;

		public ShapeFactory OriginFactory {
			get => originFactory;
			set {
				if (originFactory == null) {
					originFactory = value;
				}
				else {
					Debug.LogError("Not allowed to change origin factory.");
				}
			}
		}

		private ShapeFactory originFactory;

		private void Awake() {
			colors = new Color[meshRenderers.Length];
		}

		public void SetMaterial(Material material, int materialId) {
			for (int i = 0; i < meshRenderers.Length; i++) {
				meshRenderers[i].material = material;
			}
			MaterialId = materialId;
		}

		public void SetColor(Color color) {
			if (sharedPropertyBlock == null) {
				sharedPropertyBlock = new MaterialPropertyBlock();
			}
			sharedPropertyBlock.SetColor(colorPropertyId, color);
			for (int i = 0; i < meshRenderers.Length; i++) {
				colors[i] = color;
				meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
			}
		}

		public void SetColor(Color color, int index) {
			if (sharedPropertyBlock == null) {
				sharedPropertyBlock = new MaterialPropertyBlock();
			}
			sharedPropertyBlock.SetColor(colorPropertyId, color);
			colors[index] = color;
			meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
		}

		public override void Save(GameDataWriter writer) {
			base.Save(writer);
			writer.Write(colors.Length);
			for (int i = 0; i < colors.Length; i++) {
				writer.Write(colors[i]);
			}
			writer.Write(behaviorList.Count);
			for (int i = 0; i < behaviorList.Count; i++) {
				writer.Write((int)behaviorList[i].BehaviorType);
				behaviorList[i].Save(writer);
			}
		}

		public override void Load(GameDataReader reader) {
			base.Load(reader);
			if (reader.Version >= 5) {
				LoadColors(reader);
			}
			else {
				SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
			}

			if (reader.Version >= 6) {
				int behaviorCount = reader.ReadInt();
				for (int i = 0; i < behaviorCount; i++) {
					AddBehavior((ShapeBehaviorType)reader.ReadInt()).Load(reader);
				}
			}
			else if (reader.Version >= 4) {
				AddBehavior<RotationShapeBehavior>().AngularVelocity =
					reader.ReadVector3();
				AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
			}
		}

		public void GameUpdate() {
			for (int i = 0; i < behaviorList.Count; i++) {
				behaviorList[i].GameUpdate(this);
			}
		}

		private void LoadColors(GameDataReader reader) {
			int count = reader.ReadInt();
			int max = count <= colors.Length ? count : colors.Length;
			int i = 0;
			for (; i < max; i++) {
				SetColor(reader.ReadColor(), i);
			}

			if (count > colors.Length) {
				for (; i < count; i++) {
					reader.ReadColor();
				}
			}
			else if (count < colors.Length) {
				for (; i < colors.Length; i++) {
					SetColor(Color.white, i);
				}
			}
		}

		public void Recycle() {
			for (int i = 0; i < behaviorList.Count; i++) {
				behaviorList[i].Recycle();
			}
			behaviorList.Clear();
			OriginFactory.Reclaim(this);
		}

		public T AddBehavior<T>() where T : ShapeBehavior, new() {
			T behavior = ShapeBehaviorPool<T>.Get();
			behaviorList.Add(behavior);
			return behavior;
		}

		private ShapeBehavior AddBehavior(ShapeBehaviorType type) {
			switch (type) {
				case ShapeBehaviorType.Movement:
					return AddBehavior<MovementShapeBehavior>();
				case ShapeBehaviorType.Rotation:
					return AddBehavior<RotationShapeBehavior>();
			}
			Debug.LogError("Forgot to support " + type);
			return null;
		}
	}
}
