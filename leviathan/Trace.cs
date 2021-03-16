using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Trace : MonoBehaviour
{
	private struct Point
	{
		public Vector3 pos;

		public float time;
	}

	public Material m_material;

	public float m_width = 5f;

	public float m_ttl = 3f;

	public float m_sectionLength = 4f;

	public float m_uvScale = 0.1f;

	private Mesh[] m_lineMesh;

	private int m_currentMesh;

	private bool m_dirty = true;

	private bool m_die;

	private bool m_visible = true;

	private float m_time;

	private List<Vector3> m_vertises = new List<Vector3>();

	private List<int> m_indices = new List<int>();

	private List<Vector2> m_uvs = new List<Vector2>();

	private List<Color> m_colors = new List<Color>();

	private List<Point> m_points = new List<Point>();

	private void Start()
	{
		m_lineMesh = new Mesh[2]
		{
			new Mesh(),
			new Mesh()
		};
		m_lineMesh[0].name = "Trace mesh 0";
		m_lineMesh[1].name = "Trace mesh 1";
	}

	private void FixedUpdate()
	{
		if (m_die || NetObj.IsSimulating())
		{
			m_time += Time.fixedDeltaTime;
			if (!m_die)
			{
				AddPoint(base.transform.position);
			}
			if (m_die && (m_points.Count <= 2 || !m_visible))
			{
				Object.Destroy(base.gameObject);
			}
			if (m_points.Count > 1 && m_time - m_points[1].time > m_ttl)
			{
				m_points.RemoveAt(0);
				m_dirty = true;
			}
		}
	}

	private void Update()
	{
		if (m_visible && m_points.Count >= 2)
		{
			if (m_dirty)
			{
				m_dirty = false;
				UpdateMesh();
			}
			Graphics.DrawMesh(m_lineMesh[m_currentMesh], Matrix4x4.identity, m_material, 0, null, 0, null, castShadows: false, receiveShadows: false);
		}
	}

	public void Die()
	{
		m_die = true;
		base.transform.parent = null;
	}

	public void SetVisible(bool visible)
	{
		m_visible = visible;
	}

	private void UpdateMesh()
	{
		m_currentMesh = ((m_currentMesh == 0) ? 1 : 0);
		float num = m_width / 2f;
		float num2 = (float)(-m_points.Count) * m_uvScale;
		for (int i = 0; i < m_points.Count; i++)
		{
			Point? point = m_points[i];
			Vector3 pos = point.Value.pos;
			Vector3 zero = Vector3.zero;
			Vector3 lhs;
			if (i == m_points.Count - 1)
			{
				Vector3 pos2 = m_points[i - 1].pos;
				lhs = pos - pos2;
			}
			else
			{
				zero = m_points[i + 1].pos;
				lhs = zero - pos;
			}
			if (lhs.magnitude < 0.001f)
			{
				lhs.Set(1f, 0f, 0f);
			}
			lhs.Normalize();
			float num3 = Mathf.Max(0f, m_time - point.Value.time);
			Vector3 vector = Vector3.Cross(lhs, Vector3.up) * num;
			m_vertises.Add(pos - vector);
			m_vertises.Add(pos + vector);
			float time = point.Value.time;
			m_uvs.Add(new Vector2(time, 0f));
			m_uvs.Add(new Vector2(time, 1f));
			float a = (m_ttl - num3) / m_ttl;
			if (i == m_points.Count - 1)
			{
				a = 0f;
			}
			m_colors.Add(new Color(1f, 1f, 1f, a));
			m_colors.Add(new Color(1f, 1f, 1f, a));
			if (i < m_points.Count - 1)
			{
				num2 += m_uvScale;
				int num4 = m_vertises.Count - 2;
				m_indices.Add(num4);
				m_indices.Add(num4 + 1);
				m_indices.Add(num4 + 2);
				m_indices.Add(num4 + 2);
				m_indices.Add(num4 + 1);
				m_indices.Add(num4 + 3);
			}
		}
		Mesh mesh = m_lineMesh[m_currentMesh];
		mesh.Clear();
		mesh.vertices = m_vertises.ToArray();
		mesh.uv = m_uvs.ToArray();
		mesh.colors = m_colors.ToArray();
		mesh.triangles = m_indices.ToArray();
		Vector3 center = (m_points[0].pos + m_points[m_points.Count - 1].pos) * 0.5f;
		float num5 = Vector3.Distance(m_points[0].pos, m_points[m_points.Count - 1].pos);
		mesh.bounds = new Bounds(center, new Vector3(num5, num5, num5));
		m_vertises.Clear();
		m_indices.Clear();
		m_uvs.Clear();
		m_colors.Clear();
	}

	private void AddPoint(Vector3 point)
	{
		if (m_points.Count == 0)
		{
			Point item = default(Point);
			item.pos = point;
			item.time = m_time;
			m_points.Add(item);
			m_dirty = true;
			return;
		}
		float num = Vector3.Distance(point, m_points[m_points.Count - 1].pos);
		if (num > m_sectionLength)
		{
			Point item2 = default(Point);
			item2.pos = point;
			item2.time = m_time;
			m_points.Add(item2);
			m_dirty = true;
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(m_time);
		writer.Write((short)m_points.Count);
		foreach (Point point in m_points)
		{
			writer.Write(point.pos.x);
			writer.Write(point.pos.y);
			writer.Write(point.pos.z);
			writer.Write(point.time);
		}
	}

	public void Load(BinaryReader reader)
	{
		m_time = reader.ReadSingle();
		int num = reader.ReadInt16();
		m_points.Clear();
		for (int i = 0; i < num; i++)
		{
			Point item = default(Point);
			item.pos.x = reader.ReadSingle();
			item.pos.y = reader.ReadSingle();
			item.pos.z = reader.ReadSingle();
			item.time = reader.ReadSingle();
			m_points.Add(item);
		}
		m_dirty = true;
	}
}
