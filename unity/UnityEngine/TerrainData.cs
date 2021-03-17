using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class TerrainData : Object
	{
		public int heightmapWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int heightmapHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int heightmapResolution
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 heightmapScale
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Vector3 size
		{
			get
			{
				INTERNAL_get_size(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_size(ref value);
			}
		}

		public float wavingGrassStrength
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float wavingGrassAmount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float wavingGrassSpeed
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color wavingGrassTint
		{
			get
			{
				INTERNAL_get_wavingGrassTint(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_wavingGrassTint(ref value);
			}
		}

		public int detailWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int detailHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int detailResolution
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		internal int detailResolutionPerPatch
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public DetailPrototype[] detailPrototypes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TreeInstance[] treeInstances
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TreePrototype[] treePrototypes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int alphamapLayers
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int alphamapResolution
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int alphamapWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int alphamapHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int baseMapResolution
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		private int alphamapTextureCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		internal Texture2D[] alphamapTextures
		{
			get
			{
				Texture2D[] array = new Texture2D[alphamapTextureCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = GetAlphamapTexture(i);
				}
				return array;
			}
		}

		public SplatPrototype[] splatPrototypes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern TerrainData();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern bool HasUser(GameObject user);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void AddUser(GameObject user);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RemoveUser(GameObject user);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_size(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_size(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetHeight(int x, int y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetInterpolatedHeight(float x, float y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float[,] GetHeights(int xBase, int yBase, int width, int height);

		public void SetHeights(int xBase, int yBase, float[,] heights)
		{
			if (heights == null)
			{
				throw new NullReferenceException();
			}
			if (xBase + heights.GetLength(1) > heightmapWidth || xBase < 0 || yBase < 0 || yBase + heights.GetLength(0) > heightmapHeight)
			{
				throw new Exception($"X or Y base out of bounds. Setting up to {xBase + heights.GetLength(1)}x{yBase + heights.GetLength(0)} while map size is {heightmapWidth}x{heightmapHeight}");
			}
			Internal_SetHeights(xBase, yBase, heights.GetLength(1), heights.GetLength(0), heights);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_SetHeights(int xBase, int yBase, int width, int height, float[,] heights);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_SetHeightsDelayLOD(int xBase, int yBase, int width, int height, float[,] heights);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern int[] ComputeDelayedLod();

		internal void SetHeightsDelayLOD(int xBase, int yBase, float[,] heights)
		{
			Internal_SetHeightsDelayLOD(xBase, yBase, heights.GetLength(1), heights.GetLength(0), heights);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetSteepness(float x, float y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Vector3 GetInterpolatedNormal(float x, float y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern int GetAdjustedSize(int size);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_wavingGrassTint(out Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_wavingGrassTint(ref Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetDetailResolution(int detailResolution, int resolutionPerPatch);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void ResetDirtyDetails();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RefreshPrototypes();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int[] GetSupportedLayers(int xBase, int yBase, int totalWidth, int totalHeight);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int[,] GetDetailLayer(int xBase, int yBase, int width, int height, int layer);

		public void SetDetailLayer(int xBase, int yBase, int layer, int[,] details)
		{
			Internal_SetDetailLayer(xBase, yBase, details.GetLength(1), details.GetLength(0), layer, details);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_SetDetailLayer(int xBase, int yBase, int totalWidth, int totalHeight, int detailIndex, int[,] data);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RemoveTreePrototype(int index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RecalculateTreePositions();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RemoveDetailPrototype(int index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float[,,] GetAlphamaps(int x, int y, int width, int height);

		public void SetAlphamaps(int x, int y, float[,,] map)
		{
			if (map.GetLength(2) != alphamapLayers)
			{
				throw new Exception($"Float array size wrong (layers should be {alphamapLayers})");
			}
			Internal_SetAlphamaps(x, y, map.GetLength(1), map.GetLength(0), map);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_SetAlphamaps(int x, int y, int width, int height, float[,,] map);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void RecalculateBasemapIfDirty();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void SetBasemapDirty(bool dirty);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Texture2D GetAlphamapTexture(int index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern bool HasTreeInstances();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void AddTree(out TreeInstance tree);

		internal int RemoveTrees(Vector2 position, float radius, int prototypeIndex)
		{
			return INTERNAL_CALL_RemoveTrees(this, ref position, radius, prototypeIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int INTERNAL_CALL_RemoveTrees(TerrainData self, ref Vector2 position, float radius, int prototypeIndex);
	}
}
