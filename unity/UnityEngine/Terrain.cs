using System;
using System.Collections;

namespace UnityEngine
{
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public sealed class Terrain : MonoBehaviour
	{
		internal sealed class Renderer
		{
			internal Camera camera;

			internal TerrainRenderer terrain;

			internal TreeRenderer trees;

			internal DetailRenderer details;

			internal int lastUsedFrame;
		}

		[SerializeField]
		private TerrainData m_TerrainData;

		[SerializeField]
		private float m_TreeDistance = 5000f;

		[SerializeField]
		private float m_TreeBillboardDistance = 50f;

		[SerializeField]
		private float m_TreeCrossFadeLength = 5f;

		[SerializeField]
		private int m_TreeMaximumFullLODCount = 50;

		[SerializeField]
		private float m_DetailObjectDistance = 80f;

		[SerializeField]
		private float m_DetailObjectDensity = 1f;

		[SerializeField]
		private float m_HeightmapPixelError = 5f;

		[SerializeField]
		private float m_SplatMapDistance = 1000f;

		[SerializeField]
		private int m_HeightmapMaximumLOD;

		[SerializeField]
		private bool m_CastShadows = true;

		[SerializeField]
		private int m_LightmapIndex = -1;

		[SerializeField]
		private int m_LightmapSize = 1024;

		[SerializeField]
		private bool m_DrawTreesAndFoliage = true;

		[NonSerialized]
		private Terrain m_LeftNeighbor;

		[NonSerialized]
		private Terrain m_RightNeighbor;

		[NonSerialized]
		private Terrain m_BottomNeighbor;

		[NonSerialized]
		private Terrain m_TopNeighbor;

		[NonSerialized]
		private Vector3 m_Position;

		[NonSerialized]
		private TerrainRenderFlags m_EditorRenderFlags = TerrainRenderFlags.all;

		[NonSerialized]
		private ArrayList renderers = new ArrayList();

		internal static ArrayList ms_ActiveTerrains = new ArrayList();

		private static ArrayList ms_TempCulledTerrains = new ArrayList();

		[NonSerialized]
		private TerrainChangedFlags dirtyFlags;

		private static Terrain ms_ActiveTerrain;

		public static Terrain activeTerrain => ms_ActiveTerrain;

		public TerrainData terrainData
		{
			get
			{
				return m_TerrainData;
			}
			set
			{
				m_TerrainData = value;
			}
		}

		public float treeDistance
		{
			get
			{
				return m_TreeDistance;
			}
			set
			{
				m_TreeDistance = value;
			}
		}

		public float treeBillboardDistance
		{
			get
			{
				return m_TreeBillboardDistance;
			}
			set
			{
				m_TreeBillboardDistance = value;
			}
		}

		public float treeCrossFadeLength
		{
			get
			{
				return m_TreeCrossFadeLength;
			}
			set
			{
				m_TreeCrossFadeLength = value;
			}
		}

		public int treeMaximumFullLODCount
		{
			get
			{
				return m_TreeMaximumFullLODCount;
			}
			set
			{
				m_TreeMaximumFullLODCount = value;
			}
		}

		public float detailObjectDistance
		{
			get
			{
				return m_DetailObjectDistance;
			}
			set
			{
				m_DetailObjectDistance = value;
			}
		}

		public float detailObjectDensity
		{
			get
			{
				return m_DetailObjectDensity;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				bool flag = value != m_DetailObjectDensity;
				m_DetailObjectDensity = value;
				if (!flag)
				{
					return;
				}
				foreach (Renderer renderer in renderers)
				{
					renderer.details.ReloadAllDetails();
				}
			}
		}

		public float heightmapPixelError
		{
			get
			{
				return m_HeightmapPixelError;
			}
			set
			{
				m_HeightmapPixelError = value;
			}
		}

		public int heightmapMaximumLOD
		{
			get
			{
				return m_HeightmapMaximumLOD;
			}
			set
			{
				m_HeightmapMaximumLOD = value;
			}
		}

		public float basemapDistance
		{
			get
			{
				return m_SplatMapDistance;
			}
			set
			{
				m_SplatMapDistance = value;
			}
		}

		[Obsolete("use basemapDistance", true)]
		public float splatmapDistance
		{
			get
			{
				return m_SplatMapDistance;
			}
			set
			{
				m_SplatMapDistance = value;
			}
		}

		public int lightmapIndex
		{
			get
			{
				return m_LightmapIndex;
			}
			set
			{
				SetLightmapIndex(value);
			}
		}

		internal int lightmapSize
		{
			get
			{
				return m_LightmapSize;
			}
			set
			{
				m_LightmapSize = ((value <= 0) ? 1 : value);
				foreach (Renderer renderer in renderers)
				{
					renderer.terrain.SetLightmapSize(value);
				}
			}
		}

		public bool castShadows
		{
			get
			{
				return m_CastShadows;
			}
			set
			{
				m_CastShadows = value;
			}
		}

		internal bool drawTreesAndFoliage
		{
			get
			{
				return m_DrawTreesAndFoliage;
			}
			set
			{
				m_DrawTreesAndFoliage = value;
			}
		}

		public TerrainRenderFlags editorRenderFlags
		{
			get
			{
				return m_EditorRenderFlags;
			}
			set
			{
				m_EditorRenderFlags = value;
			}
		}

		private void SetLightmapIndex(int value)
		{
			m_LightmapIndex = value;
			foreach (Renderer renderer in renderers)
			{
				renderer.terrain.lightmapIndex = value;
				renderer.trees.lightmapIndex = value;
				renderer.details.lightmapIndex = value;
			}
		}

		private void ShiftLightmapIndex(int offset)
		{
			m_LightmapIndex += offset;
		}

		public float SampleHeight(Vector3 worldPosition)
		{
			worldPosition -= GetPosition();
			worldPosition.x /= m_TerrainData.size.x;
			worldPosition.z /= m_TerrainData.size.z;
			return m_TerrainData.GetInterpolatedHeight(worldPosition.x, worldPosition.z);
		}

		public static GameObject CreateTerrainGameObject(TerrainData assignTerrain)
		{
			GameObject gameObject = new GameObject("Terrain", typeof(Terrain), typeof(TerrainCollider));
			gameObject.isStatic = true;
			Terrain terrain = gameObject.GetComponent(typeof(Terrain)) as Terrain;
			TerrainCollider terrainCollider = gameObject.GetComponent(typeof(TerrainCollider)) as TerrainCollider;
			terrainCollider.terrainData = assignTerrain;
			terrain.terrainData = assignTerrain;
			terrain.OnEnable();
			return gameObject;
		}

		private static void ReconnectTerrainData()
		{
			ArrayList arrayList = new ArrayList(ms_ActiveTerrains);
			foreach (Terrain item in arrayList)
			{
				if (item.m_TerrainData == null)
				{
					item.OnDisable();
				}
				else if (!item.m_TerrainData.HasUser(item.gameObject))
				{
					item.OnDisable();
					item.OnEnable();
				}
			}
		}

		private static void SetLightmapIndexOnAllTerrains(int lightmapIndex)
		{
			foreach (Terrain ms_ActiveTerrain in ms_ActiveTerrains)
			{
				ms_ActiveTerrain.SetLightmapIndex(lightmapIndex);
			}
		}

		internal void ApplyDelayedHeightmapModification()
		{
			int[] array = terrainData.ComputeDelayedLod();
			if (array.Length == 0)
			{
				return;
			}
			terrainData.RecalculateTreePositions();
			foreach (Renderer renderer in renderers)
			{
				renderer.terrain.ReloadPrecomputedError();
				renderer.terrain.ReloadBounds();
				renderer.details.ReloadAllDetails();
			}
		}

		private void FlushDirty()
		{
			bool flag = false;
			bool flag2 = false;
			if ((dirtyFlags & TerrainChangedFlags.Heightmap) != 0)
			{
				flag = (flag2 = true);
			}
			if ((dirtyFlags & TerrainChangedFlags.TreeInstances) != 0)
			{
				flag2 = true;
			}
			if ((dirtyFlags & TerrainChangedFlags.DelayedHeightmapUpdate) != 0)
			{
				foreach (Renderer renderer5 in renderers)
				{
					renderer5.terrain.ReloadPrecomputedError();
				}
			}
			if (flag2)
			{
				foreach (Renderer renderer6 in renderers)
				{
					renderer6.trees.ReloadTrees();
				}
			}
			if (flag)
			{
				foreach (Renderer renderer7 in renderers)
				{
					renderer7.details.ReloadAllDetails();
				}
			}
			if ((dirtyFlags & TerrainChangedFlags.Heightmap) != 0)
			{
				foreach (Renderer renderer8 in renderers)
				{
					renderer8.terrain.ReloadAll();
				}
			}
			dirtyFlags = (TerrainChangedFlags)0;
		}

		private static void CullAllTerrains(int cullingMask)
		{
			ms_TempCulledTerrains.Clear();
			int count = ms_ActiveTerrains.Count;
			for (int i = 0; i < count; i++)
			{
				Terrain terrain = (Terrain)ms_ActiveTerrains[i];
				int layer = terrain.gameObject.layer;
				if (((1 << layer) & cullingMask) == 0)
				{
					continue;
				}
				ms_TempCulledTerrains.Add(terrain);
				Vector3 position = terrain.GetPosition();
				if (position != terrain.m_Position)
				{
					terrain.m_Position = position;
					terrain.Flush();
				}
				terrain.GarbageCollectRenderers();
				terrain.FlushDirty();
				Renderer renderer = terrain.GetRenderer();
				if (renderer != null)
				{
					terrain.terrainData.RecalculateBasemapIfDirty();
					if ((terrain.m_EditorRenderFlags & TerrainRenderFlags.heightmap) != 0)
					{
						float splatDistance = ((terrain.m_EditorRenderFlags != TerrainRenderFlags.heightmap) ? terrain.m_SplatMapDistance : float.PositiveInfinity);
						renderer.terrain.RenderStep1(renderer.camera, terrain.m_HeightmapMaximumLOD, terrain.m_HeightmapPixelError, splatDistance, layer);
					}
				}
			}
			count = ms_TempCulledTerrains.Count;
			for (int j = 0; j < count; j++)
			{
				Terrain terrain2 = (Terrain)ms_TempCulledTerrains[j];
				TerrainRenderer terrainRendererDontCreate = terrain2.GetTerrainRendererDontCreate();
				if (terrainRendererDontCreate != null && (terrain2.m_EditorRenderFlags & TerrainRenderFlags.heightmap) != 0)
				{
					TerrainRenderer left = null;
					TerrainRenderer right = null;
					TerrainRenderer top = null;
					TerrainRenderer bottom = null;
					if (terrain2.m_LeftNeighbor != null)
					{
						left = terrain2.m_LeftNeighbor.GetTerrainRendererDontCreate();
					}
					if (terrain2.m_RightNeighbor != null)
					{
						right = terrain2.m_RightNeighbor.GetTerrainRendererDontCreate();
					}
					if (terrain2.m_TopNeighbor != null)
					{
						top = terrain2.m_TopNeighbor.GetTerrainRendererDontCreate();
					}
					if (terrain2.m_BottomNeighbor != null)
					{
						bottom = terrain2.m_BottomNeighbor.GetTerrainRendererDontCreate();
					}
					terrainRendererDontCreate.SetNeighbors(left, top, right, bottom);
				}
			}
			count = ms_TempCulledTerrains.Count;
			for (int k = 0; k < count; k++)
			{
				Terrain terrain3 = (Terrain)ms_TempCulledTerrains[k];
				TerrainRenderer terrainRendererDontCreate2 = terrain3.GetTerrainRendererDontCreate();
				if (terrainRendererDontCreate2 != null && (terrain3.m_EditorRenderFlags & TerrainRenderFlags.heightmap) != 0)
				{
					terrainRendererDontCreate2.RenderStep2();
				}
			}
			count = ms_TempCulledTerrains.Count;
			for (int l = 0; l < count; l++)
			{
				Terrain terrain4 = (Terrain)ms_TempCulledTerrains[l];
				Renderer renderer2 = terrain4.GetRenderer();
				if (renderer2 != null)
				{
					int layer2 = terrain4.gameObject.layer;
					Light[] lights = Light.GetLights(LightType.Directional, layer2);
					if ((terrain4.m_EditorRenderFlags & TerrainRenderFlags.heightmap) != 0)
					{
						renderer2.terrain.RenderStep3(renderer2.camera, layer2, terrain4.m_CastShadows);
					}
					if ((terrain4.m_EditorRenderFlags & TerrainRenderFlags.details) != 0 && terrain4.m_DrawTreesAndFoliage && (double)terrain4.m_DetailObjectDistance > 0.001)
					{
						renderer2.details.Render(renderer2.camera, terrain4.m_DetailObjectDistance, layer2, terrain4.m_DetailObjectDensity);
					}
					if ((terrain4.m_EditorRenderFlags & TerrainRenderFlags.trees) != 0 && terrain4.m_DrawTreesAndFoliage && (double)terrain4.m_TreeDistance > 0.001)
					{
						renderer2.trees.Render(renderer2.camera, lights, terrain4.m_TreeBillboardDistance, terrain4.m_TreeDistance, terrain4.m_TreeCrossFadeLength, terrain4.m_TreeMaximumFullLODCount, layer2);
					}
				}
			}
		}

		private static void CullAllTerrainsShadowCaster(Light light)
		{
			foreach (Terrain ms_ActiveTerrain in ms_ActiveTerrains)
			{
				Renderer renderer = ms_ActiveTerrain.GetRenderer();
				if (renderer != null && (ms_ActiveTerrain.m_EditorRenderFlags & TerrainRenderFlags.trees) != 0 && ms_ActiveTerrain.m_DrawTreesAndFoliage && (double)ms_ActiveTerrain.m_TreeDistance > 0.001)
				{
					renderer.trees.RenderShadowCasters(light, renderer.camera, Mathf.Min(ms_ActiveTerrain.m_TreeBillboardDistance, ms_ActiveTerrain.m_TreeDistance), ms_ActiveTerrain.m_TreeMaximumFullLODCount, ms_ActiveTerrain.gameObject.layer);
				}
			}
		}

		public void AddTreeInstance(TreeInstance instance)
		{
			bool flag = m_TerrainData.HasTreeInstances();
			m_TerrainData.AddTree(out instance);
			foreach (Renderer renderer in renderers)
			{
				if (flag)
				{
					renderer.trees.InjectTree(out instance);
					continue;
				}
				renderer.trees.Cleanup();
				renderer.trees = new TreeRenderer(m_TerrainData, GetPosition(), m_LightmapIndex);
			}
		}

		public void SetNeighbors(Terrain left, Terrain top, Terrain right, Terrain bottom)
		{
			m_TopNeighbor = top;
			m_LeftNeighbor = left;
			m_RightNeighbor = right;
			m_BottomNeighbor = bottom;
		}

		public Vector3 GetPosition()
		{
			return base.transform.position;
		}

		public void Flush()
		{
			foreach (Renderer renderer in renderers)
			{
				renderer.trees.Cleanup();
				renderer.terrain.Dispose();
				renderer.details.Dispose();
			}
			renderers = new ArrayList();
		}

		private void GarbageCollectRenderers()
		{
			int renderedFrameCount = Time.renderedFrameCount;
			for (int num = renderers.Count - 1; num >= 0; num--)
			{
				Renderer renderer = (Renderer)renderers[num];
				int num2 = renderedFrameCount - renderer.lastUsedFrame;
				if (num2 > 100 || num2 < 0 || renderer.camera == null)
				{
					renderer.trees.Cleanup();
					renderer.terrain.Dispose();
					renderer.details.Dispose();
					renderers.RemoveAt(num);
				}
			}
		}

		internal void RemoveTrees(Vector2 position, float radius, int prototypeIndex)
		{
			if (m_TerrainData.RemoveTrees(position, radius, prototypeIndex) == 0)
			{
				return;
			}
			foreach (Renderer renderer in renderers)
			{
				renderer.trees.RemoveTrees(position, radius, prototypeIndex);
			}
		}

		private void OnTerrainChanged(TerrainChangedFlags flags)
		{
			if ((flags & TerrainChangedFlags.RemoveDirtyDetailsImmediately) != 0)
			{
				foreach (Renderer renderer in renderers)
				{
					renderer.details.ReloadDirtyDetails();
				}
			}
			if ((flags & TerrainChangedFlags.FlushEverythingImmediately) != 0)
			{
				Flush();
			}
			else
			{
				dirtyFlags |= flags;
			}
		}

		private void OnEnable()
		{
			if ((bool)m_TerrainData)
			{
				m_TerrainData.AddUser(base.gameObject);
			}
			ms_ActiveTerrain = this;
			if (!ms_ActiveTerrains.Contains(this))
			{
				ms_ActiveTerrains.Add(this);
			}
		}

		private void OnDisable()
		{
			ms_ActiveTerrains.Remove(this);
			if (ms_ActiveTerrain == this)
			{
				ms_ActiveTerrain = null;
			}
			if ((bool)m_TerrainData)
			{
				m_TerrainData.RemoveUser(base.gameObject);
			}
			ms_ActiveTerrains.Remove(this);
			Flush();
		}

		private TerrainRenderer GetTerrainRendererDontCreate()
		{
			Camera current = Camera.current;
			if ((current.cullingMask & (1 << base.gameObject.layer)) == 0)
			{
				return null;
			}
			foreach (Renderer renderer in renderers)
			{
				if (renderer.camera == current)
				{
					return renderer.terrain;
				}
			}
			return null;
		}

		private Renderer GetRenderer()
		{
			Camera current = Camera.current;
			if ((current.cullingMask & (1 << base.gameObject.layer)) == 0)
			{
				return null;
			}
			if (!current.IsFiltered(base.gameObject))
			{
				return null;
			}
			int renderedFrameCount = Time.renderedFrameCount;
			foreach (Renderer renderer3 in renderers)
			{
				if (renderer3.camera == current)
				{
					if (renderer3.terrain.terrainData == null)
					{
						Flush();
						break;
					}
					renderer3.lastUsedFrame = renderedFrameCount;
					return renderer3;
				}
			}
			if (m_TerrainData != null)
			{
				Vector3 position = GetPosition();
				Renderer renderer2 = new Renderer();
				renderer2.camera = current;
				renderer2.terrain = new TerrainRenderer(base.gameObject.GetInstanceID(), m_TerrainData, position, lightmapIndex);
				renderer2.terrain.SetLightmapSize(m_LightmapSize);
				renderer2.trees = new TreeRenderer(m_TerrainData, position, lightmapIndex);
				renderer2.details = new DetailRenderer(m_TerrainData, position, lightmapIndex);
				renderer2.lastUsedFrame = renderedFrameCount;
				renderers.Add(renderer2);
				return renderer2;
			}
			return null;
		}
	}
}
