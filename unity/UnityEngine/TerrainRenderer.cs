using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	internal sealed class TerrainRenderer
	{
		private IntPtr m_Ptr;

		public TerrainData terrainData
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int lightmapIndex
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TerrainRenderer(int instanceID, TerrainData terrainData, Vector3 position, int lightmapIndex)
		{
			INTERNAL_CALL_TerrainRenderer(this, instanceID, terrainData, ref position, lightmapIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_TerrainRenderer(TerrainRenderer self, int instanceID, TerrainData terrainData, ref Vector3 position, int lightmapIndex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Dispose();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadPrecomputedError();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadBounds();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadAll();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Internal_RenderStep1(Camera camera, int maxLodLevel, float tau, float splatDistance, int layer);

		public void RenderStep1(Camera camera, int maxLodLevel, float tau, float splatDistance, int layer)
		{
			Internal_RenderStep1(camera, maxLodLevel, tau, splatDistance, layer);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RenderStep2();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RenderStep3(Camera camera, int layer, bool castShadows);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetNeighbors(TerrainRenderer left, TerrainRenderer top, TerrainRenderer right, TerrainRenderer bottom);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetLightmapSize(int lightmapSize);
	}
}
