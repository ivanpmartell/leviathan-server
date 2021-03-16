using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNTrigger")]
public class MNTrigger : MNode
{
	private int m_targetNetID;

	public GameObject m_target;

	public bool m_oneShot = true;

	public bool m_disabled;

	public override void Awake()
	{
		base.Awake();
	}

	public override void DoAction()
	{
		m_disabled = !m_disabled;
	}

	public virtual void OnDrawGizmos()
	{
		if (GetTargetObj() != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(GetComponent<Transform>().position, GetTargetObj().GetComponent<Transform>().position);
		}
	}

	public virtual void Trigger()
	{
		if (!m_disabled)
		{
			if (m_oneShot)
			{
				m_disabled = true;
			}
			if (GetTargetObj() != null)
			{
				GetTargetObj().GetComponent<MNode>().DoAction();
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		GetTargetObj();
		base.SaveState(writer);
		writer.Write(m_oneShot);
		writer.Write(m_disabled);
		if (m_target == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(m_target.GetComponent<NetObj>().GetNetID());
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_oneShot = reader.ReadBoolean();
		m_disabled = reader.ReadBoolean();
		m_targetNetID = reader.ReadInt32();
	}

	public GameObject GetTargetObj()
	{
		if (m_target != null)
		{
			return m_target;
		}
		if (m_targetNetID == 0)
		{
			return null;
		}
		m_target = NetObj.GetByID(m_targetNetID).gameObject;
		return m_target;
	}
}
