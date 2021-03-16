using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Cloak")]
public class Cloak : HPModule
{
	public float m_maxEnergy = 100f;

	public float m_chargeRate = 1f;

	public float m_drainRate = 10f;

	private float m_cloakDelay = 1f;

	public GameObject m_activateEffectPrefab;

	public GameObject m_cloakEffectPrefab;

	private GameObject m_activation;

	private GameObject m_cloak;

	private float m_energy = 100f;

	private bool m_deploy;

	private bool m_deployed;

	private float m_deployTimer = -1f;

	public override void Awake()
	{
		base.Awake();
		m_energy = m_maxEnergy;
	}

	public override void Setup(Unit unit, Battery battery, int x, int y, Direction dir, DestroyedHandler destroyedCallback)
	{
		base.Setup(unit, battery, x, y, dir, destroyedCallback);
		unit.m_onFireWeapon = (Action)Delegate.Combine(unit.m_onFireWeapon, new Action(OnShipFireWeapon));
		unit.m_onTakenDamage = (Action)Delegate.Combine(unit.m_onTakenDamage, new Action(OnShipTakenDamage));
		unit.m_onMaintenanceActivation = (Action)Delegate.Combine(unit.m_onMaintenanceActivation, new Action(OnMaintenanceModeActivation));
	}

	private void OnShipFireWeapon()
	{
		if ((NetObj.m_phase != 0 || GetOwner() == NetObj.m_localPlayerID) && m_deployed)
		{
			PLog.Log("Gun fired, disabling cloak");
			Drain(m_maxEnergy, damaged: false);
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
		if (m_deployed)
		{
			time = m_energy / m_drainRate;
		}
		else
		{
			time = (m_maxEnergy - m_energy) / m_chargeRate;
		}
	}

	private void OnMaintenanceModeActivation()
	{
		if (m_deployed)
		{
			PLog.Log("Maintenance mode, disabling cloak");
			Drain(m_maxEnergy, damaged: false);
		}
	}

	private void OnShipTakenDamage()
	{
		if ((NetObj.m_phase != 0 || GetOwner() == NetObj.m_localPlayerID) && m_deployed)
		{
			PLog.Log("Damage taken, disabling cloak");
			Drain(m_maxEnergy, damaged: false);
		}
	}

	public override StatusWnd_HPModule CreateStatusWindow(GameObject guiCamera)
	{
		bool friendly = GetOwner() == NetObj.m_localPlayerID;
		return new StatusWnd_Shield(this, guiCamera, friendly);
	}

	public override void SetDeploy(bool deploy)
	{
		m_deploy = deploy;
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = visible;
		}
	}

	public override bool GetDeploy()
	{
		return m_deploy;
	}

	public override string GetStatusText()
	{
		if (m_deployed)
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
		stream.Write(m_deploy);
		stream.Write(m_deployed);
		stream.Write(m_energy);
		stream.Write(m_deployTimer);
	}

	public override void LoadOrders(BinaryReader stream)
	{
		base.LoadOrders(stream);
		m_deploy = stream.ReadBoolean();
		m_deployed = stream.ReadBoolean();
		m_energy = stream.ReadSingle();
		m_deployTimer = stream.ReadSingle();
		if (m_deployed)
		{
			Activate(firstTime: false);
		}
	}

	private void Activate(bool firstTime)
	{
		PLog.Log("Activating cloaking field");
		if (firstTime)
		{
			m_deployed = true;
			m_deployTimer = 0f;
			if (m_activation == null && IsVisible())
			{
				m_activation = UnityEngine.Object.Instantiate(m_activateEffectPrefab, m_unit.transform.position, m_unit.transform.rotation) as GameObject;
				m_activation.transform.parent = base.transform;
				UpdateCloakScale();
			}
		}
		else
		{
			SetupCloakEffect();
		}
	}

	private void Deactivate(bool destroyed)
	{
		PLog.Log("Deactivating cloaking field");
		m_deploy = false;
		m_deployed = false;
		m_energy = 0f;
		m_unit.SetCloaked(cloaked: false);
		m_deployTimer = -1f;
		if (m_activation != null)
		{
			UnityEngine.Object.Destroy(m_activation);
			m_activation = null;
		}
		if (m_cloak != null)
		{
			UnityEngine.Object.Destroy(m_cloak);
			m_cloak = null;
		}
	}

	private void UpdateCloakScale()
	{
		if (!(m_activation == null))
		{
			float num = m_deployTimer / (m_cloakDelay * 2f);
			float num2 = Mathf.Sin(num * (float)Math.PI);
			float length = m_unit.GetLength();
			float width = m_unit.GetWidth();
			m_activation.transform.localScale = new Vector3(num2 * width, num2 * 10f, num2 * length);
			if (num > 1f)
			{
				UnityEngine.Object.Destroy(m_activation);
				m_activation = null;
			}
		}
	}

	private void SetupCloakEffect()
	{
		if (!(m_cloak != null))
		{
			float length = m_unit.GetLength();
			float width = m_unit.GetWidth();
			m_cloak = UnityEngine.Object.Instantiate(m_cloakEffectPrefab, m_unit.transform.position, m_unit.transform.rotation) as GameObject;
			m_cloak.transform.parent = base.transform;
			m_cloak.transform.localScale = new Vector3(width * 1.5f, 1f, length);
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = IsVisible();
			}
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (m_deployTimer >= 0f)
		{
			m_deployTimer += Time.fixedDeltaTime;
			if (m_deployTimer > m_cloakDelay && !m_unit.IsCloaked())
			{
				m_unit.SetCloaked(cloaked: true);
				PLog.Log("Cloak active");
				SetupCloakEffect();
			}
			UpdateCloakScale();
		}
		if (!m_deployed && m_deploy && m_energy >= m_maxEnergy)
		{
			Activate(firstTime: true);
		}
		if (m_deployed && !m_deploy)
		{
			Deactivate(destroyed: false);
		}
		if (m_deployed)
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
		if (!(m_energy <= 0f))
		{
			m_energy -= energy;
			if (m_energy <= 0f)
			{
				m_energy = 0f;
				bool destroyed = damaged;
				Deactivate(destroyed);
			}
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
