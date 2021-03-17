using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	internal sealed class TreeRenderer
	{
		private IntPtr m_Ptr;

		public int lightmapIndex
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TreeRenderer(TerrainData data, Vector3 position, int lightmapIndex)
		{
			INTERNAL_CALL_TreeRenderer(this, data, ref position, lightmapIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_TreeRenderer(TreeRenderer self, TerrainData data, ref Vector3 position, int lightmapIndex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadTrees();

		public void Render(Camera camera, Light[] lights, float meshTreeDistance, float billboardTreeDistance, float crossFadeLength, int maximumMeshTrees, int layer)
		{
			RenderArguments arguments = default(RenderArguments);
			arguments.camera = camera;
			arguments.lights = lights;
			arguments.meshTreeDistance = meshTreeDistance;
			arguments.billboardTreeDistance = billboardTreeDistance;
			arguments.crossFadeLength = crossFadeLength;
			arguments.maximumMeshTrees = maximumMeshTrees;
			arguments.layer = layer;
			Render(ref arguments);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Render(ref RenderArguments arguments);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RenderShadowCasters(Light light, Camera camera, float meshTreeDistance, int maximumMeshTrees, int layer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Cleanup();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void InjectTree(out TreeInstance newTree);

		public void RemoveTrees(Vector3 pos, float radius, int prototypeIndex)
		{
			INTERNAL_CALL_RemoveTrees(this, ref pos, radius, prototypeIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_RemoveTrees(TreeRenderer self, ref Vector3 pos, float radius, int prototypeIndex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void InvalidateImposters();
	}
}
