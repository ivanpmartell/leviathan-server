using UnityEngine;

public class Hit
{
	public NetObj m_dealer;

	public int m_damage;

	public int m_armorPiercing;

	public Vector3 m_point = Vector3.zero;

	public Vector3 m_dir = Vector3.zero;

	public bool m_havePoint;

	public bool m_collision;

	public Hit(NetObj dealer, int damage, int armorPiercing, Vector3 point, Vector3 dir)
	{
		m_dealer = dealer;
		m_damage = damage;
		m_armorPiercing = armorPiercing;
		m_point = point;
		m_dir = dir;
		m_havePoint = true;
	}

	public Hit(NetObj dealer, int damage, int ap)
	{
		m_dealer = dealer;
		m_damage = damage;
		m_armorPiercing = ap;
	}

	public Hit(int damage, int ap, Vector3 point, Vector3 dir)
	{
		m_damage = damage;
		m_armorPiercing = ap;
		m_point = point;
		m_dir = dir;
		m_havePoint = true;
	}

	public Hit(int damage, int ap)
	{
		m_damage = damage;
		m_armorPiercing = ap;
	}

	public Gun GetGun()
	{
		return m_dealer as Gun;
	}

	public Unit GetUnit()
	{
		Gun gun = m_dealer as Gun;
		if (gun != null)
		{
			return gun.GetUnit();
		}
		return m_dealer as Unit;
	}
}
