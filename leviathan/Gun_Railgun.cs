using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Gun_Railgun")]
public class Gun_Railgun : Gun
{
	public GameObject m_rayPrefab;

	public GameObject m_hitEffectLowPrefab;

	public GameObject m_hitEffectHiPrefab;

	public GameObject m_rayEndEffectLowPrefab;

	public GameObject m_rayEndEffectHiPrefab;

	public float m_rayWidth = 1f;

	public float m_rayFadeTime = 0.5f;

	public float m_testOffset = 1f;

	private GameObject m_rayVisualizer;

	private Vector3 m_rayTargetPos;

	private int m_rayMask;

	private int m_solidsRayMask;

	private int m_rayLineType = -1;

	private float m_rayVisTime;

	public override void Awake()
	{
		base.Awake();
		m_rayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("shield")) | (1 << LayerMask.NameToLayer("mines"));
		m_solidsRayMask = 1 << LayerMask.NameToLayer("Default");
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		DisableRayVisualizer();
	}

	protected override bool FireProjectile(Vector3 targetPos)
	{
		StopPreFire();
		Vector3 muzzlePos = GetMuzzlePos();
		float num = Vector3.Distance(muzzlePos, targetPos);
		float range = ((!m_aim.m_spreadIgnoresRange) ? m_aim.m_maxRange : num);
		Quaternion randomSpreadDirection = GetRandomSpreadDirection(0f, range);
		FindOptimalFireDir(muzzlePos, targetPos, out var _);
		Vector3 vector = Vector3.Normalize(targetPos - muzzlePos);
		Vector3 direction = m_elevationJoint[0].rotation * (randomSpreadDirection * Vector3.forward);
		direction.y = 0f;
		direction.Normalize();
		bool hitLocalPlayer = false;
		RayTest(muzzlePos, direction, m_aim.m_maxRange, out var hitPoint, ref hitLocalPlayer);
		if (IsVisible() || hitLocalPlayer)
		{
			EnableRayVisualizer(muzzlePos, hitPoint);
		}
		return true;
	}

	public override float EstimateTimeToImpact(Vector3 targetPos)
	{
		return 0f;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetObj.m_simulating && m_rayVisualizer != null)
		{
			m_rayVisTime += Time.fixedDeltaTime;
			float num = Mathf.Clamp(m_rayVisTime / m_rayFadeTime, 0f, 1f);
			if (num >= 1f)
			{
				DisableRayVisualizer();
			}
			else if (m_rayVisualizer.renderer.material.HasProperty("_TintColor"))
			{
				Color color = m_rayVisualizer.renderer.material.GetColor("_TintColor");
				color.a = 1f - num;
				m_rayVisualizer.renderer.material.SetColor("_TintColor", color);
			}
		}
	}

	private void EnableRayVisualizer(Vector3 muzzlePos, Vector3 targetPos)
	{
		m_rayVisTime = 0f;
		m_rayTargetPos = targetPos;
		if (m_rayVisualizer == null)
		{
			m_rayVisualizer = Object.Instantiate(m_rayPrefab) as GameObject;
		}
		Vector3 position = (muzzlePos + targetPos) * 0.5f;
		float num = Vector3.Distance(muzzlePos, targetPos);
		m_rayVisualizer.transform.position = position;
		m_rayVisualizer.transform.localScale = new Vector3(m_rayWidth, 1f, num);
		m_rayVisualizer.transform.rotation = Quaternion.LookRotation(targetPos - muzzlePos, Vector3.up);
		float y = num / 8f;
		m_rayVisualizer.renderer.material.mainTextureScale = new Vector2(1f, y);
	}

	private void DisableRayVisualizer()
	{
		if (m_rayVisualizer != null)
		{
			Object.Destroy(m_rayVisualizer);
			m_rayVisualizer = null;
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_rayTargetPos.x);
		writer.Write(m_rayTargetPos.y);
		writer.Write(m_rayTargetPos.z);
		writer.Write(m_rayVisTime);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_rayTargetPos.x = reader.ReadSingle();
		m_rayTargetPos.y = reader.ReadSingle();
		m_rayTargetPos.z = reader.ReadSingle();
		m_rayVisTime = reader.ReadSingle();
	}

	private void SortRaytests(RaycastHit[] hits, out List<RaycastHit> sorted)
	{
		sorted = new List<RaycastHit>();
		foreach (RaycastHit item in hits)
		{
			sorted.Add(item);
		}
		sorted.Sort((RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
	}

	private void DoMultiHitAchievement(int hits)
	{
		if (hits >= 5)
		{
			ClientGame.instance.AwardAchievement(GetOwner(), 16);
		}
		if (hits >= 3)
		{
			ClientGame.instance.AwardAchievement(GetOwner(), 15);
		}
	}

	private void RayTest(Vector3 muzzlePos, Vector3 direction, float maxDistance, out Vector3 hitPoint, ref bool hitLocalPlayer)
	{
		Vector3 vector = muzzlePos;
		vector.y = m_testOffset;
		hitLocalPlayer = false;
		RaycastHit[] hits = Physics.RaycastAll(vector, direction, maxDistance, m_rayMask);
		int netID = GetUnit().GetNetID();
		SortRaytests(hits, out var sorted);
		int num = GetRandomDamage();
		HashSet<Unit> hashSet = new HashSet<Unit>();
		int num2 = 0;
		foreach (RaycastHit item in sorted)
		{
			if (num <= 0)
			{
				continue;
			}
			if (((1 << item.collider.gameObject.layer) & m_solidsRayMask) != 0)
			{
				hitPoint = item.point;
				hitPoint.y = muzzlePos.y;
				DoHitEffect(hitPoint, hitLocalPlayer: false);
				DoMultiHitAchievement(num2);
				return;
			}
			if (item.distance < m_aim.m_minRange || item.distance > m_aim.m_maxRange)
			{
				continue;
			}
			Section component = item.collider.GetComponent<Section>();
			if (component == null && (bool)item.collider.transform.parent)
			{
				component = item.collider.transform.parent.GetComponent<Section>();
			}
			if (component != null)
			{
				if (component.GetUnit().GetNetID() == netID || hashSet.Contains(component.GetUnit()))
				{
					continue;
				}
				hashSet.Add(component.GetUnit());
			}
			HPModule component2 = item.collider.GetComponent<HPModule>();
			if (component2 != null && component2.GetUnit().GetNetID() == netID)
			{
				continue;
			}
			ShieldGeometry component3 = item.collider.GetComponent<ShieldGeometry>();
			if (component3 != null)
			{
				if (component3.GetUnit().GetNetID() != netID)
				{
					bool flag = component3.GetUnit().GetOwner() == NetObj.m_localPlayerID;
					if (flag)
					{
						hitLocalPlayer = true;
					}
					hitPoint = item.point;
					hitPoint.y = muzzlePos.y;
					component3.Damage(new Hit(this, num, m_Damage.m_armorPiercing, item.point, direction), showDmgText: true);
					DoHitEffect(item.point, flag);
					DoMultiHitAchievement(num2);
					return;
				}
			}
			else
			{
				DoRayDamage(num, item.collider.collider, item.point, direction, ref hitLocalPlayer);
				if ((bool)component && TurnMan.instance.IsHostile(component.GetOwner(), GetOwner()))
				{
					num2++;
				}
				num /= 2;
			}
		}
		DoMultiHitAchievement(num2);
		hitPoint = vector + direction * maxDistance;
		DoEndOfRayEffect(hitPoint, hitLocalPlayer);
	}

	private bool DoRayDamage(int damage, Collider collider, Vector3 hitPos, Vector3 dir, ref bool hitLocalPlayer)
	{
		Section component = collider.GetComponent<Section>();
		if (component == null && (bool)collider.transform.parent)
		{
			component = collider.transform.parent.GetComponent<Section>();
		}
		if (component != null)
		{
			bool flag = component.GetOwner() == NetObj.m_localPlayerID;
			if (flag)
			{
				hitLocalPlayer = true;
			}
			DoHitEffect(hitPos, flag);
			component.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
			return true;
		}
		HPModule component2 = collider.GetComponent<HPModule>();
		if (component2 != null)
		{
			bool flag2 = component2.GetOwner() == NetObj.m_localPlayerID;
			if (flag2)
			{
				hitLocalPlayer = true;
			}
			DoHitEffect(hitPos, flag2);
			component2.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir), showDmgText: true);
			return true;
		}
		Platform platform = GetPlatform(collider.gameObject);
		if (platform != null)
		{
			bool flag3 = platform.GetOwner() == NetObj.m_localPlayerID;
			if (flag3)
			{
				hitLocalPlayer = true;
			}
			DoHitEffect(hitPos, flag3);
			platform.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
			return true;
		}
		Mine component3 = collider.GetComponent<Mine>();
		if (component3 != null)
		{
			bool flag4 = component3.GetOwner() == NetObj.m_localPlayerID;
			if (flag4)
			{
				hitLocalPlayer = true;
			}
			DoHitEffect(hitPos, flag4);
			component3.Damage(new Hit(this, damage, m_Damage.m_armorPiercing, hitPos, dir));
			return true;
		}
		return false;
	}

	private void DoHitEffect(Vector3 pos, bool hitLocalPlayer)
	{
		if ((IsVisible() || hitLocalPlayer) && m_hitEffectHiPrefab != null)
		{
			Object.Instantiate(m_hitEffectHiPrefab, pos, Quaternion.identity);
		}
	}

	private void DoEndOfRayEffect(Vector3 pos, bool hitLocalPlayer)
	{
		if ((IsVisible() || hitLocalPlayer) && m_rayEndEffectHiPrefab != null)
		{
			Object.Instantiate(m_rayEndEffectHiPrefab, pos, Quaternion.identity);
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

	public override void DrawOrders()
	{
		base.DrawOrders();
		if (m_lineDrawer == null)
		{
			return;
		}
		if (m_rayLineType == -1)
		{
			m_rayLineType = m_lineDrawer.GetTypeID("railAim");
		}
		Vector3 position = base.transform.position;
		position.y = m_testOffset;
		foreach (Order order in m_orders)
		{
			if (order.m_type == Order.Type.Fire)
			{
				Vector3 vector = order.GetPos() - position;
				vector.y = 0f;
				vector.Normalize();
				m_lineDrawer.DrawLine(position, position + vector * m_aim.m_maxRange, m_rayLineType, 2.5f);
			}
		}
	}
}
