#define DEBUG
using System.IO;
using UnityEngine;

internal class DamageMan
{
	private Transform m_damageEffectRoot;

	private DamageEffect[] m_damageEffects = new DamageEffect[0];

	public DamageMan(GameObject ship)
	{
		m_damageEffectRoot = ship.transform.Find("DamageEffects");
		if (m_damageEffectRoot != null && m_damageEffectRoot.childCount == 0)
		{
			m_damageEffectRoot = null;
		}
		if ((bool)m_damageEffectRoot)
		{
			m_damageEffects = m_damageEffectRoot.GetComponentsInChildren<DamageEffect>(includeInactive: true);
		}
	}

	public void SetVisible(bool visible)
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			damageEffect.SetVisible(visible);
		}
	}

	public void SetSimulating(bool enabled)
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			damageEffect.SetSimulating(enabled);
		}
	}

	public void OnSinkingUpdate()
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			damageEffect.OnSinkingUpdate();
		}
	}

	public void SaveDamageEffects(BinaryWriter writer)
	{
		writer.Write((byte)m_damageEffects.Length);
		for (int i = 0; i < m_damageEffects.Length; i++)
		{
			DamageEffect damageEffect = m_damageEffects[i];
			if (damageEffect != null)
			{
				damageEffect.SaveState(writer);
			}
		}
	}

	public void LoadDamageEffects(BinaryReader reader)
	{
		int num = reader.ReadByte();
		DebugUtils.Assert(num == m_damageEffects.Length);
		for (int i = 0; i < num; i++)
		{
			DamageEffect damageEffect = m_damageEffects[i];
			if (damageEffect != null)
			{
				damageEffect.LoadState(reader);
			}
		}
	}

	public void HealDamageEffects(int health)
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			if (!damageEffect.Heal(health))
			{
				break;
			}
		}
	}

	public void OnShipHealthChanged(float healthPercentage)
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			damageEffect.OnShipHealthChange(healthPercentage);
		}
	}

	public void EnableCloseDamageEffect(Vector3 point, int damage)
	{
		DamageEffect[] damageEffects = m_damageEffects;
		foreach (DamageEffect damageEffect in damageEffects)
		{
			if (damageEffect.IsActive())
			{
				continue;
			}
			float num = Vector3.Distance(damageEffect.transform.position, point);
			if (num < damageEffect.m_radius)
			{
				damage = damageEffect.Damage(damage);
				if (damage <= 0)
				{
					break;
				}
			}
		}
	}
}
