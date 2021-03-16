using System.IO;
using UnityEngine;

public class SmokeGrenade : Deployable
{
	public float m_ttl = 60f;

	public float m_fadeoutTime = 8f;

	public GameObject m_effectLow;

	public GameObject m_effectHigh;

	private GameObject m_effect;

	private ParticleSystem m_psystem;

	public override void Awake()
	{
		base.Awake();
		m_effect = Object.Instantiate(m_effectHigh, base.transform.position, Quaternion.identity) as GameObject;
		m_effect.transform.parent = base.transform;
		m_psystem = m_effect.GetComponentInChildren<ParticleSystem>();
		if (NetObj.m_simulating)
		{
			m_psystem.Play();
		}
		else
		{
			m_psystem.Pause();
		}
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating)
		{
			m_ttl -= Time.fixedDeltaTime;
			if (m_ttl < m_fadeoutTime)
			{
				m_psystem.enableEmission = false;
			}
			if (m_ttl <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_ttl);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_ttl = reader.ReadSingle();
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
		for (int i = 0; i < 10; i++)
		{
			m_psystem.Simulate(1f);
		}
		m_psystem.Play();
	}

	protected override void OnSetSimulating(bool enabled)
	{
		base.OnSetSimulating(enabled);
		if (enabled)
		{
			m_psystem.Play();
		}
		else
		{
			m_psystem.Pause();
		}
	}
}
