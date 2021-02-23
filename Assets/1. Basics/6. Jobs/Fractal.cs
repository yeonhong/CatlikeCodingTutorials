using UnityEngine;

namespace Jobs
{
	public class Fractal : MonoBehaviour
	{
		private struct FractalPart
		{
			public Vector3 direction, worldPosition;
			public Quaternion rotation, worldRotation;
			public float spinAngle;
		}

		[SerializeField, Range(1, 8)]
		private int depth = 4;

		[SerializeField]
		private Mesh mesh = default;

		[SerializeField]
		private Material material = default;

		private static Vector3[] directions = {
			Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
		};

		private static Quaternion[] rotations = {
			Quaternion.identity,
			Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
			Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
		};

		private FractalPart[][] parts;
		private Matrix4x4[][] matrices;
		private ComputeBuffer[] matricesBuffers;
		private static readonly int matricesId = Shader.PropertyToID("_Matrices");
		static MaterialPropertyBlock propertyBlock;

		private void OnEnable() {
			parts = new FractalPart[depth][];
			matrices = new Matrix4x4[depth][];
			matricesBuffers = new ComputeBuffer[depth];
			int stride = 16 * 4;
			for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
				parts[i] = new FractalPart[length];
				matrices[i] = new Matrix4x4[length];
				matricesBuffers[i] = new ComputeBuffer(length, stride);
			}

			parts[0][0] = CreatePart(0);
			for (int li = 1; li < parts.Length; li++) {
				FractalPart[] levelParts = parts[li];
				for (int fpi = 0; fpi < levelParts.Length; fpi += 5) {
					for (int ci = 0; ci < 5; ci++) {
						levelParts[fpi + ci] = CreatePart(ci);
					}
				}
			}

			if (propertyBlock == null) {
				propertyBlock = new MaterialPropertyBlock();
			}
		}

		private void OnDisable() {
			for (int i = 0; i < matricesBuffers.Length; i++) {
				matricesBuffers[i].Release();
			}
			parts = null;
			matrices = null;
			matricesBuffers = null;
		}

		private void OnValidate() {
			if (parts != null && enabled) {
				OnDisable();
				OnEnable();
			}
		}

		private FractalPart CreatePart(int childIndex) {
			return new FractalPart {
				direction = directions[childIndex],
				rotation = rotations[childIndex],
				worldPosition = Vector3.one,
				worldRotation = Quaternion.identity
			};
		}

		private void Update() {
			float spinAngleDelta = 22.5f * Time.deltaTime;
			FractalPart rootPart = parts[0][0];

			float objectScale = transform.lossyScale.x;
			matrices[0][0] = Matrix4x4.TRS(
				rootPart.worldPosition, rootPart.worldRotation, objectScale * Vector3.one
			);
			float scale = objectScale;

			rootPart.spinAngle += spinAngleDelta;
			rootPart.worldRotation =
				rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);
			parts[0][0] = rootPart;

			for (int li = 1; li < parts.Length; li++) {
				scale *= 0.5f;
				FractalPart[] parentParts = parts[li - 1];
				FractalPart[] levelParts = parts[li];
				Matrix4x4[] levelMatrices = matrices[li];
				for (int fpi = 0; fpi < levelParts.Length; fpi++) {
					FractalPart parent = parentParts[fpi / 5];
					FractalPart part = levelParts[fpi];
					part.spinAngle += spinAngleDelta;

					rootPart.worldRotation =
						transform.rotation * (rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f));
					rootPart.worldPosition = transform.position;

					levelParts[fpi] = part;
					levelMatrices[fpi] = Matrix4x4.TRS(
						part.worldPosition, part.worldRotation, scale * Vector3.one
					);
				}
			}

			var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
			for (int i = 0; i < matricesBuffers.Length; i++) {
				ComputeBuffer buffer = matricesBuffers[i];
				buffer.SetData(matrices[i]);
				propertyBlock.SetBuffer(matricesId, buffer);
				Graphics.DrawMeshInstancedProcedural(
					mesh, 0, material, bounds, buffer.count, propertyBlock
				);
			}
		}
	}
}