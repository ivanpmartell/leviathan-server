using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	internal sealed class DetailRenderer
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

		public DetailRenderer(TerrainData terrainData, Vector3 position, int lightmapIndex)
		{
			INTERNAL_CALL_DetailRenderer(this, terrainData, ref position, lightmapIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_DetailRenderer(DetailRenderer self, TerrainData terrainData, ref Vector3 position, int lightmapIndex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Dispose();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Render(Camera camera, float viewDistance, int layer, float detailDensity);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadAllDetails();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void ReloadDirtyDetails();
	}
}
