#define DEBUG
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Radar")]
public class Radar : HPModule
{
	private class RadarEcho
	{
		public Vector3 m_point;

		public float m_size;
	}

	public float m_maxEnergy = 100f;

	public float m_chargeRate = 1f;

	public float m_range = 200f;

	public float m_minSpeed = 2f;

	public float m_visibleDuration = 15f;

	public Material m_radarMaterial;

	public GameObject m_pingEffect;

	private bool m_deploy;

	private float m_energy = 100f;

	private float m_visibleTimer;

	private List<RadarEcho> m_echoes = new List<RadarEcho>();

	private Mesh m_mesh;

	private bool m_dirty;

	private List<Vector3> m_vertises = new List<Vector3>();

	private List<int> m_indices = new List<int>();

	private List<Vector2> m_uvs = new List<Vector2>();

	private LineDrawer m_lineDrawer;

	private int m_supplyAreaLineType;

	public override void Awake()
	{
		base.Awake();
		m_energy = m_maxEnergy;
		m_mesh = new Mesh();
		m_mesh.name = "RadarMesh";
	}

	public override void SetDeploy(bool deploy)
	{
		m_deploy = deploy;
	}

	public override bool GetDeploy()
	{
		return m_deploy;
	}

	public override StatusWnd_HPModule CreateStatusWindow(GameObject guiCamera)
	{
		bool friendly = GetOwner() == NetObj.m_localPlayerID;
		return new StatusWnd_Shield(this, guiCamera, friendly);
	}

	public override string GetStatusText()
	{
		return "Doing fart sounds";
	}

	public override void GetChargeLevel(out float i, out float time)
	{
		if (m_disabled)
		{
			base.GetChargeLevel(out i, out time);
			return;
		}
		i = m_energy / m_maxEnergy;
		time = (m_maxEnergy - m_energy) / m_chargeRate;
	}

	public override void SaveOrders(BinaryWriter stream)
	{
		base.SaveOrders(stream);
		stream.Write(m_deploy);
		stream.Write(m_energy);
		stream.Write(m_visibleTimer);
		stream.Write(m_echoes.Count);
		foreach (RadarEcho echo in m_echoes)
		{
			stream.Write(echo.m_point.x);
			stream.Write(echo.m_point.y);
			stream.Write(echo.m_point.z);
			stream.Write(echo.m_size);
		}
	}

	public override void LoadOrders(BinaryReader stream)
	{
		base.LoadOrders(stream);
		m_deploy = stream.ReadBoolean();
		m_energy = stream.ReadSingle();
		m_visibleTimer = stream.ReadSingle();
		int num = stream.ReadInt32();
		m_echoes.Clear();
		for (int i = 0; i < num; i++)
		{
			RadarEcho radarEcho = new RadarEcho();
			radarEcho.m_point.x = stream.ReadSingle();
			radarEcho.m_point.y = stream.ReadSingle();
			radarEcho.m_point.z = stream.ReadSingle();
			radarEcho.m_size = stream.ReadSingle();
			m_echoes.Add(radarEcho);
		}
		m_dirty = true;
	}

	protected override void FixedUpdate()
	{
		if (m_unit == null || TurnMan.instance == null)
		{
			return;
		}
		base.FixedUpdate();
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (m_visibleTimer > 0f)
		{
			m_visibleTimer -= Time.fixedDeltaTime;
			if (m_visibleTimer <= 0f)
			{
				m_echoes.Clear();
				m_dirty = true;
			}
		}
		if (IsDisabled() || m_unit.IsDead())
		{
			return;
		}
		if (m_deploy && m_energy >= m_maxEnergy)
		{
			m_deploy = false;
			m_energy = 0f;
			m_visibleTimer = m_visibleDuration;
			DoRadarPing();
		}
		if (m_energy < m_maxEnergy)
		{
			m_energy += m_chargeRate * Time.fixedDeltaTime;
			if (m_energy > m_maxEnergy)
			{
				m_energy = m_maxEnergy;
			}
		}
	}

	public override void Update()
	{
		if (m_unit == null || TurnMan.instance == null)
		{
			return;
		}
		base.Update();
		if (IsDisabled() || m_unit.IsDead())
		{
			return;
		}
		DrawRadarArea();
		if (GetOwnerTeam() == TurnMan.instance.GetPlayerTeam(NetObj.m_localPlayerID) && m_echoes.Count > 0)
		{
			if (m_dirty)
			{
				m_dirty = false;
				UpdateMesh();
			}
			float a = Mathf.Clamp(m_visibleDuration - m_visibleTimer, 0f, 1f);
			float b = Mathf.Clamp(m_visibleTimer, 0f, 1f);
			float a2 = Mathf.Min(a, b);
			Color color = m_radarMaterial.color;
			color.a = a2;
			m_radarMaterial.color = color;
			Graphics.DrawMesh(m_mesh, Matrix4x4.identity, m_radarMaterial, 0, null, 0, null, castShadows: false, receiveShadows: false);
		}
	}

	protected virtual bool SetupLineDrawer()
	{
		if (m_lineDrawer == null)
		{
			m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
			if (m_lineDrawer == null)
			{
				return false;
			}
			m_supplyAreaLineType = m_lineDrawer.GetTypeID("radar");
			DebugUtils.Assert(m_supplyAreaLineType >= 0);
		}
		return true;
	}

	private void DrawRadarArea()
	{
		if (SetupLineDrawer() && GetOwnerTeam() == TurnMan.instance.GetPlayerTeam(NetObj.m_localPlayerID) && m_unit.IsSelected())
		{
			Vector3 position = base.transform.position;
			position.y += 2f;
			m_lineDrawer.DrawXZCircle(position, m_range, 40, m_supplyAreaLineType, 0.15f);
		}
	}

	private void UpdateMesh()
	{
		float y = 0f;
		foreach (RadarEcho echo in m_echoes)
		{
			int count = m_vertises.Count;
			m_indices.Add(count);
			m_indices.Add(count + 1);
			m_indices.Add(count + 2);
			m_indices.Add(count + 2);
			m_indices.Add(count + 3);
			m_indices.Add(count);
			float size = echo.m_size;
			m_vertises.Add(echo.m_point + new Vector3(0f - size, y, 0f - size));
			m_vertises.Add(echo.m_point + new Vector3(size, y, 0f - size));
			m_vertises.Add(echo.m_point + new Vector3(size, y, size));
			m_vertises.Add(echo.m_point + new Vector3(0f - size, y, size));
			m_uvs.Add(new Vector2(0f, 0f));
			m_uvs.Add(new Vector2(1f, 0f));
			m_uvs.Add(new Vector2(1f, 1f));
			m_uvs.Add(new Vector2(0f, 1f));
		}
		m_mesh.Clear();
		m_mesh.vertices = m_vertises.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
		m_vertises.Clear();
		m_indices.Clear();
		m_uvs.Clear();
	}

	public override float GetMaxEnergy()
	{
		return m_maxEnergy;
	}

	public override float GetEnergy()
	{
		return m_energy;
	}

	private void DoRadarPing()
	{
		if (MessageLog.instance != null && m_unit.GetOwner() != NetObj.m_localPlayerID)
		{
			MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, "$label_radarpingdetected", string.Empty, string.Empty, 2f);
		}
		if (IsVisible() && (bool)m_pingEffect)
		{
			Object.Instantiate(m_pingEffect, base.transform.position + new Vector3(0f, 5f, 0f), Quaternion.identity);
		}
		if (NetObj.m_phase != 0)
		{
			List<NetObj> all = NetObj.GetAll();
			m_echoes.Clear();
			Vector3 position = base.transform.position;
			int owner = GetOwner();
			foreach (NetObj item in all)
			{
				Unit unit = item as Unit;
				if (!(unit == null) && !item.IsSeenByPlayer(owner))
				{
					Vector3 position2 = item.transform.position;
					float num = Vector3.Distance(position2, position);
					if (num < m_range && unit.GetVelocity().magnitude > m_minSpeed)
					{
						RadarEcho radarEcho = new RadarEcho();
						radarEcho.m_point = position2;
						radarEcho.m_size = unit.GetLength() / 2f;
						m_echoes.Add(radarEcho);
					}
				}
			}
		}
		m_dirty = true;
	}

	public override string GetTooltip()
	{
		string text = GetName() + "\nHP: " + m_health;
		string text2 = text;
		return text2 + "\nEnergy: " + m_energy + " / " + m_maxEnergy;
	}
}
