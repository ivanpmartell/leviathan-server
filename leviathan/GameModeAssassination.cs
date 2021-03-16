#define DEBUG
using System.Collections.Generic;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeAssassination")]
internal class GameModeAssassination : GameMode
{
	public string m_kingShip;

	public override void Setup(GameSettings gameSettings)
	{
		base.Setup(gameSettings);
		string @string = PlayerPrefs.GetString("VO", string.Empty);
		VOSystem.instance.SetAnnouncer(@string);
	}

	private int GetPlayersInTeam(List<PlayerInitData> players, int team)
	{
		int num = 0;
		foreach (PlayerInitData player in players)
		{
			if (player.m_team == team)
			{
				num++;
			}
		}
		return num;
	}

	public override void OnSimulationComplete()
	{
		TurnMan instance = TurnMan.instance;
		int num = 0;
		for (int i = 0; i < instance.GetNrOfPlayers(); i++)
		{
			int totalShipsSunk = instance.GetTotalShipsSunk(i);
			if (totalShipsSunk > num)
			{
				num = totalShipsSunk;
			}
		}
		if (num >= 3)
		{
			instance.SetTurnMusic("action_4");
			return;
		}
		if (num >= 2)
		{
			instance.SetTurnMusic("action_3");
			return;
		}
		if (num >= 1)
		{
			instance.SetTurnMusic("action_2");
			return;
		}
		for (int j = 0; j < instance.GetNrOfPlayers(); j++)
		{
			if (instance.GetPlayerTurnDamage(j) > 0)
			{
				instance.SetTurnMusic("action_1");
				break;
			}
		}
	}

	public override void InitializeGame(List<PlayerInitData> players)
	{
		m_achYouAreTiny = true;
		m_achAtlasCrush = true;
		int[] array = new int[4];
		foreach (PlayerInitData player in players)
		{
			bool twoKings = players.Count == 3 && GetPlayersInTeam(players, player.m_team) == 1;
			int teamPlayerNR = array[player.m_team]++;
			SpawnPlayerShips(player.m_id, player.m_team, teamPlayerNR, player.m_fleet, twoKings);
		}
	}

	private void SpawnPlayerShips(int playerID, int team, int teamPlayerNR, FleetDef playerFleet, bool twoKings)
	{
		GameObject spawnPoint = GetSpawnPoint();
		if (spawnPoint == null)
		{
			PLog.LogError("Could not find spawnpoint for team " + team + " playernr " + teamPlayerNR);
		}
		else
		{
			SpawnFleet(spawnPoint, playerID, playerFleet);
			if (twoKings)
			{
				CreateKing(spawnPoint, new Vector3(25f, 0f, -50f), playerID);
				CreateKing(spawnPoint, new Vector3(-25f, 0f, -50f), playerID);
			}
			else
			{
				CreateKing(spawnPoint, new Vector3(0f, 0f, -50f), playerID);
			}
		}
	}

	private void CreateKing(GameObject spawnPoint, Vector3 offset, int playerID)
	{
		Vector3 pos = spawnPoint.transform.TransformPoint(offset);
		pos.y = 0f;
		GameObject gameObject = ShipFactory.instance.CreateShip(m_kingShip, pos, spawnPoint.transform.rotation, playerID);
		if (gameObject != null)
		{
			Unit component = gameObject.GetComponent<Unit>();
			if (component != null)
			{
				component.SetKing(king: true);
			}
		}
	}

	public override GameOutcome GetOutcome()
	{
		if (m_gameSettings.m_nrOfPlayers == 1)
		{
			return GameOutcome.None;
		}
		int num = ((m_gameSettings.m_nrOfPlayers != 4 && m_gameSettings.m_nrOfPlayers != 3) ? 1 : 2);
		if (TurnMan.instance.GetTeamScore(0) >= num)
		{
			return GameOutcome.GameOver;
		}
		if (TurnMan.instance.GetTeamScore(1) >= num)
		{
			return GameOutcome.GameOver;
		}
		int num2 = 0;
		for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
		{
			if (!IsPlayerDead(i))
			{
				num2++;
			}
		}
		if (num2 <= 1)
		{
			return GameOutcome.GameOver;
		}
		return GameOutcome.None;
	}

	public override bool IsPlayerDead(int playerID)
	{
		return CheckNrOfUnits(playerID) == 0;
	}

	public override int GetWinnerTeam(int nrOfPlayers)
	{
		int teamScore = TurnMan.instance.GetTeamScore(0);
		int teamScore2 = TurnMan.instance.GetTeamScore(1);
		if (teamScore == teamScore2)
		{
			return -1;
		}
		if (teamScore > teamScore2)
		{
			return 0;
		}
		return 1;
	}

	public override int GetPlayerPlace(int playerID, int nrOfPlayers)
	{
		int playerTeam = TurnMan.instance.GetPlayerTeam(playerID);
		if (playerTeam < 0 || playerTeam > 1)
		{
			return 2;
		}
		int teamScore = TurnMan.instance.GetTeamScore(0);
		int teamScore2 = TurnMan.instance.GetTeamScore(1);
		int num = ((nrOfPlayers != 4 && nrOfPlayers != 3) ? 1 : 2);
		if (teamScore >= num && teamScore2 >= num)
		{
			return 0;
		}
		if (playerTeam == 0)
		{
			if (teamScore > teamScore2)
			{
				return 0;
			}
			return 1;
		}
		if (teamScore2 > teamScore)
		{
			return 0;
		}
		return 1;
	}

	public override void OnUnitKilled(Unit unit)
	{
		if (unit.IsKing())
		{
			int num = unit.GetLastDamageDealer();
			int owner = unit.GetOwner();
			int playerTeam = TurnMan.instance.GetPlayerTeam(owner);
			if (num < 0)
			{
				num = owner;
			}
			TurnMan.instance.AddFlagshipKiller(owner, num);
			switch (playerTeam)
			{
			case 0:
				TurnMan.instance.AddTeamScore(1, 1);
				break;
			case 1:
				TurnMan.instance.AddTeamScore(0, 1);
				break;
			default:
				DebugUtils.Assert(condition: false, "Invalid team " + playerTeam);
				break;
			}
			if (unit.IsVisible())
			{
				HitText.instance.AddDmgText(unit.GetNetID(), unit.transform.position, string.Empty, Constants.m_assassinatedText);
			}
		}
	}

	public override bool HasObjectives()
	{
		return false;
	}

	public override void CheckAchivements(UserManClient m_userManClient)
	{
		base.CheckAchivements(m_userManClient);
		CheckVersusAchivements(m_userManClient);
	}
}
