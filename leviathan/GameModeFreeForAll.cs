using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeFreeForAll")]
internal class GameModeFreeForAll : GameMode
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
		if (num <= 1)
		{
			return GameOutcome.GameOver;
		}
		return GameOutcome.None;
	}

	public override bool IsPlayerDead(int playerID)
	{
		return CheckNrOfUnits(playerID) == 0;
	}
}
