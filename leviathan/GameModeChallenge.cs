using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeChallenge")]
internal class GameModeChallenge : GameMode
{
	public override void SimulationUpdate(float dt)
	{
	}

	public override GameOutcome GetOutcome()
	{
		int num = 0;
		for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
		{
			if (!IsPlayerDead(i))
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return GameOutcome.Defeat;
		}
		return GameOutcome.None;
	}

	public override bool IsPlayerDead(int playerID)
	{
		return CheckNrOfUnits(playerID) == 0;
	}

	public override bool HasObjectives()
	{
		return true;
	}

	public override void CheckAchivements(UserManClient m_userManClient)
	{
		base.CheckAchivements(m_userManClient);
	}
}
