using System;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
	public LineType[] m_lineTypes;

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		LineType[] lineTypes = m_lineTypes;
		foreach (LineType lineType in lineTypes)
		{
			lineType.Draw(deltaTime);
		}
	}

	public void DrawLine(Vector3 start, Vector3 end, int type)
	{
		DrawLine(start, end, type, 1f);
	}

	public void DrawLine(Vector3 start, Vector3 end, int type, float width)
	{
		float scale = Mathf.Tan((float)Math.PI / 180f * base.camera.fieldOfView * 0.5f);
		GetLineType(type)?.DrawLine(scale, base.transform.position, start, end, width);
	}

	public void DrawCurvedLine(Vector3 start, Vector3 end, Vector3 offset, int type, float width, int sections)
	{
		LineType lineType = GetLineType(type);
		if (lineType != null)
		{
			float scale = Mathf.Tan((float)Math.PI / 180f * base.camera.fieldOfView * 0.5f);
			Vector3 vector = (end - start) / sections;
			Vector3 start2 = start;
			for (int i = 0; i <= sections; i++)
			{
				float f = (float)i / (float)sections * (float)Math.PI;
				Vector3 vector2 = start + i * vector + Mathf.Sin(f) * offset;
				lineType.DrawLine(scale, base.transform.position, start2, vector2, width);
				start2 = vector2;
			}
		}
	}

	public void DrawLine(List<Vector3> points, int type, float width)
	{
		if (points.Count >= 2)
		{
			float scale = Mathf.Tan((float)Math.PI / 180f * base.camera.fieldOfView * 0.5f);
			GetLineType(type)?.DrawLine(scale, base.transform.position, points, width);
		}
	}

	public void DrawXZCircle(Vector3 center, float radius, int sections, int type, float lineWidth)
	{
		float scale = Mathf.Tan((float)Math.PI / 180f * base.camera.fieldOfView * 0.5f);
		Vector3 position = base.transform.position;
		LineType lineType = GetLineType(type);
		Vector3 start = center + new Vector3(0f, 0f, radius);
		float num = (float)Math.PI * 2f / (float)sections;
		for (float num2 = 0f; num2 <= (float)Math.PI * 2f + num; num2 += num)
		{
			Vector3 vector = center + new Vector3(Mathf.Sin(num2) * radius, 0f, Mathf.Cos(num2) * radius);
			lineType.DrawLine(scale, position, start, vector, lineWidth);
			start = vector;
		}
	}

	private LineType GetLineType(int id)
	{
		if (id < 0 || id >= m_lineTypes.Length)
		{
			return null;
		}
		return m_lineTypes[id];
	}

	public int GetTypeID(string name)
	{
		for (int i = 0; i < m_lineTypes.Length; i++)
		{
			if (m_lineTypes[i].m_name == name)
			{
				return i;
			}
		}
		PLog.LogError("Missing line type " + name);
		return -1;
	}
}
