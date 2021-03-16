using UnityEngine;

[AddComponentMenu("Scripts/Projectiles/Projectile_Deploy")]
public class Projectile_Deploy : Projectile
{
	public GameObject m_objectPrefab;

	protected override void OnHit(Vector3 pos, Collider other)
	{
		if (m_hasHit)
		{
			return;
		}
		Section component = other.gameObject.GetComponent<Section>();
		HPModule component2 = other.gameObject.GetComponent<HPModule>();
		ShieldGeometry component3 = other.gameObject.GetComponent<ShieldGeometry>();
		if (!component && other.gameObject.transform.parent != null)
		{
			component = other.gameObject.transform.parent.GetComponent<Section>();
		}
		Unit ownerUnit = GetOwnerUnit();
		if (component != null)
		{
			if (ownerUnit != null && component.GetUnit().GetNetID() != ownerUnit.GetNetID())
			{
				HitEffect(pos, HitType.Armor);
				Remove();
			}
			return;
		}
		if (component2 != null)
		{
			if (ownerUnit != null && component2.GetUnit().GetNetID() != ownerUnit.GetNetID())
			{
				HitEffect(pos, HitType.Armor);
				Remove();
			}
			return;
		}
		if (component3 != null)
		{
			if (component3.GetUnit().GetNetID() != ownerUnit.GetNetID())
			{
				HitEffect(pos, HitType.Armor);
				Remove();
			}
			return;
		}
		if (other.tag == "Water")
		{
			HitEffect(pos, HitType.Water);
			Deploy(pos);
		}
		else
		{
			HitEffect(pos, HitType.Ground);
		}
		Remove();
	}

	private void Deploy(Vector3 pos)
	{
		if (m_objectPrefab != null)
		{
			GameObject gameObject = ObjectFactory.Clone(m_objectPrefab, pos, Quaternion.Euler(new Vector3(0f, PRand.Range(0, 360), 0f)));
			Deployable component = gameObject.GetComponent<Deployable>();
			if (component != null)
			{
				component.Setup(GetOwner(), m_gunID, IsVisible(), GetSeenByMask(), m_damage, m_armorPiercing, m_splashRadius, m_splashDamageFactor);
			}
			Mine component2 = gameObject.GetComponent<Mine>();
			if (component2 != null)
			{
				component2.Setup(GetOwner(), m_gunID, IsVisible(), GetSeenByMask(), m_damage, m_armorPiercing, m_splashRadius, m_splashDamageFactor);
			}
		}
	}
}
