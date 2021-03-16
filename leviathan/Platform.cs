#define DEBUG
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Units/Platform")]
public class Platform : Unit
{
	private int m_health;

	public GameObject m_explosionPrefab;

	public GameObject m_explosionLowPrefab;

	public GameObject m_destroyedSmoke;

	public GameObject m_platformPrefab;

	public GameObject m_destroyedPlatformPrefab;

	public int m_player = 7;

	public int m_maxHealth = 500;

	public int m_armorClass = 10;

	public float m_baseSightRange = 100f;

	public float m_Width = 40f;

	public float m_length = 40f;

	public List<string> m_batteryModules;

	public string m_placeGun;

	public string m_displayName = "$platform";

	public string m_currentAnimation = string.Empty;

	private float m_destructionTimer = -1f;

	private GameObject m_visual;

	public bool m_immuneToDamage;

	public bool m_showTrueHealth;

	public bool m_alwaysVisible;

	public bool m_supplyEnabled;

	public float m_supplyRadius = 20f;

	public int m_resources = 1000;

	public int m_maxResources = 1000;

	public float m_supplyDelay = 0.5f;

	public int m_resupplyRate = 15;

	public GameObject m_supplyEffect;

	protected LineDrawer m_lineDrawer;

	private int m_supplyAreaLineType;

	private int m_supplyAreaDisabledLineType;

	private float m_supplyTimer;

	private float m_supplyEffectTimer;

	private int m_supplyMask;

	public Platform()
	{
		UnitAi unitAi = (m_Ai = new UnitAi());
	}

	public override void Awake()
	{
		base.Awake();
		m_health = m_maxHealth;
		m_supplyMask = (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("projectiles"));
		if ((bool)m_supplyEffect)
		{
			m_supplyEffect.GetComponent<ParticleSystem>().enableEmission = false;
		}
	}

	public virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		if ((bool)m_platformPrefab)
		{
			Gizmos.DrawWireCube(size: new Vector3(40f, 40f, 40f), center: base.transform.position);
		}
	}

	public virtual void OnEvent(string eventName)
	{
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (!(componentInChildren == null))
		{
			m_currentAnimation = eventName;
			if (eventName == string.Empty)
			{
				componentInChildren.Stop();
			}
			else
			{
				componentInChildren.Play(eventName);
			}
		}
	}

	public void EventWarning(string eventName)
	{
		if (Application.isEditor)
		{
			string text = base.name + "(" + GetNetID() + ") of type " + GetType().ToString();
			string text2 = "Recived event '" + eventName + "' that it do not care about.";
			MessageLog.instance.ShowMessage(MessageLog.TextPosition.Bottom, text, text2, string.Empty, 2f);
			PLog.Log(text + " " + text2);
		}
	}

	public override void Start()
	{
		base.Start();
		SetOwner(m_player);
		SetName(Localize.instance.Translate(m_displayName));
		if (IsDead())
		{
			if ((bool)m_destroyedSmoke)
			{
				GameObject gameObject = Object.Instantiate(m_destroyedSmoke, base.transform.position, Quaternion.identity) as GameObject;
				gameObject.transform.parent = base.transform;
			}
			if (m_destroyedPlatformPrefab != null)
			{
				m_visual = Object.Instantiate(m_destroyedPlatformPrefab, base.transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f)), base.transform.rotation) as GameObject;
				m_visual.transform.parent = base.transform;
			}
			else if (base.collider.enabled)
			{
				base.collider.enabled = false;
			}
		}
		else
		{
			SetupBatterys();
			if (m_platformPrefab != null)
			{
				m_visual = Object.Instantiate(m_platformPrefab, base.transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f)), base.transform.rotation) as GameObject;
				m_visual.transform.parent = base.transform;
			}
			OnEvent(m_currentAnimation);
		}
		if ((bool)m_visual)
		{
			SetVisualVisiblility(m_visual, IsVisible());
		}
		base.Start();
	}

	public override void SetOwner(int owner)
	{
		base.SetOwner(owner);
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.SetOwner(owner);
			battery.Setup(this, 0);
		}
	}

	public override void Update()
	{
		base.Update();
		SetSightRange(m_baseSightRange);
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

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (m_destructionTimer >= 0f)
		{
			m_destructionTimer -= Time.fixedDeltaTime;
			if (m_destructionTimer < 0f)
			{
				Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
				Battery[] array = componentsInChildren;
				foreach (Battery battery in array)
				{
					battery.RemoveAll();
				}
				m_destructionTimer = -1f;
				Object.Destroy(m_visual);
				if (m_destroyedPlatformPrefab != null)
				{
					m_visual = Object.Instantiate(m_destroyedPlatformPrefab, base.transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f)), base.transform.rotation) as GameObject;
					m_visual.transform.parent = base.transform;
				}
				else if (base.collider.enabled)
				{
					base.collider.enabled = false;
				}
				OnKilled();
			}
		}
		if (IsDead())
		{
			return;
		}
		m_supplyTimer += Time.fixedDeltaTime;
		if (!(m_supplyTimer >= m_supplyDelay))
		{
			return;
		}
		m_supplyTimer -= m_supplyDelay;
		if (!SupplyUnitsInRadius() && m_resources < m_maxResources)
		{
			m_resources += (int)((float)m_resupplyRate * m_supplyDelay);
			if (m_resources > m_maxResources)
			{
				m_resources = m_maxResources;
			}
		}
	}

	public override bool IsValidTarget()
	{
		if (m_allowAutotarget)
		{
			if (IsDead())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool SupplyUnitsInRadius()
	{
		if (!m_supplyEnabled)
		{
			return false;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, m_supplyRadius, m_supplyMask);
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		int resources = m_resources;
		int ownerTeam = GetOwnerTeam();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!(collider.attachedRigidbody == null) && !(collider.attachedRigidbody.gameObject == base.gameObject))
			{
				Unit component = collider.attachedRigidbody.GetComponent<Unit>();
				if (!(component == null) && !(this == component) && component.GetOwnerTeam() == ownerTeam && !hashSet.Contains(component.gameObject))
				{
					hashSet.Add(component.gameObject);
				}
			}
		}
		bool result = hashSet.Count != 0;
		if (m_resources <= 0)
		{
			return result;
		}
		foreach (GameObject item in hashSet)
		{
			Unit component2 = item.GetComponent<Unit>();
			component2.Supply(ref m_resources);
			if (m_resources <= 0)
			{
				break;
			}
		}
		if (m_resources != resources)
		{
			m_supplyEffectTimer = m_supplyDelay + 0.1f;
			if (m_supplyEffect != null)
			{
				m_supplyEffect.GetComponent<ParticleSystem>().enableEmission = true;
			}
			return true;
		}
		return result;
	}

	private void SetupBatterys()
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		if (m_batteryModules == null || m_batteryModules.Count < 1)
		{
			Battery[] array = componentsInChildren;
			foreach (Battery battery in array)
			{
				if (m_placeGun.Length != 0 && battery.CanPlaceAt(0, 0, 1, 1, null))
				{
					battery.AddHPModule(m_placeGun, 0, 0, Direction.Forward);
				}
			}
			return;
		}
		int num = 0;
		Battery[] array2 = componentsInChildren;
		foreach (Battery battery2 in array2)
		{
			if (num < m_batteryModules.Count && battery2.CanPlaceAt(0, 0, 1, 1, null))
			{
				battery2.AddHPModule(m_batteryModules[num], 0, 0, Direction.Forward);
			}
			num++;
		}
	}

	protected override void OnSetSimulating(bool enabled)
	{
		base.OnSetSimulating(enabled);
	}

	public override bool TestLOS(NetObj obj)
	{
		float num = Vector3.Distance(base.transform.position, obj.transform.position);
		if (num > m_baseSightRange)
		{
			return false;
		}
		return true;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_destructionTimer);
		writer.Write(m_baseSightRange);
		writer.Write((short)m_health);
		writer.Write((short)m_maxHealth);
		writer.Write(m_Width);
		writer.Write(m_length);
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		writer.Write((byte)componentsInChildren.Length);
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.SaveState(writer);
		}
		writer.Write(m_alwaysVisible);
		writer.Write(m_currentAnimation);
		writer.Write(m_resources);
		writer.Write(m_supplyTimer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_destructionTimer = reader.ReadSingle();
		m_baseSightRange = reader.ReadSingle();
		m_player = GetOwner();
		m_health = reader.ReadInt16();
		m_maxHealth = reader.ReadInt16();
		m_Width = reader.ReadSingle();
		m_length = reader.ReadSingle();
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		int num = reader.ReadByte();
		DebugUtils.Assert(num == componentsInChildren.Length, "number of batteries missmatch");
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.LoadState(reader);
		}
		m_alwaysVisible = reader.ReadBoolean();
		m_currentAnimation = reader.ReadString();
		m_resources = reader.ReadInt32();
		m_supplyTimer = reader.ReadSingle();
	}

	public override string GetTooltip()
	{
		string empty = string.Empty;
		empty = empty + GetName() + "\n";
		if (m_health > 0)
		{
			string text = empty;
			empty = text + "Health: " + m_health + "\n";
		}
		else
		{
			empty += "Health: Destroyed\n";
		}
		return empty + "Resources: " + m_resources + "\n";
	}

	public override bool Damage(Hit hit)
	{
		if (m_health <= 0 || m_dead)
		{
			return true;
		}
		if (m_destructionTimer > 0f)
		{
			return false;
		}
		if (m_immuneToDamage)
		{
			HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipDeflectHit);
			return false;
		}
		int healthDamage;
		GameRules.HitOutcome hitOutcome = GameRules.CalculateDamage(m_health, m_armorClass, hit.m_damage, hit.m_armorPiercing, out healthDamage);
		m_health -= healthDamage;
		if (IsVisible())
		{
			switch (hitOutcome)
			{
			case GameRules.HitOutcome.CritHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipCriticalHit);
				break;
			case GameRules.HitOutcome.PiercedArmor:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipPiercingHit);
				break;
			case GameRules.HitOutcome.GlancingHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipGlancingHit);
				break;
			case GameRules.HitOutcome.Deflected:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipDeflectHit);
				break;
			}
			if (m_health <= 0)
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, GetName(), Constants.m_shipDestroyedHit);
			}
		}
		if (m_health <= 0)
		{
			m_destructionTimer = 2f;
			GameObject gameObject = Object.Instantiate(m_explosionPrefab, base.transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f)), Quaternion.identity) as GameObject;
			gameObject.transform.position = base.transform.position;
		}
		return healthDamage > 0;
	}

	public override void SetVisible(bool visible)
	{
		if (IsVisible() != visible)
		{
			base.SetVisible(visible);
			SetVisualVisiblility(base.gameObject, visible);
		}
	}

	public void SetVisualVisiblility(GameObject visual, bool visible)
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = visible;
		}
		if ((bool)visual.renderer)
		{
			visual.renderer.renderer.enabled = visible;
		}
	}

	public override int GetHealth()
	{
		if (m_showTrueHealth)
		{
			return m_health;
		}
		if (m_immuneToDamage)
		{
			HPModule componentInChildren = base.transform.GetComponentInChildren<HPModule>();
			if ((bool)componentInChildren)
			{
				return componentInChildren.GetHealth();
			}
			return m_health;
		}
		return m_health;
	}

	public override int GetMaxHealth()
	{
		if (m_showTrueHealth)
		{
			return m_maxHealth;
		}
		if (m_immuneToDamage)
		{
			HPModule componentInChildren = base.transform.GetComponentInChildren<HPModule>();
			if ((bool)componentInChildren)
			{
				return componentInChildren.GetMaxHealth();
			}
			return m_health;
		}
		return m_maxHealth;
	}

	public override float GetLength()
	{
		return m_length;
	}

	public override float GetWidth()
	{
		return m_Width;
	}

	protected bool SetupLineDrawer()
	{
		if (m_lineDrawer == null)
		{
			m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
			if (m_lineDrawer == null)
			{
				return false;
			}
			m_supplyAreaLineType = m_lineDrawer.GetTypeID("supplyArea");
			m_supplyAreaDisabledLineType = m_lineDrawer.GetTypeID("supplyAreaDisabled");
			DebugUtils.Assert(m_supplyAreaLineType > 0);
			return true;
		}
		return true;
	}

	private void DrawSupplyArea()
	{
		if (m_supplyEnabled && SetupLineDrawer() && GetOwnerTeam() == TurnMan.instance.GetPlayerTeam(NetObj.m_localPlayerID))
		{
			Vector3 position = base.transform.position;
			position.y += 2f;
			if (IsSupplying())
			{
				m_lineDrawer.DrawXZCircle(position, m_supplyRadius, 40, m_supplyAreaLineType, 0.15f);
			}
			else
			{
				m_lineDrawer.DrawXZCircle(position, m_supplyRadius, 40, m_supplyAreaDisabledLineType, 0.15f);
			}
		}
	}

	private bool IsSupplying()
	{
		if (m_supplyEffectTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public int GetResources()
	{
		return m_resources;
	}

	public int GetMaxResources()
	{
		return m_maxResources;
	}

	public override string GetName()
	{
		if (m_placeGun.Length == 0)
		{
			return base.GetName();
		}
		return Localize.instance.TranslateKey(m_placeGun + "_name");
	}
}
