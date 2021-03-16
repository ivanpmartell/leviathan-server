using System.IO;
using UnityEngine;

public class ShipSpawner : NetObj
{
	public string m_spawnShip;

	public int m_targetOwner = 7;

	private bool m_spawned;

	private void FixedUpdate()
	{
		if (NetObj.m_simulating && !m_spawned)
		{
			m_spawned = true;
			ShipFactory.instance.CreateShip(m_spawnShip, base.transform.position, base.transform.rotation, m_targetOwner);
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
		writer.Write(m_spawnShip);
		writer.Write(m_targetOwner);
		writer.Write(m_spawned);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		Vector3 position = new Vector3(0f, 0f, 0f);
		Quaternion rotation = new Quaternion(0f, 0f, 0f, 0f);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.position = position;
		base.transform.rotation = rotation;
		m_spawnShip = reader.ReadString();
		m_targetOwner = reader.ReadInt32();
		m_spawned = reader.ReadBoolean();
	}
}
