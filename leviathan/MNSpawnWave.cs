using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNSpawnWave")]
public class MNSpawnWave : MNode
{
	public enum SpawnType
	{
		Spawn,
		Message
	}

	[Serializable]
	public class Spawn
	{
		public SpawnType m_spawnType;

		public float m_delay;

		public string m_type;

		public int m_number = 1;

		private int m_total;

		public void SaveState(BinaryWriter writer)
		{
			writer.Write(m_delay);
			writer.Write(m_type);
			writer.Write(m_number);
			writer.Write(m_total);
			writer.Write((int)m_spawnType);
		}

		public void LoadState(BinaryReader reader)
		{
			m_delay = reader.ReadSingle();
			m_type = reader.ReadString();
			m_number = reader.ReadInt32();
			m_total = reader.ReadInt32();
			m_spawnType = (SpawnType)reader.ReadInt32();
		}

		public void Update(MNSpawnWave wave, float time)
		{
			if (!(time < m_delay) && m_total != 0)
			{
				m_total = 0;
				if (m_spawnType == SpawnType.Spawn)
				{
					wave.SpawnIt(m_type);
				}
				if (m_spawnType == SpawnType.Message)
				{
					MessageLog.instance.ShowMessage(MessageLog.TextPosition.Middle, m_type, string.Empty, string.Empty, 2f);
				}
			}
		}

		public void Reset()
		{
			m_total = m_number;
		}
	}

	[Serializable]
	public class WaveAction
	{
		public string m_name;

		public float m_delay;

		public List<Spawn> m_spawns = new List<Spawn>();

		public void SaveState(BinaryWriter writer)
		{
			writer.Write(m_name);
			writer.Write(m_delay);
			writer.Write(m_spawns.Count);
			foreach (Spawn spawn in m_spawns)
			{
				spawn.SaveState(writer);
			}
		}

		public void LoadState(BinaryReader reader)
		{
			m_name = reader.ReadString();
			m_delay = reader.ReadSingle();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Spawn spawn = new Spawn();
				spawn.LoadState(reader);
				m_spawns.Add(spawn);
			}
		}

		public void Update(MNSpawnWave wave, float time)
		{
			if (time < m_delay)
			{
				return;
			}
			wave.SetName(m_name);
			foreach (Spawn spawn in m_spawns)
			{
				spawn.Update(wave, time - m_delay);
			}
		}

		public void Reset()
		{
			foreach (Spawn spawn in m_spawns)
			{
				spawn.Reset();
			}
		}
	}

	public enum Mode
	{
		Off,
		On
	}

	public string m_area;

	public string m_wavefile;

	public string m_wavename;

	public Mode m_status;

	private List<WaveAction> m_waveaction = new List<WaveAction>();

	private string m_name;

	private float m_time;

	public override void Awake()
	{
		base.Awake();
		LoadWaves();
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating && m_status == Mode.On)
		{
			m_time += Time.fixedDeltaTime;
			UpdateSpawn();
		}
	}

	public override void DoAction()
	{
		if (m_status == Mode.Off)
		{
			OnEvent("on");
		}
	}

	public override void OnEvent(string eventName)
	{
		if (eventName == "on")
		{
			PLog.Log("MNSpawnWave: On");
			m_time = 0f;
			m_status = Mode.On;
			Reset();
		}
		else if (eventName == "off")
		{
			PLog.Log("MNSpawnWave: off");
			m_status = Mode.Off;
		}
		else
		{
			EventWarning(eventName);
		}
	}

	private void Reset()
	{
		foreach (WaveAction item in m_waveaction)
		{
			item.Reset();
		}
	}

	private void UpdateSpawn()
	{
		foreach (WaveAction item in m_waveaction)
		{
			item.Update(this, m_time);
		}
	}

	public void SpawnIt(string shipname)
	{
		NetObj[] spawners = GetSpawners(m_name);
		NetObj[] array = spawners;
		foreach (NetObj netObj in array)
		{
			netObj.GetComponent<MNSpawn>().m_spawnShip = shipname;
			netObj.GetComponent<MNSpawn>().DoAction();
		}
	}

	public NetObj[] GetSpawners(string name)
	{
		List<NetObj> list = new List<NetObj>();
		NetObj[] allToSave = NetObj.GetAllToSave();
		NetObj[] array = allToSave;
		foreach (NetObj netObj in array)
		{
			MNSpawn component = netObj.GetComponent<MNSpawn>();
			if (!(component == null) && !(component.m_area != m_area) && component.m_name == name)
			{
				list.Add(netObj);
			}
		}
		return list.ToArray();
	}

	public void LoadWaves()
	{
		if (m_wavefile.Length == 0)
		{
			return;
		}
		TextAsset textAsset = Resources.Load(m_wavefile) as TextAsset;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		for (XmlNode xmlNode = xmlDocument.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "wave" && xmlNode.Attributes["name"].Value == m_wavename)
			{
				LoadWave(xmlNode.FirstChild);
			}
		}
		m_wavefile = string.Empty;
		Reset();
	}

	public void LoadWave(XmlNode it)
	{
		while (it != null)
		{
			if (it.Name == "waveActions")
			{
				WaveAction waveAction = new WaveAction();
				waveAction.m_name = it.Attributes["area"].Value;
				waveAction.m_delay = float.Parse(it.Attributes["delay"].Value);
				for (XmlNode xmlNode = it.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
				{
					if (xmlNode.Name == "spawn")
					{
						Spawn spawn = new Spawn();
						spawn.m_spawnType = SpawnType.Spawn;
						spawn.m_type = xmlNode.Attributes["type"].Value;
						spawn.m_number = int.Parse(xmlNode.Attributes["nr"].Value);
						spawn.m_delay = float.Parse(xmlNode.Attributes["delay"].Value);
						waveAction.m_spawns.Add(spawn);
					}
					if (xmlNode.Name == "message")
					{
						Spawn spawn2 = new Spawn();
						spawn2.m_spawnType = SpawnType.Message;
						spawn2.m_type = xmlNode.Attributes["text"].Value;
						spawn2.m_delay = float.Parse(xmlNode.Attributes["delay"].Value);
						waveAction.m_spawns.Add(spawn2);
					}
				}
				m_waveaction.Add(waveAction);
			}
			it = it.NextSibling;
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_wavefile);
		writer.Write(m_time);
		writer.Write(m_area);
		writer.Write((int)m_status);
		writer.Write(m_waveaction.Count);
		foreach (WaveAction item in m_waveaction)
		{
			item.SaveState(writer);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_wavefile = reader.ReadString();
		m_time = reader.ReadSingle();
		m_area = reader.ReadString();
		m_status = (Mode)reader.ReadInt32();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			WaveAction waveAction = new WaveAction();
			waveAction.LoadState(reader);
			m_waveaction.Add(waveAction);
		}
	}

	public void SetName(string name)
	{
		m_name = name;
	}
}
