using System;
using System.Collections;

namespace UnityEngine
{
	internal class InternalStaticBatchingUtility
	{
		internal class SortGO : IComparer
		{
			int IComparer.Compare(object a, object b)
			{
				if (a == b)
				{
					return 0;
				}
				Renderer renderer = GetRenderer(a as GameObject);
				Renderer renderer2 = GetRenderer(b as GameObject);
				int num = GetMaterialId(renderer).CompareTo(GetMaterialId(renderer2));
				if (num == 0)
				{
					num = GetLightmapIndex(renderer).CompareTo(GetLightmapIndex(renderer2));
				}
				return num;
			}

			private static int GetMaterialId(Renderer renderer)
			{
				if (renderer == null)
				{
					return 0;
				}
				return renderer.sharedMaterial.GetInstanceID();
			}

			private static int GetLightmapIndex(Renderer renderer)
			{
				if (renderer == null)
				{
					return -1;
				}
				return renderer.lightmapIndex;
			}

			private static Renderer GetRenderer(GameObject go)
			{
				if (go == null)
				{
					return null;
				}
				MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
				if (meshFilter == null)
				{
					return null;
				}
				return meshFilter.renderer;
			}
		}

		private const int MaxVerticesInBatch = 64000;

		private const string CombinedMeshPrefix = "Combined Mesh";

		public static void Combine(GameObject staticBatchRoot, bool generateTriangleStrips)
		{
			Combine(staticBatchRoot, combineOnlyStatic: false, generateTriangleStrips);
		}

		public static void Combine(GameObject staticBatchRoot, bool combineOnlyStatic, bool generateTriangleStrips)
		{
			GameObject[] array = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
			ArrayList arrayList = new ArrayList();
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if ((!(staticBatchRoot != null) || gameObject.transform.IsChildOf(staticBatchRoot.transform)) && (!combineOnlyStatic || gameObject.isStaticBatchable))
				{
					arrayList.Add(gameObject);
				}
			}
			array = (GameObject[])arrayList.ToArray(typeof(GameObject));
			if (!Application.HasProLicense() && !Application.HasAdvancedLicense() && staticBatchRoot != null && array.Length > 0)
			{
				Debug.LogError("Your Unity license is not sufficient for Static Batching.");
			}
			Combine(array, staticBatchRoot, generateTriangleStrips);
		}

		public static void Combine(GameObject[] gos, GameObject staticBatchRoot, bool generateTriangleStrips)
		{
			Matrix4x4 matrix4x = Matrix4x4.identity;
			Transform staticBatchRootTransform = null;
			if ((bool)staticBatchRoot)
			{
				matrix4x = staticBatchRoot.transform.worldToLocalMatrix;
				staticBatchRootTransform = staticBatchRoot.transform;
			}
			int batchIndex = 0;
			int num = 0;
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			Array.Sort(gos, new SortGO());
			foreach (GameObject gameObject in gos)
			{
				MeshFilter meshFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
				if (meshFilter == null || meshFilter.sharedMesh == null || meshFilter.renderer == null || !meshFilter.renderer.enabled || meshFilter.renderer.staticBatchIndex != 0)
				{
					continue;
				}
				if (num + meshFilter.sharedMesh.vertexCount > 64000)
				{
					MakeBatch(arrayList, arrayList2, staticBatchRootTransform, batchIndex++, generateTriangleStrips);
					arrayList.Clear();
					arrayList2.Clear();
					num = 0;
				}
				MeshSubsetCombineUtility.MeshInstance meshInstance = default(MeshSubsetCombineUtility.MeshInstance);
				meshInstance.mesh = meshFilter.sharedMesh;
				meshInstance.renderer = meshFilter.renderer;
				meshInstance.transform = matrix4x * meshFilter.transform.localToWorldMatrix;
				meshInstance.lightmapTilingOffset = meshFilter.renderer.lightmapTilingOffset;
				bool reverseCullOrder = TransformHasNegativeScale(meshFilter.transform);
				arrayList.Add(meshInstance);
				Material[] array = meshFilter.renderer.sharedMaterials;
				if (array.Length > meshInstance.mesh.subMeshCount)
				{
					Debug.LogWarning("Mesh has more materials (" + array.Length + ") than subsets (" + meshInstance.mesh.subMeshCount + ")");
					Material[] array2 = new Material[meshInstance.mesh.subMeshCount];
					for (int j = 0; j < meshInstance.mesh.subMeshCount; j++)
					{
						array2[j] = meshFilter.renderer.sharedMaterials[j];
					}
					meshFilter.renderer.sharedMaterials = array2;
					array = array2;
				}
				for (int k = 0; k < Math.Min(array.Length, meshInstance.mesh.subMeshCount); k++)
				{
					MeshSubsetCombineUtility.SubMeshInstance subMeshInstance = default(MeshSubsetCombineUtility.SubMeshInstance);
					subMeshInstance.mesh = meshFilter.sharedMesh;
					subMeshInstance.vertexOffset = num;
					subMeshInstance.subMeshIndex = k;
					subMeshInstance.gameObject = gameObject;
					subMeshInstance.reverseCullOrder = reverseCullOrder;
					arrayList2.Add(subMeshInstance);
				}
				num += meshInstance.mesh.vertexCount;
			}
			MakeBatch(arrayList, arrayList2, staticBatchRootTransform, batchIndex, generateTriangleStrips);
		}

		private static void MakeBatch(ArrayList meshes, ArrayList subsets, Transform staticBatchRootTransform, int batchIndex, bool generateTriangleStrips)
		{
			if (meshes.Count >= 2)
			{
				MeshSubsetCombineUtility.MeshInstance[] meshes2 = (MeshSubsetCombineUtility.MeshInstance[])meshes.ToArray(typeof(MeshSubsetCombineUtility.MeshInstance));
				MeshSubsetCombineUtility.SubMeshInstance[] array = (MeshSubsetCombineUtility.SubMeshInstance[])subsets.ToArray(typeof(MeshSubsetCombineUtility.SubMeshInstance));
				string text = "Combined Mesh";
				text = text + " (root: " + ((!(staticBatchRootTransform != null)) ? "scene" : staticBatchRootTransform.name) + ")";
				if (batchIndex > 0)
				{
					text = text + " " + (batchIndex + 1);
				}
				Mesh combinedMesh = StaticBatchingUtility.InternalCombineVertices(meshes2, text);
				StaticBatchingUtility.InternalCombineIndices(array, generateTriangleStrips, ref combinedMesh);
				int num = 0;
				MeshSubsetCombineUtility.SubMeshInstance[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					MeshSubsetCombineUtility.SubMeshInstance subMeshInstance = array2[i];
					GameObject gameObject = subMeshInstance.gameObject;
					Mesh sharedMesh = combinedMesh;
					MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
					meshFilter.sharedMesh = sharedMesh;
					gameObject.renderer.SetSubsetIndex(subMeshInstance.subMeshIndex, num);
					gameObject.renderer.staticBatchRootTransform = staticBatchRootTransform;
					gameObject.renderer.enabled = false;
					gameObject.renderer.enabled = true;
					num++;
				}
			}
		}

		private static bool TransformHasNegativeScale(Transform transform)
		{
			bool flag = false;
			Vector3 lossyScale = transform.lossyScale;
			flag ^= (double)lossyScale.x < 0.0;
			flag ^= (double)lossyScale.y < 0.0;
			return flag ^ ((double)lossyScale.z < 0.0);
		}
	}
}
