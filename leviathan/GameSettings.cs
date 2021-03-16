using PTech;

public class GameSettings
{
	public int m_localPlayerID;

	public bool m_localAdmin;

	public int m_campaignID;

	public int m_gameID;

	public string m_gameName;

	public string m_level;

	public string m_campaign;

	public GameType m_gameType;

	public FleetSizeClass m_fleetSizeClass;

	public FleetSize m_fleetSizeLimits = new FleetSize(0, 0);

	public float m_targetScore;

	public double m_maxTurnTime = -1.0;

	public int m_nrOfPlayers;

	public MapInfo m_mapInfo;

	public CampaignInfo m_campaignInfo;
}
