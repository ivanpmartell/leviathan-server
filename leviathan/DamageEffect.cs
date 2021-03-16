using System.IO;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
	public float m_radius = 5f;

	public float m_delay = 0.1f;

	public int m_maxHealth = 30;

	public float m_shipHealthActivationPercentage = -1f;

	public bool m_hatesWater;

	public GameObject m_startEffect;

	public GameObject m_persistentEffect;

	public GameObject m_startEffectLow;

	public GameObject m_persistentEffectLow;

	private bool m_simulating;

	private bool m_active;

	private bool m_visible = true;

	private bool m_sinking;

	private int m_health = 30;

	private float m_activateTimer = -1f;

	private GameObject m_startEffectInstance;

	private GameObject m_persistentEffectInstance;

	private void Awake()
	{
		m_health = m_maxHealth;
	}

	public void Activate(bool firstTime)
	{
		if (!m_active)
		{
			m_active = true;
			if (firstTime)
			{
				m_activateTimer = m_delay;
			}
			else
			{
				InternalActivate(firstTime: false);
			}
		}
	}

	public void Deactivate()
	{
		if (m_active)
		{
			m_active = false;
			if (m_startEffectInstance != null)
			{
				Object.Destroy(m_startEffectInstance);
				m_startEffectInstance = null;
			}
			if (m_persistentEffectInstance != null)
			{
				Object.Destroy(m_persistentEffectInstance);
				m_persistentEffectInstance = null;
			}
		}
	}

	public void OnShipHealthChange(float healthPercentage)
	{
		if (!(m_shipHealthActivationPercentage >= 0f))
		{
			return;
		}
		if (m_active)
		{
			if (healthPercentage > m_shipHealthActivationPercentage)
			{
				Deactivate();
			}
		}
		else if (healthPercentage <= m_shipHealthActivationPercentage)
		{
			Activate(firstTime: true);
		}
	}

	public int Damage(int dmg)
	{
		if (m_shipHealthActivationPercentage >= 0f)
		{
			return dmg;
		}
		int num = dmg;
		if (num > m_health)
		{
			num = m_health;
		}
		m_health -= num;
		if (m_health <= 0)
		{
			Activate(firstTime: true);
		}
		return num;
	}

	public bool Heal(int health)
	{
		if (m_health >= m_maxHealth)
		{
			return true;
		}
		m_health += health;
		if (m_health > m_maxHealth)
		{
			m_health = m_maxHealth;
		}
		if (m_health == m_maxHealth)
		{
			Deactivate();
		}
		return false;
	}

	public bool IsActive()
	{
		return m_active;
	}

	public void Update()
	{
		if (m_activateTimer >= 0f)
		{
			m_activateTimer -= Time.deltaTime;
			if (m_activateTimer < 0f)
			{
				InternalActivate(firstTime: true);
			}
		}
	}

	public void OnSinkingUpdate()
	{
		if (m_hatesWater && m_active && base.transform.position.y < -1f)
		{
			Deactivate();
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, m_radius);
	}

	public void SaveState(BinaryWriter writer)
	{
		writer.Write(m_health);
		writer.Write(m_active);
	}

	public void LoadState(BinaryReader reader)
	{
		m_health = reader.ReadInt32();
		if (reader.ReadBoolean())
		{
			Activate(firstTime: false);
		}
	}

	public void SetVisible(bool visible)
	{
		if (m_visible != visible)
		{
			m_visible = visible;
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = visible;
			}
		}
	}

	private void InternalActivate(bool firstTime)
	{
		GameObject startEffect = m_startEffect;
		GameObject persistentEffect = m_persistentEffect;
		if (firstTime && startEffect != null)
		{
			m_startEffectInstance = Object.Instantiate(startEffect, base.transform.position, base.transform.rotation) as GameObject;
			m_startEffectInstance.transform.parent = base.gameObject.transform;
			CamShaker component = base.gameObject.GetComponent<CamShaker>();
			if (component != null)
			{
				component.Trigger();
			}
		}
		if (persistentEffect != null)
		{
			m_persistentEffectInstance = Object.Instantiate(persistentEffect, base.transform.position, base.transform.rotation) as GameObject;
			m_persistentEffectInstance.transform.parent = base.gameObject.transform;
			if (!firstTime)
			{
				ParticleSystem[] componentsInChildren = m_persistentEffectInstance.GetComponentsInChildren<ParticleSystem>();
				ParticleSystem[] array = componentsInChildren;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Simulate(10f);
				}
			}
		}
		ParticleSystem[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array2 = componentsInChildren2;
		foreach (ParticleSystem particleSystem2 in array2)
		{
			if (m_simulating)
			{
				particleSystem2.Play();
			}
			else
			{
				particleSystem2.Pause();
			}
		}
		Renderer[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array3 = componentsInChildren3;
		foreach (Renderer renderer in array3)
		{
			renderer.enabled = m_visible;
		}
	}

	public void SetSimulating(bool enabled)
	{
		m_simulating = enabled;
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (m_simulating)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Pause();
			}
		}
	}
}
