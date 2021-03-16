using UnityEngine;

public class waves : MonoBehaviour
{
	public float m_waveHeight;

	public float m_damp;

	public int m_size = 100;

	private float m_maxDepth = 20f;

	private Mesh m_mesh;

	private float[] m_heights;

	private float[] m_vel;

	private float[] m_waterDepth;

	private void Start()
	{
		m_heights = new float[m_size * m_size];
		m_vel = new float[m_size * m_size];
		m_waterDepth = new float[m_size * m_size];
		m_mesh = new Mesh();
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
		m_mesh.vertices = new Vector3[m_size * m_size];
		m_mesh.uv = array2;
		m_mesh.SetTriangles(array, 0);
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		component.mesh = m_mesh;
		GenerateMesh();
		m_mesh.RecalculateBounds();
		DetectDepths();
	}

	private void DetectDepths()
	{
		int layerMask = 1;
		float num = 2f;
		for (int i = 0; i < m_size; i++)
		{
			for (int j = 0; j < m_size; j++)
			{
				Vector3 vector = base.transform.position + new Vector3(j, num, i);
				if (Physics.Linecast(vector, vector + new Vector3(0f, 0f - m_maxDepth, 0f), out var hitInfo, layerMask))
				{
					m_waterDepth[i * m_size + j] = hitInfo.distance - num;
				}
				else
				{
					m_waterDepth[i * m_size + j] = m_maxDepth;
				}
			}
		}
	}

	private void Update()
	{
		UpdateSurface();
		GenerateMesh();
		if (Input.GetMouseButton(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (base.collider.Raycast(ray, out var hitInfo, 100f))
			{
				SetVel(hitInfo.point, -10f);
			}
		}
		for (int i = 0; i < m_size; i++)
		{
			SetOffset(0, i, Mathf.Sin(Time.time) * 2f);
		}
	}

	private void GenerateMesh()
	{
		Vector3[] array = new Vector3[m_size * m_size];
		int num = 0;
		for (int i = 0; i < m_size; i++)
		{
			for (int j = 0; j < m_size; j++)
			{
				float y = GetWaveHeight(j, i) + m_heights[i * m_size + j];
				ref Vector3 reference = ref array[num++];
				reference = new Vector3(j, y, i);
			}
		}
		m_mesh.vertices = array;
		Color[] array2 = new Color[m_size * m_size];
		for (int k = 0; k < m_size; k++)
		{
			for (int l = 0; l < m_size; l++)
			{
				float num2 = m_waterDepth[k * m_size + l] / m_maxDepth;
				ref Color reference2 = ref array2[k * m_size + l];
				reference2 = new Color(num2, num2, num2, 1f);
			}
		}
		m_mesh.colors = array2;
		m_mesh.RecalculateNormals();
	}

	private void UpdateSurface()
	{
		for (int i = 1; i < m_size - 1; i++)
		{
			for (int j = 1; j < m_size - 1; j++)
			{
				float num = m_heights[i * m_size + j];
				float num2 = m_heights[i * m_size + j + 1] - num;
				float num3 = m_heights[i * m_size + j - 1] - num;
				float num4 = m_heights[(i + 1) * m_size + j] - num;
				float num5 = m_heights[(i - 1) * m_size + j] - num;
				float num6 = (num2 + num3 + num4 + num5) / 4f;
				m_vel[i * m_size + j] += num6;
				m_vel[i * m_size + j] -= m_vel[i * m_size + j] * m_damp;
				if (m_vel[i * m_size + j] > 0f)
				{
				}
			}
		}
		for (int k = 0; k < m_size; k++)
		{
			for (int l = 0; l < m_size; l++)
			{
				float num7 = m_vel[k * m_size + l] * Time.deltaTime;
				m_heights[k * m_size + l] += num7;
			}
		}
	}

	private float GetWaveHeight(int x, int y)
	{
		return Mathf.Sin(Time.time * 2f + (float)x * 0.2f) * m_waveHeight;
	}

	private float GetWorldWaveHeight(Vector3 worldPos)
	{
		Vector3 vector = worldPos - base.transform.position;
		int num = (int)vector.x;
		int num2 = (int)vector.z;
		if (num < 0 || num2 < 0 || num >= m_size || num2 >= m_size)
		{
			return 0f;
		}
		return base.transform.position.y + GetWaveHeight(num, num2);
	}

	private void SetOffset(int x, int y, float offset)
	{
		if (x >= 0 && y >= 0 && x < m_size && y < m_size)
		{
			m_heights[y * m_size + x] = offset;
		}
	}

	private void SetOffset(Vector3 worldPos, float offset)
	{
		Vector3 vector = worldPos - base.transform.position;
		int num = (int)vector.x;
		int num2 = (int)vector.z;
		if (num >= 0 && num2 >= 0 && num < m_size && num2 < m_size)
		{
			m_heights[num2 * m_size + num] = offset;
		}
	}

	private void SetVel(Vector3 worldPos, float vel)
	{
		Vector3 vector = worldPos - base.transform.position;
		int num = (int)vector.x;
		int num2 = (int)vector.z;
		if (num >= 0 && num2 >= 0 && num < m_size && num2 < m_size)
		{
			m_vel[num2 * m_size + num] = vel;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!other.attachedRigidbody)
		{
			return;
		}
		BoxCollider boxCollider = other as BoxCollider;
		if (boxCollider == null)
		{
			return;
		}
		Vector3[] array = new Vector3[8];
		Vector3 vector = boxCollider.size * 0.5f;
		ref Vector3 reference = ref array[0];
		reference = other.transform.TransformPoint(boxCollider.center + new Vector3(vector.x, vector.y, vector.z));
		ref Vector3 reference2 = ref array[1];
		reference2 = other.transform.TransformPoint(boxCollider.center + new Vector3(0f - vector.x, vector.y, vector.z));
		ref Vector3 reference3 = ref array[2];
		reference3 = other.transform.TransformPoint(boxCollider.center + new Vector3(vector.x, vector.y, 0f - vector.z));
		ref Vector3 reference4 = ref array[3];
		reference4 = other.transform.TransformPoint(boxCollider.center + new Vector3(0f - vector.x, vector.y, 0f - vector.z));
		ref Vector3 reference5 = ref array[4];
		reference5 = other.transform.TransformPoint(boxCollider.center + new Vector3(vector.x, 0f - vector.y, vector.z));
		ref Vector3 reference6 = ref array[5];
		reference6 = other.transform.TransformPoint(boxCollider.center + new Vector3(0f - vector.x, 0f - vector.y, vector.z));
		ref Vector3 reference7 = ref array[6];
		reference7 = other.transform.TransformPoint(boxCollider.center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z));
		ref Vector3 reference8 = ref array[7];
		reference8 = other.transform.TransformPoint(boxCollider.center + new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z));
		float num = 0.3f;
		float num2 = 0.05f;
		Vector3[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Vector3 vector2 = array2[i];
			Debug.DrawLine(vector2, vector2 + new Vector3(0f, 1f, 0f));
			float worldWaveHeight = GetWorldWaveHeight(vector2);
			if (vector2.y < worldWaveHeight)
			{
				float num3 = vector2.y - worldWaveHeight;
				Vector3 pointVelocity = other.attachedRigidbody.GetPointVelocity(vector2);
				if (Mathf.Abs(num3) < 2f)
				{
					SetVel(vector2, 0f - pointVelocity.magnitude);
				}
				float y = Mathf.Abs(num3 * num3) * num;
				other.attachedRigidbody.AddForceAtPosition(new Vector3(0f, y, 0f), vector2);
				other.attachedRigidbody.AddForceAtPosition(-pointVelocity * num2, vector2);
			}
		}
	}
}
