using System.Collections.Generic;
using System.IO;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameMode")]
public abstract class GameMode : MonoBehaviour
{
	protected GameSettings m_gameSettings;

	private int m_currentSpawn;

	private List<GameObject> m_spawns;

	public bool m_achYouAreTiny;

	public bool m_achAtlasCrush;

	public virtual void Setup(GameSettings gameSettings)
	{
		m_gameSettings = gameSettings;
		Unit.m_onKilled = OnUnitKilled;
	}

	public virtual void Close()
	{
		Unit.m_onKilled = null;
	}

	public virtual void InitializeGame(List<PlayerInitData> players)
	{
		foreach (PlayerInitData player in players)
		{
			if (player.m_fleet != null)
			{
				SpawnPlayerShips(player.m_id, player.m_fleet);
			}
		}
	}

	public GameObject GetSpawnPoint()
	{
		if (m_spawns == null)
		{
			m_spawns = new List<GameObject>();
			GameObject gameObject = GameObject.Find("playerspawns");
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				Transform child = gameObject.transform.GetChild(i);
				m_spawns.Add(child.gameObject);
			}
		}
		if (m_spawns.Count == 0)
		{
			PLog.LogError("Out of spawnpoints on level.");
		}
		int index = Random.Range(0, m_spawns.Count - 1);
		GameObject result = m_spawns[index];
		m_spawns.RemoveAt(index);
		return result;
	}

	public virtual void OnStateLoaded()
	{
	}

	public virtual void OnSimulationComplete()
	{
	}

	protected int CheckNrOfUnits(int owner)
	{
		int num = 0;
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Ship component = item.GetComponent<Ship>();
			if (component != null && !component.IsDead() && component.GetOwner() == owner)
			{
				num++;
			}
		}
		return num;
	}

	public virtual int GetTargetScore()
	{
		return 0;
	}

	public virtual void SimulationUpdate(float dt)
	{
	}

	public virtual void LoadState(BinaryReader reader)
	{
		m_achYouAreTiny = reader.ReadBoolean();
		m_achAtlasCrush = reader.ReadBoolean();
	}

	public virtual void SaveState(BinaryWriter writer)
	{
		writer.Write(m_achYouAreTiny);
		writer.Write(m_achAtlasCrush);
	}

	public abstract GameOutcome GetOutcome();

	public virtual int GetWinnerTeam(int nrOfPlayers)
	{
		return -1;
	}

	public virtual int GetPlayerPlace(int playerID, int nrOfPlayers)
	{
		return 0;
	}

	public abstract bool IsPlayerDead(int playerID);

	public virtual void OnUnitKilled(Unit unit)
	{
	}

	public virtual void OnHPModuleKilled(HPModule module)
	{
	}

	public virtual bool HasObjectives()
	{
		return true;
	}

	protected void SpawnPlayerShips(int playerID, FleetDef playerFleet)
	{
		GameObject gameObject = GameObject.Find("PlayerStart" + playerID);
		if (gameObject == null)
		{
			PLog.LogError("Could not find spawnpoint for player " + playerID);
		}
		else
		{
			SpawnFleet(gameObject, playerID, playerFleet);
		}
	}

	protected void SpawnFleet(GameObject spawnPoint, int playerID, FleetDef playerFleet)
	{
		float num = 4f;
		float num2 = 0f;
		foreach (ShipDef ship in playerFleet.m_ships)
		{
			num2 += num + ShipFactory.GetShipWidth(ship) + num;
		}
		float num3 = (0f - num2) / 2f;
		for (int i = 0; i < playerFleet.m_ships.Count; i++)
		{
			ShipDef shipDef = playerFleet.m_ships[i];
			float shipWidth = ShipFactory.GetShipWidth(shipDef);
			num3 += num + shipWidth / 2f;
			Vector3 position = new Vector3(num3, 0f, 0f);
			num3 += shipWidth / 2f + num;
			Vector3 pos = spawnPoint.transform.TransformPoint(position);
			pos.y = 0f;
			ShipFactory.CreateShip(shipDef, pos, spawnPoint.transform.rotation, playerID);
			if (shipDef.m_prefab != "MP-MB1")
			{
				m_achYouAreTiny = false;
			}
			if (shipDef.m_prefab != "MP-CS1")
			{
				m_achAtlasCrush = false;
			}
		}
	}

	public virtual void CheckAchivements(UserManClient m_userManClient)
	{
	}

	public void CheckVersusAchivements(UserManClient m_userManClient)
	{
		if (TurnMan.instance.GetPlayer(m_gameSettings.m_localPlayerID).m_team == GetWinnerTeam(m_gameSettings.m_nrOfPlayers))
		{
			if (!IsPlayerDead(m_gameSettings.m_localPlayerID) && TurnMan.instance.GetTotalShipsLost(m_gameSettings.m_localPlayerID) == 0)
			{
				m_userManClient.UnlockAchievement(14);
			}
			if (m_achYouAreTiny)
			{
				m_userManClient.UnlockAchievement(13);
			}
			if (m_achAtlasCrush)
			{
				m_userManClient.UnlockAchievement(12);
			}
		}
	}
}
