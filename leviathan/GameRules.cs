using System.Collections.Generic;
using UnityEngine;

internal class GameRules
{
	public enum HitOutcome
	{
		CritHit,
		PiercedArmor,
		GlancingHit,
		Deflected
	}

	public static HitOutcome CalculateDamage(int health, int armorClass, int damage, int armorPiercing, out int healthDamage)
	{
		if (damage <= 0)
		{
			healthDamage = 0;
			return HitOutcome.Deflected;
		}
		if (armorClass < 1)
		{
			armorClass = 1;
		}
		if (armorPiercing >= armorClass)
		{
			int num = armorPiercing - armorClass;
			float num2 = num / armorClass;
			bool flag = PRand.Value() <= num2;
			if (flag)
			{
				healthDamage = damage * 2;
			}
			else
			{
				healthDamage = damage;
			}
			if (healthDamage > health)
			{
				healthDamage = health;
			}
			return (!flag) ? HitOutcome.PiercedArmor : HitOutcome.CritHit;
		}
		float num3 = Mathf.Clamp((float)armorPiercing / (float)armorClass, 0f, 1f);
		if (PRand.Value() <= num3)
		{
			float num4 = (float)armorPiercing / (float)armorClass * (float)damage;
			int num5 = (healthDamage = PRand.Range(1, (int)num4));
			if (healthDamage > health)
			{
				healthDamage = health;
			}
			return HitOutcome.GlancingHit;
		}
		healthDamage = 0;
		return HitOutcome.Deflected;
	}

	public static void DoAreaDamage(Gun dealer, Vector3 center, float radius, int maxSplashDamage, int armorPiercing, Collider hitCollider)
	{
		int layerMask = (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("hpmodules"));
		Collider[] array = Physics.OverlapSphere(center, radius, layerMask);
		HashSet<Unit> hashSet = new HashSet<Unit>();
		if (hitCollider != null)
		{
			Section section = Projectile.GetSection(hitCollider.gameObject);
			if (section != null)
			{
				hashSet.Add(section.GetUnit());
			}
			Platform platform = Projectile.GetPlatform(hitCollider.gameObject);
			if (platform != null)
			{
				hashSet.Add(platform);
			}
		}
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Vector3 a = collider.ClosestPointOnBounds(center);
			float num = Vector3.Distance(a, center);
			float num2 = 1f - Mathf.Clamp01(num / radius);
			int num3 = (int)(num2 * (float)maxSplashDamage);
			if (num3 <= 0)
			{
				continue;
			}
			Section section2 = Projectile.GetSection(collider.gameObject);
			if (section2 != null)
			{
				if (!hashSet.Contains(section2.GetUnit()))
				{
					hashSet.Add(section2.GetUnit());
					section2.Damage(new Hit(dealer, num3, armorPiercing));
				}
				continue;
			}
			Platform platform2 = Projectile.GetPlatform(collider.gameObject);
			if (platform2 != null)
			{
				if (!hashSet.Contains(platform2))
				{
					hashSet.Add(platform2);
					platform2.Damage(new Hit(dealer, num3, armorPiercing));
				}
			}
			else
			{
				HPModule component = collider.gameObject.GetComponent<HPModule>();
				if (component != null)
				{
					component.Damage(new Hit(dealer, num3, armorPiercing), showDmgText: true);
				}
			}
		}
	}
}
