namespace UnityEngine
{
	internal class MeshSubsetCombineUtility
	{
		public struct MeshInstance
		{
			public Mesh mesh;

			public Renderer renderer;

			public Matrix4x4 transform;

			public Vector4 lightmapTilingOffset;
		}

		public struct SubMeshInstance
		{
			public Mesh mesh;

			public int vertexOffset;

			public GameObject gameObject;

			public int subMeshIndex;

			public bool reverseCullOrder;
		}

		public static Mesh CombineVertices(MeshInstance[] combines, string combinedMeshName)
		{
			int num = 0;
			for (int i = 0; i < combines.Length; i++)
			{
				MeshInstance meshInstance = combines[i];
				num += meshInstance.mesh.vertexCount;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			for (int j = 0; j < combines.Length; j++)
			{
				MeshInstance meshInstance2 = combines[j];
				if (meshInstance2.mesh.normals.Length > 0)
				{
					flag = true;
				}
				if (meshInstance2.mesh.tangents.Length > 0)
				{
					flag2 = true;
				}
				if (meshInstance2.mesh.uv.Length > 0)
				{
					flag3 = true;
				}
				if (meshInstance2.mesh.uv1.Length > 0)
				{
					flag4 = true;
				}
				if (!meshInstance2.lightmapTilingOffset.Equals(new Vector4(1f, 1f, 0f, 0f)))
				{
					flag4 = true;
				}
				if (meshInstance2.mesh.colors.Length > 0)
				{
					flag5 = true;
				}
			}
			Vector3[] array = new Vector3[num];
			Vector3[] array2 = new Vector3[flag ? num : 0];
			Vector4[] array3 = new Vector4[flag2 ? num : 0];
			Vector2[] array4 = new Vector2[flag3 ? num : 0];
			Vector2[] array5 = new Vector2[flag4 ? num : 0];
			Color[] array6 = new Color[flag5 ? num : 0];
			int offset = 0;
			for (int k = 0; k < combines.Length; k++)
			{
				MeshInstance meshInstance3 = combines[k];
				TransformPositions(meshInstance3.mesh.vertexCount, meshInstance3.mesh.vertices, array, ref offset, meshInstance3.transform);
			}
			if (flag)
			{
				offset = 0;
				for (int l = 0; l < combines.Length; l++)
				{
					MeshInstance meshInstance4 = combines[l];
					Matrix4x4 transform = meshInstance4.transform;
					transform = transform.inverse.transpose;
					TransformNormals(meshInstance4.mesh.vertexCount, meshInstance4.mesh.normals, array2, ref offset, transform);
				}
			}
			if (flag2)
			{
				offset = 0;
				for (int m = 0; m < combines.Length; m++)
				{
					MeshInstance meshInstance5 = combines[m];
					Matrix4x4 transform2 = meshInstance5.transform;
					transform2 = transform2.inverse.transpose;
					TransformTangents(meshInstance5.mesh.vertexCount, meshInstance5.mesh.tangents, array3, ref offset, transform2);
				}
			}
			if (flag3)
			{
				offset = 0;
				for (int n = 0; n < combines.Length; n++)
				{
					MeshInstance meshInstance6 = combines[n];
					Copy(meshInstance6.mesh.vertexCount, meshInstance6.mesh.uv, array4, ref offset);
				}
			}
			if (flag4)
			{
				offset = 0;
				for (int num2 = 0; num2 < combines.Length; num2++)
				{
					MeshInstance meshInstance7 = combines[num2];
					Vector2[] array7 = ((meshInstance7.mesh.uv1.Length != 0) ? meshInstance7.mesh.uv1 : meshInstance7.mesh.uv);
					if (array7.Length <= 0)
					{
						Debug.LogError("Mesh has no UV (texture coordinates) data.");
					}
					else if (!meshInstance7.lightmapTilingOffset.Equals(new Vector4(1f, 1f, 0f, 0f)))
					{
						CopyLightmapUV(meshInstance7.mesh.vertexCount, array7, array5, meshInstance7.lightmapTilingOffset, ref offset);
					}
					else
					{
						Copy(meshInstance7.mesh.vertexCount, array7, array5, ref offset);
					}
				}
			}
			if (flag5)
			{
				offset = 0;
				for (int num3 = 0; num3 < combines.Length; num3++)
				{
					MeshInstance meshInstance8 = combines[num3];
					CopyColors(meshInstance8.mesh.vertexCount, meshInstance8.mesh.colors, array6, ref offset);
				}
			}
			Mesh mesh = new Mesh();
			mesh.name = combinedMeshName;
			mesh.vertices = array;
			mesh.normals = array2;
			mesh.colors = array6;
			mesh.uv = array4;
			mesh.uv1 = array5;
			mesh.tangents = array3;
			return mesh;
		}

		public static void CombineIndices(SubMeshInstance[] combines, bool generateStrips, ref Mesh combinedMesh)
		{
			int[] array = new int[1];
			if (generateStrips)
			{
				for (int i = 0; i < combines.Length; i++)
				{
					SubMeshInstance subMeshInstance = combines[i];
					if (subMeshInstance.mesh.GetTriangleStrip(subMeshInstance.subMeshIndex).Length == 0)
					{
						generateStrips = false;
					}
				}
			}
			int num = 0;
			combinedMesh.subMeshCount = combines.Length;
			for (int j = 0; j < combines.Length; j++)
			{
				SubMeshInstance subMeshInstance2 = combines[j];
				int[] array2 = ((!generateStrips) ? subMeshInstance2.mesh.GetTriangles(subMeshInstance2.subMeshIndex) : subMeshInstance2.mesh.GetTriangleStrip(subMeshInstance2.subMeshIndex));
				int num2 = array2.Length;
				if (!subMeshInstance2.reverseCullOrder)
				{
					array = new int[num2];
					for (int k = 0; k < num2; k++)
					{
						array[k] = array2[k] + subMeshInstance2.vertexOffset;
					}
				}
				else if (generateStrips)
				{
					array = new int[num2 + 1];
					array[0] = array2[0] + subMeshInstance2.vertexOffset;
					for (int l = 0; l < num2; l++)
					{
						array[l + 1] = array2[l] + subMeshInstance2.vertexOffset;
					}
				}
				else
				{
					array = new int[num2];
					for (int m = 0; m < num2; m++)
					{
						array[m] = array2[num2 - m - 1] + subMeshInstance2.vertexOffset;
					}
				}
				combinedMesh.SetTrianglesDontRebuildCollisionTriangles(array, generateStrips, num);
				num++;
			}
		}

		private static void Copy(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
		{
			for (int i = 0; i < src.Length; i++)
			{
				ref Vector2 reference = ref dst[i + offset];
				reference = src[i];
			}
			offset += vertexcount;
		}

		private static void TransformPositions(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
		{
			for (int i = 0; i < src.Length; i++)
			{
				ref Vector3 reference = ref dst[i + offset];
				reference = transform.MultiplyPoint(src[i]);
			}
			offset += vertexcount;
		}

		private static void TransformNormals(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
		{
			int i;
			for (i = 0; i < src.Length; i++)
			{
				ref Vector3 reference = ref dst[i + offset];
				reference = transform.MultiplyVector(src[i]).normalized;
			}
			for (; i < vertexcount; i++)
			{
				ref Vector3 reference2 = ref dst[i + offset];
				reference2 = Vector3.up;
			}
			offset += vertexcount;
		}

		private static void CopyLightmapUV(int vertexcount, Vector2[] src, Vector2[] dst, Vector4 uvScaleOffset, ref int offset)
		{
			for (int i = 0; i < src.Length; i++)
			{
				dst[i + offset].x = src[i].x * uvScaleOffset.x + uvScaleOffset.z;
				dst[i + offset].y = src[i].y * uvScaleOffset.y + uvScaleOffset.w;
			}
			offset += vertexcount;
		}

		private static void CopyColors(int vertexcount, Color[] src, Color[] dst, ref int offset)
		{
			int i;
			for (i = 0; i < src.Length; i++)
			{
				ref Color reference = ref dst[i + offset];
				reference = src[i];
			}
			for (; i < vertexcount; i++)
			{
				ref Color reference2 = ref dst[i + offset];
				reference2 = Color.white;
			}
			offset += vertexcount;
		}

		private static void TransformTangents(int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
		{
			int i;
			for (i = 0; i < src.Length; i++)
			{
				Vector4 vector = src[i];
				Vector3 v = new Vector3(vector.x, vector.y, vector.z);
				v = transform.MultiplyVector(v).normalized;
				ref Vector4 reference = ref dst[i + offset];
				reference = new Vector4(v.x, v.y, v.z, vector.w);
			}
			for (; i < vertexcount; i++)
			{
				ref Vector4 reference2 = ref dst[i + offset];
				reference2 = Vector3.forward;
			}
			offset += vertexcount;
		}
	}
}
