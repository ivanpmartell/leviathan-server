using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class StaticBatchingUtility
	{
		public static void Combine(GameObject staticBatchRoot)
		{
			InternalStaticBatchingUtility.Combine(staticBatchRoot, generateTriangleStrips: true);
		}

		public static void Combine(GameObject[] gos, GameObject staticBatchRoot)
		{
			InternalStaticBatchingUtility.Combine(gos, staticBatchRoot, generateTriangleStrips: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern Mesh InternalCombineVertices(MeshSubsetCombineUtility.MeshInstance[] meshes, string meshName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void InternalCombineIndices(MeshSubsetCombineUtility.SubMeshInstance[] submeshes, bool generateStrips, ref Mesh combinedMesh);
	}
}
