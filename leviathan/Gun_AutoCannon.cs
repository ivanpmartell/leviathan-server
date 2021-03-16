using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Gun_AutoCannon")]
public class Gun_AutoCannon : Gun
{
	protected override bool FireProjectile(Vector3 targetPos)
	{
		int num = SetUpProjectiles(GetMuzzlePos(), targetPos, 9999);
		if (num > 0)
		{
			return true;
		}
		return false;
	}

	protected int SetUpProjectiles(Vector3 muzzlePos, Vector3 targetPos, int max)
	{
		float num = Vector3.Distance(muzzlePos, targetPos);
		float range = ((!m_aim.m_spreadIgnoresRange) ? m_aim.m_maxRange : num);
		Quaternion randomSpreadDirection = GetRandomSpreadDirection(0f, range);
		if (FindOptimalFireDir(muzzlePos, targetPos, out var dir))
		{
			Vector3 dir2 = dir * (randomSpreadDirection * Vector3.forward);
			GameObject gameObject = ObjectFactory.Clone(m_projectile, muzzlePos, m_elevationJoint[0].rotation);
			Projectile component = gameObject.GetComponent<Projectile>();
			component.Setup(dir2, GetOwner(), m_muzzleVel, m_gravity, this, GetRandomDamage(), m_Damage.m_armorPiercing, m_Damage.m_splashRadius, m_Damage.m_splashDamageFactor, m_aim.m_minRange, m_aim.m_maxRange);
			return 1;
		}
		return 0;
	}

	public override Dictionary<string, string> GetShipEditorInfo()
	{
		return base.GetShipEditorInfo();
	}
}
