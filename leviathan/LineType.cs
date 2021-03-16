using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineType
{
	public string m_name = "Give Me A Name";

	public Material m_material;

	public float m_uvScale = 1f;

	public float m_uvScroll;

	public bool m_distanceScale = true;

	private float m_uvOffset;

	private Mesh[] m_lineMesh;

	private int m_currentMesh;

	private List<Vector3> m_vertises = new List<Vector3>();

	private List<int> m_indices = new List<int>();

	private List<Vector2> m_uvs = new List<Vector2>();

	public void Draw(float dt)
	{
		m_uvOffset -= m_uvScroll * dt;
		if (m_indices.Count != 0)
		{
			if (m_lineMesh == null)
			{
				m_lineMesh = new Mesh[2]
				{
					new Mesh(),
					new Mesh()
				};
				m_lineMesh[0].name = "Line mesh 0:" + m_name;
				m_lineMesh[1].name = "Line mesh 1:" + m_name;
			}
			Mesh mesh = m_lineMesh[m_currentMesh];
			mesh.Clear();
			mesh.vertices = m_vertises.ToArray();
			mesh.uv = m_uvs.ToArray();
			mesh.triangles = m_indices.ToArray();
			Graphics.DrawMesh(mesh, Matrix4x4.identity, m_material, 0, null, 0, null, castShadows: false, receiveShadows: false);
			m_vertises.Clear();
			m_indices.Clear();
			m_uvs.Clear();
			m_currentMesh = ((m_currentMesh == 0) ? 1 : 0);
		}
	}

	public void DrawLine(float scale, Vector3 camPos, Vector3 start, Vector3 end, float width)
	{
		if (!(start == end))
		{
			Vector3 normalized = (end - start).normalized;
			Vector3 vector = Vector3.Cross(normalized, Vector3.up) * (width / 2f);
			int count = m_vertises.Count;
			m_indices.Add(count);
			m_indices.Add(count + 1);
			m_indices.Add(count + 2);
			m_indices.Add(count + 2);
			m_indices.Add(count + 1);
			m_indices.Add(count + 3);
			float num = 1f;
			float num2 = 1f;
			if (m_distanceScale)
			{
				num = 0.05f * scale * Vector3.Distance(start, camPos);
				num2 = 0.05f * scale * Vector3.Distance(end, camPos);
			}
			m_vertises.Add(start - vector * num);
			m_vertises.Add(start + vector * num);
			m_vertises.Add(end - vector * num2);
			m_vertises.Add(end + vector * num2);
			float num3 = m_uvScale * Vector3.Distance(start, end);
			m_uvs.Add(new Vector2(m_uvOffset, 0f));
			m_uvs.Add(new Vector2(m_uvOffset, 1f));
			m_uvs.Add(new Vector2(m_uvOffset + num3, 0f));
			m_uvs.Add(new Vector2(m_uvOffset + num3, 1f));
		}
	}

	public void DrawLine(float scale, Vector3 camPos, List<Vector3> points, float width)
	{
		float num = m_uvOffset;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 vector = points[i];
			float num2 = 1f;
			if (m_distanceScale)
			{
				num2 = 0.05f * scale * Vector3.Distance(vector, camPos);
			}
			Vector3 vector2 = Vector3.zero;
			Vector3 vector4;
			if (i == points.Count - 1)
			{
				Vector3 vector3 = points[i - 1];
				Vector3 normalized = (vector - vector3).normalized;
				vector4 = Vector3.Cross(normalized, Vector3.up) * (width / 2f);
			}
			else
			{
				vector2 = points[i + 1];
				Vector3 normalized2 = (vector2 - vector).normalized;
				vector4 = Vector3.Cross(normalized2, Vector3.up) * (width / 2f);
			}
			m_vertises.Add(vector - vector4 * num2);
			m_vertises.Add(vector + vector4 * num2);
			m_uvs.Add(new Vector2(num, 0f));
			m_uvs.Add(new Vector2(num, 1f));
			if (i < points.Count - 1)
			{
				num += m_uvScale * Vector3.Distance(vector, vector2) * (1f / num2);
				int num3 = m_vertises.Count - 2;
				m_indices.Add(num3);
				m_indices.Add(num3 + 1);
				m_indices.Add(num3 + 2);
				m_indices.Add(num3 + 2);
				m_indices.Add(num3 + 1);
				m_indices.Add(num3 + 3);
			}
		}
	}
}
