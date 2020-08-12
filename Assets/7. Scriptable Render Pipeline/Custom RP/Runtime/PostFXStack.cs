using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
	public partial class PostFXStack
	{
		private const string bufferName = "Post FX";
		private CommandBuffer buffer = new CommandBuffer {
			name = bufferName
		};
		private int fxSourceId = Shader.PropertyToID("_PostFXSource");

		private enum Passes
		{
			Copy
		}

		private ScriptableRenderContext context;
		private Camera camera;
		private PostFXSettings settings;

		public bool IsActive => settings != null;

		public void Setup(ScriptableRenderContext context, Camera camera, PostFXSettings settings) {
			this.context = context;
			this.camera = camera;
			this.settings =	camera.cameraType <= CameraType.SceneView ? settings : null;
			ApplySceneViewState();
		}

		public void Render(int sourceId) {
			//buffer.Blit(sourceId, BuiltinRenderTextureType.CameraTarget);
			Draw(sourceId, BuiltinRenderTextureType.CameraTarget, Passes.Copy);
			context.ExecuteCommandBuffer(buffer);
			buffer.Clear();
		}

		void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, Passes pass) {
			buffer.SetGlobalTexture(fxSourceId, from);
			buffer.SetRenderTarget(to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			buffer.DrawProcedural(
				Matrix4x4.identity, settings.Material, (int)pass,
				MeshTopology.Triangles, 3
			);
		}
	} 
}