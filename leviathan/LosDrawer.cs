#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

internal class LosDrawer
{
	private Material m_material;

	private LineDrawer m_lineDrawer;

	private int m_sightRangeLineType;

	private int m_sightRangeSelectedLineType;

	private Mesh m_circleMesh;

	private Material m_quadMaterial;

	private Material m_clearDepthMaterial;

	private Mesh m_quadMesh;

	private Mesh m_clearMesh;

	public LosDrawer()
	{
		m_material = Resources.Load("circletest") as Material;
		m_quadMaterial = Resources.Load("cutplane") as Material;
		m_clearDepthMaterial = Resources.Load("cleardepth") as Material;
		m_circleMesh = CreateCircle(new Vector3(0f, 0f, 0f), 1f, 40);
		m_quadMesh = CreateBoxMesh(5000f, 0f);
		m_clearMesh = CreateBoxMesh(5000f, -5f);
	}

	private Mesh CreateBoxMesh(float size, float height)
	{
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		ref Vector3 reference = ref array[0];
		reference = new Vector3(0f - size, height, 0f - size);
		ref Vector3 reference2 = ref array[1];
		reference2 = new Vector3(size, height, 0f - size);
		ref Vector3 reference3 = ref array[2];
		reference3 = new Vector3(size, height, size);
		ref Vector3 reference4 = ref array[3];
		reference4 = new Vector3(0f - size, height, size);
		ref Vector2 reference5 = ref array2[0];
		reference5 = new Vector2(0f - size, 0f - size);
		ref Vector2 reference6 = ref array2[1];
		reference6 = new Vector2(size, 0f - size);
		ref Vector2 reference7 = ref array2[2];
		reference7 = new Vector2(size, size);
		ref Vector2 reference8 = ref array2[3];
		reference8 = new Vector2(0f - size, size);
		array3[0] = 0;
		array3[1] = 1;
		array3[2] = 3;
		array3[3] = 1;
		array3[4] = 2;
		array3[5] = 3;
		Mesh mesh = new Mesh();
		mesh.name = "LosMesh";
		mesh.vertices = array;
		mesh.triangles = array3;
		mesh.uv = array2;
		mesh.RecalculateBounds();
		return mesh;
	}

	private bool SetupLineDrawer()
	{
		if (Camera.main != null)
		{
			m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
			if (!m_lineDrawer)
			{
				return false;
			}
			m_sightRangeLineType = m_lineDrawer.GetTypeID("sightRange");
			m_sightRangeSelectedLineType = m_lineDrawer.GetTypeID("sightRangeSelected");
			DebugUtils.Assert(m_sightRangeLineType >= 0 && m_sightRangeSelectedLineType >= 0);
			return true;
		}
		return false;
	}

	public void Draw()
	{
		if (!SetupLineDrawer())
		{
			return;
		}
		int localPlayer = NetObj.GetLocalPlayer();
		if (localPlayer < 0)
		{
			return;
		}
		int playerTeam = TurnMan.instance.GetPlayerTeam(localPlayer);
		Graphics.DrawMesh(m_clearMesh, Matrix4x4.identity, m_clearDepthMaterial, 0, null, 0, null, castShadows: false, receiveShadows: false);
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Unit unit = item as Unit;
			if (!unit || unit.GetOwnerTeam() != playerTeam || !unit.CanLOS())
			{
				continue;
			}
			Vector3 position = unit.transform.position;
			position.y = 3f;
			float sightRange = unit.GetSightRange();
			if (sightRange != 0f)
			{
				if (unit.IsSelected())
				{
					m_lineDrawer.DrawXZCircle(position, sightRange, 40, m_sightRangeSelectedLineType, 0.2f);
				}
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(position, Quaternion.identity, new Vector3(sightRange, sightRange, sightRange));
				Graphics.DrawMesh(m_circleMesh, matrix, m_material, 0, null, 0, null, castShadows: false, receiveShadows: false);
			}
		}
		Graphics.DrawMesh(m_quadMesh, Matrix4x4.identity, m_quadMaterial, 0, null, 0, null, castShadows: false, receiveShadows: false);
	}

	private Mesh CreateCircle(Vector3 center, float radius, int sections)
	{
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		list.Add(center);
		list3.Add(new Vector2(center.x, center.z));
		Vector3 item = center + new Vector3(0f, 0f, radius);
		list.Add(item);
		list3.Add(new Vector2(item.x, item.z));
		float num = (float)Math.PI * 2f / (float)sections;
		for (float num2 = 0f; num2 <= (float)Math.PI * 2f + num; num2 += num)
		{
			Vector3 item2 = center + new Vector3(Mathf.Sin(num2) * radius, 0f, Mathf.Cos(num2) * radius);
			list2.Add(0);
			list2.Add(list.Count - 1);
			list2.Add(list.Count);
			list.Add(item2);
			list3.Add(new Vector2(item2.x, item2.z));
		}
		Mesh mesh = new Mesh();
		mesh.name = "LosCircle";
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.RecalculateBounds();
		return mesh;
	}
}
