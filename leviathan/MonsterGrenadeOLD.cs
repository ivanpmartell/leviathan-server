using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MonsterGrenadeOLD : Deployable
{
	public float m_ttl = 30f;

	public float m_fadeoutTime = 8f;

	public float m_effectRadius = 20f;

	public float m_spawnDelayMin = 0.9f;

	public float m_spawnDelayMax = 1.2f;

	public GameObject m_effectLow;

	public GameObject m_effectHigh;

	public GameObject m_bodyPrefab;

	public GameObject m_headPrefab;

	public GameObject m_tailPrefab;

	private List<Animation> m_bodyParts = new List<Animation>();

	private GameObject m_effect;

	private ParticleSystem m_psystem;

	private float m_spawnTimer;

	public override void Awake()
	{
		base.Awake();
		m_effect = Object.Instantiate(m_effectHigh, base.transform.position, Quaternion.identity) as GameObject;
		m_effect.transform.parent = base.transform;
		m_psystem = m_effect.GetComponentInChildren<ParticleSystem>();
		Pause(NetObj.m_simulating);
	}

	private void Pause(bool simulating)
	{
		Animation[] componentsInChildren = base.transform.GetComponentsInChildren<Animation>();
		if (simulating)
		{
			m_psystem.Play();
			Animation[] array = componentsInChildren;
			foreach (Animation animation in array)
			{
				if ((bool)animation["rolling"])
				{
					animation["rolling"].speed = 1f;
				}
			}
		}
		else
		{
			m_psystem.Pause();
			Animation[] array2 = componentsInChildren;
			foreach (Animation animation2 in array2)
			{
				if ((bool)animation2["rolling"])
				{
					animation2["rolling"].speed = 0f;
				}
			}
		}
		if (m_ttl < m_fadeoutTime)
		{
			m_psystem.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void FixedUpdate()
	{
		if (!NetObj.m_simulating)
		{
			return;
		}
		m_spawnTimer -= Time.fixedDeltaTime;
		m_ttl -= Time.fixedDeltaTime;
		if (m_spawnTimer < 0f)
		{
			m_spawnTimer = Random.Range(m_spawnDelayMin, m_spawnDelayMax);
			SpawnBodyPart();
		}
		if (m_ttl < m_fadeoutTime)
		{
			ParticleSystem[] componentsInChildren = base.transform.GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.enableEmission = false;
			}
			foreach (Animation bodyPart in m_bodyParts)
			{
				bodyPart.transform.position -= new Vector3(0f, Time.fixedDeltaTime, 0f);
			}
		}
		RemoveOldBodyParts();
		if (m_ttl <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void RemoveOldBodyParts()
	{
		for (int i = 0; i < m_bodyParts.Count; i++)
		{
			if (!m_bodyParts[i].isPlaying)
			{
				PLog.Log("Removing old body part " + i);
				Object.Destroy(m_bodyParts[i].gameObject);
				m_bodyParts.RemoveAt(i);
				break;
			}
		}
	}

	private void SpawnBodyPart()
	{
		switch (Random.Range(0, 3))
		{
		case 0:
			SpawnHead();
			break;
		case 1:
			SpawnBody();
			break;
		case 2:
			SpawnTail();
			break;
		}
	}

	private void SpawnHead()
	{
		Vector3 vector = new Vector3(Random.Range(0f - m_effectRadius, m_effectRadius), -9f, Random.Range(0f - m_effectRadius, m_effectRadius));
		Quaternion rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
		GameObject gameObject = Object.Instantiate(m_headPrefab, base.transform.position + vector, rotation) as GameObject;
		gameObject.transform.parent = base.transform;
		m_bodyParts.Add(gameObject.animation);
	}

	private void SpawnBody()
	{
		Vector3 vector = new Vector3(Random.Range(0f - m_effectRadius, m_effectRadius), -13f, Random.Range(0f - m_effectRadius, m_effectRadius));
		Quaternion rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
		GameObject gameObject = Object.Instantiate(m_bodyPrefab, base.transform.position + vector, rotation) as GameObject;
		gameObject.transform.parent = base.transform;
		m_bodyParts.Add(gameObject.animation);
	}

	private void SpawnTail()
	{
		Vector3 vector = new Vector3(Random.Range(0f - m_effectRadius, m_effectRadius), -13f, Random.Range(0f - m_effectRadius, m_effectRadius));
		Quaternion rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
		GameObject gameObject = Object.Instantiate(m_tailPrefab, base.transform.position + vector, rotation) as GameObject;
		gameObject.transform.parent = base.transform;
		m_bodyParts.Add(gameObject.animation);
	}

	private void OnTriggerStay(Collider other)
	{
		Section component = other.GetComponent<Section>();
		if ((bool)component)
		{
			Ship ship = component.GetUnit() as Ship;
			ship.SetInMonsterMineField(base.gameObject);
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
		Pause(NetObj.m_simulating);
	}
}
