using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNRepeater")]
public class MNRepeater : MNode
{
	public int[] m_targetNetID = new int[0];

	public GameObject[] m_repeatTargets = new GameObject[0];

	public override void Awake()
	{
		base.Awake();
	}

	protected virtual void Destroy()
	{
	}

	protected virtual void Update()
	{
	}

	public virtual void OnDrawGizmos()
	{
		for (int i = 0; i < m_repeatTargets.Length; i++)
		{
			GameObject targetObj = GetTargetObj(i);
			if (targetObj != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(GetComponent<Transform>().position, targetObj.GetComponent<Transform>().position);
			}
		}
	}

	public override void DoAction()
	{
		for (int i = 0; i < m_repeatTargets.Length; i++)
		{
			GameObject targetObj = GetTargetObj(i);
			if (targetObj != null)
			{
				targetObj.GetComponent<MNode>().DoAction();
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		for (int i = 0; i < m_repeatTargets.Length; i++)
		{
			GetTargetObj(i);
		}
		writer.Write(m_repeatTargets.Length);
		GameObject[] repeatTargets = m_repeatTargets;
		foreach (GameObject gameObject in repeatTargets)
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
		int num = reader.ReadInt32();
		m_targetNetID = new int[num];
		m_repeatTargets = new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			m_targetNetID[i] = reader.ReadInt32();
		}
	}

	public GameObject GetTargetObj(int index)
	{
		if (m_repeatTargets[index] != null)
		{
			return m_repeatTargets[index];
		}
		if (index >= m_targetNetID.Length)
		{
			return null;
		}
		if (m_targetNetID[index] == 0)
		{
			return null;
		}
		m_repeatTargets[index] = NetObj.GetByID(m_targetNetID[index]).gameObject;
		return m_repeatTargets[index];
	}
}
