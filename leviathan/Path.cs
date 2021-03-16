using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Path : NetObj
{
	private Vector3[] m_nodes = new Vector3[0];

	public override void Awake()
	{
		base.Awake();
	}

	public List<GameObject> GetNodes()
	{
		List<GameObject> list = new List<GameObject>();
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < base.gameObject.transform.GetChildCount(); i++)
		{
			GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
			try
			{
				hashSet.Add(Convert.ToInt32(gameObject.name));
			}
			catch (Exception)
			{
			}
		}
		List<int> list2 = new List<int>();
		foreach (int item in hashSet)
		{
			list2.Add(item);
		}
		list2.Sort();
		foreach (int item2 in list2)
		{
			list.Add(base.gameObject.transform.FindChild(Convert.ToString(item2)).gameObject);
		}
		return list;
	}

	private Vector3[] TransferNodes()
	{
		List<GameObject> nodes = GetNodes();
		if (nodes.Count == 0)
		{
			return null;
		}
		Vector3[] array = new Vector3[nodes.Count];
		for (int i = 0; i < nodes.Count; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = nodes[i].transform.position;
		}
		return array;
	}

	public void OnDrawGizmos()
	{
		Vector3[] array = TransferNodes();
		if (array != null && array.Length != 0)
		{
			Vector3 to = array[0];
			Vector3[] array2 = array;
			foreach (Vector3 vector in array2)
			{
				Gizmos.DrawSphere(vector, 1f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(vector, to);
				to = vector;
			}
			to = array[0];
			Vector3[] array3 = array;
			foreach (Vector3 vector2 in array3)
			{
				Gizmos.DrawLine(vector2, to);
				to = vector2;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!NetObj.m_simulating)
		{
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		Vector3[] array = TransferNodes();
		if (array != null)
		{
			m_nodes = array;
		}
		base.SaveState(writer);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(m_nodes.Length);
		Vector3[] nodes = m_nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			Vector3 vector = nodes[i];
			writer.Write(vector.x);
			writer.Write(vector.y);
			writer.Write(vector.z);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		int num = reader.ReadInt32();
		m_nodes = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			m_nodes[i].x = reader.ReadSingle();
			m_nodes[i].y = reader.ReadSingle();
			m_nodes[i].z = reader.ReadSingle();
		}
	}

	public Vector3[] GetPoints()
	{
		return m_nodes;
	}
}
