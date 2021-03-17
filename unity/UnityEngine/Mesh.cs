using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Mesh : Object
	{
		public Vector3[] vertices
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3[] normals
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector4[] tangents
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector2[] uv
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector2[] uv2
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector2[] uv1
		{
			get
			{
				return uv2;
			}
			set
			{
				uv2 = value;
			}
		}

		public Bounds bounds
		{
			get
			{
				INTERNAL_get_bounds(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_bounds(ref value);
			}
		}

		public Color[] colors
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color32[] colors32
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int[] triangles
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int vertexCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int subMeshCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public BoneWeight[] boneWeights
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Matrix4x4[] bindposes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Mesh()
		{
			Internal_Create(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_Create(Mesh mono);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Clear(bool keepVertexLayout);

		public void Clear()
		{
			bool keepVertexLayout = true;
			Clear(keepVertexLayout);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_bounds(out Bounds value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_bounds(ref Bounds value);

		public void RecalculateBounds()
		{
			INTERNAL_CALL_RecalculateBounds(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_RecalculateBounds(Mesh self);

		public void RecalculateNormals()
		{
			INTERNAL_CALL_RecalculateNormals(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_RecalculateNormals(Mesh self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Optimize();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int[] GetTriangles(int submesh);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetTriangles(int[] triangles, int submesh);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void SetTrianglesDontRebuildCollisionTriangles(int[] triangles, bool isStrip, int submesh);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetTriangleStrip(int[] triangles, int submesh);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int[] GetTriangleStrip(int submesh);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void CombineMeshes(CombineInstance[] combine, bool mergeSubMeshes, bool useMatrices);

		public void CombineMeshes(CombineInstance[] combine, bool mergeSubMeshes)
		{
			bool useMatrices = true;
			CombineMeshes(combine, mergeSubMeshes, useMatrices);
		}

		public void CombineMeshes(CombineInstance[] combine)
		{
			bool useMatrices = true;
			bool mergeSubMeshes = true;
			CombineMeshes(combine, mergeSubMeshes, useMatrices);
		}
	}
}
