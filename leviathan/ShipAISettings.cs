using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ShipAISettings
{
	public enum AiMission
	{
		None,
		Defend,
		Attack,
		Patrol,
		Goto,
		BossC1M3,
		Inactive
	}

	public enum AiCombat
	{
		None,
		Offensive,
		Defensive,
		Suicide
	}

	public enum PlayerId
	{
		NoChange = -1,
		Player1,
		Player2,
		Player3,
		Player4,
		Neutral,
		Enemy,
		Enemy2,
		Enemy3,
		Enemy4,
		Enemy5
	}

	public PlayerId m_targetOwner = PlayerId.Enemy;

	public AiMission m_mission = AiMission.Defend;

	public AiCombat m_combatStyle;

	public GameObject m_target;

	public GameObject m_onCombat;

	private int m_targetNetID;

	private int m_onCombatNetID;

	public virtual void SaveState(BinaryWriter writer)
	{
		writer.Write((int)m_targetOwner);
		writer.Write((int)m_mission);
		writer.Write((int)m_combatStyle);
		if (GetTarget() == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(GetTarget().GetComponent<NetObj>().GetNetID());
		}
		if (GetOnCombat() == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(GetOnCombat().GetComponent<NetObj>().GetNetID());
		}
	}

	public virtual void LoadState(BinaryReader reader)
	{
		m_targetOwner = (PlayerId)reader.ReadInt32();
		m_mission = (AiMission)reader.ReadInt32();
		m_combatStyle = (AiCombat)reader.ReadInt32();
		m_targetNetID = reader.ReadInt32();
		m_onCombatNetID = reader.ReadInt32();
	}

	public void Transfer(ShipAISettings aiSetting)
	{
		if (aiSetting.m_combatStyle != 0)
		{
			m_combatStyle = aiSetting.m_combatStyle;
		}
		if (aiSetting.m_mission != 0)
		{
			m_mission = aiSetting.m_mission;
		}
		m_target = aiSetting.m_target;
		m_targetNetID = aiSetting.m_targetNetID;
		m_onCombat = aiSetting.m_onCombat;
		m_onCombatNetID = aiSetting.m_onCombatNetID;
	}

	public GameObject GetTarget()
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

	public GameObject GetOnCombat()
	{
		if (m_onCombat != null)
		{
			return m_onCombat;
		}
		if (m_onCombatNetID == 0)
		{
			return null;
		}
		m_onCombat = NetObj.GetByID(m_onCombatNetID).gameObject;
		return m_onCombat;
	}

	public void RunOnCombatEvent()
	{
		if (m_onCombatNetID != 0)
		{
			NetObj byID = NetObj.GetByID(m_onCombatNetID);
			if ((bool)byID)
			{
				byID.gameObject.GetComponent<MNode>().DoAction();
				m_onCombatNetID = 0;
			}
		}
	}

	public void OnDrawGizmosSelected(GameObject parent)
	{
		GameObject target = GetTarget();
		if (!(target == null))
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(parent.transform.position, target.transform.position);
		}
	}
}
