using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeCampaign")]
internal class GameModeCampaign : GameMode
{
	private float m_turnTime;

	public override void Setup(GameSettings gameSettings)
	{
		base.Setup(gameSettings);
		m_turnTime = 0f;
	}

	public override void SimulationUpdate(float dt)
	{
		m_turnTime += dt;
	}

	public override GameOutcome GetOutcome()
	{
		if (m_turnTime > 1f)
		{
			bool flag = true;
			for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
			{
				if (!IsPlayerDead(i))
				{
					flag = false;
				}
			}
			if (flag)
			{
				return GameOutcome.Defeat;
			}
		}
		return TurnMan.instance.GetOutcome();
	}

	public override bool IsPlayerDead(int playerID)
	{
		return CheckNrOfUnits(playerID) == 0;
	}

	public override void OnUnitKilled(Unit unit)
	{
		int totalValue = unit.GetTotalValue();
		int owner = unit.GetOwner();
		int playerTeam = TurnMan.instance.GetPlayerTeam(owner);
		if (IsPlayerDead(owner) && owner <= 3)
		{
			if (m_gameSettings.m_localPlayerID == owner)
			{
				MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, "$message_campaign_idead", "$message_campaign_failedteam", string.Empty, 2f);
			}
			else
			{
				MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, TurnMan.instance.GetPlayer(owner).m_name + " $message_campaign_xdead", string.Empty, string.Empty, 2f);
			}
		}
		if (playerTeam != 0)
		{
			TurnMan.instance.AddTeamScore(0, totalValue);
		}
	}

	public override bool HasObjectives()
	{
		return true;
	}

	public override void CheckAchivements(UserManClient m_userManClient)
	{
		base.CheckAchivements(m_userManClient);
		if (TurnMan.instance.GetOutcome() == GameOutcome.Victory)
		{
			if (m_gameSettings.m_level == "t1m3")
			{
				m_userManClient.UnlockAchievement(17);
			}
			if (m_gameSettings.m_level == "c1m3")
			{
				m_userManClient.UnlockAchievement(18);
			}
			if (m_gameSettings.m_level == "c1m6")
			{
				m_userManClient.UnlockAchievement(19);
			}
			if (m_gameSettings.m_level == "c1m9")
			{
				m_userManClient.UnlockAchievement(11);
			}
			if (m_gameSettings.m_level == "cm_wave")
			{
				m_userManClient.UnlockAchievement(4);
			}
			if (m_gameSettings.m_level == "cm_wave_2")
			{
				m_userManClient.UnlockAchievement(5);
			}
			if (m_gameSettings.m_level == "cm_wave_3")
			{
				m_userManClient.UnlockAchievement(6);
			}
			if (m_gameSettings.m_level == "cm_arctic")
			{
				m_userManClient.UnlockAchievement(10);
			}
			if (m_gameSettings.m_level == "cm_sluice")
			{
				m_userManClient.UnlockAchievement(7);
			}
			if (TurnMan.instance.m_missionAchievement != -1)
			{
				m_userManClient.UnlockAchievement(TurnMan.instance.m_missionAchievement);
			}
		}
	}
}
