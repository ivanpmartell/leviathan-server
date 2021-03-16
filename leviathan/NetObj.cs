#define DEBUG
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetObj : MonoBehaviour
{
	private static Dictionary<int, NetObj> m_wsObjects = new Dictionary<int, NetObj>();

	private static List<NetObj> m_wsObjectList = new List<NetObj>();

	private static int m_nextNetID = 0;

	public bool m_stayVisible;

	protected static int m_localPlayerID = -1;

	protected static bool m_simulating = false;

	protected static TurnPhase m_phase = TurnPhase.Planning;

	protected static bool m_drawOrders = false;

	protected bool m_save = true;

	protected bool m_updateSeenBy = true;

	private int m_netID;

	private bool m_visible = true;

	private int m_seenBy;

	private int m_owner = 7;

	public virtual void Awake()
	{
		if (m_nextNetID >= 1)
		{
			m_netID = m_nextNetID++;
			m_wsObjects.Add(m_netID, this);
			m_wsObjectList.Add(this);
		}
	}

	public virtual void OnDestroy()
	{
		if (m_netID >= 1)
		{
			m_wsObjects.Remove(m_netID);
			int num = m_wsObjectList.IndexOf(this);
			if (num >= 0)
			{
				int index = m_wsObjectList.Count - 1;
				m_wsObjectList[num] = m_wsObjectList[index];
				m_wsObjectList.RemoveAt(index);
			}
		}
	}

	public int GetNetID()
	{
		return m_netID;
	}

	public void SetNetID(int id)
	{
		if (m_netID >= 1)
		{
			m_wsObjects.Remove(m_netID);
			m_wsObjectList.Remove(this);
		}
		m_netID = id;
		m_wsObjects.Add(m_netID, this);
		m_wsObjectList.Add(this);
	}

	public static NetObj GetByID(int id)
	{
		if (m_wsObjects.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public static List<NetObj> GetAll()
	{
		return m_wsObjectList;
	}

	public static NetObj[] GetAllToSave()
	{
		List<NetObj> list = new List<NetObj>();
		foreach (NetObj wsObject in m_wsObjectList)
		{
			if (wsObject.m_save)
			{
				if (wsObject.gameObject == null)
				{
					PLog.LogError("Object has been destroyed " + wsObject.gameObject.name + ", Please inform mr DVOID about this incident...or burn in hell for all eternity!!!");
				}
				else
				{
					list.Add(wsObject);
				}
			}
		}
		return list.ToArray();
	}

	public static void ResetObjectDB()
	{
		m_nextNetID = 0;
		m_wsObjects.Clear();
		m_wsObjectList.Clear();
	}

	public static void SetNextNetID(int id)
	{
		m_nextNetID = id;
	}

	public static int GetNextNetID()
	{
		return m_nextNetID;
	}

	public virtual void SaveState(BinaryWriter writer)
	{
		writer.Write(GetNetID());
		writer.Write((byte)m_owner);
		writer.Write((short)m_seenBy);
	}

	public virtual void LoadState(BinaryReader reader)
	{
		int netID = reader.ReadInt32();
		SetNetID(netID);
		SetOwner(reader.ReadByte());
		m_seenBy = reader.ReadInt16();
	}

	public virtual bool IsVisible()
	{
		return m_visible;
	}

	public virtual void SetVisible(bool visible)
	{
		m_visible = visible;
	}

	public virtual bool IsSeenByPlayer(int playerID)
	{
		if (playerID < 0)
		{
			return true;
		}
		DebugUtils.Assert(playerID >= 0);
		int playerTeam = TurnMan.instance.GetPlayerTeam(playerID);
		return IsSeenByTeam(playerTeam);
	}

	private void SetSeenByPlayer(int playerID)
	{
		if (TurnMan.instance != null)
		{
			int playerTeam = TurnMan.instance.GetPlayerTeam(playerID);
			m_seenBy |= 1 << playerTeam;
		}
	}

	public virtual bool IsSeenByTeam(int teamID)
	{
		int num = 1 << teamID;
		return (m_seenBy & num) != 0;
	}

	public int GetSeenByMask()
	{
		return m_seenBy;
	}

	public void SetSeenByMask(int mask)
	{
		m_seenBy = mask;
	}

	public void UpdateSeenByMask(int mask)
	{
		if (m_phase == TurnPhase.Testing)
		{
			mask = m_seenBy & mask;
		}
		if (m_stayVisible)
		{
			m_seenBy |= mask;
		}
		else
		{
			m_seenBy = mask;
		}
	}

	protected virtual void OnSetSimulating(bool enabled)
	{
	}

	protected virtual void OnSetDrawOrders(bool enabled)
	{
	}

	public virtual void SetOwner(int owner)
	{
		m_owner = owner;
		SetSeenByPlayer(owner);
	}

	public int GetOwner()
	{
		return m_owner;
	}

	public int GetOwnerTeam()
	{
		return TurnMan.instance.GetPlayerTeam(m_owner);
	}

	public bool GetUpdateSeenBy()
	{
		return m_updateSeenBy;
	}

	public static void SetDrawOrders(bool enabled)
	{
		m_drawOrders = enabled;
		foreach (NetObj wsObject in m_wsObjectList)
		{
			wsObject.OnSetDrawOrders(enabled && wsObject.GetOwner() == m_localPlayerID);
		}
	}

	public static bool GetDrawOrders()
	{
		return m_drawOrders;
	}

	public static void SetPhase(TurnPhase phase)
	{
		m_phase = phase;
	}

	public static void SetSimulating(bool enabled)
	{
		m_simulating = enabled;
		foreach (NetObj wsObject in m_wsObjectList)
		{
			wsObject.OnSetSimulating(enabled);
		}
	}

	public static bool IsSimulating()
	{
		return m_simulating;
	}

	public static void SetLocalPlayer(int localPlayerID)
	{
		m_localPlayerID = localPlayerID;
	}

	public static int GetLocalPlayer()
	{
		return m_localPlayerID;
	}
}
