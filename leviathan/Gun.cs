#define DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Gun")]
public abstract class Gun : HPModule
{
	public enum Stance
	{
		FireAtWill,
		HoldFire
	}

	[Serializable]
	public class Aim
	{
		public Order.FireVisual m_orderMarkerType;

		public bool m_noAutotarget;

		public bool m_spreadIgnoresRange;

		public bool m_staticTargetOnly;

		public bool m_useHighAngle;

		public bool m_manualTarget = true;

		public Stance m_stance;

		public float m_maxRot = 75f;

		public float m_rotationSpeed = 0.8f;

		public float m_elevationSpeed = 0.8f;

		public float m_minRange = 15f;

		public float m_maxRange = 50f;

		public float m_baseSpread;

		public GameObject viewConePrefab;
	}

	[Serializable]
	public class GunDamage
	{
		public int m_damage_min = 5;

		public int m_damage_max = 5;

		public int m_armorPiercing;

		public float m_splashDamageFactor;

		public float m_splashRadius;
	}

	[Serializable]
	public class FireSettings
	{
		public float m_basePreFireTime;

		public int m_baseSalvo = 1;

		public float m_salvoDelay = 0.1f;

		public bool repeatMuzzleCycle = true;

		public bool m_barrage = true;

		public bool m_removeInvalidOrder;

		public float m_baseReloadTime;

		public int m_baseMaxAmmo = -1;

		public int m_ammoSupplyCost = 1;

		public int m_ammoPerSupply = 1;
	}

	public static GenericFactory<AIState<Gun>> m_aiStateFactory = new GenericFactory<AIState<Gun>>();

	protected AIStateMachine<Gun> m_stateMachine;

	public Aim m_aim = new Aim();

	public GunDamage m_Damage = new GunDamage();

	public FireSettings m_fire = new FireSettings();

	public Transform[] m_elevationJoint = new Transform[0];

	public List<GunMuzzleDef> m_muzzleJoints;

	public GameObject m_muzzleEffect;

	public GameObject m_muzzleEffectLow;

	public GameObject m_outOfAmmoIconPrefab;

	public GameObject m_preFireLowPrefab;

	public GameObject m_preFireHiPrefab;

	private GameObject m_preFireEffect;

	public float m_muzzleVel = 30f;

	public float m_gravity = -1f;

	public GameObject m_projectile;

	public bool m_canDeploy;

	protected GunTarget m_target;

	protected ViewCone viewConeScript;

	protected bool m_viewConeVisible;

	protected GameObject m_viewConeInstance;

	protected int currentMuzzleJointIndex;

	protected int m_landRayMask;

	protected int m_unitsRayMask;

	protected float m_loadedSalvo;

	private float m_spread;

	private float m_reloadTime;

	private float m_preFireTime;

	private int m_salvo;

	private int m_maxAmmo;

	private int m_ammo;

	private bool m_deploy;

	private GameObject m_outOfAmmoIcon;

	protected LineDrawer m_lineDrawer;

	private int m_targetLineMaterialID = -1;

	private int m_inRangeLineMaterialID = -1;

	private int m_outOfRangeLineMaterialID = -1;

	public GameObject ViewConeInstance
	{
		get
		{
			return m_viewConeInstance;
		}
		private set
		{
			m_viewConeInstance = value;
		}
	}

	public static void RegisterAIStates()
	{
		m_aiStateFactory.Register<GunGuard>("guard");
		m_aiStateFactory.Register<GunReload>("reload");
		m_aiStateFactory.Register<GunAim>("aim");
		m_aiStateFactory.Register<GunFire>("fire");
		m_aiStateFactory.Register<GunFireBeam>("firebeam");
		m_aiStateFactory.Register<GunThink>("think");
		m_aiStateFactory.Register<GunFollowOrder>("followorder");
		m_aiStateFactory.Register<GunDeploy>("deploy");
		m_aiStateFactory.Register<GunOff>("off");
	}

	public override void Awake()
	{
		base.Awake();
		m_landRayMask = 1 << LayerMask.NameToLayer("Default");
		m_unitsRayMask = 1 << LayerMask.NameToLayer("units");
		if (m_gravity < 0f)
		{
			m_gravity = 5f;
		}
		m_stateMachine = new AIStateMachine<Gun>(this, m_aiStateFactory);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		TimedDestruction[] componentsInChildren = base.transform.GetComponentsInChildren<TimedDestruction>();
		TimedDestruction[] array = componentsInChildren;
		foreach (TimedDestruction timedDestruction in array)
		{
			timedDestruction.transform.parent = null;
		}
		if ((bool)m_preFireEffect)
		{
			UnityEngine.Object.Destroy(m_preFireEffect);
		}
		m_preFireEffect = null;
	}

	public override void Setup(Unit unit, Battery battery, int x, int y, Direction dir, DestroyedHandler destroyedCallback)
	{
		base.Setup(unit, battery, x, y, dir, destroyedCallback);
		UpdateStats();
		m_stateMachine.PushState("think");
		m_ammo = m_maxAmmo;
		LoadGun();
	}

	public override StatusWnd_HPModule CreateStatusWindow(GameObject guiCamera)
	{
		bool friend = GetOwner() == NetObj.m_localPlayerID;
		return new StatusWnd_Gun(this, guiCamera, friend);
	}

	public override void Supply(ref int resources)
	{
		if (resources > 0)
		{
			base.Supply(ref resources);
			if (resources >= m_fire.m_ammoSupplyCost && m_maxAmmo >= 0 && (float)m_maxAmmo - ((float)m_ammo + m_loadedSalvo) >= (float)m_fire.m_ammoPerSupply)
			{
				m_ammo += m_fire.m_ammoPerSupply;
				resources -= m_fire.m_ammoSupplyCost;
			}
		}
	}

	public override List<string> GetHardpointInfo()
	{
		List<string> list = new List<string>();
		if (m_Damage.m_damage_min != 0 && m_Damage.m_damage_max != 0)
		{
			if (m_Damage.m_damage_min == m_Damage.m_damage_max)
			{
				list.Add(Localize.instance.Translate("$DamageRange") + ": " + m_Damage.m_damage_max);
			}
			else
			{
				list.Add(Localize.instance.Translate("$DamageRange") + ": " + m_Damage.m_damage_min + " - " + m_Damage.m_damage_max);
			}
		}
		if (m_Damage.m_armorPiercing != 0)
		{
			list.Add(Localize.instance.Translate("$ArmorPiercingShort") + ": " + m_Damage.m_armorPiercing);
		}
		return list;
	}

	public void HideViewCone()
	{
		if (m_viewConeInstance != null)
		{
			UnityEngine.Object.Destroy(m_viewConeInstance);
			m_viewConeInstance = null;
		}
	}

	public void ShowViewCone()
	{
		if (!(m_aim.viewConePrefab == null) && !(m_viewConeInstance != null))
		{
			m_viewConeInstance = UnityEngine.Object.Instantiate(m_aim.viewConePrefab) as GameObject;
			m_viewConeInstance.transform.parent = base.transform;
			m_viewConeInstance.transform.localPosition = Vector3.zero;
			m_viewConeInstance.transform.localRotation = Quaternion.identity;
			ViewCone component = m_viewConeInstance.GetComponent<ViewCone>();
			DebugUtils.Assert(component != null, "Missing viewcone script on viewcone");
			component.Setup(m_aim.m_minRange, m_aim.m_maxRange, m_aim.m_maxRot);
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write((short)1);
		m_stateMachine.Save(writer);
		writer.Write(m_loadedSalvo);
		writer.Write((short)m_ammo);
		if (m_target != null)
		{
			writer.Write(value: true);
			m_target.Save(writer);
		}
		else
		{
			writer.Write(value: false);
		}
		writer.Write(m_visual.transform.localRotation.x);
		writer.Write(m_visual.transform.localRotation.y);
		writer.Write(m_visual.transform.localRotation.z);
		writer.Write(m_visual.transform.localRotation.w);
		if (m_elevationJoint.Length > 0)
		{
			writer.Write(m_elevationJoint[0].localRotation.x);
			writer.Write(m_elevationJoint[0].localRotation.y);
			writer.Write(m_elevationJoint[0].localRotation.z);
			writer.Write(m_elevationJoint[0].localRotation.w);
		}
		writer.Write(m_fire.repeatMuzzleCycle);
		writer.Write((byte)currentMuzzleJointIndex);
		if ((bool)m_preFireEffect)
		{
			writer.Write(value: true);
		}
		else
		{
			writer.Write(value: false);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		short num = reader.ReadInt16();
		m_stateMachine.Load(reader);
		m_loadedSalvo = reader.ReadSingle();
		m_ammo = reader.ReadInt16();
		if (reader.ReadBoolean())
		{
			m_target = new GunTarget(reader);
		}
		Quaternion q = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		Utils.NormalizeQuaternion(ref q);
		m_visual.transform.localRotation = q;
		if (m_elevationJoint.Length > 0)
		{
			Quaternion localRotation = default(Quaternion);
			localRotation.x = reader.ReadSingle();
			localRotation.y = reader.ReadSingle();
			localRotation.z = reader.ReadSingle();
			localRotation.w = reader.ReadSingle();
			Transform[] elevationJoint = m_elevationJoint;
			foreach (Transform transform in elevationJoint)
			{
				transform.localRotation = localRotation;
			}
		}
		m_fire.repeatMuzzleCycle = reader.ReadBoolean();
		currentMuzzleJointIndex = reader.ReadByte();
		if (reader.ReadBoolean())
		{
			PreFireGun();
		}
	}

	public override void ClearOrders()
	{
		base.ClearOrders();
		m_deploy = false;
	}

	public override void AddOrder(Order order)
	{
		base.AddOrder(order);
		UpdateFireOrder(order);
	}

	public override void OnOrdersChanged()
	{
		base.OnOrdersChanged();
		UpdateFireOrders();
	}

	public override void SaveOrders(BinaryWriter stream)
	{
		base.SaveOrders(stream);
		stream.Write((int)m_aim.m_stance);
		stream.Write(m_deploy);
	}

	public override void LoadOrders(BinaryReader stream)
	{
		base.LoadOrders(stream);
		SetStance((Stance)stream.ReadInt32());
		m_deploy = stream.ReadBoolean();
	}

	public override void DrawOrders()
	{
		if (GetOwner() != NetObj.m_localPlayerID)
		{
			return;
		}
		SetupLineDrawer();
		Vector3 vector = base.transform.position + new Vector3(0f, 1f, 0f);
		DrawTargetLine(vector);
		foreach (Order order in m_orders)
		{
			if (order.m_type != Order.Type.Fire)
			{
				continue;
			}
			Vector3 pos = order.GetPos();
			int type = ((!order.IsInFiringCone()) ? m_outOfRangeLineMaterialID : m_inRangeLineMaterialID);
			if (m_aim.m_useHighAngle)
			{
				float num = Vector3.Distance(vector, pos);
				float y = num / 2f;
				int sections = Mathf.Max(8, (int)(num / 10f));
				m_lineDrawer.DrawCurvedLine(vector, pos, new Vector3(0f, y, 0f), type, 0.1f, sections);
			}
			else if (order.IsLOSBlocked())
			{
				if (Mathf.Sin(Time.time * 30f) > 0f)
				{
					m_lineDrawer.DrawLine(vector, pos, type, 0.1f);
				}
			}
			else
			{
				m_lineDrawer.DrawLine(vector, pos, type, 0.1f);
			}
		}
	}

	protected bool SetupLineDrawer()
	{
		if (m_lineDrawer == null)
		{
			m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
			if (!m_lineDrawer)
			{
				return false;
			}
			m_targetLineMaterialID = m_lineDrawer.GetTypeID("target");
			m_inRangeLineMaterialID = m_lineDrawer.GetTypeID("orderInRange");
			m_outOfRangeLineMaterialID = m_lineDrawer.GetTypeID("orderOutOfRange");
			return true;
		}
		return true;
	}

	protected void DrawTargetLine(Vector3 line)
	{
		if (m_target == null)
		{
			return;
		}
		Vector3 vector = base.transform.position + new Vector3(0f, 1f, 0f);
		if (m_target.GetTargetWorldPos(out var worldPos, GetOwnerTeam()))
		{
			if (m_aim.m_useHighAngle)
			{
				float num = Vector3.Distance(vector, worldPos);
				float y = num / 2f;
				int sections = Mathf.Max(8, (int)(num / 10f));
				m_lineDrawer.DrawCurvedLine(vector, worldPos, new Vector3(0f, y, 0f), m_targetLineMaterialID, 0.1f, sections);
			}
			else
			{
				m_lineDrawer.DrawLine(vector, worldPos, m_targetLineMaterialID, 0.1f);
			}
		}
	}

	private void UpdateFireOrders()
	{
		foreach (Order order in m_orders)
		{
			UpdateFireOrder(order);
		}
	}

	protected virtual void UpdateFireOrder(Order o)
	{
		int ownerTeam = GetOwnerTeam();
		if (o.m_type == Order.Type.Fire)
		{
			Vector3 pos = o.GetPos();
			bool lOSBlocked = !TestLOF(pos);
			bool inFiringCone = InFiringCone(pos);
			o.SetLOSBlocked(lOSBlocked);
			o.SetInFiringCone(inFiringCone);
		}
	}

	public virtual void StartFiring()
	{
	}

	public virtual void StopFiring()
	{
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		SetTarget(null);
	}

	public virtual void PreFireGun()
	{
		if (m_preFireEffect != null)
		{
			return;
		}
		if (m_preFireHiPrefab != null)
		{
			m_preFireEffect = UnityEngine.Object.Instantiate(m_preFireHiPrefab, GetMuzzlePos(), m_muzzleJoints[0].joint.rotation) as GameObject;
			m_preFireEffect.transform.parent = base.transform;
		}
		if (m_preFireEffect != null)
		{
			Renderer[] componentsInChildren = m_preFireEffect.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = IsVisible();
			}
		}
	}

	public void StopPreFire()
	{
		if ((bool)m_preFireEffect)
		{
			UnityEngine.Object.Destroy(m_preFireEffect);
		}
		m_preFireEffect = null;
	}

	public bool FireGun()
	{
		if (m_loadedSalvo <= 0f || m_target == null)
		{
			return false;
		}
		if (GetOptimalTargetPosition(m_target, out var targetPos) && FireProjectile(targetPos))
		{
			if (m_unit.m_onFireWeapon != null)
			{
				m_unit.m_onFireWeapon();
			}
			AnimateFire();
			m_loadedSalvo -= 1f;
			return true;
		}
		return false;
	}

	public bool LoadGun()
	{
		if (m_maxAmmo < 0)
		{
			m_loadedSalvo = m_salvo;
			return true;
		}
		int num = m_salvo - (int)m_loadedSalvo;
		if (num > 0 && m_ammo <= 0)
		{
			return false;
		}
		while (num > 0 && m_ammo > 0)
		{
			num--;
			m_ammo--;
			m_loadedSalvo += 1f;
		}
		return true;
	}

	protected abstract bool FireProjectile(Vector3 targetPos);

	public override void SetSelected(bool selected, bool explicitSelected)
	{
		if (selected != IsSelected())
		{
			base.SetSelected(selected, explicitSelected);
			if (selected)
			{
				ShowViewCone();
			}
			else
			{
				HideViewCone();
			}
		}
	}

	public float GetLoadedSalvo()
	{
		return m_loadedSalvo;
	}

	public float GetReloadTime()
	{
		return m_reloadTime;
	}

	public float GetPreFireTime()
	{
		return m_preFireTime;
	}

	public float GetSalvoDelay()
	{
		return m_fire.m_salvoDelay;
	}

	public int GetAmmo()
	{
		return m_ammo;
	}

	public int GetSalvoSize()
	{
		return m_salvo;
	}

	public int GetMaxAmmo()
	{
		return m_maxAmmo;
	}

	public bool GetBarrage()
	{
		return m_fire.m_barrage;
	}

	public Stance GetStance()
	{
		return m_aim.m_stance;
	}

	public void SetStance(Stance stance)
	{
		m_aim.m_stance = stance;
	}

	public override void SetDeploy(bool deploy)
	{
		m_deploy = deploy;
	}

	public override bool GetDeploy()
	{
		return m_deploy;
	}

	public virtual float GetTargetRadius(Vector3 targetPos)
	{
		return 0f;
	}

	public virtual bool IsContinuous()
	{
		return false;
	}

	public virtual bool IsFiring()
	{
		return false;
	}

	public bool GetStaticTargetOnly()
	{
		return m_aim.m_staticTargetOnly;
	}

	public Order.FireVisual GetOrderMarkerType()
	{
		return m_aim.m_orderMarkerType;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetObj.m_simulating && !m_disabled)
		{
			if (base.transform.position.y < -2f && m_unit.IsSinking())
			{
				Damage(new Hit(m_health, 100), showDmgText: false);
			}
			else
			{
				m_stateMachine.Update(Time.fixedDeltaTime);
			}
		}
	}

	protected virtual void AnimateFire()
	{
		if (IsVisible() && m_visual != null)
		{
			string animationName = GetCurrentMuzzleJointDef().animationName;
			Animation[] componentsInChildren = m_visual.GetComponentsInChildren<Animation>();
			int num = ((componentsInChildren != null) ? componentsInChildren.Length : 0);
			if (num > 0)
			{
				Animation[] array = componentsInChildren;
				foreach (Animation animation in array)
				{
					if (!(animation.GetClip(animationName) == null))
					{
						animation.Play(animationName);
					}
				}
			}
			if (m_muzzleJoints.Count > 0 && m_muzzleEffect != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_muzzleEffect, GetMuzzlePos(), m_muzzleJoints[0].joint.rotation) as GameObject;
				gameObject.transform.parent = base.transform;
			}
		}
		SetNextMuzzleJoint();
	}

	public GunTarget GetTarget()
	{
		return m_target;
	}

	public void SetTarget(GunTarget target)
	{
		m_target = target;
	}

	public bool GetRemoveInvalidTarget()
	{
		return m_fire.m_removeInvalidOrder;
	}

	public GunTarget FindTarget()
	{
		Unit unit = null;
		Vector3 position = Vector3.zero;
		float num = float.MaxValue;
		int owner = GetOwner();
		int ownerTeam = GetOwnerTeam();
		TurnMan instance = TurnMan.instance;
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Unit unit2 = item as Unit;
			if (!(unit2 != null) || !unit2.IsValidTarget() || !instance.IsHostile(owner, unit2.GetOwner()) || !unit2.IsSeenByTeam(ownerTeam))
			{
				continue;
			}
			Ship ship = item as Ship;
			if ((!(ship != null) || !ship.GetAi().m_inactive) && InFiringCone(unit2, out var inConePoint) && TestLOF(inConePoint))
			{
				float num2 = Vector3.Distance(base.transform.position, inConePoint);
				if (num2 < num)
				{
					unit = unit2;
					position = inConePoint;
					num = num2;
				}
			}
		}
		if (unit != null)
		{
			int netID = unit.GetNetID();
			Vector3 localTargetPos = unit.transform.InverseTransformPoint(position);
			return new GunTarget(netID, localTargetPos);
		}
		return null;
	}

	public virtual bool ResetTower()
	{
		if (m_elevationJoint.Length == 0)
		{
			return true;
		}
		Quaternion quaternion = Quaternion.RotateTowards(m_visual.transform.localRotation, Quaternion.identity, m_aim.m_rotationSpeed);
		m_visual.transform.localRotation = quaternion;
		Quaternion quaternion2 = Quaternion.RotateTowards(m_elevationJoint[0].localRotation, Quaternion.identity, m_aim.m_elevationSpeed);
		Transform[] elevationJoint = m_elevationJoint;
		foreach (Transform transform in elevationJoint)
		{
			transform.localRotation = quaternion2;
		}
		float num = Quaternion.Angle(quaternion, Quaternion.identity);
		float num2 = Quaternion.Angle(quaternion2, Quaternion.identity);
		return num < 1f && num2 < 1f;
	}

	protected bool FindOptimalFireDir(Vector3 muzzlePos, Vector3 target, out Quaternion dir)
	{
		Vector3 normalized = (target - muzzlePos).normalized;
		Vector3 forward = normalized;
		forward.y = 0f;
		forward.Normalize();
		Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
		float num = FindElevationAngle(muzzlePos, target, m_muzzleVel);
		if (float.IsNaN(num))
		{
			dir = m_elevationJoint[0].rotation;
			return false;
		}
		Quaternion quaternion2 = Quaternion.Euler(0f - num, 0f, 0f);
		dir = quaternion * quaternion2;
		return true;
	}

	public virtual bool AimAt(Vector3 target)
	{
		if (m_elevationJoint.Length == 0)
		{
			return true;
		}
		Vector3 muzzlePos = GetMuzzlePos();
		Vector3 normalized = (target - muzzlePos).normalized;
		Vector3 forward = normalized;
		forward.y = 0f;
		forward.Normalize();
		Quaternion b = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		Quaternion quaternion = Quaternion.LookRotation(forward, base.transform.up);
		Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation) * quaternion;
		Vector3 eulerAngles = quaternion2.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		quaternion2.eulerAngles = eulerAngles;
		Quaternion quaternion3 = Quaternion.RotateTowards(m_visual.transform.localRotation, quaternion2, m_aim.m_rotationSpeed);
		bool flag = Quaternion.Angle(quaternion3, quaternion2) < 0.5f;
		if (flag)
		{
			quaternion3 = quaternion2;
		}
		if (m_aim.m_maxRot < 0f || Quaternion.Angle(quaternion3, b) <= m_aim.m_maxRot)
		{
			m_visual.transform.localRotation = quaternion3;
		}
		float num = FindElevationAngle(GetMuzzlePos(), target, m_muzzleVel);
		if (float.IsNaN(num))
		{
			return false;
		}
		Vector3 forward2 = m_visual.transform.forward;
		forward2.y = 0f;
		forward2.Normalize();
		float num2 = Vector3.Angle(forward2, m_visual.transform.forward);
		if (m_visual.transform.forward.y < 0f)
		{
			num2 = 0f - num2;
		}
		Quaternion quaternion4 = Quaternion.Euler(0f - num + num2, 0f, 0f);
		Quaternion quaternion5 = Quaternion.RotateTowards(m_elevationJoint[0].localRotation, quaternion4, m_aim.m_elevationSpeed);
		bool flag2 = Quaternion.Angle(quaternion5, quaternion4) < 0.5f;
		if (flag2)
		{
			quaternion5 = quaternion4;
		}
		Transform[] elevationJoint = m_elevationJoint;
		foreach (Transform transform in elevationJoint)
		{
			transform.localRotation = quaternion5;
		}
		return flag && flag2;
	}

	protected virtual float FindElevationAngle(Vector3 muzzlePos, Vector3 target, float muzzleVel)
	{
		float num = Vector2.Distance(new Vector2(target.x, target.z), new Vector2(muzzlePos.x, muzzlePos.z));
		float num2 = target.y - muzzlePos.y;
		float num3 = Mathf.Atan(num2 / num);
		float num4 = 0.5f * Mathf.Asin(m_gravity * num / (muzzleVel * muzzleVel));
		num4 += num3;
		num4 *= 57.29578f;
		if (m_aim.m_useHighAngle)
		{
			num4 = 90f - num4;
		}
		return num4;
	}

	public override string GetStatusText()
	{
		AIState<Gun> activeState = m_stateMachine.GetActiveState();
		if (activeState != null)
		{
			return activeState.GetStatusText();
		}
		return string.Empty;
	}

	public override void GetChargeLevel(out float i, out float time)
	{
		if (m_disabled)
		{
			base.GetChargeLevel(out i, out time);
			return;
		}
		AIState<Gun> activeState = m_stateMachine.GetActiveState();
		if (activeState != null)
		{
			activeState.GetCharageLevel(out i, out time);
			return;
		}
		i = -1f;
		time = -1f;
	}

	public override string GetTooltip()
	{
		string text = GetName() + "\nHP: " + m_health;
		return text + "\nStatus: " + GetStatusText();
	}

	public bool InRange(Unit unit)
	{
		float num = unit.GetLength() / 2f;
		float num2 = Vector3.Distance(unit.transform.position, base.transform.position);
		return num2 + num >= m_aim.m_minRange && num2 - num <= m_aim.m_maxRange;
	}

	public bool InRange(Vector3 pos)
	{
		float num = Vector3.Distance(pos, base.transform.position);
		return num >= m_aim.m_minRange && num <= m_aim.m_maxRange;
	}

	public bool InFiringCone(Unit unit, out Vector3 inConePoint)
	{
		inConePoint = Vector3.zero;
		if (!InRange(unit))
		{
			return false;
		}
		if (m_aim.m_maxRot < 0f)
		{
			return true;
		}
		Vector3[] targetPoints = unit.GetTargetPoints();
		Vector3[] array = targetPoints;
		foreach (Vector3 vector in array)
		{
			Vector3 vector2 = vector - base.transform.position;
			float magnitude = vector2.magnitude;
			Vector3 to = vector2;
			to.y = 0f;
			to.Normalize();
			float num = Vector3.Angle(base.transform.forward, to);
			if (num <= m_aim.m_maxRot && magnitude >= m_aim.m_minRange && magnitude <= m_aim.m_maxRange)
			{
				inConePoint = vector;
				return true;
			}
		}
		return false;
	}

	public bool InFiringCone(GunTarget target)
	{
		if (target.GetTargetWorldPos(out var worldPos, GetOwnerTeam()))
		{
			return InFiringCone(worldPos);
		}
		return false;
	}

	public bool InFiringCone(Vector3 point)
	{
		if (!InRange(point))
		{
			return false;
		}
		if (m_aim.m_maxRot < 0f)
		{
			return true;
		}
		Vector3 to = point - base.transform.position;
		to.y = 0f;
		to.Normalize();
		float num = Vector3.Angle(base.transform.forward, to);
		return num <= m_aim.m_maxRot;
	}

	private Unit GetCollisionUnit(RaycastHit hit)
	{
		Unit component;
		if (hit.rigidbody != null)
		{
			component = hit.rigidbody.GetComponent<Unit>();
		}
		else
		{
			component = hit.collider.GetComponent<Unit>();
			if (component == null)
			{
				component = hit.collider.transform.parent.GetComponent<Unit>();
			}
		}
		return component;
	}

	public bool TestLOF(Vector3 point)
	{
		if (m_aim.m_useHighAngle)
		{
			return true;
		}
		Vector3 position = base.transform.position;
		Vector3 vector = point - base.transform.position;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		if (magnitude <= 1f)
		{
			return true;
		}
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(position, normalized, out hitInfo, magnitude - 1f, m_landRayMask) && hitInfo.distance > 1f)
		{
			return false;
		}
		position.y = 1f;
		normalized.y = 0f;
		normalized.Normalize();
		int ownerTeam = GetOwnerTeam();
		RaycastHit[] array = Physics.RaycastAll(position, normalized, magnitude - 1f, m_unitsRayMask);
		RaycastHit[] array2 = array;
		foreach (RaycastHit hit in array2)
		{
			Unit collisionUnit = GetCollisionUnit(hit);
			if (collisionUnit != m_unit && collisionUnit.GetOwnerTeam() == ownerTeam)
			{
				return false;
			}
		}
		return true;
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		if (m_outOfAmmoIcon != null)
		{
			m_outOfAmmoIcon.renderer.enabled = visible;
		}
		if (m_preFireEffect != null)
		{
			Renderer[] componentsInChildren = m_preFireEffect.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = visible;
			}
		}
	}

	protected virtual int GetRandomDamage()
	{
		if (CheatMan.instance.GetInstaGib() && TurnMan.instance.IsHuman(GetOwner()))
		{
			return 25000;
		}
		return PRand.Range(m_Damage.m_damage_min, m_Damage.m_damage_max);
	}

	protected static void ApplyAccuracyOnTarget(ref Vector3 target, int accuracy, float accuracyRadius)
	{
		if (accuracy != 100 && accuracyRadius != 0f && PRand.Range(1, 100) > accuracy)
		{
			float max = accuracyRadius * Mathf.Clamp(1.3f - (float)accuracy / 100f, 0.05f, 1f);
			float num = PRand.Range(1f, max);
			bool flag = PRand.Range(0, 100) > 33;
			bool flag2 = PRand.Range(0, 100) > 33;
			if (!flag && !flag2)
			{
				flag = (flag2 = true);
			}
			float num2 = 0f;
			float num3 = 0f;
			float f = PRand.Range(1f, 359f);
			if (flag)
			{
				num2 = Mathf.Cos(f) * num;
			}
			if (flag2)
			{
				num3 = Mathf.Sin(f) * num;
			}
			target.x += num2;
			target.z += num3;
		}
	}

	protected GunMuzzleDef GetCurrentMuzzleJointDef()
	{
		if (currentMuzzleJointIndex >= 0 && currentMuzzleJointIndex <= m_muzzleJoints.Count - 1)
		{
			return m_muzzleJoints[currentMuzzleJointIndex];
		}
		return null;
	}

	protected void SetNextMuzzleJoint()
	{
		if (m_muzzleJoints == null || m_muzzleJoints.Count <= 1)
		{
			return;
		}
		currentMuzzleJointIndex++;
		if (currentMuzzleJointIndex > m_muzzleJoints.Count - 1)
		{
			if (m_fire.repeatMuzzleCycle)
			{
				currentMuzzleJointIndex = 0;
			}
			else
			{
				currentMuzzleJointIndex = m_muzzleJoints.Count - 1;
			}
		}
	}

	protected void SetPrevMuzzleJoint()
	{
		if (m_muzzleJoints == null || m_muzzleJoints.Count <= 1)
		{
			return;
		}
		currentMuzzleJointIndex--;
		if (currentMuzzleJointIndex < 0)
		{
			if (m_fire.repeatMuzzleCycle)
			{
				currentMuzzleJointIndex = m_muzzleJoints.Count - 1;
			}
			else
			{
				currentMuzzleJointIndex = 0;
			}
		}
	}

	protected Vector3 GetMuzzlePos()
	{
		GunMuzzleDef currentMuzzleJointDef = GetCurrentMuzzleJointDef();
		DebugUtils.Assert(currentMuzzleJointDef != null);
		return currentMuzzleJointDef.joint.position;
	}

	public virtual float EstimateTimeToImpact(Vector3 targetPos)
	{
		float num = Vector3.Distance(base.transform.position, targetPos);
		return num / m_muzzleVel;
	}

	public bool GetOptimalTargetPosition(GunTarget target, out Vector3 targetPos)
	{
		if (target.GetTargetWorldPos(out targetPos, GetOwnerTeam()))
		{
			NetObj targetObject = target.GetTargetObject();
			if (targetObject == null)
			{
				return true;
			}
			float num = EstimateTimeToImpact(targetPos);
			Unit unit = targetObject as Unit;
			if (unit != null)
			{
				targetPos += unit.GetVelocity() * num;
			}
			else
			{
				HPModule hPModule = target.GetTargetObject() as HPModule;
				if (hPModule != null)
				{
					targetPos += hPModule.GetVelocity() * num;
				}
			}
			return true;
		}
		return false;
	}

	private void UpdateStats()
	{
		m_spread = m_aim.m_baseSpread;
		m_salvo = m_fire.m_baseSalvo;
		m_reloadTime = m_fire.m_baseReloadTime;
		m_preFireTime = m_fire.m_basePreFireTime;
		m_maxAmmo = m_fire.m_baseMaxAmmo;
	}

	private void SetOutOfAmmoIcon(bool enabled)
	{
		if (m_outOfAmmoIconPrefab == null)
		{
			return;
		}
		if (enabled)
		{
			if (m_outOfAmmoIcon == null)
			{
				m_outOfAmmoIcon = UnityEngine.Object.Instantiate(m_outOfAmmoIconPrefab) as GameObject;
				m_outOfAmmoIcon.transform.parent = base.transform;
				m_outOfAmmoIcon.transform.localPosition = new Vector3(0.5f, base.collider.bounds.max.y + 0.1f, 0f);
				m_outOfAmmoIcon.renderer.enabled = IsVisible();
			}
		}
		else if (m_outOfAmmoIcon != null)
		{
			UnityEngine.Object.Destroy(m_outOfAmmoIcon);
			m_outOfAmmoIcon = null;
		}
	}

	protected Quaternion GetRandomSpreadDirection(float aim, float range)
	{
		float num = 57.29578f * Mathf.Atan(m_spread / range);
		num -= num * aim;
		return Quaternion.Euler((PRand.Value() - 0.5f) * num * 0.5f, (PRand.Value() - 0.5f) * num, 0f);
	}

	public Platform GetPlatform(GameObject go)
	{
		Platform component = go.GetComponent<Platform>();
		if (component == null && go.transform.parent != null)
		{
			component = go.transform.parent.GetComponent<Platform>();
		}
		return component;
	}

	public override Dictionary<string, string> GetShipEditorInfo()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (m_Damage.m_damage_min == m_Damage.m_damage_max)
		{
			dictionary["$Damage"] = m_Damage.m_damage_max.ToString();
		}
		else
		{
			dictionary["$Damage"] = m_Damage.m_damage_min + " - " + m_Damage.m_damage_max;
		}
		dictionary["$Range"] = m_aim.m_minRange + " - " + m_aim.m_maxRange;
		dictionary["$Salvo"] = m_fire.m_baseSalvo.ToString();
		dictionary["$SplashDamage"] = m_Damage.m_splashRadius.ToString();
		dictionary["$ArmorPiercing"] = m_Damage.m_armorPiercing.ToString();
		dictionary["$Reload"] = m_fire.m_baseReloadTime.ToString();
		dictionary["$Rotation"] = (m_aim.m_maxRot * 2f).ToString();
		dictionary["$Ammo"] = m_fire.m_baseMaxAmmo.ToString();
		return dictionary;
	}
}
