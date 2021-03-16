using System;
using System.IO;
using UnityEngine;

public class WaterSurface : MonoBehaviour
{
	public GameObject m_camera;

	public float m_waveHeight = 1.5f;

	public bool m_alwaysUpdate;

	public float m_forceCameraHeight = -1f;

	public int m_mapSize = 500;

	public float m_direction;

	public Shader m_lowQualityShader;

	private float m_tileSize = 1f;

	private int m_detail = 1;

	private int m_size;

	private float m_time;

	private bool m_simulating;

	private Mesh m_mesh;

	private Vector3[] m_vertises;

	private Vector3[] m_normals;

	private Vector4[] m_tangents;

	private Vector2 m_rotX;

	private Vector2 m_rotY;

	private void Start()
	{
		if ((bool)m_camera)
		{
			m_camera.camera.depthTextureMode = DepthTextureMode.Depth;
		}
		if (QualitySettings.names[QualitySettings.GetQualityLevel()] == "Very Low")
		{
			base.renderer.material.shader = m_lowQualityShader;
		}
		m_size = 120 / m_detail;
		m_mesh = new Mesh();
		m_vertises = new Vector3[m_size * m_size];
		m_normals = new Vector3[m_size * m_size];
		m_tangents = new Vector4[m_size * m_size];
		int[] array = new int[m_size * m_size * 6];
		int num = 0;
		for (int i = 0; i < m_size - 1; i++)
		{
			for (int j = 0; j < m_size - 1; j++)
			{
				array[num++] = i * m_size + j;
				array[num++] = (i + 1) * m_size + j;
				array[num++] = i * m_size + j + 1;
				array[num++] = i * m_size + j + 1;
				array[num++] = (i + 1) * m_size + j;
				array[num++] = (i + 1) * m_size + j + 1;
			}
		}
		Vector2[] array2 = new Vector2[m_size * m_size];
		int num2 = 0;
		for (int k = 0; k < m_size; k++)
		{
			for (int l = 0; l < m_size; l++)
			{
				ref Vector2 reference = ref array2[num2++];
				reference = new Vector2(l, k);
			}
		}
		Vector3[] array3 = new Vector3[m_size * m_size];
		int num3 = 0;
		for (int m = 0; m < m_size; m++)
		{
			for (int n = 0; n < m_size; n++)
			{
				ref Vector3 reference2 = ref array3[num3++];
				reference2 = new Vector3(0f, 1f, 0f);
			}
		}
		m_mesh.name = "WaterMesh";
		m_mesh.vertices = new Vector3[m_size * m_size];
		m_mesh.normals = array3;
		m_mesh.uv = array2;
		m_mesh.SetTriangles(array, 0);
		m_mesh.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(10000f, 10000f, 10000f));
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		component.mesh = m_mesh;
		GenerateMesh();
		GenerateDepthTexture();
		m_rotX = new Vector2(Mathf.Cos(m_direction), Mathf.Sin(m_direction));
		m_rotY = new Vector2(0f - Mathf.Sin(m_direction), Mathf.Cos(m_direction));
	}

	private void GenerateDepthTexture()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float y = 10f;
		int num = 256;
		int mapSize = m_mapSize;
		float num2 = -10f;
		float num3 = -6f;
		int layerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("beach"));
		int layerMask2 = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("shallow")) | (1 << LayerMask.NameToLayer("beach"));
		float[] array = new float[num * num];
		float[] array2 = new float[num * num];
		Vector3 direction = new Vector3(0f, -1f, 0f);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 origin = new Vector3(((float)j + 0.5f) / (float)num * (float)mapSize - (float)(mapSize / 2), y, ((float)i + 0.5f) / (float)num * (float)mapSize - (float)(mapSize / 2));
				float num4 = num2;
				if (Physics.Raycast(origin, direction, out var hitInfo, 100f, layerMask2))
				{
					num4 = hitInfo.point.y;
				}
				if (num4 > 1f)
				{
					num4 = 1f;
				}
				array[j + i * num] = num4;
				float num5 = num2;
				if (Physics.Raycast(origin, direction, out hitInfo, 100f, layerMask))
				{
					num5 = hitInfo.point.y;
				}
				if (num5 > 1f)
				{
					num5 = 1f;
				}
				array2[j + i * num] = num5;
			}
		}
		Texture2D texture2D = new Texture2D(num, num);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				Color color = default(Color);
				float num6 = array[l + k * num];
				color.r = Mathf.Clamp(1f - num6 / num2, 0f, 1f);
				float num7 = array2[l + k * num];
				float num8 = Mathf.Clamp(1f - num7 / num3, 0f, 1f);
				if ((double)num8 > 0.8)
				{
					num8 = 0f;
				}
				color.a = num8;
				texture2D.SetPixel(l, k, color);
			}
		}
		texture2D.Apply();
		base.renderer.material.SetTexture("_DepthMapTex", texture2D);
		PLog.Log(" GenerateDepthTexture time " + (Time.realtimeSinceStartup - realtimeSinceStartup) + "  size " + m_mapSize);
	}

	public void SetMapSize(int size)
	{
		m_mapSize = size;
	}

	public void SaveState(BinaryWriter writer)
	{
		writer.Write(m_time);
	}

	public void LoadState(BinaryReader reader)
	{
		m_time = reader.ReadSingle();
	}

	private void Update()
	{
		if (m_camera != null)
		{
			float num = Mathf.Tan((float)Math.PI / 180f * m_camera.camera.fieldOfView * m_camera.camera.aspect * 0.5f);
			num /= (float)m_size;
			num *= 3.5f;
			float num2 = 32f * (float)m_detail;
			float num3 = m_camera.transform.position.y;
			if (m_forceCameraHeight > 0f)
			{
				num3 = m_forceCameraHeight;
			}
			float num4 = Mathf.Clamp(num3 * num * (float)m_detail, 4 * m_detail, num2);
			int minPow = Utils.GetMinPow2((int)num4);
			Vector3 position = m_camera.transform.position;
			float num5 = Vector3.Angle(m_camera.transform.TransformDirection(Vector3.forward), Vector3.down);
			float num6 = Mathf.Tan((float)Math.PI / 180f * num5) * m_camera.transform.position.y;
			position.z += num6;
			Vector3 position2 = position - new Vector3((float)(m_size * minPow) * 0.5f, 0f, (float)(m_size * minPow) * 0.5f);
			position2.x = (float)(int)(position2.x / num2) * num2;
			position2.z = (float)(int)(position2.z / num2) * num2;
			position2.y = 0f;
			base.transform.position = position2;
			if ((float)minPow != m_tileSize)
			{
				m_tileSize = minPow;
				GenerateMesh();
			}
		}
		base.renderer.material.SetMatrix("modelMatrix", base.transform.localToWorldMatrix);
		base.renderer.material.SetFloat("mapSize", m_mapSize);
		base.renderer.material.SetFloat("_WaveDirection", m_direction);
	}

	private void FixedUpdate()
	{
		if (m_simulating || m_alwaysUpdate)
		{
			m_time += Time.fixedDeltaTime;
		}
		base.renderer.material.SetFloat("waveHeight", m_waveHeight);
		base.renderer.material.SetFloat("waterTime", m_time);
	}

	private void GenerateMesh()
	{
		int num = 0;
		for (int i = 0; i < m_size; i++)
		{
			for (int j = 0; j < m_size; j++)
			{
				Vector3 vector = new Vector3((float)j * m_tileSize, 0f, (float)i * m_tileSize);
				m_vertises[num++] = vector;
			}
		}
		int num2 = 0;
		for (int k = 0; k < m_size - 1; k++)
		{
			for (int l = 0; l < m_size - 1; l++)
			{
				Vector3 vector2 = m_vertises[k * m_size + l];
				Vector3 vector3 = m_vertises[k * m_size + l + 1];
				Vector3 vector4 = m_vertises[(k + 1) * m_size + l];
				Vector3 normalized = (vector3 - vector2).normalized;
				Vector3 normalized2 = (vector4 - vector2).normalized;
				Vector3 vector5 = Vector3.Cross(normalized2, normalized);
				m_normals[k * m_size + l] = vector5;
				ref Vector4 reference = ref m_tangents[k * m_size + l];
				reference = new Vector4(normalized.x, normalized.y, normalized.z, 1f);
				num2++;
			}
		}
		m_mesh.vertices = m_vertises;
		m_mesh.normals = m_normals;
		m_mesh.tangents = m_tangents;
	}

	private float GetLocalWaveHeightAtWorldPos(Vector3 worldPos)
	{
		Vector2 vector = new Vector2(0f, 0f);
		vector += worldPos.x * m_rotX;
		vector += worldPos.z * m_rotY;
		float num = 0f;
		num += Mathf.Sin(m_time * 0.3f + vector.x * 0.1f) * Mathf.Sin(m_time * 0.2f + vector.x * 0.05f);
		num += Mathf.Sin(m_time * 0.6f + (vector.x + vector.y) * 0.5f) * Mathf.Sin(m_time * 0.7f + (vector.x + vector.y) * 0.2f) * 0.1f;
		return num * m_waveHeight;
	}

	public float GetForceAt(Vector3 worldPos, float boyancy)
	{
		float worldWaveHeight = GetWorldWaveHeight(worldPos);
		if (worldPos.y < worldWaveHeight)
		{
			float num = Mathf.Abs(worldWaveHeight - worldPos.y);
			return num * num * boyancy;
		}
		return 0f;
	}

	public float GetWorldWaveHeight(Vector3 worldPos)
	{
		return base.transform.position.y + GetLocalWaveHeightAtWorldPos(worldPos);
	}

	public void SetSimulating(bool simulating)
	{
		m_simulating = simulating;
	}
}
