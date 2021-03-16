using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Gun_Beam")]
public class Gun_Beam : Gun
{
	public GameObject m_rayPrefab;

	public GameObject m_hitEffectLowPrefab;

	public GameObject m_hitEffectHiPrefab;

	public float m_rayWidth = 1f;

	private GameObject m_rayVisualizer;

	private GameObject m_hitEffect;

	private GameObject m_muzzleEffectInstance;

	private bool m_firing;

	private Vector3 m_rayTargetPos;

	private float m_dmgTimer;

	private int m_rayMask;

	private float m_rayVisTime;

	public override void Awake()
	{
		base.Awake();
		m_rayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("shield")) | (1 << LayerMask.NameToLayer("mines"));
	}

	protected override bool FireProjectile(Vector3 targetPos)
	{
		return false;
	}

	public override bool IsContinuous()
	{
		return true;
	}

	public override bool IsFiring()
	{
		return m_firing;
	}

	public override void StartFiring()
	{
		m_firing = true;
	}

	public override void StopFiring()
	{
		m_firing = false;
	}

	public override float EstimateTimeToImpact(Vector3 targetPos)
	{
		return 0f;
	}

	public override void Update()
	{
		base.Update();
		if (NetObj.m_simulating)
		{
			m_rayVisTime += Time.deltaTime;
			if (m_rayVisualizer != null)
			{
				m_rayVisualizer.renderer.material.SetFloat("_rayTime", m_rayVisTime);
			}
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetObj.m_simulating)
		{
			m_dmgTimer -= Time.fixedDeltaTime;
			bool flag = false;
			if (m_target == null)
			{
				StopFiring();
			}
			if (m_firing && m_target.GetTargetWorldPos(out var _, GetOwnerTeam()))
			{
				if (m_loadedSalvo > 0f)
				{
					if (m_unit.m_onFireWeapon != null)
					{
						m_unit.m_onFireWeapon();
					}
					m_loadedSalvo -= Time.fixedDeltaTime;
					if (m_loadedSalvo < 0f)
					{
						m_loadedSalvo = 0f;
					}
					Vector3 muzzlePos = GetMuzzlePos();
					Vector3 forward = m_elevationJoint[0].forward;
					RayTest(muzzlePos, forward, m_aim.m_maxRange, out var hitPos);
					if (IsVisible())
					{
						flag = true;
						EnableRayVisualizer(hitPos);
					}
				}
				else
				{
					StopFiring();
				}
			}
			if (!flag)
			{
				DisableRayVisualizer();
			}
			if (m_hitEffect != null && m_hitEffect.audio != null)
			{
				m_hitEffect.audio.volume = 1f;
			}
		}
		else if (m_hitEffect != null && m_hitEffect.audio != null)
		{
			m_hitEffect.audio.volume = 0f;
		}
	}

	private void EnableRayVisualizer(Vector3 targetPos)
	{
		m_rayTargetPos = targetPos;
		if (m_rayVisualizer == null)
		{
			m_rayVisualizer = Object.Instantiate(m_rayPrefab) as GameObject;
			m_rayVisualizer.transform.parent = base.transform;
		}
		Vector3 muzzlePos = GetMuzzlePos();
		Vector3 position = (muzzlePos + targetPos) * 0.5f;
		float num = Vector3.Distance(muzzlePos, targetPos);
		m_rayVisualizer.transform.position = position;
		m_rayVisualizer.transform.localScale = new Vector3(m_rayWidth, 1f, num);
		m_rayVisualizer.transform.rotation = Quaternion.LookRotation(targetPos - muzzlePos, Vector3.up);
		float y = num / 8f;
		m_rayVisualizer.renderer.material.mainTextureScale = new Vector2(1f, y);
		if (m_hitEffect == null)
		{
			if (m_hitEffectHiPrefab != null)
			{
				m_hitEffect = Object.Instantiate(m_hitEffectHiPrefab) as GameObject;
			}
			if (m_hitEffect != null)
			{
				m_hitEffect.transform.parent = base.transform;
				if (m_hitEffect.audio != null)
				{
					m_hitEffect.audio.volume = 0f;
				}
			}
		}
		if (m_hitEffect != null)
		{
			m_hitEffect.transform.position = targetPos;
		}
		if (m_muzzleEffectInstance == null)
		{
			if (m_muzzleEffect != null)
			{
				m_muzzleEffectInstance = Object.Instantiate(m_muzzleEffect) as GameObject;
			}
			if (m_muzzleEffectInstance != null)
			{
				m_muzzleEffectInstance.transform.parent = m_muzzleJoints[0].joint;
				m_muzzleEffectInstance.transform.localPosition = Vector3.zero;
				m_muzzleEffectInstance.transform.localRotation = Quaternion.identity;
			}
		}
		if (!(m_muzzleEffectInstance != null))
		{
		}
	}

	private void DisableRayVisualizer()
	{
		if (m_rayVisualizer != null)
		{
			Object.Destroy(m_rayVisualizer);
			m_rayVisualizer = null;
		}
		if (m_hitEffect != null)
		{
			Object.Destroy(m_hitEffect);
			m_hitEffect = null;
		}
		if (m_muzzleEffectInstance != null)
		{
			Object.Destroy(m_muzzleEffectInstance);
			m_muzzleEffectInstance = null;
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_firing);
		writer.Write(m_dmgTimer);
		writer.Write(m_rayTargetPos.x);
		writer.Write(m_rayTargetPos.y);
		writer.Write(m_rayTargetPos.z);
		writer.Write(m_rayVisTime);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_firing = reader.ReadBoolean();
		m_dmgTimer = reader.ReadSingle();
		m_rayTargetPos.x = reader.ReadSingle();
		m_rayTargetPos.y = reader.ReadSingle();
		m_rayTargetPos.z = reader.ReadSingle();
		m_rayVisTime = reader.ReadSingle();
		if (m_firing)
		{
			EnableRayVisualizer(m_rayTargetPos);
		}
	}

	private void RayTest(Vector3 muzzlePos, Vector3 direction, float maxDistance, out Vector3 hitPos)
	{
		RaycastHit[] array = Physics.RaycastAll(muzzlePos, direction, maxDistance, m_rayMask);
		int netID = GetUnit().GetNetID();
		float num = 999999f;
		RaycastHit raycastHit = default(RaycastHit);
		bool flag = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit2 = array2[i];
			if (!(raycastHit2.distance < num))
			{
				continue;
			}
			Section component = raycastHit2.collider.GetComponent<Section>();
			if (component == null && (bool)raycastHit2.collider.transform.parent)
			{
				component = raycastHit2.collider.transform.parent.GetComponent<Section>();
			}
			if (component != null)
			{
				if (component.GetUnit().GetNetID() != netID)
				{
					num = raycastHit2.distance;
					raycastHit = raycastHit2;
					flag = true;
				}
				continue;
			}
			HPModule component2 = raycastHit2.collider.GetComponent<HPModule>();
			if (component2 != null)
			{
				if (component2.GetUnit().GetNetID() != netID)
				{
					num = raycastHit2.distance;
					raycastHit = raycastHit2;
					flag = true;
				}
				continue;
			}
			ShieldGeometry component3 = raycastHit2.collider.GetComponent<ShieldGeometry>();
			if (component3 != null)
			{
				if (component3.GetUnit().GetNetID() != netID)
				{
					num = raycastHit2.distance;
					raycastHit = raycastHit2;
					flag = true;
				}
			}
			else
			{
				num = raycastHit2.distance;
				raycastHit = raycastHit2;
				flag = true;
			}
		}
		if (flag)
		{
			hitPos = raycastHit.point;
			if (m_dmgTimer <= 0f)
			{
				m_dmgTimer = 0.1f;
				int damage = (int)((float)GetRandomDamage() * 0.1f);
				DoRayDamage(damage, raycastHit.collider, raycastHit.point, direction);
			}
		}
		else
		{
			hitPos = muzzlePos + direction * maxDistance;
		}
	}

	private void DoRayDamage(int damage, Collider collider, Vector3 hitPos, Vector3 dir)
	{
		Section component = collider.GetComponent<Section>();
		if (component == null && (bool)collider.transform.parent)
		{
			component = collider.transform.parent.GetComponent<Section>();
		}
		if (component != null)
		{
			component.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
			return;
		}
		HPModule component2 = collider.GetComponent<HPModule>();
		if (component2 != null)
		{
			component2.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir), showDmgText: true);
			return;
		}
		ShieldGeometry component3 = collider.GetComponent<ShieldGeometry>();
		if (component3 != null)
		{
			component3.Damage(new Hit(this, damage * 2, m_Damage.m_armorPiercing, hitPos, dir), showDmgText: true);
			return;
		}
		Platform platform = GetPlatform(collider.gameObject);
		if (platform != null)
		{
			platform.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
			return;
		}
		Mine component4 = collider.GetComponent<Mine>();
		if (component4 != null)
		{
			component4.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
		}
	}

	public override Dictionary<string, string> GetShipEditorInfo()
	{
		return base.GetShipEditorInfo();
	}

	protected override float FindElevationAngle(Vector3 muzzlePos, Vector3 target, float muzzleVel)
	{
		target.y -= 0.5f;
		float num = Vector2.Distance(new Vector2(target.x, target.z), new Vector2(muzzlePos.x, muzzlePos.z));
		float num2 = target.y - muzzlePos.y;
		float num3 = Mathf.Atan(num2 / num);
		return num3 * 57.29578f;
	}
}
