#define DEBUG
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/HPModule")]
public class HPModule : NetObj, IOrderable
{
	public enum HPModuleType
	{
		Offensive,
		Defensive,
		Any
	}

	public delegate void DestroyedHandler(HPModule module);

	public GameObject m_visual;

	public GameObject m_disableIconPrefab;

	public Texture2D m_GUITexture;

	public bool m_editByPlayer = true;

	public HPModuleSettings m_settings;

	public int m_width = 1;

	public int m_length = 1;

	public HPModuleType m_type;

	public int m_maxHealth = 20;

	public int m_armorClass = 10;

	public int m_repairAmount = 2;

	public GameObject m_selectionMarkerPrefab;

	public GameObject m_orderMarkerPrefab;

	public GameObject m_disableEffect;

	public GameObject m_enableEffect;

	public GameObject m_persistantDisableEffectPrefab;

	public GameObject m_persistantDisableEffectPrefabLow;

	public GameObject m_selectSound;

	protected int m_health = 20;

	protected Vector2i m_gridPos;

	protected Unit m_unit;

	protected Battery m_battery;

	protected Direction m_dir;

	protected LinkedList<Order> m_orders = new LinkedList<Order>();

	protected DestroyedHandler m_onDestroyed;

	protected bool m_disabled;

	private Material[] m_originalMaterials;

	private GameObject m_marker;

	private GameObject m_disableIcon;

	private GameObject m_persistantDisableEffect;

	private float m_destructionTimer = -1f;

	private float m_repairTimer;

	public override void Awake()
	{
		base.Awake();
		m_save = false;
		m_updateSeenBy = false;
		m_health = m_maxHealth;
		m_settings = ComponentDB.instance.GetModule(base.name.Substring(0, base.name.Length - 7));
		DebugUtils.Assert(m_settings != null);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write((short)m_health);
		writer.Write(m_disabled);
		writer.Write(m_destructionTimer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_health = reader.ReadInt16();
		m_disabled = reader.ReadBoolean();
		m_destructionTimer = reader.ReadSingle();
		if (!GetUnit().IsDead())
		{
			SetPersistantDisableEffect(m_disabled);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (m_destructionTimer >= 0f)
		{
			m_destructionTimer -= Time.fixedDeltaTime;
			if (m_destructionTimer < 0f)
			{
				Damage(new Hit(GetMaxHealth(), 100), showDmgText: false);
			}
		}
		if (m_disabled && !m_unit.IsSinking())
		{
			UpdateRepair(Time.fixedDeltaTime);
		}
	}

	private void UpdateRepair(float dt)
	{
		m_repairTimer += dt;
		if (m_repairTimer >= 1f)
		{
			m_repairTimer = 0f;
			Heal(m_repairAmount);
		}
	}

	public void Heal(int health)
	{
		m_health += health;
		if (m_health > m_maxHealth)
		{
			m_health = m_maxHealth;
		}
		if (m_disabled && m_maxHealth == m_health)
		{
			OnEnabled();
		}
	}

	public virtual List<string> GetHardpointInfo()
	{
		return new List<string>();
	}

	public virtual void Update()
	{
		if (NetObj.m_drawOrders)
		{
			DrawOrders();
		}
	}

	public virtual void Supply(ref int resources)
	{
		if (resources > 12 && m_health < m_maxHealth)
		{
			resources -= 12;
			m_health += 10;
			if (m_health > m_maxHealth)
			{
				m_health = m_maxHealth;
			}
		}
	}

	public void SetDir(Direction dir)
	{
		m_dir = dir;
	}

	public virtual void Setup(Unit unit, Battery battery, int x, int y, Direction dir, DestroyedHandler destroyedCallback)
	{
		m_unit = unit;
		m_battery = battery;
		m_gridPos = new Vector2i(x, y);
		m_dir = dir;
		m_onDestroyed = destroyedCallback;
		switch (m_dir)
		{
		case Direction.Forward:
			break;
		case Direction.Right:
			base.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			break;
		case Direction.Backward:
			base.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			break;
		case Direction.Left:
			base.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
			break;
		}
	}

	public Direction GetDir()
	{
		return m_dir;
	}

	public virtual Vector3 GetVelocity()
	{
		return m_unit.GetVelocity();
	}

	public virtual void GetChargeLevel(out float i, out float time)
	{
		if (m_disabled)
		{
			i = m_health / m_maxHealth;
			time = (m_maxHealth - m_health) / m_repairAmount;
		}
		i = -1f;
		time = -1f;
	}

	public virtual string GetTooltip()
	{
		return base.gameObject.name;
	}

	public void EnableSelectionMarker(bool enabled)
	{
		if (enabled != IsSelected())
		{
			if (enabled)
			{
				m_marker = Object.Instantiate(m_selectionMarkerPrefab, base.transform.position, base.transform.rotation) as GameObject;
				m_marker.transform.parent = base.transform;
				Vector3 size = (base.collider as BoxCollider).bounds.size;
				float num = ((!(size.x > size.z)) ? size.x : size.x);
				m_marker.transform.localScale = new Vector3(num, num, num);
				m_marker.transform.localPosition = new Vector3(0f, 0.2f, 0f);
			}
			else
			{
				Object.Destroy(m_marker);
				m_marker = null;
			}
		}
	}

	public virtual void SetSelected(bool selected, bool explicitSelected)
	{
		if (selected != IsSelected())
		{
			EnableSelectionMarker(selected);
			if (selected && m_selectSound != null && explicitSelected)
			{
				Object.Instantiate(m_selectSound, base.transform.position, Quaternion.identity);
			}
		}
	}

	public Section GetSection()
	{
		return m_battery.GetSection();
	}

	public bool IsSelected()
	{
		return m_marker != null;
	}

	public void TimedDestruction(float time)
	{
		if (m_destructionTimer < 0f)
		{
			m_destructionTimer = time;
		}
	}

	public bool Damage(Hit hit, bool showDmgText)
	{
		if (m_health <= 0 || m_disabled)
		{
			if (GetUnit().IsDead())
			{
				OnEnabled();
				m_disabled = true;
			}
			return true;
		}
		int healthDamage;
		GameRules.HitOutcome hitOutcome = GameRules.CalculateDamage(m_health, m_armorClass, hit.m_damage, hit.m_armorPiercing, out healthDamage);
		m_health -= healthDamage;
		if (hit.m_dealer != null && healthDamage > 0)
		{
			Unit unit = hit.GetUnit();
			Gun gun = hit.GetGun();
			if (unit != null)
			{
				UnitAi ai = m_unit.GetAi();
				ai.SetTargetId(unit);
			}
			string gunName = ((!(gun != null)) ? string.Empty : gun.name);
			TurnMan.instance.AddPlayerDamage(hit.m_dealer.GetOwner(), healthDamage, hit.m_dealer.GetOwnerTeam() == GetOwnerTeam(), gunName);
		}
		if (IsVisible() && showDmgText)
		{
			switch (hitOutcome)
			{
			case GameRules.HitOutcome.CritHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_moduleCriticalHit);
				break;
			case GameRules.HitOutcome.PiercedArmor:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_modulePiercingHit);
				break;
			case GameRules.HitOutcome.GlancingHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_moduleGlancingHit);
				break;
			case GameRules.HitOutcome.Deflected:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_moduleDeflectHit);
				break;
			}
			if (m_health <= 0)
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, GetName(), Constants.m_moduleDisabledHit);
			}
		}
		if (m_health <= 0)
		{
			OnDisabled();
		}
		if (hit.m_havePoint && hit.m_dir != Vector3.zero)
		{
			Ship ship = GetUnit() as Ship;
			if (ship != null)
			{
				ship.ApplyImpulse(hit.m_point, hit.m_dir * healthDamage, separate: false);
			}
		}
		if (m_unit.m_onTakenDamage != null)
		{
			m_unit.m_onTakenDamage();
		}
		return healthDamage > 0;
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
		m_onDestroyed(this);
	}

	private void OnEnabled()
	{
		if (m_disabled)
		{
			if (m_enableEffect != null)
			{
				Object.Instantiate(m_enableEffect, base.transform.position, base.transform.rotation);
			}
			SetPersistantDisableEffect(enable: false);
			m_disabled = false;
		}
	}

	protected virtual void OnDisabled()
	{
		if (m_disabled)
		{
			return;
		}
		m_disabled = true;
		if (!GetUnit().IsDead())
		{
			if (m_disableEffect != null && IsVisible())
			{
				Object.Instantiate(m_disableEffect, base.transform.position, base.transform.rotation);
			}
			SetPersistantDisableEffect(enable: true);
		}
	}

	private void SetPersistantDisableEffect(bool enable)
	{
		if (enable)
		{
			GameObject persistantDisableEffectPrefab = m_persistantDisableEffectPrefab;
			if (m_persistantDisableEffect == null && persistantDisableEffectPrefab != null)
			{
				m_persistantDisableEffect = Object.Instantiate(persistantDisableEffectPrefab, base.transform.position, base.transform.rotation) as GameObject;
				m_persistantDisableEffect.transform.parent = base.transform;
				Renderer[] componentsInChildren = m_persistantDisableEffect.GetComponentsInChildren<Renderer>();
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = IsVisible();
				}
			}
		}
		else if (m_persistantDisableEffect != null)
		{
			Object.Destroy(m_persistantDisableEffect);
			m_persistantDisableEffect = null;
		}
	}

	public virtual Dictionary<string, string> GetShipEditorInfo()
	{
		return new Dictionary<string, string>();
	}

	public Unit GetUnit()
	{
		return m_unit;
	}

	public string GetName()
	{
		return Localize.instance.TranslateKey(base.name + "_name");
	}

	public string GetAbbr()
	{
		return Localize.instance.TranslateKey(base.name + "_abbr");
	}

	public string GetProductName()
	{
		return Localize.instance.TranslateKey(base.name + "_productname");
	}

	public int GetHealth()
	{
		return m_health;
	}

	public int GetMaxHealth()
	{
		return m_maxHealth;
	}

	public int GetWidth()
	{
		if (m_dir == Direction.Forward || m_dir == Direction.Backward)
		{
			return m_width;
		}
		return m_length;
	}

	public int GetLength()
	{
		if (m_dir == Direction.Forward || m_dir == Direction.Backward)
		{
			return m_length;
		}
		return m_width;
	}

	public Vector2i GetGridPos()
	{
		return m_gridPos;
	}

	public int GetTotalValue()
	{
		return m_settings.m_value;
	}

	public bool IsDisabled()
	{
		return m_disabled;
	}

	public virtual float GetMaxEnergy()
	{
		return 0f;
	}

	public virtual float GetEnergy()
	{
		return 0f;
	}

	public virtual void SetDeploy(bool deploy)
	{
	}

	public virtual bool GetDeploy()
	{
		return false;
	}

	public virtual string GetStatusText()
	{
		return string.Empty;
	}

	public virtual StatusWnd_HPModule CreateStatusWindow(GameObject guiCamera)
	{
		return null;
	}

	public virtual void OnOrdersChanged()
	{
	}

	public virtual void ClearOrders()
	{
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
	}

	public Order GetFirstOrder()
	{
		if (m_orders.Count > 0)
		{
			return m_orders.First.Value;
		}
		return null;
	}

	public virtual bool RemoveFirstOrder()
	{
		if (m_orders.Count > 0)
		{
			Order value = m_orders.First.Value;
			m_orders.RemoveFirst();
			value.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			return true;
		}
		return false;
	}

	public virtual bool RemoveLastOrder()
	{
		if (m_orders.Count > 0)
		{
			Order value = m_orders.Last.Value;
			m_orders.RemoveLast();
			value.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			return true;
		}
		return false;
	}

	protected override void OnSetDrawOrders(bool enabled)
	{
		foreach (Order order in m_orders)
		{
			order.SetMarkerEnabled(enabled, m_orderMarkerPrefab);
		}
	}

	public virtual bool RemoveOrder(Order order)
	{
		if (m_orders.Remove(order))
		{
			order.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			return true;
		}
		return false;
	}

	public virtual bool IsLastOrder(Order order)
	{
		if (m_orders.Count > 0)
		{
			return m_orders.Last.Value == order;
		}
		return false;
	}

	public virtual void AddOrder(Order order)
	{
		m_orders.AddLast(order);
		order.SetMarkerEnabled(NetObj.m_drawOrders, m_orderMarkerPrefab);
	}

	public virtual void LoadOrders(BinaryReader stream)
	{
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
		int num = stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Order order = new Order(this, stream);
			AddOrder(order);
		}
	}

	public virtual void SaveOrders(BinaryWriter stream)
	{
		stream.Write(m_orders.Count);
		foreach (Order order in m_orders)
		{
			order.Save(stream);
		}
	}

	public virtual void DrawOrders()
	{
	}

	public override bool IsSeenByPlayer(int playerID)
	{
		return m_unit.IsSeenByPlayer(playerID);
	}

	public override bool IsSeenByTeam(int teamID)
	{
		return m_unit.IsSeenByTeam(teamID);
	}

	public void SetHighlight(bool enabled)
	{
		Renderer[] componentsInChildren = m_visual.GetComponentsInChildren<Renderer>();
		if (enabled)
		{
			if (m_originalMaterials != null)
			{
				return;
			}
			List<Material> list = new List<Material>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				foreach (Material item in sharedMaterials)
				{
					list.Add(item);
				}
				Material[] materials = renderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_Highlight", 0.3f);
				}
			}
			m_originalMaterials = list.ToArray();
		}
		else
		{
			if (m_originalMaterials == null)
			{
				return;
			}
			int num = 0;
			Renderer[] array2 = componentsInChildren;
			foreach (Renderer renderer2 in array2)
			{
				Material[] array3 = new Material[renderer2.sharedMaterials.Length];
				for (int m = 0; m < renderer2.sharedMaterials.Length; m++)
				{
					array3[m] = m_originalMaterials[num++];
				}
				renderer2.materials = array3;
			}
			m_originalMaterials = null;
		}
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		Renderer[] componentsInChildren = m_visual.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = visible;
		}
		if (m_persistantDisableEffect != null)
		{
			Renderer[] componentsInChildren2 = m_persistantDisableEffect.GetComponentsInChildren<Renderer>();
			Renderer[] array2 = componentsInChildren2;
			foreach (Renderer renderer2 in array2)
			{
				renderer2.enabled = IsVisible();
			}
		}
		if (m_disableIcon != null)
		{
			m_disableIcon.renderer.enabled = visible;
		}
	}

	private void SetDisableIcon(bool enabled)
	{
		if (m_disableIconPrefab == null)
		{
			return;
		}
		if (enabled)
		{
			if (m_disableIcon == null)
			{
				m_disableIcon = Object.Instantiate(m_disableIconPrefab) as GameObject;
				m_disableIcon.transform.parent = base.transform;
				m_disableIcon.transform.localPosition = new Vector3(-0.5f, 2f, 0f);
				m_disableIcon.renderer.enabled = IsVisible();
			}
		}
		else if (m_disableIcon != null)
		{
			Object.Destroy(m_disableIcon);
			m_disableIcon = null;
		}
	}
}
