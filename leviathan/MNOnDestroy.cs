using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNOnDestroy")]
public class MNOnDestroy : MNTrigger
{
	public string m_group = string.Empty;

	public int[] m_destroyedNetID = new int[0];

	public GameObject[] m_destroyed = new GameObject[0];

	public override void Awake()
	{
		base.Awake();
	}

	public virtual void OnDrawGizmosSelected()
	{
		for (int i = 0; i < m_destroyed.Length; i++)
		{
			GameObject targetObj = GetTargetObj(i);
			if (targetObj != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(GetComponent<Transform>().position, targetObj.GetComponent<Transform>().position);
			}
		}
	}

	protected void FixedUpdate()
	{
		if (NetObj.m_simulating && !m_disabled && CheckNrOfUnits(m_group) == 0)
		{
			Trigger();
		}
	}

	private int CheckNrOfUnits(string group)
	{
		int num = 0;
		for (int i = 0; i < m_destroyed.Length; i++)
		{
			GameObject targetObj = GetTargetObj(i);
			if (targetObj != null)
			{
				Platform component = targetObj.GetComponent<Platform>();
				if ((bool)component && !component.IsDead())
				{
					num++;
				}
				MNSpawn component2 = targetObj.GetComponent<MNSpawn>();
				if ((bool)component2 && component2.ShouldSpawn() && !component2.SpawnedBeenDestroyed())
				{
					num++;
				}
			}
		}
		return num;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_group);
		writer.Write(m_destroyed.Length);
		GameObject[] destroyed = m_destroyed;
		foreach (GameObject gameObject in destroyed)
		{
			if (gameObject == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(gameObject.GetComponent<NetObj>().GetNetID());
			}
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_group = reader.ReadString();
		int num = reader.ReadInt32();
		m_destroyedNetID = new int[num];
		m_destroyed = new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			m_destroyedNetID[i] = reader.ReadInt32();
		}
	}

	public GameObject GetTargetObj(int index)
	{
		if (m_destroyed[index] != null)
		{
			return m_destroyed[index];
		}
		if (index >= m_destroyedNetID.Length)
		{
			return null;
		}
		if (m_destroyedNetID[index] == 0)
		{
			return null;
		}
		m_destroyed[index] = NetObj.GetByID(m_destroyedNetID[index]).gameObject;
		return m_destroyed[index];
	}
}
