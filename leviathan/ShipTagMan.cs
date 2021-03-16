using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class ShipTagMan
{
	private GameObject m_guiCamera;

	private GameType m_gameType;

	private List<ShipTag> m_tags = new List<ShipTag>();

	private bool m_visible = true;

	private List<GameObject> m_statusIconPrefabs = new List<GameObject>();

	public ShipTagMan(GameObject guiCamera)
	{
		m_guiCamera = guiCamera;
		Unit.m_onCreated = (Action<Unit>)Delegate.Remove(Unit.m_onCreated, new Action<Unit>(OnUnitCreated));
		Unit.m_onRemoved = (Action<Unit>)Delegate.Remove(Unit.m_onRemoved, new Action<Unit>(OnUnitRemoved));
		Unit.m_onCreated = (Action<Unit>)Delegate.Combine(Unit.m_onCreated, new Action<Unit>(OnUnitCreated));
		Unit.m_onRemoved = (Action<Unit>)Delegate.Combine(Unit.m_onRemoved, new Action<Unit>(OnUnitRemoved));
		SetupPrefabList();
	}

	public void SetGameType(GameType type)
	{
		m_gameType = type;
	}

	public void Close()
	{
		Unit.m_onCreated = (Action<Unit>)Delegate.Remove(Unit.m_onCreated, new Action<Unit>(OnUnitCreated));
		Unit.m_onRemoved = (Action<Unit>)Delegate.Remove(Unit.m_onRemoved, new Action<Unit>(OnUnitRemoved));
		foreach (ShipTag tag in m_tags)
		{
			tag.Close();
		}
	}

	public void Update(Camera camera, float dt)
	{
		int localPlayer = NetObj.GetLocalPlayer();
		int playerTeam = TurnMan.instance.GetPlayerTeam(localPlayer);
		foreach (ShipTag tag in m_tags)
		{
			tag.Update(camera, localPlayer, playerTeam);
		}
	}

	private void OnUnitCreated(Unit unit)
	{
		if (unit.m_shipTag != Unit.ShipTagType.None)
		{
			ShipTag shipTag = new ShipTag(unit, m_guiCamera, m_gameType, m_statusIconPrefabs);
			shipTag.SetVisible(m_visible);
			m_tags.Add(shipTag);
		}
	}

	private void OnUnitRemoved(Unit unit)
	{
		for (int i = 0; i < m_tags.Count; i++)
		{
			if (m_tags[i].GetUnit() == unit)
			{
				m_tags[i].Close();
				m_tags.RemoveAt(i);
				break;
			}
		}
	}

	public void SetVisible(bool visible)
	{
		m_visible = visible;
		foreach (ShipTag tag in m_tags)
		{
			tag.SetVisible(visible);
		}
	}

	private void SetupPrefabList()
	{
		for (int i = 0; i < 7; i++)
		{
			m_statusIconPrefabs.Add(null);
		}
		m_statusIconPrefabs[0] = Resources.Load("gui/ShipTagStatusListItems/StatusHPDisabledListItem") as GameObject;
		m_statusIconPrefabs[1] = Resources.Load("gui/ShipTagStatusListItems/StatusMoveDisabledListItem") as GameObject;
		m_statusIconPrefabs[2] = Resources.Load("gui/ShipTagStatusListItems/StatusOOCListItem") as GameObject;
		m_statusIconPrefabs[3] = Resources.Load("gui/ShipTagStatusListItems/StatusRepairListItem") as GameObject;
		m_statusIconPrefabs[4] = Resources.Load("gui/ShipTagStatusListItems/StatusSinkListItem") as GameObject;
		m_statusIconPrefabs[5] = Resources.Load("gui/ShipTagStatusListItems/StatusViewDisabledListItem") as GameObject;
		m_statusIconPrefabs[6] = Resources.Load("gui/ShipTagStatusListItems/StatusGroundedListItem") as GameObject;
	}
}
