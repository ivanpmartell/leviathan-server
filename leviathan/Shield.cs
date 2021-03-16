using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Shield")]
public class Shield : HPModule
{
	public enum DeployType
	{
		None,
		Forward,
		Backward,
		Left,
		Right
	}

	public GameObject m_shieldPrefab;

	public GameObject m_shieldPreviewPrefab;

	public float m_maxEnergy = 100f;

	public float m_chargeRate = 1f;

	public float m_drainRate = 10f;

	private DeployType m_deploy;

	private DeployType m_deployed;

	private float m_energy = 100f;

	private GameObject m_shield;

	private GameObject m_shieldPreview;

	private float m_timeInTurn;

	public override void Awake()
	{
		base.Awake();
		m_energy = m_maxEnergy;
	}

	public override StatusWnd_HPModule CreateStatusWindow(GameObject guiCamera)
	{
		bool friendly = GetOwner() == NetObj.m_localPlayerID;
		return new StatusWnd_Shield(this, guiCamera, friendly);
	}

	public void SetDeployShield(DeployType type)
	{
		m_deploy = type;
		if (NetObj.m_phase == TurnPhase.Planning && NetObj.m_localPlayerID == GetOwner())
		{
			SetupShieldPreview(type);
		}
	}

	private void SetupShieldPreview(DeployType type)
	{
		if (m_shieldPreview != null)
		{
			Object.Destroy(m_shieldPreview);
			m_shieldPreview = null;
		}
		if (type != 0)
		{
			m_shieldPreview = Object.Instantiate(m_shieldPreviewPrefab) as GameObject;
			SetupShieldDirection(type, m_shieldPreview);
		}
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		if (m_shield != null)
		{
			ShieldGeometry component = m_shield.GetComponent<ShieldGeometry>();
			if (component != null)
			{
				component.SetVisible(visible);
			}
		}
	}

	public DeployType GetDeployShield()
	{
		return m_deploy;
	}

	public override string GetStatusText()
	{
		if (m_deployed != 0)
		{
			return "Draining";
		}
		if (m_energy < m_maxEnergy)
		{
			return "Charging";
		}
		return "Standby";
	}

	public override void SaveOrders(BinaryWriter stream)
	{
		base.SaveOrders(stream);
		stream.Write((byte)m_deploy);
		stream.Write((byte)m_deployed);
		stream.Write(m_energy);
	}

	public override void LoadOrders(BinaryReader stream)
	{
		base.LoadOrders(stream);
		DeployType deployShield = (DeployType)stream.ReadByte();
		SetDeployShield(deployShield);
		m_deployed = (DeployType)stream.ReadByte();
		m_energy = stream.ReadSingle();
		if (m_deployed != 0)
		{
			Activate(m_deployed, firstTime: false);
		}
	}

	public override void GetChargeLevel(out float i, out float time)
	{
		if (m_disabled)
		{
			base.GetChargeLevel(out i, out time);
			return;
		}
		i = m_energy / m_maxEnergy;
		if (m_deployed != 0)
		{
			time = m_energy / m_drainRate;
		}
		else
		{
			time = (m_maxEnergy - m_energy) / m_chargeRate;
		}
	}

	private void Activate(DeployType type, bool firstTime)
	{
		m_deployed = type;
		if (m_shield == null)
		{
			m_shield = Object.Instantiate(m_shieldPrefab) as GameObject;
			SetupShieldDirection(type, m_shield);
			m_shield.GetComponent<ShieldGeometry>().Setup(m_unit, this, firstTime);
		}
	}

	private void SetupShieldDirection(DeployType type, GameObject shield)
	{
		Vector3 vector = Vector3.zero;
		Vector3 forward = Vector3.zero;
		switch (type)
		{
		case DeployType.Forward:
			vector = m_unit.transform.TransformDirection(new Vector3(0f, 0f, m_unit.GetLength() / 2f));
			forward = m_unit.transform.TransformDirection(Vector3.forward);
			break;
		case DeployType.Backward:
			vector = m_unit.transform.TransformDirection(new Vector3(0f, 0f, (0f - m_unit.GetLength()) / 2f));
			forward = m_unit.transform.TransformDirection(-Vector3.forward);
			break;
		case DeployType.Left:
			vector = m_unit.transform.TransformDirection(new Vector3((0f - m_unit.GetWidth()) / 2f, 0f, 0f));
			forward = m_unit.transform.TransformDirection(Vector3.left);
			break;
		case DeployType.Right:
			vector = m_unit.transform.TransformDirection(new Vector3(m_unit.GetWidth() / 2f, 0f, 0f));
			forward = m_unit.transform.TransformDirection(Vector3.right);
			break;
		}
		shield.transform.parent = base.gameObject.transform;
		shield.transform.position = m_unit.transform.position;
		shield.transform.position += vector;
		shield.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
	}

	private void Deactivate(bool destroyed)
	{
		if (m_shield != null)
		{
			m_shield.GetComponent<ShieldGeometry>().Deactivate(destroyed);
		}
		m_deployed = DeployType.None;
	}

	private UnitAi.AttackDirection GetShieldDirection()
	{
		Battery component = base.transform.parent.gameObject.GetComponent<Battery>();
		switch (component.GetSectionType())
		{
		case Section.SectionType.Front:
			return UnitAi.AttackDirection.Front;
		case Section.SectionType.Rear:
			return UnitAi.AttackDirection.Back;
		case Section.SectionType.Mid:
			if ((base.transform.position - GetUnit().transform.position).x < 0f)
			{
				return UnitAi.AttackDirection.Right;
			}
			return UnitAi.AttackDirection.Left;
		default:
			return UnitAi.AttackDirection.None;
		}
	}

	private DeployType AiToShieldDir(UnitAi.AttackDirection dir)
	{
		return dir switch
		{
			UnitAi.AttackDirection.None => DeployType.None, 
			UnitAi.AttackDirection.Front => DeployType.Forward, 
			UnitAi.AttackDirection.Back => DeployType.Backward, 
			UnitAi.AttackDirection.Left => DeployType.Left, 
			UnitAi.AttackDirection.Right => DeployType.Right, 
			_ => DeployType.None, 
		};
	}

	private void UpdateShieldAi()
	{
		if ((double)m_timeInTurn == 0.0)
		{
			m_timeInTurn += Time.fixedDeltaTime;
			if (GetOwner() > 3 && !(m_energy < m_maxEnergy))
			{
				Unit unit = NetObj.GetByID(GetUnit().GetAi().m_targetId) as Unit;
				if (!(unit == null))
				{
					UnitAi.AttackDirection attackDirection = GetUnit().GetAi().GetAttackDirection(unit.transform.position);
					DeployType deployType = (m_deploy = AiToShieldDir(attackDirection));
				}
			}
		}
		else
		{
			m_timeInTurn += Time.fixedDeltaTime;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetObj.m_simulating)
		{
			return;
		}
		UpdateShieldAi();
		if (m_deployed == DeployType.None && m_deploy != 0 && m_energy >= m_maxEnergy)
		{
			Activate(m_deploy, firstTime: true);
			m_deploy = DeployType.None;
		}
		if (m_deployed != 0)
		{
			Drain(m_drainRate * Time.fixedDeltaTime, damaged: false);
		}
		else if (!IsDisabled() && !m_unit.IsDead())
		{
			m_energy += m_chargeRate * Time.fixedDeltaTime;
			if (m_energy > m_maxEnergy)
			{
				m_energy = m_maxEnergy;
			}
		}
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		Drain(m_maxEnergy, damaged: true);
	}

	public void Drain(float energy, bool damaged)
	{
		m_energy -= energy;
		if (m_energy <= 0f)
		{
			m_energy = 0f;
			bool destroyed = damaged;
			Deactivate(destroyed);
		}
	}

	public override float GetMaxEnergy()
	{
		return m_maxEnergy;
	}

	public override float GetEnergy()
	{
		return m_energy;
	}

	public override string GetTooltip()
	{
		string text = GetName() + "\nHP: " + m_health;
		string text2 = text;
		return text2 + "\nEnergy: " + m_energy + " / " + m_maxEnergy;
	}

	public override List<string> GetHardpointInfo()
	{
		List<string> list = new List<string>();
		list.Add(Localize.instance.Translate("$Energy") + " " + m_maxEnergy);
		return list;
	}
}
