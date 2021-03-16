using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Weapons/Monster Mine")]
public class AoeMonster : Unit
{
	public float m_ttl;

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

	protected virtual void OnDeploy()
	{
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_deployed);
		writer.Write(m_armTimer);
		writer.Write(m_gunID);
		writer.Write(m_ttl);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_deployed = reader.ReadBoolean();
		m_armTimer = reader.ReadSingle();
		m_gunID = reader.ReadInt32();
		m_ttl = reader.ReadSingle();
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

	public float GetVisibleDistance()
	{
		return 1000f;
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
		return true;
	}

	public void Disarm()
	{
		Object.Destroy(base.gameObject);
	}
}
