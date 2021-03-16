using System.Collections.Generic;
using UnityEngine;

public class ViewCone : MonoBehaviour
{
	private Mesh m_mesh;

	public bool hasSetMeshData;

	public int m_detail = 8;

	public void Setup(float minDist, float maxDist, float fow)
	{
		if (hasSetMeshData)
		{
			return;
		}
		if (fow > 179.9999f)
		{
			fow = 179.9999f;
		}
		Color item = new Color(0f, 1f, 0f, 0.5f);
		List<Vector3> list = new List<Vector3>();
		List<Color> list2 = new List<Color>();
		Vector3 zero = Vector3.zero;
		Vector3 forward = Vector3.forward;
		Vector3 vector = minDist * forward;
		Vector3 start = zero + Quaternion.AngleAxis(0f - fow, Vector3.up) * vector;
		Vector3 end = zero + Quaternion.AngleAxis(fow, Vector3.up) * vector;
		Vector3 vector2 = maxDist * forward;
		Vector3 end2 = zero + Quaternion.AngleAxis(0f - fow, Vector3.up) * vector2;
		Vector3 start2 = zero + Quaternion.AngleAxis(fow, Vector3.up) * vector2;
		List<Vector3> list3 = new List<Vector3>();
		list3.Clear();
		list3 = AddPointsBetween(start, end2, m_detail - 1, includeStart: true, includeEnd: true);
		list.AddRange(list3);
		for (int i = 0; i < list3.Count; i++)
		{
			list2.Add(item);
		}
		list3.Clear();
		if (m_detail > 1)
		{
			for (int j = 1; j < m_detail + 1; j++)
			{
				float num = 1f - (float)j / ((float)m_detail + 1f);
				Vector3 item2 = zero + Quaternion.AngleAxis(0f - fow * num, Vector3.up) * vector2;
				list3.Add(item2);
			}
		}
		if (m_detail > 1)
		{
			for (int k = 1; k < m_detail + 1; k++)
			{
				float num2 = (float)k / ((float)m_detail + 1f);
				Vector3 item3 = zero + Quaternion.AngleAxis(fow * num2, Vector3.up) * vector2;
				list3.Add(item3);
			}
		}
		list.AddRange(list3);
		for (int l = 0; l < list3.Count; l++)
		{
			list2.Add(item);
		}
		list3 = AddPointsBetween(start2, end, m_detail - 1, includeStart: true, includeEnd: false);
		list.AddRange(list3);
		for (int m = 0; m < list3.Count; m++)
		{
			list2.Add(item);
		}
		Vector3 vector3 = vector;
		List<Vector3> list4 = new List<Vector3>();
		if (m_detail > 1)
		{
			for (int num3 = m_detail + 1; num3 > 1; num3--)
			{
				float num4 = (float)num3 / ((float)m_detail + 1f);
				Vector3 item4 = zero + Quaternion.AngleAxis(fow * num4, Vector3.up) * vector3;
				list4.Add(item4);
			}
		}
		if (m_detail > 1)
		{
			for (int num5 = m_detail + 1; num5 > 1; num5--)
			{
				float num6 = 1f - (float)num5 / ((float)m_detail + 1f);
				Vector3 item5 = zero + Quaternion.AngleAxis(0f - fow * num6, Vector3.up) * vector3;
				list4.Add(item5);
			}
		}
		list.AddRange(list4);
		for (int n = 0; n < list4.Count; n++)
		{
			list2.Add(item);
		}
		Vector2[] array = new Vector2[list.Count];
		for (int num7 = 0; num7 < list.Count; num7++)
		{
			ref Vector2 reference = ref array[num7];
			reference = new Vector2(0f, 0f);
		}
		m_mesh = new Mesh();
		m_mesh.name = "ViewCone";
		m_mesh.vertices = list.ToArray();
		m_mesh.colors = list2.ToArray();
		m_mesh.uv = array;
		List<Vector2> list5 = new List<Vector2>();
		for (int num8 = 0; num8 < list.Count; num8++)
		{
			Vector3 vector4 = list[num8];
			list5.Add(new Vector2(vector4.x, vector4.z));
		}
		Triangulator triangulator = new Triangulator(list5.ToArray());
		int[] triangles = triangulator.Triangulate();
		m_mesh.triangles = triangles;
		m_mesh.RecalculateNormals();
		m_mesh.RecalculateBounds();
		m_mesh.Optimize();
		MeshFilter component = GetComponent<MeshFilter>();
		component.mesh = m_mesh;
		hasSetMeshData = component != null && component.mesh != null && component.mesh.vertexCount > 2;
	}

	private void OnDestroy()
	{
		Object.Destroy(m_mesh);
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
