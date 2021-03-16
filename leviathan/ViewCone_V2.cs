using System.Collections.Generic;
using UnityEngine;

public class ViewCone_V2 : MonoBehaviour
{
	private MeshFilter meshFilter;

	public bool hasSetMeshData;

	public void Setup(float minDist, float maxDist, float fow, int detail)
	{
		if (hasSetMeshData)
		{
			return;
		}
		Mesh mesh = new Mesh();
		Color item = new Color(1f, 0f, 0f, 0.3f);
		Color item2 = new Color(0f, 1f, 0f, 0.25f);
		List<Vector3> list = new List<Vector3>();
		List<Color> list2 = new List<Color>();
		Vector3 zero = Vector3.zero;
		Vector3 vector = base.transform.localRotation * Vector3.forward;
		Vector3 vector2 = minDist * vector;
		Vector3 vector3 = zero + Quaternion.AngleAxis(0f - fow, Vector3.up) * vector2;
		Vector3 vector4 = zero + vector2;
		Vector3 vector5 = zero + Quaternion.AngleAxis(fow, Vector3.up) * vector2;
		Vector3 vector6 = maxDist * vector;
		Vector3 end = vector4 + Quaternion.AngleAxis(0f - fow, Vector3.up) * vector6;
		Vector3 item3 = vector4 + vector6;
		Vector3 start = vector4 + Quaternion.AngleAxis(fow, Vector3.up) * vector6;
		List<Vector3> list3 = new List<Vector3>();
		list3 = AddPointsBetween(zero, vector3, detail - 1, includeStart: true, includeEnd: true);
		list.AddRange(list3);
		for (int i = 0; i < list3.Count; i++)
		{
			list2.Add(item);
		}
		list3.Clear();
		if (detail > 1)
		{
			for (int j = 1; j < detail + 1; j++)
			{
				float num = 1f - (float)j / ((float)detail + 1f);
				Vector3 item4 = zero + Quaternion.AngleAxis(0f - fow * num, Vector3.up) * vector2;
				list3.Add(item4);
			}
		}
		list.AddRange(list3);
		for (int k = 0; k < list3.Count; k++)
		{
			list2.Add(item);
		}
		list.Add(vector4);
		list2.Add(item);
		list3.Clear();
		if (detail > 1)
		{
			for (int l = 1; l < detail + 1; l++)
			{
				float num2 = (float)l / ((float)detail + 1f);
				Vector3 item5 = zero + Quaternion.AngleAxis(fow * num2, Vector3.up) * vector2;
				list3.Add(item5);
			}
		}
		list.AddRange(list3);
		for (int m = 0; m < list3.Count; m++)
		{
			list2.Add(item);
		}
		list3 = AddPointsBetween(vector5, zero, detail - 1, includeStart: true, includeEnd: false);
		list.AddRange(list3);
		for (int n = 0; n < list3.Count; n++)
		{
			list2.Add(item);
		}
		list3 = AddPointsBetween(vector3, end, detail - 1, includeStart: false, includeEnd: true);
		list.AddRange(list3);
		for (int num3 = 0; num3 < list3.Count; num3++)
		{
			list2.Add(item2);
		}
		list3.Clear();
		if (detail > 1)
		{
			for (int num4 = 1; num4 < detail + 1; num4++)
			{
				float num5 = (float)num4 / ((float)detail + 1f);
				Vector3 item6 = zero + Quaternion.AngleAxis(fow * num5, Vector3.up) * vector6;
				list3.Add(item6);
			}
		}
		list.AddRange(list3);
		list2.Add(item2);
		for (int num6 = 0; num6 < list3.Count; num6++)
		{
			list2.Add(item2);
		}
		list.Add(item3);
		list3.Clear();
		if (detail > 1)
		{
			for (int num7 = 1; num7 < detail + 1; num7++)
			{
				float num8 = 1f - (float)num7 / ((float)detail + 1f);
				Vector3 item7 = zero + Quaternion.AngleAxis(0f - fow * num8, Vector3.up) * vector6;
				list3.Add(item7);
			}
		}
		list.AddRange(list3);
		for (int num9 = 0; num9 < list3.Count; num9++)
		{
			list2.Add(item2);
		}
		list3 = AddPointsBetween(start, vector5, detail - 1, includeStart: false, includeEnd: false);
		list.AddRange(list3);
		for (int num10 = 0; num10 < list3.Count; num10++)
		{
			list2.Add(item2);
		}
		mesh.vertices = list.ToArray();
		mesh.colors = list2.ToArray();
		List<Vector2> list4 = new List<Vector2>();
		for (int num11 = 0; num11 < list.Count; num11++)
		{
			Vector3 vector7 = list[num11];
			list4.Add(new Vector2(vector7.x, vector7.z));
		}
		Triangulator triangulator = new Triangulator(list4.ToArray());
		int[] array2 = (mesh.triangles = triangulator.Triangulate());
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = base.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		}
		meshFilter.mesh = mesh;
		hasSetMeshData = meshFilter != null && meshFilter.mesh != null && meshFilter.mesh.vertexCount > 2;
	}

	private static List<Vector3> AddPointsBetween(Vector3 start, Vector3 end, int numNew, bool includeStart, bool includeEnd)
	{
		List<Vector3> list = new List<Vector3>();
		if (numNew == 0)
		{
			if (includeStart)
			{
				list.Add(start);
			}
			if (includeEnd)
			{
				list.Add(end);
			}
			return list;
		}
		if (includeStart)
		{
			list.Add(start);
		}
		Vector3 vector = end - start;
		Vector3 normalized = vector.normalized;
		normalized.y = 0f;
		float num = vector.magnitude / (float)(numNew + 1);
		for (int i = 1; i < numNew + 1; i++)
		{
			Vector3 item = start + normalized * (num * (float)i);
			list.Add(item);
		}
		if (includeEnd)
		{
			list.Add(end);
		}
		return list;
	}
}
