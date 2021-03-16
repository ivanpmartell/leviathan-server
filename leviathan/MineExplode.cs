using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Weapons/MineExplode")]
public class MineExplode : Mine
{
	public int m_damage = 500;

	public int m_armorPiercing = 25;

	public float m_splashRadius = 10f;

	public float m_splashDamageFactor = 0.5f;

	public float m_visibleDistance = 10f;

	public GameObject m_radiusMesh;

	public override void Setup(int ownerID, int unitID, bool visible, int seenByMask, int damage, int ap, float splashRadius, float splashDmgFactor)
	{
		base.Setup(ownerID, unitID, visible, seenByMask, damage, ap, splashRadius, splashDmgFactor);
		m_damage = damage;
		m_armorPiercing = ap;
		m_splashRadius = splashRadius;
		m_splashDamageFactor = splashDmgFactor;
		m_radiusMesh.renderer.enabled = false;
	}

	public override void Awake()
	{
		base.Awake();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void Update()
	{
		base.Update();
		m_radiusMesh.transform.Rotate(0f, Time.deltaTime * 50f, 0f);
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write((short)m_damage);
		writer.Write((short)m_armorPiercing);
		writer.Write(m_splashRadius);
		writer.Write(m_splashDamageFactor);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_damage = reader.ReadInt16();
		m_armorPiercing = reader.ReadInt16();
		m_splashRadius = reader.ReadSingle();
		m_splashDamageFactor = reader.ReadSingle();
	}

	protected override void OnDeploy()
	{
		base.OnDeploy();
		if (IsVisible())
		{
			m_radiusMesh.renderer.enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!NetObj.m_simulating)
		{
			return;
		}
		Section component = other.GetComponent<Section>();
		if (!component && other.gameObject.transform.parent != null)
		{
			component = other.gameObject.transform.parent.GetComponent<Section>();
		}
		if (component != null && (NetObj.m_phase != 0 || IsSeenByPlayer(NetObj.m_localPlayerID)) && GetArmTimer() <= 0f)
		{
			Gun dealer = NetObj.GetByID(m_gunID) as Gun;
			int maxSplashDamage = (int)((float)m_damage * m_splashDamageFactor);
			component.Damage(new Hit(dealer, m_damage, m_armorPiercing, base.transform.position, (other.transform.position - base.transform.position).normalized));
			GameRules.DoAreaDamage(dealer, base.transform.position, m_splashRadius, maxSplashDamage, m_armorPiercing, other);
			if (m_hitEffect != null && IsVisible())
			{
				Object.Instantiate(m_hitEffect, base.transform.position, Quaternion.identity);
			}
			Object.Destroy(base.gameObject);
		}
	}

	public float GetVisibleDistance()
	{
		return m_visibleDistance;
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		if (visible)
		{
			m_radiusMesh.renderer.enabled = m_deployed;
		}
		else
		{
			m_radiusMesh.renderer.enabled = false;
		}
	}
}
