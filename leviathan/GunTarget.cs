using System.IO;
using UnityEngine;

public class GunTarget
{
	private int m_targetID = -1;

	private Vector3 m_targetPos;

	public GunTarget(Vector3 worldPos)
	{
		m_targetPos = worldPos;
	}

	public GunTarget(int targetNetID, Vector3 localTargetPos)
	{
		m_targetID = targetNetID;
		m_targetPos = localTargetPos;
	}

	public GunTarget(BinaryReader reader)
	{
		Load(reader);
	}

	public bool IsEqual(GunTarget other)
	{
		return other.m_targetID == m_targetID && other.m_targetPos == m_targetPos;
	}

	public bool IsValid()
	{
		if (m_targetID >= 1)
		{
			return NetObj.GetByID(m_targetID) != null;
		}
		return true;
	}

	public bool GetTargetWorldPos(out Vector3 worldPos, int ownerTeam)
	{
		if (m_targetID >= 1)
		{
			NetObj byID = NetObj.GetByID(m_targetID);
			if (byID != null)
			{
				if (!IsTargetAlive(byID))
				{
					worldPos = Vector3.zero;
					return false;
				}
				if (!byID.IsSeenByTeam(ownerTeam))
				{
					worldPos = Vector3.zero;
					return false;
				}
				worldPos = byID.transform.TransformPoint(m_targetPos);
				Ship ship = byID as Ship;
				if (ship != null && worldPos.y > ship.m_deckHeight)
				{
					worldPos.y = ship.m_deckHeight;
				}
				return true;
			}
			worldPos = Vector3.zero;
			return false;
		}
		worldPos = m_targetPos;
		return true;
	}

	public bool IsTargetAlive()
	{
		if (m_targetID >= 1)
		{
			NetObj byID = NetObj.GetByID(m_targetID);
			if (byID != null)
			{
				return IsTargetAlive(byID);
			}
			return false;
		}
		return true;
	}

	private bool IsTargetAlive(NetObj obj)
	{
		Unit unit = obj as Unit;
		if (unit != null)
		{
			return !unit.IsDead();
		}
		HPModule hPModule = obj as HPModule;
		if (hPModule != null)
		{
			return !hPModule.IsDisabled();
		}
		return false;
	}

	public NetObj GetTargetObject()
	{
		if (m_targetID >= 1)
		{
			return NetObj.GetByID(m_targetID);
		}
		return null;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(m_targetID);
		writer.Write(m_targetPos.x);
		writer.Write(m_targetPos.y);
		writer.Write(m_targetPos.z);
	}

	public void Load(BinaryReader reader)
	{
		m_targetID = reader.ReadInt32();
		m_targetPos.x = reader.ReadSingle();
		m_targetPos.y = reader.ReadSingle();
		m_targetPos.z = reader.ReadSingle();
	}
}
