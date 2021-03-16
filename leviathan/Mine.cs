using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Weapons/Mine")]
public class Mine : Unit
{
	public float m_ttl;

	public int m_health = 20;

	public int m_armorClass = 8;

	public GameObject m_hitEffect;

	public GameObject m_disarmEffect;

	public GameObject m_disarmEffectLow;

	public float m_armDelay = 20f;

	protected bool m_deployed;

	protected int m_gunID;

	private float m_armTimer;

	private WaterSurface m_waterSurface;

	public virtual void Setup(int ownerID, int gunID, bool visible, int seenByMask, int damage, int ap, float splashRadius, float splashDmgFactor)
	{
		m_gunID = gunID;
		SetOwner(ownerID);
		SetVisible(visible);
		SetSeenByMask(seenByMask);
	}

	public override void Awake()
	{
		base.Awake();
		GameObject gameObject = GameObject.Find("WaterSurface");
		if (gameObject != null)
		{
			m_waterSurface = gameObject.GetComponent<WaterSurface>();
		}
		m_armTimer = m_armDelay;
	}

	protected override void FixedUpdate()
	{
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (m_ttl != 0f)
		{
			m_ttl -= Time.fixedDeltaTime;
			if (m_ttl <= 0f)
			{
				OnTimeout();
				Object.Destroy(base.gameObject);
				return;
			}
		}
		if (m_waterSurface != null)
		{
			Vector3 position = base.transform.position;
			position.y = m_waterSurface.GetWorldWaveHeight(position);
			base.transform.position = position;
		}
		if (m_armTimer > 0f)
		{
			m_armTimer -= Time.fixedDeltaTime;
		}
		if (!m_deployed && m_armTimer <= 0f)
		{
			m_deployed = true;
			OnDeploy();
		}
	}

	protected virtual void OnTimeout()
	{
	}

	protected virtual void OnDeploy()
	{
	}

	public override bool IsValidTarget()
	{
		return false;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_deployed);
		writer.Write(m_armTimer);
		writer.Write(m_gunID);
		writer.Write(m_ttl);
		writer.Write(m_health);
		writer.Write(m_armorClass);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_deployed = reader.ReadBoolean();
		m_armTimer = reader.ReadSingle();
		m_gunID = reader.ReadInt32();
		m_ttl = reader.ReadSingle();
		m_health = reader.ReadInt32();
		m_armorClass = reader.ReadInt32();
		if (!m_deployed)
		{
		}
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
		}
	}

	public float GetArmTimer()
	{
		return m_armTimer;
	}

	public bool IsDeployed()
	{
		return m_deployed;
	}

	public override float GetWidth()
	{
		return 2f;
	}

	public override float GetLength()
	{
		return 2f;
	}

	public override bool Damage(Hit hit)
	{
		if (m_health <= 0)
		{
			return true;
		}
		int healthDamage;
		GameRules.HitOutcome hitOutcome = GameRules.CalculateDamage(m_health, m_armorClass, hit.m_damage, hit.m_armorPiercing, out healthDamage);
		m_health -= healthDamage;
		if (m_health <= 0)
		{
			Explode();
		}
		if (IsVisible())
		{
			switch (hitOutcome)
			{
			case GameRules.HitOutcome.CritHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipCriticalHit);
				break;
			case GameRules.HitOutcome.PiercedArmor:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipPiercingHit);
				break;
			case GameRules.HitOutcome.GlancingHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipGlancingHit);
				break;
			case GameRules.HitOutcome.Deflected:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipDeflectHit);
				break;
			}
			if (m_health <= 0)
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipDestroyedHit);
			}
		}
		return true;
	}

	public void Disarm()
	{
		Object.Destroy(base.gameObject);
		HitText.instance.AddDmgText(-1, base.gameObject.transform.position, "Mine Dissarmed", Constants.m_shipCriticalHit);
		if (m_disarmEffect != null)
		{
			Object.Instantiate(m_disarmEffect, base.transform.position, Quaternion.identity);
		}
	}

	protected void Explode()
	{
		if (m_hitEffect != null && IsVisible())
		{
			Object.Instantiate(m_hitEffect, base.transform.position, Quaternion.identity);
		}
		Object.Destroy(base.gameObject);
	}
}
