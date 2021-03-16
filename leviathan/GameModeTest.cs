using UnityEngine;

[AddComponentMenu("Scripts/GameModes/GameModeTest")]
internal class GameModeTest : GameMode
{
	public override void SimulationUpdate(float dt)
	{
	}

	public override GameOutcome GetOutcome()
	{
		return GameOutcome.None;
	}

	public override bool IsPlayerDead(int playerID)
	{
		return CheckNrOfUnits(playerID) == 0;
	}
}
