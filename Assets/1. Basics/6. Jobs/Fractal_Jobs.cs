using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
/*
 * 현재 사용중인 코드는 Burst에 최적화되어 있지 않습니다. 
 * Burst가 최적화 할 수없는 호출 명령은 우리가 호출하는 정적 Quaternion 메서드에 해당합니다. 
 * Burst는 벡터화를 염두에두고 설계된 Unity의 수학 라이브러리와 함께 작동하도록 특별히 최적화되었습니다.
 */

namespace Jobs
{
	public class Fractal_Jobs : MonoBehaviour
	{
		// burstCompile이라고 지정해야 버스트컴파일처리.
		[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
		private struct UpdateFractalLevelJob : IJobFor
		{
			public float spinAngleDelta;
			public float scale;

			[ReadOnly]
			public NativeArray<FractalPart> parents;

			public NativeArray<FractalPart> parts;

			[WriteOnly]
			public NativeArray<float3x4> matrices;

			/*	[ReadOnly] - [WriteOnly]
			 *	여러 프로세스가 동일한 데이터를 병렬로 수정하는 경우 먼저 작업을 수행하는 임의의 데이터가됩니다. 
			 *	두 프로세스가 동일한 배열 요소를 설정하면 마지막 요소가 이깁니다. 
			 *	한 프로세스가 다른 프로세스가 설정 한 동일한 요소를 가져 오면 이전 값이나 새 값을 가져옵니다. 
			 *	최종 결과는 정확한 타이밍에 따라 달라지며 제어 할 수없는 경우 감지 및 수정이 
			 *	매우 어려운 일관성없는 동작이 발생할 수 있습니다. 이러한 현상을 경쟁 조건이라고합니다. 
			 *	ReadOnly 속성은이 데이터가 작업 실행 중에 일정하게 유지됨을 나타냅니다. 
			 *	즉, 결과가 항상 동일하기 때문에 프로세스에서 병렬로 안전하게 읽을 수 있습니다.
			 *	컴파일러는 작업이 ReadOnly 데이터에 쓰지 않고 WriteOnly 데이터에서 읽지 않도록 강제합니다. 
			 *	어쨌든 실수로 그렇게한다면 컴파일러는 우리가 의미 론적 실수를했다고 알려줄 것입니다. 
			 */

			public void Execute(int i) {
				FractalPart parent = parents[i / 5];
				FractalPart part = parts[i];
				part.spinAngle += spinAngleDelta;
				part.worldRotation = mul(parent.worldRotation,
					mul(part.rotation, quaternion.RotateY(part.spinAngle))
				);
				part.worldPosition =
					parent.worldPosition +
					mul(parent.worldRotation, (1.5f * scale * part.direction));
				parts[i] = part;

				float3x3 r = float3x3(part.worldRotation) * scale;
				matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
			}
		}

		private struct FractalPart
		{
			public float3 direction, worldPosition;
			public quaternion rotation, worldRotation;
			public float spinAngle;
		}

		private static readonly int matricesId = Shader.PropertyToID("_Matrices");
		private static MaterialPropertyBlock propertyBlock;
		private static float3[] directions = {
			up(), right(), left(), forward(), back()
		};
		private static quaternion[] rotations = {
			quaternion.identity,
			quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
			quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
		};

		[SerializeField, Range(1, 8)]
		private int depth = 4;

		[SerializeField]
		private Mesh mesh = default;

		[SerializeField]
		private Material material = default;
		private NativeArray<FractalPart>[] parts;
		private NativeArray<float3x4>[] matrices;
		private ComputeBuffer[] matricesBuffers;

		private void OnEnable() {
			parts = new NativeArray<FractalPart>[depth];
			matrices = new NativeArray<float3x4>[depth];
			matricesBuffers = new ComputeBuffer[depth];
			int stride = 12 * 4;
			for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
				parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
				matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
				matricesBuffers[i] = new ComputeBuffer(length, stride);
			}

			parts[0][0] = CreatePart(0);
			for (int li = 1; li < parts.Length; li++) {
				NativeArray<FractalPart> levelParts = parts[li];
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
				parts[i].Dispose();
				matrices[i].Dispose();
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
				rotation = rotations[childIndex]
			};
		}

		private void Update() {
			float spinAngleDelta = 0.125f * PI * Time.deltaTime;
			FractalPart rootPart = parts[0][0];
			rootPart.spinAngle += spinAngleDelta;
			rootPart.worldRotation = mul(transform.rotation,
				mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle))
			);
			rootPart.worldPosition = transform.position;
			parts[0][0] = rootPart;
			float objectScale = transform.lossyScale.x;
			float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
			matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

			float scale = objectScale;
			JobHandle jobHandle = default;
			for (int li = 1; li < parts.Length; li++) {
				scale *= 0.5f;
				jobHandle = new UpdateFractalLevelJob {
					spinAngleDelta = spinAngleDelta,
					scale = scale,
					parents = parts[li - 1],
					parts = parts[li],
					matrices = matrices[li]
				}.ScheduleParallel(parts[li].Length, 5, jobHandle);
				// job을 계속 등록.
			}
			jobHandle.Complete(); // 등록된 job을 일괄 계산.

			var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
			bounds.extents = Vector3.one * 0.5f;
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