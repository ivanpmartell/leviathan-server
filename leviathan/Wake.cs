using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Wake : MonoBehaviour
{
	private struct Point
	{
		public Vector3 pos;

		public Vector3 dir;

		public float time;

		public float alpha;
	}

	public Material m_material;

	public float m_width = 5f;

	public float m_ttl = 3f;

	public float m_fadeIn = 1f;

	public float m_sectionLength = 4f;

	public float m_particleLength = 4f;

	public float m_uvScale = 0.1f;

	public float m_minSpeed = -1f;

	public float m_maxSpeed = -1f;

	public float m_scalePower = 2f;

	public bool m_canReverse;

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
		if (m_maxSpeed <= m_minSpeed)
		{
			m_maxSpeed = m_minSpeed + 0.01f;
		}
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
			}
			m_dirty = true;
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
		float particleLength = m_particleLength;
		for (int i = 0; i < m_points.Count; i++)
		{
			Vector3 pos = m_points[i].pos;
			Vector3 dir = m_points[i].dir;
			float num3 = Mathf.Max(0f, m_time - m_points[i].time);
			float alpha = m_points[i].alpha;
			float f = 1f - num3 / m_ttl;
			f = Mathf.Pow(f, m_scalePower);
			f = 1f - f;
			float num4 = num * f;
			Vector3 vector = Vector3.Cross(dir, Vector3.up) * num4;
			m_vertises.Add(pos - vector - dir * particleLength);
			m_vertises.Add(pos + vector - dir * particleLength);
			m_vertises.Add(pos - vector);
			m_vertises.Add(pos + vector);
			m_uvs.Add(new Vector2(0f, 0f));
			m_uvs.Add(new Vector2(0f, 1f));
			m_uvs.Add(new Vector2(1f, 0f));
			m_uvs.Add(new Vector2(1f, 1f));
			float a = num3 / m_fadeIn;
			float b = (m_ttl - num3) / m_ttl;
			float a2 = Mathf.Min(a, b) * alpha;
			m_colors.Add(new Color(1f, 1f, 1f, a2));
			m_colors.Add(new Color(1f, 1f, 1f, a2));
			m_colors.Add(new Color(1f, 1f, 1f, a2));
			m_colors.Add(new Color(1f, 1f, 1f, a2));
			if (i > 0)
			{
				num2 += m_uvScale;
				int num5 = m_vertises.Count - 4;
				m_indices.Add(num5);
				m_indices.Add(num5 + 1);
				m_indices.Add(num5 + 2);
				m_indices.Add(num5 + 2);
				m_indices.Add(num5 + 1);
				m_indices.Add(num5 + 3);
			}
		}
		Mesh mesh = m_lineMesh[m_currentMesh];
		mesh.Clear();
		mesh.vertices = m_vertises.ToArray();
		mesh.uv = m_uvs.ToArray();
		mesh.colors = m_colors.ToArray();
		mesh.triangles = m_indices.ToArray();
		Vector3 center = (m_points[0].pos + m_points[m_points.Count - 1].pos) * 0.5f;
		float num6 = Vector3.Distance(m_points[0].pos, m_points[m_points.Count - 1].pos);
		mesh.bounds = new Bounds(center, new Vector3(num6, num6, num6));
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
			item.alpha = 0f;
			m_points.Add(item);
			return;
		}
		float num = Vector3.Distance(point, m_points[m_points.Count - 1].pos);
		if (!(num > m_sectionLength))
		{
			return;
		}
		Point item2 = default(Point);
		item2.pos = point;
		item2.dir = base.transform.forward;
		item2.time = m_time;
		item2.alpha = 1f;
		if (m_minSpeed >= 0f)
		{
			float num2 = m_time - m_points[m_points.Count - 1].time;
			Vector3 rhs = point - m_points[m_points.Count - 1].pos;
			float num3 = Vector3.Dot(base.transform.forward, rhs) / num2;
			if (m_canReverse)
			{
				num3 = Mathf.Abs(num3);
			}
			float value = (num3 - m_minSpeed) / (m_maxSpeed - m_minSpeed);
			value = (item2.alpha = Mathf.Clamp(value, 0f, 1f));
		}
		m_points.Add(item2);
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
			writer.Write(point.dir.x);
			writer.Write(point.dir.z);
			writer.Write(point.time);
			writer.Write((byte)(point.alpha * 255f));
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
			item.dir.x = reader.ReadSingle();
			item.dir.y = 0f;
			item.dir.z = reader.ReadSingle();
			item.time = reader.ReadSingle();
			item.alpha = (float)(int)reader.ReadByte() / 255f;
			m_points.Add(item);
		}
		m_dirty = true;
	}
}
