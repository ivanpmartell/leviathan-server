using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNTimer")]
public class MNTimer : MNTrigger
{
	private float m_currentTime;

	public float m_time;

	public override void Awake()
	{
		base.Awake();
		m_currentTime = m_time;
	}

	protected void Destroy()
	{
		Debug.Break();
	}

	protected void Update()
	{
	}

	protected void FixedUpdate()
	{
		if (NetObj.m_simulating && !m_disabled)
		{
			m_currentTime -= Time.fixedDeltaTime;
			if (m_currentTime <= 0f)
			{
				m_currentTime = m_time;
				Trigger();
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_currentTime);
		writer.Write(m_time);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_currentTime = reader.ReadSingle();
		m_time = reader.ReadSingle();
	}
}
