using System.Collections.Generic;

public class EndGameData
{
	public GameOutcome m_outcome;

	public string m_outcomeText = string.Empty;

	public int m_winnerTeam = -1;

	public int m_localPlayerID;

	public int m_autoJoinGameID;

	public int m_turns;

	public EndGame_PlayerStatistics m_localPlayer;

	public List<EndGame_PlayerStatistics> m_players = new List<EndGame_PlayerStatistics>();

	public string m_AccoladeDestroy = string.Empty;

	public string m_AccoladeHarmless = string.Empty;

	public string m_AccoladeShields = string.Empty;

	public string m_AccoladeDistance = string.Empty;

	public string m_AccoladePlanning = string.Empty;
}
