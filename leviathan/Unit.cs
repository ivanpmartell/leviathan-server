#define DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Units/Unit")]
public class Unit : NetObj, IOrderable
{
	public enum ShipTagType
	{
		Normal,
		Mini,
		None
	}

	public enum ObjectiveTypes
	{
		None,
		Move,
		Destroy,
		Defend
	}

	public static Action<Unit> m_onKilled;

	public static Action<Unit> m_onCreated;

	public static Action<Unit> m_onRemoved;

	public Action m_onTakenDamage;

	public Action m_onFireWeapon;

	public Action m_onMaintenanceActivation;

	public GameObject m_selectionMarkerPrefab;

	public GameObject m_orderMarkerPrefab;

	public GameObject m_selectSound;

	private string m_name = "Unknown";

	public UnitSettings m_settings;

	public bool m_king;

	public bool m_allowAutotarget;

	public ShipTagType m_shipTag;

	public bool m_centerShipTag;

	protected LinkedList<Order> m_orders = new LinkedList<Order>();

	protected bool m_dead;

	protected float m_deadTime;

	protected bool m_cloaked;

	protected int m_lastDamageDealer = -1;

	private GameObject m_marker;

	protected string m_group = string.Empty;

	private bool m_blockedRoute;

	private float m_sightRange;

	private ObjectiveTypes m_objective;

	private GameObject m_objectiveIcon;

	protected UnitAi m_Ai;

	public override void Awake()
	{
		base.Awake();
		if (m_onCreated != null)
		{
			m_onCreated(this);
		}
		m_settings = ComponentDB.instance.GetUnit(base.name);
		DebugUtils.Assert(m_settings != null);
	}

	public override void OnDestroy()
	{
		if (m_onRemoved != null)
		{
			m_onRemoved(this);
		}
		base.OnDestroy();
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
	}

	public virtual void Start()
	{
	}

	protected virtual void FixedUpdate()
	{
		if (NetObj.m_simulating && m_dead)
		{
			m_deadTime += Time.fixedDeltaTime * 0.2f;
			if (m_deadTime > 1f)
			{
				m_deadTime = 1f;
			}
		}
	}

	public virtual void Update()
	{
		UpdateMarker();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write((short)1);
		writer.Write(m_name);
		writer.Write(m_dead);
		writer.Write(m_deadTime);
		writer.Write(m_king);
		writer.Write(m_cloaked);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
		writer.Write((byte)m_lastDamageDealer);
		writer.Write(m_group);
		writer.Write((int)m_objective);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		short num = reader.ReadInt16();
		m_name = reader.ReadString();
		m_dead = reader.ReadBoolean();
		m_deadTime = reader.ReadSingle();
		m_king = reader.ReadBoolean();
		m_cloaked = reader.ReadBoolean();
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
		m_lastDamageDealer = reader.ReadByte();
		m_group = reader.ReadString();
		ObjectiveTypes objective = (ObjectiveTypes)reader.ReadInt32();
		SetObjective(objective);
	}

	public virtual void SaveOrders(BinaryWriter stream)
	{
		stream.Write((short)1);
		stream.Write(m_blockedRoute);
		stream.Write(m_orders.Count);
		foreach (Order order in m_orders)
		{
			order.Save(stream);
		}
	}

	public virtual void LoadOrders(BinaryReader stream)
	{
		short num = stream.ReadInt16();
		m_blockedRoute = stream.ReadBoolean();
		ClearOrders();
		int num2 = stream.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			Order order = new Order(this, stream);
			AddOrder(order);
		}
	}

	public void SetBlockedRoute(bool blocked)
	{
		m_blockedRoute = blocked;
	}

	public bool IsRouteBlocked()
	{
		return m_blockedRoute;
	}

	public bool IsSelected()
	{
		return m_marker != null;
	}

	public virtual void SetSelected(bool selected, bool explicitSelected)
	{
		if (selected == IsSelected())
		{
			return;
		}
		if (selected && m_selectionMarkerPrefab != null)
		{
			TurnMan.instance.GetPlayerColors(GetOwner(), out var primaryColor);
			if (m_marker != null)
			{
				UnityEngine.Object.Destroy(m_marker);
			}
			m_marker = UnityEngine.Object.Instantiate(m_selectionMarkerPrefab, base.transform.position, base.transform.rotation) as GameObject;
			m_marker.transform.parent = base.transform;
			m_marker.GetComponent<UnitSelectionMarker>().Setup(GetMarkerSize(), primaryColor);
		}
		else if (m_marker != null)
		{
			UnityEngine.Object.Destroy(m_marker);
			m_marker = null;
		}
		if (selected && m_selectSound != null && explicitSelected)
		{
			UnityEngine.Object.Instantiate(m_selectSound, base.transform.position, Quaternion.identity);
		}
	}

	protected Vector3 GetMarkerSize()
	{
		return new Vector3(GetWidth() * 1.2f, 0f, GetLength() * 1.2f);
	}

	public virtual float GetLength()
	{
		return 0f;
	}

	public virtual float GetWidth()
	{
		return 0f;
	}

	public virtual Vector3 GetVelocity()
	{
		return Vector3.zero;
	}

	protected override void OnSetDrawOrders(bool enabled)
	{
		foreach (Order order in m_orders)
		{
			order.SetMarkerEnabled(enabled, m_orderMarkerPrefab);
		}
	}

	public void AddOrder(Order order)
	{
		m_orders.AddLast(order);
		order.SetMarkerEnabled(NetObj.m_drawOrders, m_orderMarkerPrefab);
		OnOrdersChanged();
	}

	public bool RemoveOrder(Order order)
	{
		if (m_orders.Remove(order))
		{
			order.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			OnOrdersChanged();
			return true;
		}
		return false;
	}

	public bool RemoveFirstOrder()
	{
		if (m_orders.Count > 0)
		{
			Order value = m_orders.First.Value;
			m_orders.RemoveFirst();
			value.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			OnOrdersChanged();
			return true;
		}
		return false;
	}

	public bool RemoveLastOrder()
	{
		if (m_orders.Count > 0)
		{
			Order value = m_orders.Last.Value;
			m_orders.RemoveLast();
			value.SetMarkerEnabled(enabled: false, m_orderMarkerPrefab);
			OnOrdersChanged();
			return true;
		}
		return false;
	}

	public virtual void OnOrdersChanged()
	{
	}

	public bool IsOrdersEmpty()
	{
		return m_orders.Count == 0;
	}

	public bool IsLastOrder(Order order)
	{
		if (m_orders.Count > 0)
		{
			return m_orders.Last.Value == order;
		}
		return false;
	}

	public virtual void ClearOrders()
	{
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
	}

	public virtual void ClearMoveOrders()
	{
		while (m_orders.Count > 0)
		{
			RemoveLastOrder();
		}
	}

	public bool IsMoveOrdersValid()
	{
		Vector3 from = base.transform.position;
		foreach (Order order in m_orders)
		{
			if (order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward)
			{
				Vector3 pos = order.GetPos();
				if (!CanMove(from, pos))
				{
					return false;
				}
				from = pos;
			}
		}
		return true;
	}

	public virtual void OnChildOrderLostFocus(Order order)
	{
	}

	public virtual void OnChildOrderGotFocus(Order order)
	{
	}

	protected virtual bool CanMove(Vector3 from, Vector3 to)
	{
		from.y = -2f;
		to.y = -2f;
		int layerMask = 1;
		RaycastHit hitInfo;
		return !Physics.Linecast(from, to, out hitInfo, layerMask);
	}

	public virtual string GetTooltip()
	{
		return m_name;
	}

	public virtual bool Damage(Hit hit)
	{
		return false;
	}

	public virtual bool TestLOS(NetObj obj)
	{
		return false;
	}

	public virtual bool TestLOS(Vector3 pos)
	{
		return false;
	}

	public virtual bool IsInSmoke()
	{
		return false;
	}

	public bool IsCloaked()
	{
		return m_cloaked;
	}

	public virtual bool IsDoingMaintenance()
	{
		return false;
	}

	public void SetCloaked(bool cloaked)
	{
		if (cloaked)
		{
			ClearGunOrdersAndTargets();
		}
		m_cloaked = cloaked;
	}

	protected virtual void ClearGunOrdersAndTargets()
	{
	}

	public virtual Vector3[] GetViewPoints()
	{
		return new Vector3[1] { base.transform.position };
	}

	public virtual Vector3[] GetTargetPoints()
	{
		return new Vector3[1] { base.transform.position };
	}

	public bool IsDead()
	{
		return m_dead;
	}

	public virtual bool IsValidTarget()
	{
		if (IsDead())
		{
			return false;
		}
		return true;
	}

	public void SetName(string name)
	{
		m_name = name;
	}

	public virtual int GetTotalValue()
	{
		return 0;
	}

	public string GetGroup()
	{
		return m_group;
	}

	public void SetGroup(string group)
	{
		m_group = group;
	}

	public virtual void Supply(ref int resources)
	{
	}

	protected virtual void OnKilled()
	{
		if (!m_dead)
		{
			SetObjective(ObjectiveTypes.None);
			m_dead = true;
			if (m_onKilled != null)
			{
				m_onKilled(this);
			}
		}
	}

	public virtual string GetName()
	{
		return m_name;
	}

	public virtual int GetHealth()
	{
		return 0;
	}

	public virtual int GetMaxHealth()
	{
		return 0;
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		if (!m_objectiveIcon)
		{
			return;
		}
		Transform transform = m_objectiveIcon.transform.FindChild("particle");
		if (!(transform == null))
		{
			if (IsKing())
			{
				visible = true;
			}
			if (visible)
			{
				transform.GetComponent<ParticleSystem>().Play();
			}
			else
			{
				transform.GetComponent<ParticleSystem>().Stop();
			}
		}
	}

	public void UpdateMarker()
	{
		Camera camera = Camera.main;
		if (!(camera == null) && !(m_objectiveIcon == null))
		{
			float num = Vector3.Distance(camera.transform.position, base.transform.position);
			float num2 = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f) * 0.04f * num;
			m_objectiveIcon.transform.localScale = new Vector3(num2, num2, num2);
			Transform transform = m_objectiveIcon.transform.FindChild("particle");
			if (transform != null)
			{
				transform.GetComponent<ParticleSystem>().startSize = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f) * num;
			}
		}
	}

	public void SetObjective(ObjectiveTypes objectivety)
	{
		if (IsKing())
		{
			int localPlayer = NetObj.GetLocalPlayer();
			if (localPlayer < 0)
			{
				return;
			}
			int playerTeam = TurnMan.instance.GetPlayerTeam(localPlayer);
			objectivety = ((GetOwnerTeam() != playerTeam) ? ObjectiveTypes.Destroy : ObjectiveTypes.Defend);
		}
		if (objectivety == m_objective || IsDead())
		{
			return;
		}
		m_objective = objectivety;
		if (m_objectiveIcon != null)
		{
			UnityEngine.Object.Destroy(m_objectiveIcon);
			m_objectiveIcon = null;
		}
		if (objectivety != 0)
		{
			string text = "Defend";
			if (objectivety == ObjectiveTypes.Move)
			{
				text = "ObjectiveIcon";
			}
			if (objectivety == ObjectiveTypes.Destroy)
			{
				text = "AttackIcon";
			}
			if (objectivety == ObjectiveTypes.Defend)
			{
				text = "DefendIcon";
			}
			m_objectiveIcon = ObjectFactory.instance.Create(text, base.transform.position, Quaternion.identity);
			m_objectiveIcon.transform.parent = base.transform;
			m_objectiveIcon.transform.localPosition = new Vector3(0f, 10f, 0f);
			SetVisible(IsVisible());
		}
	}

	public bool CanLOS()
	{
		if (!m_dead)
		{
			return true;
		}
		if (m_deadTime < 1f)
		{
			return true;
		}
		return false;
	}

	public virtual float GetSightRange()
	{
		if (!m_dead)
		{
			return m_sightRange;
		}
		return Mathf.Lerp(m_sightRange, 0f, m_deadTime);
	}

	public void SetSightRange(float sightrange)
	{
		m_sightRange = sightrange;
	}

	public void SetKing(bool king)
	{
		m_king = king;
	}

	public bool IsKing()
	{
		return m_king;
	}

	public virtual bool IsTakingWater()
	{
		return false;
	}

	public virtual bool IsSinking()
	{
		return false;
	}

	public UnitAi GetAi()
	{
		return m_Ai;
	}

	public int GetLastDamageDealer()
	{
		return m_lastDamageDealer;
	}

	public GameObject GetObjectiveIcon()
	{
		return m_objectiveIcon;
	}
}
