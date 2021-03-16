using System;
using System.IO;
using UnityEngine;

[Serializable]
public class UnitAi
{
	public enum AttackDirection
	{
		Front,
		Back,
		Left,
		Right,
		None
	}

	public bool m_inactive;

	public int m_targetId = -1;

	public float m_nextScan;

	public Vector3 m_position = default(Vector3);

	public Vector3? m_goalPosition;

	public Vector3? m_goalFacing;

	public new string ToString()
	{
		string text = "UnitAi:\n";
		text = text + "   Target: " + m_targetId;
		Unit unit = NetObj.GetByID(m_targetId) as Unit;
		if ((bool)unit)
		{
			text = text + " " + unit.name;
		}
		text += "\n";
		Vector3? goalPosition = m_goalPosition;
		text = (goalPosition.HasValue ? (text + "   Pos=" + m_goalPosition.ToString()) : (text + "   Pos=NULL"));
		Vector3? goalFacing = m_goalFacing;
		if (!goalFacing.HasValue)
		{
			return text + "/Face=NULL\n";
		}
		return text + "/Face=" + m_goalFacing.ToString() + "\n";
	}

	public virtual void SaveState(BinaryWriter writer)
	{
		writer.Write(m_inactive);
		writer.Write(m_targetId);
		writer.Write(m_nextScan);
		Utils.WriteVector3(writer, m_position);
		Utils.WriteVector3Nullable(writer, m_goalPosition);
		Utils.WriteVector3Nullable(writer, m_goalFacing);
	}

	public virtual void LoadState(BinaryReader reader)
	{
		m_inactive = reader.ReadBoolean();
		m_targetId = reader.ReadInt32();
		m_nextScan = reader.ReadSingle();
		m_position = Utils.ReadVector3(reader);
		Utils.ReadVector3Nullable(reader, out m_goalPosition);
		Utils.ReadVector3Nullable(reader, out m_goalFacing);
	}

	public virtual AttackDirection GetAttackDirection(Vector3 position)
	{
		return AttackDirection.None;
	}

	public bool HasEnemy()
	{
		if (m_targetId > 0)
		{
			return true;
		}
		return false;
	}

	public virtual void SetTargetId(Unit target)
	{
		m_targetId = target.GetNetID();
	}

	public Unit VerifyTarget()
	{
		Unit unit = NetObj.GetByID(m_targetId) as Unit;
		if (unit == null)
		{
			m_targetId = 0;
			return null;
		}
		if (unit.IsDead())
		{
			m_targetId = 0;
			return null;
		}
		return unit;
	}
}
