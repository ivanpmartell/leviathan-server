using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MonsterGrenade : Deployable
{
	public float m_lifeTime = 30f;

	public float m_fadeoutTime = 8f;

	public float m_fadeinTime = 1f;

	public float m_effectRadius = 20f;

	public float m_spawnDelayMin = 0.9f;

	public float m_spawnDelayMax = 1.2f;

	public GameObject m_effectLow;

	public GameObject m_effectHigh;

	public GameObject m_spawnEffectHigh;

	public GameObject m_spawnEffectLow;

	public GameObject m_bodyPrefab;

	public GameObject m_headPrefab;

	public GameObject m_tailPrefab;

	private List<Animation> m_bodyParts = new List<Animation>();

	private Material m_whirlMaterial;

	private Animation m_head;

	private bool m_headActive;

	private float m_headUpdate;

	private float m_ttl = -1f;

	private GameObject m_effect;

	private ParticleSystem m_psystem;

	private float m_spawnTimer;

	private int m_rayMask;

	public override void Awake()
	{
		base.Awake();
		if (m_ttl == -1f)
		{
			m_ttl = m_lifeTime;
		}
		m_effect = Object.Instantiate(m_effectHigh, base.transform.position, Quaternion.identity) as GameObject;
		m_effect.transform.parent = base.transform;
		m_psystem = m_effect.GetComponentInChildren<ParticleSystem>();
		m_rayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("units"));
		m_whirlMaterial = m_effect.transform.FindChild("whirl").renderer.material;
		UpdateWhirl();
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
		if ((bool)m_head)
		{
			if (!NetObj.m_simulating)
			{
				m_head["leviathan_idle"].speed = 0f;
			}
			else
			{
				m_head["leviathan_idle"].speed = 1f;
			}
		}
		if (m_ttl < m_fadeoutTime)
		{
			m_psystem.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating)
		{
			m_spawnTimer -= Time.fixedDeltaTime;
			m_ttl -= Time.fixedDeltaTime;
			UpdateHead(Time.fixedDeltaTime);
			UpdateArms(Time.fixedDeltaTime);
			UpdateWhirl();
			if (m_ttl < m_fadeoutTime)
			{
				float num = 1f - m_ttl / m_fadeoutTime;
				Vector3 position = base.transform.position;
				position.y = -20f * num;
				base.transform.position = position;
			}
			if (m_ttl <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void UpdateWhirl()
	{
		float value = 1f;
		if (m_ttl <= m_fadeoutTime)
		{
			value = m_ttl / m_fadeoutTime;
		}
		if (m_ttl >= m_lifeTime - m_fadeinTime)
		{
			value = (m_lifeTime - m_ttl) / m_fadeinTime;
		}
		m_whirlMaterial.SetFloat("_Opacity", value);
		m_whirlMaterial.SetFloat("_SimTime", m_ttl);
	}

	private void UpdateArms(float dt)
	{
		if (m_spawnTimer < 0f && m_ttl > m_fadeoutTime)
		{
			m_spawnTimer = Random.Range(m_spawnDelayMin, m_spawnDelayMax);
			SpawnBodyPart();
		}
		RemoveOldBodyParts();
	}

	private void UpdateHead(float dt)
	{
		if (m_ttl < m_fadeoutTime)
		{
			if (m_headActive)
			{
				DespawnHead();
			}
			return;
		}
		m_headUpdate += dt;
		if (!(m_headUpdate > 1f))
		{
			return;
		}
		m_headUpdate = 0f;
		if (!m_headActive)
		{
			if ((bool)m_head && !m_head.isPlaying)
			{
				Object.Destroy(m_head);
				m_head = null;
			}
			Quaternion quaternion = Quaternion.Euler(0f, PRand.Range(0, 360), 0f);
			Vector3 vector = quaternion * new Vector3(0f, 0f, 0f - m_effectRadius);
			Vector3 pos = base.transform.position + vector;
			if (!Blocked(pos))
			{
				SpawnHead(pos, quaternion, firstTime: true);
			}
		}
		else if (Blocked(m_head.transform.position))
		{
			DespawnHead();
		}
	}

	private bool Blocked(Vector3 pos)
	{
		if (Physics.CheckSphere(pos, 5f, m_rayMask))
		{
			return true;
		}
		return false;
	}

	private void RemoveOldBodyParts()
	{
		for (int i = 0; i < m_bodyParts.Count; i++)
		{
			if (!m_bodyParts[i].isPlaying)
			{
				Object.Destroy(m_bodyParts[i].gameObject);
				m_bodyParts.RemoveAt(i);
				break;
			}
		}
	}

	private void SpawnBodyPart()
	{
		SpawnTail();
	}

	private void SpawnHead(Vector3 pos, Quaternion rot, bool firstTime)
	{
		m_headActive = true;
		GameObject gameObject = Object.Instantiate(m_headPrefab, pos, rot) as GameObject;
		gameObject.transform.parent = base.transform;
		m_head = gameObject.animation;
		if (firstTime)
		{
			gameObject.animation.CrossFade("leviathan_ascend", 0f);
			gameObject.animation.CrossFadeQueued("leviathan_idle", 0.2f);
			Object.Instantiate(m_spawnEffectHigh, pos, Quaternion.identity);
		}
		else
		{
			gameObject.animation.CrossFade("leviathan_idle", 0f);
		}
	}

	private void DespawnHead()
	{
		m_headActive = false;
		if (!(m_head == null))
		{
			m_head.CrossFade("leviathan_descend", 0.2f);
		}
	}

	private void SpawnBody()
	{
		Vector3 vector = new Vector3(Random.Range(0f - m_effectRadius, m_effectRadius), -13f, Random.Range(0f - m_effectRadius, m_effectRadius));
		Quaternion rot = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
		FixFacing(ref rot);
		GameObject gameObject = Object.Instantiate(m_bodyPrefab, base.transform.position + vector, rot) as GameObject;
		gameObject.transform.parent = base.transform;
		m_bodyParts.Add(gameObject.animation);
	}

	private void FixFacing(ref Quaternion rot)
	{
		if (m_head != null)
		{
			float num = Quaternion.Dot(rot, m_head.transform.rotation);
			if (num < 0f)
			{
				rot *= Quaternion.Euler(0f, 180f, 0f);
			}
		}
	}

	private void SpawnTail()
	{
		Vector3 vector = new Vector3(Random.Range(0f - m_effectRadius, m_effectRadius), -13f, Random.Range(0f - m_effectRadius, m_effectRadius));
		Quaternion rot = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
		FixFacing(ref rot);
		GameObject gameObject = Object.Instantiate(m_tailPrefab, base.transform.position + vector, rot) as GameObject;
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
		writer.Write(m_headActive);
		writer.Write(m_headUpdate);
		if (m_headActive)
		{
			writer.Write(m_head.transform.position.x);
			writer.Write(m_head.transform.position.y);
			writer.Write(m_head.transform.position.z);
			writer.Write(m_head.transform.rotation.x);
			writer.Write(m_head.transform.rotation.y);
			writer.Write(m_head.transform.rotation.z);
			writer.Write(m_head.transform.rotation.w);
		}
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
		m_headActive = reader.ReadBoolean();
		m_headUpdate = reader.ReadSingle();
		if (m_headActive)
		{
			Vector3 pos = default(Vector3);
			pos.x = reader.ReadSingle();
			pos.y = reader.ReadSingle();
			pos.z = reader.ReadSingle();
			Quaternion rot = default(Quaternion);
			rot.x = reader.ReadSingle();
			rot.y = reader.ReadSingle();
			rot.z = reader.ReadSingle();
			rot.w = reader.ReadSingle();
			SpawnHead(pos, rot, firstTime: false);
		}
		for (int i = 0; i < 10; i++)
		{
			m_psystem.Simulate(1f);
		}
		m_psystem.Play();
		UpdateWhirl();
	}

	protected override void OnSetSimulating(bool enabled)
	{
		base.OnSetSimulating(enabled);
		Pause(NetObj.m_simulating);
	}
}
