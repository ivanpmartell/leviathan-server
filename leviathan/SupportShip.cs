#define DEBUG
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Units/SupportShip")]
public class SupportShip : Ship
{
	public int m_resources = 1000;

	public int m_maxResources = 1000;

	public float m_supplyDelay = 0.5f;

	public float m_supplyRadius = 20f;

	public GameObject m_supplyEffect;

	public int m_resupplyRate = 2;

	private float m_supplyEffectTimer;

	private float m_supplyTimer;

	private int m_supplyMask;

	private int m_supplyAreaLineType;

	private int m_supplyAreaDisabledLineType;

	private bool m_supplyEnabled = true;

	public override void Awake()
	{
		base.Awake();
		m_supplyMask = (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("projectiles"));
		m_supplyEffect.GetComponent<ParticleSystem>().enableEmission = false;
		m_resources = m_maxResources;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetObj.m_simulating || IsDead())
		{
			return;
		}
		m_supplyTimer += Time.fixedDeltaTime;
		if (!(m_supplyTimer >= m_supplyDelay))
		{
			return;
		}
		m_supplyTimer -= m_supplyDelay;
		if (m_resources < m_maxResources)
		{
			m_resources += (int)((float)m_resupplyRate * m_supplyDelay);
			if (m_resources > m_maxResources)
			{
				m_resources = m_maxResources;
			}
		}
		SupplyUnitsInRadius();
	}

	public override void Update()
	{
		base.Update();
		if (!IsDead())
		{
			DrawSupplyArea();
		}
		if (m_supplyEffectTimer > 0f)
		{
			m_supplyEffectTimer -= Time.deltaTime;
			if (m_supplyEffectTimer <= 0f && m_supplyEffect != null)
			{
				m_supplyEffect.GetComponent<ParticleSystem>().enableEmission = false;
			}
		}
	}

	public override string GetTooltip()
	{
		string tooltip = base.GetTooltip();
		return tooltip + "Supply:" + m_resources;
	}

	protected override bool SetupLineDrawer()
	{
		if (base.SetupLineDrawer())
		{
			m_supplyAreaLineType = m_lineDrawer.GetTypeID("supplyArea");
			m_supplyAreaDisabledLineType = m_lineDrawer.GetTypeID("supplyAreaDisabled");
			DebugUtils.Assert(m_supplyAreaLineType > 0);
			return true;
		}
		return false;
	}

	private void DrawSupplyArea()
	{
		if (SetupLineDrawer() && GetOwnerTeam() == TurnMan.instance.GetPlayerTeam(NetObj.m_localPlayerID))
		{
			Vector3 position = base.transform.position;
			position.y += 2f;
			if (m_supplyEnabled)
			{
				m_lineDrawer.DrawXZCircle(position, m_supplyRadius, 40, m_supplyAreaLineType, 0.15f);
			}
			else
			{
				m_lineDrawer.DrawXZCircle(position, m_supplyRadius, 40, m_supplyAreaDisabledLineType, 0.15f);
			}
		}
	}

	private void SupplyUnitsInRadius()
	{
		if (!m_supplyEnabled || m_resources <= 0)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, m_supplyRadius, m_supplyMask);
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		int resources = m_resources;
		int ownerTeam = GetOwnerTeam();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Mine component = collider.GetComponent<Mine>();
			if (component != null)
			{
				component.Disarm();
			}
			else
			{
				if (collider.attachedRigidbody == null || collider.attachedRigidbody.gameObject == base.gameObject)
				{
					continue;
				}
				Unit component2 = collider.attachedRigidbody.GetComponent<Unit>();
				if (!(component2 == null) && !(this == component2) && component2.GetOwnerTeam() == ownerTeam && !hashSet.Contains(component2.gameObject))
				{
					hashSet.Add(component2.gameObject);
					component2.Supply(ref m_resources);
					if (m_resources <= 0)
					{
						break;
					}
				}
			}
		}
		if (m_resources != resources)
		{
			m_supplyEffectTimer = m_supplyDelay + 0.1f;
			if (m_supplyEffect != null)
			{
				m_supplyEffect.GetComponent<ParticleSystem>().enableEmission = true;
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_resources);
		writer.Write(m_supplyTimer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_resources = reader.ReadInt32();
		m_supplyTimer = reader.ReadSingle();
	}

	public override void SaveOrders(BinaryWriter stream)
	{
		base.SaveOrders(stream);
		stream.Write(m_supplyEnabled);
	}

	public override void LoadOrders(BinaryReader stream)
	{
		base.LoadOrders(stream);
		m_supplyEnabled = stream.ReadBoolean();
	}

	public int GetResources()
	{
		return m_resources;
	}

	public int GetMaxResources()
	{
		return m_maxResources;
	}

	public bool IsSupplyEnabled()
	{
		return m_supplyEnabled;
	}

	public void SetSupplyEnabled(bool enabled)
	{
		m_supplyEnabled = enabled;
	}
}
