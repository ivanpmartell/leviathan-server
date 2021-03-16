using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Projectiles/Projectile")]
public class Projectile : NetObj
{
	protected enum HitType
	{
		Normal,
		Armor,
		Ground,
		Water
	}

	public GameObject m_hitEffect;

	public GameObject m_armorHitEffect;

	public GameObject m_waterHitEffect;

	public GameObject m_groundHitEffect;

	public GameObject m_hitEffectLow;

	public GameObject m_armorHitEffectLow;

	public GameObject m_waterHitEffectLow;

	public GameObject m_groundHitEffectLow;

	private float m_ttl = 60f;

	protected Vector3 m_startPos;

	protected Vector3 m_dir = Vector3.one;

	protected Vector3 m_vel = Vector3.one;

	protected Vector3 m_prevPos = Vector3.zero;

	protected float m_originalPower;

	protected int m_gunID;

	protected float m_gravity = -1f;

	protected int m_damage;

	protected int m_armorPiercing;

	protected float m_splashRadius;

	protected float m_splashDamageFactor;

	protected float m_minRange;

	protected float m_maxRange = 10000f;

	protected bool m_hasHit;

	private int m_rayMask;

	public override void Awake()
	{
		base.Awake();
		m_rayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("shield")) | (1 << LayerMask.NameToLayer("mines"));
	}

	private void SetMobileChildActivity(bool active)
	{
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating)
		{
			m_ttl -= Time.fixedDeltaTime;
			if (m_ttl <= 0f)
			{
				Remove();
			}
			SpecializedFixedUpdate();
			DoRayTest();
		}
	}

	protected virtual void SpecializedFixedUpdate()
	{
		m_vel.y -= m_gravity * Time.fixedDeltaTime;
		base.transform.position += m_vel * Time.fixedDeltaTime;
		UpdateLookDirection();
	}

	private void UpdateLookDirection()
	{
		base.transform.rotation = Quaternion.LookRotation(m_vel);
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(100);
		writer.Write(m_gunID);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(m_startPos.x);
		writer.Write(m_startPos.y);
		writer.Write(m_startPos.z);
		writer.Write(m_prevPos.x);
		writer.Write(m_prevPos.y);
		writer.Write(m_prevPos.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
		writer.Write(m_vel.x);
		writer.Write(m_vel.y);
		writer.Write(m_vel.z);
		writer.Write(m_gravity);
		writer.Write(m_ttl);
		writer.Write(m_originalPower);
		writer.Write((short)m_damage);
		writer.Write((short)m_armorPiercing);
		writer.Write(m_splashRadius);
		writer.Write(m_splashDamageFactor);
		writer.Write(m_hasHit);
		Trace[] componentsInChildren = base.gameObject.GetComponentsInChildren<Trace>(includeInactive: true);
		Trace[] array = componentsInChildren;
		foreach (Trace trace in array)
		{
			trace.Save(writer);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		int num = reader.ReadInt32();
		m_gunID = reader.ReadInt32();
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		m_startPos.x = reader.ReadSingle();
		m_startPos.y = reader.ReadSingle();
		m_startPos.z = reader.ReadSingle();
		m_prevPos.x = reader.ReadSingle();
		m_prevPos.y = reader.ReadSingle();
		m_prevPos.z = reader.ReadSingle();
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
		m_vel.x = reader.ReadSingle();
		m_vel.y = reader.ReadSingle();
		m_vel.z = reader.ReadSingle();
		m_gravity = reader.ReadSingle();
		m_ttl = reader.ReadSingle();
		m_originalPower = reader.ReadSingle();
		m_damage = reader.ReadInt16();
		m_armorPiercing = reader.ReadInt16();
		if (num >= 100)
		{
			m_splashRadius = reader.ReadSingle();
			m_splashDamageFactor = reader.ReadSingle();
		}
		m_hasHit = reader.ReadBoolean();
		Trace[] componentsInChildren = base.gameObject.GetComponentsInChildren<Trace>(includeInactive: true);
		Trace[] array = componentsInChildren;
		foreach (Trace trace in array)
		{
			trace.Load(reader);
		}
		UpdateLookDirection();
	}

	public virtual void Setup(Vector3 dir, int owner, float vel, float gravity, Gun gun, int damage, int armorPiercing, float splashRadius, float splashDamageFactor, float minRange, float maxRange)
	{
		SetOwner(owner);
		SetVisible(gun.GetUnit().IsVisible());
		SetSeenByMask(gun.GetUnit().GetSeenByMask());
		m_startPos = base.transform.position;
		m_gunID = gun.GetNetID();
		m_dir = dir;
		m_prevPos = base.transform.position;
		m_originalPower = vel;
		m_damage = damage;
		m_armorPiercing = armorPiercing;
		m_splashRadius = splashRadius;
		m_splashDamageFactor = splashDamageFactor;
		m_minRange = minRange;
		m_maxRange = maxRange;
		m_vel = m_dir * m_originalPower;
		m_gravity = gravity;
	}

	protected void HitEffect(Vector3 pos, HitType type)
	{
		if (IsVisible())
		{
			GameObject gameObject = null;
			switch (type)
			{
			case HitType.Ground:
				gameObject = m_groundHitEffect;
				break;
			case HitType.Water:
				gameObject = m_waterHitEffect;
				break;
			case HitType.Normal:
				gameObject = m_hitEffect;
				break;
			case HitType.Armor:
				gameObject = m_armorHitEffect;
				break;
			}
			if (gameObject != null)
			{
				Object.Instantiate(gameObject, pos, Quaternion.identity);
			}
		}
	}

	public Unit GetOwnerUnit()
	{
		Gun gun = NetObj.GetByID(m_gunID) as Gun;
		if (gun != null)
		{
			return gun.GetUnit();
		}
		return null;
	}

	public Gun GetOwnerGun()
	{
		return NetObj.GetByID(m_gunID) as Gun;
	}

	protected void DoSplashDamage(Vector3 pos, Collider hitCollider, int damage)
	{
		if (m_splashDamageFactor > 0f && m_splashRadius > 0f)
		{
			int maxSplashDamage = (int)((float)damage * m_splashDamageFactor);
			GameRules.DoAreaDamage(GetOwnerGun(), pos, m_splashRadius, maxSplashDamage, m_armorPiercing, hitCollider);
		}
	}

	private void DoRayTest()
	{
		float num = Vector3.SqrMagnitude(m_prevPos - base.transform.position);
		if (!(num < 4f))
		{
			Vector3 vector = base.transform.position - m_prevPos;
			float magnitude = vector.magnitude;
			vector.Normalize();
			m_prevPos = base.transform.position;
			magnitude *= 2f;
			Vector3 origin = base.transform.position - vector * magnitude;
			if (Physics.Raycast(origin, vector, out var hitInfo, magnitude, m_rayMask))
			{
				OnHit(hitInfo.point, hitInfo.collider);
			}
		}
	}

	public static Platform GetPlatform(GameObject go)
	{
		Platform component = go.GetComponent<Platform>();
		if (component == null && go.transform.parent != null)
		{
			component = go.transform.parent.GetComponent<Platform>();
		}
		return component;
	}

	public static Section GetSection(GameObject go)
	{
		Section component = go.GetComponent<Section>();
		if (!component && go.transform.parent != null)
		{
			component = go.transform.parent.GetComponent<Section>();
		}
		return component;
	}

	protected virtual void OnHit(Vector3 pos, Collider other)
	{
		if (m_hasHit)
		{
			return;
		}
		int damage = m_damage;
		float num = Vector3.Distance(m_startPos, pos);
		if (num < m_minRange || num > m_maxRange)
		{
			damage = 0;
		}
		Section section = GetSection(other.gameObject);
		HPModule component = other.gameObject.GetComponent<HPModule>();
		Platform platform = GetPlatform(other.gameObject);
		ShieldGeometry component2 = other.gameObject.GetComponent<ShieldGeometry>();
		Mine component3 = other.gameObject.GetComponent<Mine>();
		Unit ownerUnit = GetOwnerUnit();
		Gun ownerGun = GetOwnerGun();
		if (ownerUnit == null || ownerUnit == null)
		{
			Remove();
		}
		else if (section != null)
		{
			if (section.GetUnit().GetNetID() != ownerUnit.GetNetID())
			{
				if (section.Damage(new Hit(ownerGun, damage, m_armorPiercing, pos, m_vel.normalized)))
				{
					HitEffect(pos, HitType.Normal);
				}
				else
				{
					HitEffect(pos, HitType.Armor);
				}
				DoSplashDamage(pos, other, damage);
				Remove();
			}
		}
		else if (component3 != null)
		{
			if (component3.Damage(new Hit(ownerGun, damage, m_armorPiercing, pos, m_vel.normalized)))
			{
				HitEffect(component3.transform.position, HitType.Normal);
			}
			else
			{
				HitEffect(component3.transform.position, HitType.Armor);
			}
			Remove();
		}
		else if (component != null)
		{
			if (component.GetUnit().GetNetID() == ownerUnit.GetNetID())
			{
				return;
			}
			Hit hit = new Hit(ownerGun, damage, m_armorPiercing, pos, m_vel.normalized);
			if (component.Damage(hit, showDmgText: true))
			{
				Section section2 = component.GetSection();
				if (section2 != null)
				{
					section2.Damage(hit);
				}
				HitEffect(pos, HitType.Normal);
			}
			else
			{
				HitEffect(pos, HitType.Armor);
			}
			DoSplashDamage(pos, other, damage);
			Remove();
		}
		else if (platform != null)
		{
			if (platform.GetNetID() != ownerUnit.GetNetID())
			{
				if (platform.Damage(new Hit(ownerGun, damage, m_armorPiercing, pos, m_vel.normalized)))
				{
					HitEffect(pos, HitType.Normal);
				}
				else
				{
					HitEffect(pos, HitType.Armor);
				}
				DoSplashDamage(pos, other, damage);
				Remove();
			}
		}
		else if (component2 != null)
		{
			if (component2.GetUnit().GetNetID() != ownerUnit.GetNetID())
			{
				component2.Damage(new Hit(ownerGun, damage, m_armorPiercing, pos, m_vel.normalized), showDmgText: true);
				Remove();
			}
		}
		else
		{
			if (other.tag == "Water")
			{
				HitEffect(pos, HitType.Water);
			}
			else
			{
				HitEffect(pos, HitType.Ground);
			}
			DoSplashDamage(pos, null, damage);
			Remove();
		}
	}

	protected void Remove()
	{
		m_hasHit = true;
		Trace componentInChildren = GetComponentInChildren<Trace>();
		if (componentInChildren != null)
		{
			componentInChildren.Die();
		}
		Object.Destroy(base.gameObject);
	}

	public override void SetVisible(bool visible)
	{
		if (IsVisible() != visible)
		{
			base.SetVisible(visible);
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = visible;
			}
			Trace componentInChildren = GetComponentInChildren<Trace>();
			if (componentInChildren != null)
			{
				componentInChildren.SetVisible(visible);
			}
		}
	}

	protected override void OnSetSimulating(bool enabled)
	{
		ParticleEmitter componentInChildren = base.gameObject.GetComponentInChildren<ParticleEmitter>();
		if ((bool)componentInChildren)
		{
			componentInChildren.enabled = enabled;
		}
	}
}
