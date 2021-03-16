using System.Collections.Generic;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeBlitz")]
internal class GameModeBlitz : GameMode
{
	private int m_targetScore = 250;

	public override void Setup(GameSettings gameSettings)
	{
		base.Setup(gameSettings);
		if (gameSettings.m_nrOfPlayers >= 3)
		{
			m_targetScore = (int)((float)gameSettings.m_fleetSizeLimits.max * gameSettings.m_targetScore * 2f);
		}
		else
		{
			m_targetScore = (int)((float)gameSettings.m_fleetSizeLimits.max * gameSettings.m_targetScore);
		}
		string @string = PlayerPrefs.GetString("VO", string.Empty);
		VOSystem.instance.SetAnnouncer(@string);
	}

	public override void InitializeGame(List<PlayerInitData> players)
	{
		m_achYouAreTiny = true;
		m_achAtlasCrush = true;
		int[] array = new int[4];
		foreach (PlayerInitData player in players)
		{
			int teamPlayerNR = array[player.m_team]++;
			SpawnPlayerShips(player.m_id, player.m_team, teamPlayerNR, player.m_fleet);
		}
	}

	public override void OnSimulationComplete()
	{
		int targetScore = GetTargetScore();
		int teamScore = TurnMan.instance.GetTeamScore(0);
		int teamScore2 = TurnMan.instance.GetTeamScore(1);
		int num = targetScore / 4;
		int num2 = targetScore / 2;
		int num3 = targetScore / 4 * 3;
		if (teamScore > num3 || teamScore2 > num3)
		{
			TurnMan.instance.SetTurnMusic("action_4");
			return;
		}
		if (teamScore > num2 || teamScore2 > num2)
		{
			TurnMan.instance.SetTurnMusic("action_3");
			return;
		}
		if (teamScore > num || teamScore2 > num)
		{
			TurnMan.instance.SetTurnMusic("action_2");
			return;
		}
		for (int i = 0; i < TurnMan.instance.GetNrOfPlayers(); i++)
		{
			if (TurnMan.instance.GetPlayerTurnDamage(i) > 0)
			{
				TurnMan.instance.SetTurnMusic("action_1");
				break;
			}
		}
	}

	public override int GetTargetScore()
	{
		return m_targetScore;
	}

	private void SpawnPlayerShips(int playerID, int team, int teamPlayerNR, FleetDef playerFleet)
	{
		GameObject spawnPoint = GetSpawnPoint();
		if (spawnPoint == null)
		{
			PLog.LogError("Could not find spawnpoint for team " + team + " playernr " + teamPlayerNR);
		}
		else
		{
			SpawnFleet(spawnPoint, playerID, playerFleet);
		}
	}

	public override GameOutcome GetOutcome()
	{
		if (TurnMan.instance.GetTeamScore(0) >= m_targetScore)
		{
			return GameOutcome.GameOver;
		}
		if (TurnMan.instance.GetTeamScore(1) >= m_targetScore)
		{
			return GameOutcome.GameOver;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
		{
			if (!IsPlayerDead(i))
			{
				if (TurnMan.instance.GetPlayerTeam(i) == 0)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (num == 0 || num2 == 0)
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
		if (teamScore >= m_targetScore && teamScore2 >= m_targetScore)
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
		int totalValue = unit.GetTotalValue();
		int owner = unit.GetOwner();
		int playerTeam = TurnMan.instance.GetPlayerTeam(owner);
		PLog.Log("Unit killed " + unit.name + " value " + totalValue + " owner " + owner + " team " + playerTeam);
		switch (playerTeam)
		{
		case 0:
			TurnMan.instance.AddTeamScore(1, totalValue);
			break;
		case 1:
			TurnMan.instance.AddTeamScore(0, totalValue);
			break;
		}
		if (unit.IsVisible())
		{
			HitText.instance.AddDmgText(unit.GetNetID(), unit.transform.position, totalValue.ToString(), Constants.m_pointsText);
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
