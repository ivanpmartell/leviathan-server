using UnityEngine;

public class ShieldGeometry : MonoBehaviour
{
	public GameObject m_hitEffectLow;

	public GameObject m_hitEffect;

	public GameObject m_hitEffectBigLow;

	public GameObject m_hitEffectBig;

	public GameObject m_activateSound;

	public GameObject m_deactivateSound;

	public GameObject m_destroyedSound;

	public GameObject m_visual;

	private Unit m_unit;

	private Shield m_ownerShield;

	private float m_hitAlpha;

	private float m_fadeAlpha;

	private float m_hitTimer = -1f;

	private float m_hitDuration = 0.5f;

	private float m_activateTimer = -1f;

	private float m_deactivateTimer = -1f;

	private float m_activateDuration = 1f;

	public void Setup(Unit unit, Shield ownerShild, bool firstTime)
	{
		m_unit = unit;
		m_ownerShield = ownerShild;
		if (firstTime)
		{
			m_activateTimer = 0f;
			m_fadeAlpha = 0f;
			UpdateAlpha();
			SetScale(0f);
			if (ownerShild.IsVisible())
			{
				m_activateSound.audio.Play();
			}
		}
		else
		{
			m_fadeAlpha = 1f;
			UpdateAlpha();
			SetScale(1f);
		}
		SetVisible(m_ownerShield.IsVisible());
	}

	public void Deactivate(bool destroyed)
	{
		base.collider.enabled = false;
		m_deactivateTimer = 0f;
		if (m_ownerShield.IsVisible())
		{
			if (destroyed)
			{
				m_destroyedSound.audio.Play();
			}
			else
			{
				m_deactivateSound.audio.Play();
			}
		}
	}

	public Unit GetUnit()
	{
		return m_unit;
	}

	private void UpdateAlpha()
	{
		float value = m_fadeAlpha * 0.7f + m_hitAlpha * 0.5f;
		m_visual.renderer.material.SetFloat("_Opacity", value);
	}

	private void SetScale(float i)
	{
		m_visual.transform.localScale = new Vector3(i, i, i);
	}

	protected void Update()
	{
		if (m_activateTimer >= 0f)
		{
			m_activateTimer += Time.deltaTime;
			float num = m_activateTimer / m_activateDuration;
			if (num >= 1f)
			{
				m_activateTimer = -1f;
				num = 1f;
			}
			m_fadeAlpha = num;
			SetScale(num);
			UpdateAlpha();
		}
		else if (m_deactivateTimer >= 0f)
		{
			m_deactivateTimer += Time.deltaTime;
			float num2 = m_deactivateTimer / m_activateDuration;
			if (num2 >= 1f)
			{
				m_deactivateTimer = -1f;
				num2 = 1f;
				Object.Destroy(base.gameObject);
			}
			m_fadeAlpha = 1f - num2;
			SetScale(1f - num2);
			UpdateAlpha();
		}
		if (m_hitTimer >= 0f)
		{
			m_hitTimer += Time.deltaTime;
			float num3 = m_hitTimer / m_hitDuration;
			if (num3 >= 1f)
			{
				m_hitAlpha = 0f;
				m_hitTimer = -1f;
			}
			else
			{
				m_hitAlpha = 1f - num3;
			}
			UpdateAlpha();
		}
	}

	public void Damage(Hit hit, bool showDmgText)
	{
		TurnMan.instance.AddShieldAbsorb(m_ownerShield.GetOwner(), hit.m_damage);
		float num = hit.m_damage;
		m_ownerShield.Drain(num, damaged: true);
		m_hitTimer = 0f;
		if (m_ownerShield.IsVisible() && hit.m_havePoint)
		{
			GameObject gameObject = null;
			gameObject = ((!(num >= m_ownerShield.GetMaxEnergy() / 6f)) ? m_hitEffect : m_hitEffectBig);
			if (gameObject != null)
			{
				Object.Instantiate(gameObject, hit.m_point, Quaternion.LookRotation(-hit.m_dir));
			}
		}
		if (m_ownerShield.IsVisible() && showDmgText)
		{
			Vector3 pos = ((!hit.m_havePoint) ? m_ownerShield.transform.position : hit.m_point);
			HitText.instance.AddDmgText(m_ownerShield.GetNetID(), pos, string.Empty, Constants.m_shieldAbsorbedText);
		}
	}

	public void SetVisible(bool visible)
	{
		Renderer[] componentsInChildren = m_visual.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = visible;
		}
	}
}
