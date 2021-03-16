using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PTech
{
	internal class Game
	{
		public delegate bool OnSaveReplayDelegate(Game game, User user, string name);

		private const int m_turnFrames = 300;

		public Action<Game> m_onGameOver;

		public Action<User, Game> m_onNewTurnNotification;

		public Action<Game, User, int> m_onInviteFriend;

		public OnSaveReplayDelegate m_onSaveReplay;

		private List<GamePlayer> m_players = new List<GamePlayer>();

		private List<TurnData> m_turns = new List<TurnData>();

		private PackMan m_packMan;

		private ChatServer m_chatServer = new ChatServer();

		private static int m_nextGameID = 1;

		private static int m_nextCampaignID = 1;

		private int m_gameID;

		private int m_campaignID;

		private int m_autoJoinNextGameID;

		private bool m_isNewCampaign;

		private DateTime m_createDate;

		private DateTime m_turnStartDate;

		private float m_timeSinceActive;

		private bool m_replayMode;

		private string m_gameName;

		private string m_campaignName;

		private string m_levelName;

		private GameType m_gameType;

		private int m_nrOfPlayers;

		private FleetSizeClass m_fleetSizeClass;

		private FleetSize m_fleetSizeLimits;

		private float m_targetScore = 0.5f;

		private double m_maxTurnTime = 10.0;

		private float m_autoCommitTimeout = 5f;

		private MapInfo m_mapInfo;

		private float m_timeSyncTimer;

		private float m_timeSyncSendDelay = 5f;

		private GamePlayer m_sentInitiateRequestTo;

		private bool m_gotInitState;

		private bool m_gameStarted;

		private bool m_gameEnded;

		private bool m_disbanded;

		private bool m_publicGame;

		private int m_currentTurn = -1;

		private int m_winnerTeam = -1;

		private GameOutcome m_gameOutcome;

		public Game(string gameName, int gameID, int campaignID, GameType gameType, string campaign, string level, int players, FleetSizeClass fleetClass, float targetScore, double maxTurnTime, MapInfo mapinfo, PackMan packMan)
		{
			m_gameID = ((gameID > 0) ? gameID : m_nextGameID++);
			m_campaignID = campaignID;
			m_gameName = gameName;
			m_campaignName = campaign;
			m_levelName = level;
			m_gameType = gameType;
			m_nrOfPlayers = players;
			m_mapInfo = mapinfo;
			m_fleetSizeClass = fleetClass;
			m_fleetSizeLimits = FleetSizes.sizes[(int)fleetClass];
			m_targetScore = targetScore;
			m_packMan = packMan;
			m_createDate = DateTime.Now;
			m_maxTurnTime = maxTurnTime;
			if (gameType == GameType.Campaign || gameType == GameType.Challenge)
			{
				if (m_mapInfo.m_fleetLimit != null)
				{
					m_fleetSizeClass = FleetSizeClass.Custom;
					m_fleetSizeLimits = new FleetSize(m_mapInfo.m_fleetLimit.min, m_mapInfo.m_fleetLimit.max / players);
				}
				if (m_campaignID <= 0)
				{
					m_isNewCampaign = true;
					m_campaignID = m_nextCampaignID++;
				}
			}
		}

		public void SetupReplayMode()
		{
			m_replayMode = true;
			m_currentTurn = 0;
		}

		public void Close()
		{
			foreach (GamePlayer player in m_players)
			{
				User user = player.GetUser();
				if (user != null)
				{
					user.m_inGames--;
				}
			}
		}

		public static void SetNextGameID(int gameID)
		{
			m_nextGameID = gameID;
		}

		public static int GetNextGameID()
		{
			return m_nextGameID;
		}

		public static void SetNextCampaignID(int campaignID)
		{
			m_nextCampaignID = campaignID;
		}

		public static int GetNextCampaignID()
		{
			return m_nextCampaignID;
		}

		public bool MayJoin(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				if (m_players.Count < m_nrOfPlayers)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public List<User> GetNextGameUserList()
		{
			List<User> list = new List<User>();
			foreach (GamePlayer player in m_players)
			{
				if (!player.m_leftGame)
				{
					list.Add(player.GetUser());
				}
			}
			return list;
		}

		public List<KeyValuePair<User, bool>> GetActiveUserList()
		{
			List<KeyValuePair<User, bool>> list = new List<KeyValuePair<User, bool>>();
			foreach (GamePlayer player in m_players)
			{
				if (!player.m_leftGame && !player.m_seenEndGame)
				{
					bool value = NeedPlayerAttention(player);
					list.Add(new KeyValuePair<User, bool>(player.GetUser(), value));
				}
			}
			return list;
		}

		public int GetOnlinePlayers()
		{
			int num = 0;
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					num++;
				}
			}
			return num;
		}

		public List<string> GetUserNames()
		{
			List<string> list = new List<string>();
			foreach (GamePlayer player in m_players)
			{
				list.Add(player.m_userName);
			}
			return list;
		}

		public void InternalSetUser(string name, User user)
		{
			GetGamePlayerByName(name)?.SetUser(user);
		}

		public bool AddUserToGame(User user, bool admin)
		{
			if (!MayJoin(user))
			{
				return false;
			}
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				AddNewPlayer(user, admin);
				if ((m_gameType == GameType.Assassination || m_gameType == GameType.Points) && m_nrOfPlayers == 3)
				{
					UnreadyAllPlayers();
				}
				SendLobbyToAll();
			}
			return true;
		}

		public bool JoinGame(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				return false;
			}
			gamePlayerByUser.m_inGame = true;
			user.m_rpc.Register("Commit", RPC_Commit);
			user.m_rpc.Register("LeaveGame", RPC_LeaveGame);
			user.m_rpc.Register("KickSelf", RPC_KickSelf);
			user.m_rpc.Register("SetFleet", RPC_SetFleet);
			user.m_rpc.Register("FleetUpdated", RPC_FleetUpdated);
			user.m_rpc.Register("SwitchTeam", RPC_SwitchTeam);
			user.m_rpc.Register("ReadyToStart", RPC_ReadyToStart);
			user.m_rpc.Register("SaveReplay", RPC_SaveReplay);
			user.m_rpc.Register("SeenEndGame", RPC_SeenEndGame);
			user.m_rpc.Register("SimulationResults", RPC_SimulationResults);
			if (gamePlayerByUser.m_admin)
			{
				user.m_rpc.Register("KickPlayer", RPC_KickPlayer);
				user.m_rpc.Register("DisbandGame", RPC_DisbandGame);
				user.m_rpc.Register("Invite", RPC_Invite);
				user.m_rpc.Register("RenameGame", RPC_RenameGame);
				user.m_rpc.Register("AllowMatchmaking", RPC_AllowMatchmaking);
			}
			if (m_replayMode)
			{
				user.m_rpc.Register("RequestReplayTurn", RPC_RequestReplayTurn);
			}
			m_chatServer.Register(gamePlayerByUser, sendOldMessages: true);
			SendGameSettings(gamePlayerByUser);
			SendPlayerStatusToAll();
			if (!m_gameStarted)
			{
				SendLobbyToAll();
			}
			else
			{
				SendGameStarted(gamePlayerByUser);
				TurnData currentTurnData = GetCurrentTurnData();
				if (currentTurnData != null)
				{
					if (AllCommited(currentTurnData) && !m_replayMode)
					{
						RequestSimulation(gamePlayerByUser, currentTurnData);
					}
					else
					{
						SendCurrentTurn(gamePlayerByUser, startReplay: true);
					}
				}
			}
			return true;
		}

		private int GetUnusedPlayerID()
		{
			int i;
			for (i = 0; GetGamePlayerByID(i) != null; i++)
			{
			}
			return i;
		}

		public bool IsAdmin(User user)
		{
			return GetGamePlayerByUser(user)?.m_admin ?? false;
		}

		private GamePlayer AddNewPlayer(User user, bool admin)
		{
			int unusedPlayerID = GetUnusedPlayerID();
			GamePlayer gamePlayer = new GamePlayer(unusedPlayerID);
			gamePlayer.SetUser(user);
			m_players.Add(gamePlayer);
			gamePlayer.m_admin = admin;
			if (m_campaignID > 0)
			{
				AddCampaignContent(gamePlayer);
				if (!user.HaveCampaignFleet(m_campaignID))
				{
					CreateNewCampaignFleet(gamePlayer);
				}
			}
			if (m_gameType == GameType.Points || m_gameType == GameType.Assassination)
			{
				if (m_nrOfPlayers == 2)
				{
					if (gamePlayer.m_id == 0)
					{
						gamePlayer.m_team = 0;
					}
					else
					{
						gamePlayer.m_team = 1;
					}
				}
				if (m_nrOfPlayers == 4)
				{
					if (gamePlayer.m_id <= 1)
					{
						gamePlayer.m_team = 0;
					}
					else
					{
						gamePlayer.m_team = 1;
					}
				}
			}
			return gamePlayer;
		}

		private void AddCampaignContent(GamePlayer p)
		{
			string name = ((!(m_mapInfo.m_contentPack == string.Empty)) ? m_mapInfo.m_contentPack : "Base");
			ContentPack pack = m_packMan.GetPack(name);
			if (pack == null)
			{
				PLog.LogError("Failed to find campaign content pack " + m_mapInfo.m_contentPack);
			}
			else
			{
				p.GetUser().SetCampaignContentPack(m_campaignID, pack);
			}
		}

		private void CreateNewCampaignFleet(GamePlayer player)
		{
			if (!(m_mapInfo.m_defaults != string.Empty))
			{
				return;
			}
			XmlDocument xmlDocument = Utils.LoadXml("shared_settings/campaign_defs/" + m_mapInfo.m_defaults);
			if (xmlDocument == null)
			{
				PLog.LogError("Failed to load shipfleet file " + m_mapInfo.m_defaults);
				return;
			}
			FleetDefUtils.LoadFleetsAndShipsXMLFile(xmlDocument, out var fleets, out var blueprints, ComponentDB.instance);
			foreach (ShipDef item in blueprints)
			{
				item.m_campaignID = m_campaignID;
				player.GetUser().AddShipDef(item);
			}
			if (fleets.Count > 0)
			{
				FleetDef fleetDef;
				if (fleets.Count < m_nrOfPlayers)
				{
					PLog.LogError("Missing fleet for " + m_nrOfPlayers + " players in map " + m_levelName + " using fleet 0");
					fleetDef = fleets[0];
				}
				else
				{
					fleetDef = fleets[m_nrOfPlayers - 1];
				}
				fleetDef.m_campaignID = m_campaignID;
				player.GetUser().AddFleetDef(fleetDef);
				SetPlayerFleet(player.GetUser().m_name, fleetDef.m_name);
			}
		}

		public void Update(float dt)
		{
			if (!m_gameStarted && IsReadyToStart())
			{
				StartGame();
			}
			if (m_gameStarted && !m_gotInitState && m_sentInitiateRequestTo == null)
			{
				SendInitRequest();
			}
			UpdateTimeSync(dt);
			UpdateAutoCommit();
			if (GetConnectedPlayers() == 0)
			{
				m_timeSinceActive += dt;
			}
			else
			{
				m_timeSinceActive = 0f;
			}
		}

		private void StartGame()
		{
			m_gameStarted = true;
			SendGameStartedToAll();
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					m_chatServer.Unregister(player);
					m_chatServer.Register(player, sendOldMessages: false);
				}
			}
		}

		private void UpdateModuleAndshipUsageStats(GamePlayer player)
		{
			UserStats stats = player.GetUser().m_stats;
			Dictionary<string, int> moduleUsage = FleetDefUtils.GetModuleUsage(player.m_fleet);
			foreach (KeyValuePair<string, int> item in moduleUsage)
			{
				stats.AddModuleUsage(item.Key, item.Value);
			}
			Dictionary<string, int> shipUsage = FleetDefUtils.GetShipUsage(player.m_fleet);
			foreach (KeyValuePair<string, int> item2 in shipUsage)
			{
				stats.AddShipUsage(item2.Key, item2.Value);
			}
		}

		private void SendEndGameToAll()
		{
			foreach (GamePlayer player in m_players)
			{
				SendEndGame(player);
			}
		}

		private void SendEndGame(GamePlayer p)
		{
			if (!p.m_inGame)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(p.m_id);
			list.Add((int)m_gameOutcome);
			list.Add(m_winnerTeam);
			list.Add(m_autoJoinNextGameID);
			list.Add(m_currentTurn);
			list.Add(m_players.Count);
			foreach (GamePlayer player in m_players)
			{
				list.Add(player.m_id);
				list.Add(player.m_team);
				list.Add(player.GetUser().m_name);
				list.Add(player.GetUser().m_flag);
				list.Add(player.m_place);
				list.Add(player.m_score);
				list.Add(player.m_teamScore);
				list.Add(player.m_flagshipKiller[0]);
				list.Add(player.m_flagshipKiller[1]);
			}
			p.GetUser().m_rpc.Invoke("EndGame", list);
		}

		private void SendLobbyToAll()
		{
			if (m_gameStarted)
			{
				return;
			}
			foreach (GamePlayer player in m_players)
			{
				SendLobby(player);
			}
		}

		private void SendLobby(GamePlayer p)
		{
			if (!p.m_inGame)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(p.GetUser().m_name);
			list.Add(m_publicGame);
			list.Add(m_mapInfo.m_noFleet);
			list.Add(m_players.Count);
			foreach (GamePlayer player in m_players)
			{
				list.Add(player.m_id);
				list.Add(player.m_team);
				list.Add(player.GetUser().m_name);
				list.Add(player.GetUser().m_flag);
				list.Add(player.m_admin);
				list.Add(player.m_readyToStart);
				list.Add((int)player.GetPlayerPresenceStatus());
				if (!m_mapInfo.m_noFleet)
				{
					if (player.m_fleet != null)
					{
						list.Add(player.m_fleet.m_name);
						list.Add(player.m_fleet.m_value);
						continue;
					}
					int num = -1;
					if (player.m_selectedFleetName != string.Empty)
					{
						FleetDef fleetDef = player.GetUser().GetFleetDef(player.m_selectedFleetName, m_campaignID);
						if (fleetDef != null)
						{
							num = fleetDef.m_value;
						}
					}
					if (num == -1)
					{
						player.m_selectedFleetName = string.Empty;
					}
					list.Add(player.m_selectedFleetName);
					list.Add(num);
				}
				else
				{
					list.Add(string.Empty);
					list.Add(-1);
				}
			}
			p.GetUser().m_rpc.Invoke("Lobby", list);
		}

		private void SendGameSettingsToAll()
		{
			foreach (GamePlayer player in m_players)
			{
				SendGameSettings(player);
			}
		}

		private void SendGameSettings(GamePlayer p)
		{
			if (p.m_inGame)
			{
				List<object> list = new List<object>();
				list.Add(p.m_id);
				list.Add(p.m_admin);
				list.Add(m_campaignID);
				list.Add(m_gameID);
				list.Add(m_gameName);
				list.Add((int)m_gameType);
				list.Add(m_campaignName);
				list.Add(m_levelName);
				list.Add((int)m_fleetSizeClass);
				list.Add(m_fleetSizeLimits.min);
				list.Add(m_fleetSizeLimits.max);
				list.Add(m_targetScore);
				list.Add(m_maxTurnTime);
				list.Add(m_nrOfPlayers);
				p.GetUser().m_rpc.Invoke("GameSettings", list);
			}
		}

		private void SendGameStartedToAll()
		{
			foreach (GamePlayer player in m_players)
			{
				SendGameStarted(player);
			}
		}

		private void SendGameStarted(GamePlayer p)
		{
			if (p.m_inGame)
			{
				p.GetUser().m_rpc.Invoke("GameStarted");
			}
		}

		public void SetAutoJoinNextGameID(int gameID)
		{
			m_autoJoinNextGameID = gameID;
		}

		public int GetNrOfPlayers()
		{
			return m_players.Count;
		}

		public int GetMaxPlayers()
		{
			return m_nrOfPlayers;
		}

		public string GetName()
		{
			return m_gameName;
		}

		public int GetGameID()
		{
			return m_gameID;
		}

		public int GetCampaignID()
		{
			return m_campaignID;
		}

		public string GetLevelName()
		{
			return m_levelName;
		}

		public string GetCampaign()
		{
			return m_campaignName;
		}

		public GameType GetGameType()
		{
			return m_gameType;
		}

		public float GetTargetScore()
		{
			return m_targetScore;
		}

		public FleetSizeClass GetFleetSizeClass()
		{
			return m_fleetSizeClass;
		}

		public FleetSize GetFleetSizeLimits()
		{
			return m_fleetSizeLimits;
		}

		public int GetTurn()
		{
			return m_currentTurn;
		}

		public bool IsPublicGame()
		{
			return m_publicGame;
		}

		public void SetPublicGame(bool enabled)
		{
			if (m_publicGame != enabled)
			{
				m_publicGame = enabled;
				SendLobbyToAll();
			}
		}

		public bool GameIsVisibleForPlayer(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				return false;
			}
			return !gamePlayerByUser.m_leftGame && !gamePlayerByUser.m_seenEndGame;
		}

		public int GetConnectedPlayers()
		{
			int num = 0;
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					num++;
				}
			}
			return num;
		}

		public bool IsUserInGame(User user)
		{
			return GetGamePlayerByUser(user) != null;
		}

		public float GetTimeSinceActive()
		{
			return m_timeSinceActive;
		}

		private TurnData GetCurrentTurnData()
		{
			if (m_currentTurn >= 0)
			{
				return m_turns[m_currentTurn];
			}
			return null;
		}

		private void RPC_SwitchTeam(RPC rpc, List<object> args)
		{
			if (m_gameStarted || (m_gameType != GameType.Points && m_gameType != GameType.Assassination))
			{
				return;
			}
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null)
			{
				gamePlayerByRPC.m_team = ((gamePlayerByRPC.m_team == 0) ? 1 : 0);
				gamePlayerByRPC.m_readyToStart = false;
				if ((m_gameType == GameType.Assassination || m_gameType == GameType.Points) && m_nrOfPlayers == 3)
				{
					UnreadyAllPlayers();
				}
				SendLobbyToAll();
			}
		}

		private void UnreadyAllPlayers()
		{
			foreach (GamePlayer player in m_players)
			{
				player.m_readyToStart = false;
			}
		}

		private void RPC_FleetUpdated(RPC rpc, List<object> args)
		{
			if (!m_gameStarted)
			{
				GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
				if (gamePlayerByRPC != null)
				{
					SendLobbyToAll();
				}
			}
		}

		private void RPC_SetFleet(RPC rpc, List<object> args)
		{
			if (!m_gameStarted && m_gameType != GameType.Campaign && m_gameType != GameType.Challenge)
			{
				GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
				if (gamePlayerByRPC != null)
				{
					string fleetName = args[0] as string;
					SetPlayerFleet(gamePlayerByRPC.GetUser().m_name, fleetName);
					SendLobbyToAll();
				}
			}
		}

		public void SetPlayerFleet(string player, string fleetName)
		{
			GamePlayer gamePlayerByName = GetGamePlayerByName(player);
			if (gamePlayerByName != null)
			{
				FleetDef fleetDef = gamePlayerByName.GetUser().GetFleetDef(fleetName, m_campaignID);
				if (fleetDef == null)
				{
					PLog.LogError("failed to find player " + gamePlayerByName.GetUser().m_name + " fleet " + fleetName);
					return;
				}
				gamePlayerByName.m_selectedFleetName = fleetName;
				gamePlayerByName.m_fleet = null;
				gamePlayerByName.m_readyToStart = false;
			}
		}

		public string GetPlayerFleet(string player)
		{
			return GetGamePlayerByName(player)?.m_selectedFleetName;
		}

		private void RPC_SeenEndGame(RPC rpc, List<object> args)
		{
			if (m_gameEnded)
			{
				GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
				if (gamePlayerByRPC != null)
				{
					gamePlayerByRPC.m_seenEndGame = true;
				}
			}
		}

		private void RPC_ReadyToStart(RPC rpc, List<object> args)
		{
			if (m_gameStarted)
			{
				return;
			}
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC == null)
			{
				return;
			}
			if (gamePlayerByRPC.m_readyToStart)
			{
				gamePlayerByRPC.m_readyToStart = false;
				gamePlayerByRPC.m_fleet = null;
			}
			else
			{
				if (!m_mapInfo.m_noFleet && gamePlayerByRPC.m_selectedFleetName == string.Empty)
				{
					return;
				}
				if (!m_mapInfo.m_noFleet)
				{
					FleetDef fleetDef = gamePlayerByRPC.GetUser().GetFleetDef(gamePlayerByRPC.m_selectedFleetName, m_campaignID);
					bool dubble = false;
					if ((m_gameType == GameType.Assassination || m_gameType == GameType.Points) && m_nrOfPlayers == 3 && GetPlayersInTeam(gamePlayerByRPC.m_team) == 1)
					{
						dubble = true;
					}
					if (!m_fleetSizeLimits.ValidSize(fleetDef.m_value, dubble))
					{
						return;
					}
					gamePlayerByRPC.m_fleet = fleetDef.Clone();
				}
				gamePlayerByRPC.m_readyToStart = true;
			}
			SendLobbyToAll();
		}

		private void RPC_KickSelf(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC == null || gamePlayerByRPC.m_admin)
			{
				return;
			}
			if (m_gameStarted)
			{
				if (gamePlayerByRPC.m_surrender)
				{
					gamePlayerByRPC.m_leftGame = true;
				}
			}
			else
			{
				string userName = gamePlayerByRPC.m_userName;
				KickUser(userName);
				SendLobbyToAll();
			}
		}

		private void RPC_KickPlayer(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null && gamePlayerByRPC.m_admin && !m_gameStarted)
			{
				string name = (string)args[0];
				KickUser(name);
				SendLobbyToAll();
			}
		}

		private void RPC_DisbandGame(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null && gamePlayerByRPC.m_admin)
			{
				m_disbanded = true;
				while (m_players.Count > 0)
				{
					KickUser(m_players[0].GetUser().m_name);
				}
			}
		}

		private void RPC_AllowMatchmaking(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null && gamePlayerByRPC.m_admin && !m_gameStarted)
			{
				bool publicGame = (bool)args[0];
				SetPublicGame(publicGame);
			}
		}

		private void RPC_RenameGame(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null && gamePlayerByRPC.m_admin)
			{
				m_gameName = (string)args[0];
				SendGameSettingsToAll();
				SendLobbyToAll();
			}
		}

		private void RPC_Invite(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			int arg = (int)args[0];
			if (m_onInviteFriend != null)
			{
				m_onInviteFriend(this, gamePlayerByRPC.GetUser(), arg);
			}
		}

		private bool IsReadyToStart()
		{
			if (m_players.Count < m_nrOfPlayers)
			{
				return false;
			}
			foreach (GamePlayer player in m_players)
			{
				if (!player.m_readyToStart)
				{
					return false;
				}
				if (!m_mapInfo.m_noFleet && player.m_selectedFleetName == string.Empty)
				{
					return false;
				}
			}
			if (m_gameType == GameType.Points || m_gameType == GameType.Assassination)
			{
				if (m_nrOfPlayers != 2 && m_nrOfPlayers != 4 && m_nrOfPlayers != 3)
				{
					return false;
				}
				int playersInTeam = GetPlayersInTeam(0);
				int playersInTeam2 = GetPlayersInTeam(1);
				if (m_nrOfPlayers == 2)
				{
					return playersInTeam == 1 && playersInTeam2 == 1;
				}
				if (m_nrOfPlayers == 3)
				{
					return (playersInTeam == 2 && playersInTeam2 == 1) || (playersInTeam == 1 && playersInTeam2 == 2);
				}
				if (m_nrOfPlayers == 4)
				{
					return playersInTeam == 2 && playersInTeam2 == 2;
				}
				return false;
			}
			return true;
		}

		private int GetPlayersInTeam(int team)
		{
			int num = 0;
			foreach (GamePlayer player in m_players)
			{
				if (player.m_team == team)
				{
					num++;
				}
			}
			return num;
		}

		private void KickUser(string name)
		{
			GamePlayer gamePlayerByName = GetGamePlayerByName(name);
			if (gamePlayerByName != null)
			{
				if (gamePlayerByName.m_inGame)
				{
					OnUserDisconnected(gamePlayerByName.GetUser());
					gamePlayerByName.GetUser().m_rpc.Invoke("Kicked");
				}
				m_players.Remove(gamePlayerByName);
				gamePlayerByName.GetUser().m_inGames--;
				if (m_campaignID > 0)
				{
					gamePlayerByName.GetUser().ClearCampaign(m_campaignID);
				}
			}
		}

		private void RPC_LeaveGame(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (gamePlayerByRPC != null)
			{
				float num = (float)args[0];
				AddUserPlanningTime(gamePlayerByRPC, num);
				OnUserDisconnected(gamePlayerByRPC.GetUser());
			}
		}

		private void AddUserPlanningTime(GamePlayer player, double time)
		{
			player.GetUser().m_stats.m_totalPlanningTime += (long)time;
		}

		public void OnUserConnectedToServer(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser != null)
			{
				SendPlayerStatusToAll();
			}
		}

		public void OnUserDisconnected(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				return;
			}
			if (gamePlayerByUser.m_inGame)
			{
				m_chatServer.Unregister(gamePlayerByUser);
				gamePlayerByUser.m_inGame = false;
				if (user.m_rpc != null)
				{
					user.m_rpc.Unregister("Commit");
					user.m_rpc.Unregister("LeaveGame");
					user.m_rpc.Unregister("KickSelf");
					user.m_rpc.Unregister("InitialState");
					user.m_rpc.Unregister("SimulationResults");
					user.m_rpc.Unregister("Chat");
					user.m_rpc.Unregister("SetFleet");
					user.m_rpc.Unregister("KickPlayer");
					user.m_rpc.Unregister("SeenEndGame");
					user.m_rpc.Unregister("RequestReplayTurn");
					user.m_rpc.Unregister("RenameGame");
					user.m_rpc.Unregister("AllowMatchmaking");
				}
				if (m_sentInitiateRequestTo == gamePlayerByUser)
				{
					PLog.LogWarning("Player " + gamePlayerByUser.m_userName + " disconnected, reseting init request from that player");
					m_sentInitiateRequestTo = null;
				}
			}
			SendPlayerStatusToAll();
			SendLobbyToAll();
		}

		public bool HasStarted()
		{
			return m_gameStarted;
		}

		public double GetMaxTurnTime()
		{
			return m_maxTurnTime;
		}

		public bool IsFinished()
		{
			if (m_players.Count == 0)
			{
				return true;
			}
			if (m_disbanded)
			{
				return true;
			}
			if (!m_gameEnded)
			{
				return false;
			}
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					return false;
				}
				if (!player.m_leftGame && !player.m_seenEndGame)
				{
					return false;
				}
			}
			return true;
		}

		public GameOutcome GetOutcome()
		{
			return m_gameOutcome;
		}

		private void SendCurrentTurn(GamePlayer player, bool startReplay)
		{
			TurnData currentTurnData = GetCurrentTurnData();
			bool flag = currentTurnData.Commited(player.m_id);
			bool flag2 = !flag && !player.m_dead && !m_gameEnded;
			byte[] item = ((!flag) ? new byte[0] : currentTurnData.m_newOrders[player.m_id]);
			List<object> list = new List<object>();
			list.Add(m_currentTurn);
			list.Add((int)currentTurnData.GetTurnType());
			list.Add(flag2);
			list.Add(GetCurrentTurnDuration());
			list.Add(item);
			list.Add(currentTurnData.m_startState);
			list.Add(currentTurnData.m_startOrders);
			list.Add(currentTurnData.m_endState);
			list.Add(currentTurnData.m_endOrders);
			list.Add(currentTurnData.GetPlaybackFrames());
			list.Add(currentTurnData.GetFrames());
			list.Add(currentTurnData.m_startSurrenders);
			list.Add(startReplay);
			player.GetUser().m_rpc.Invoke("TurnData", list);
			if (m_gameEnded && !m_replayMode)
			{
				SendEndGame(player);
			}
		}

		private void SendCurrentTurnToAll(bool includeReplay)
		{
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					SendCurrentTurn(player, includeReplay);
				}
			}
		}

		private bool SendInitRequest()
		{
			GamePlayer firstConnectedPlayer = GetFirstConnectedPlayer();
			if (firstConnectedPlayer != null)
			{
				RequestInitialState(firstConnectedPlayer);
				return true;
			}
			return false;
		}

		private void RequestInitialState(GamePlayer player)
		{
			List<object> list = new List<object>();
			foreach (GamePlayer player2 in m_players)
			{
				if (!m_mapInfo.m_noFleet)
				{
					list.Add(true);
					list.Add(player2.m_fleet.ToArray());
				}
				else
				{
					list.Add(false);
				}
				list.Add(player2.m_id);
				list.Add(player2.m_userName);
				list.Add(player2.m_team);
				list.Add(player2.GetUser().m_flag);
			}
			player.GetUser().m_rpc.Invoke("DoInitiation", list);
			player.GetUser().m_rpc.Register("InitialState", RPC_InitialState);
			m_sentInitiateRequestTo = player;
		}

		private void RPC_InitialState(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			if (m_sentInitiateRequestTo == gamePlayerByRPC)
			{
				byte[] startState = (byte[])args[0];
				byte[] startOrders = (byte[])args[1];
				byte[] endState = (byte[])args[2];
				byte[] endOrders = (byte[])args[3];
				int playbackFrames = (int)args[4];
				int[] startSurrenders = new int[0];
				SetupNewTurn(startState, startOrders, startSurrenders, endState, endOrders, TurnType.Normal, playbackFrames, 300);
				rpc.Unregister("InitialState");
				m_gotInitState = true;
				m_sentInitiateRequestTo = null;
				SendCurrentTurnToAll(includeReplay: true);
				SendNewTurnNotifications();
			}
		}

		private void SetupNewTurn(byte[] startState, byte[] startOrders, int[] startSurrenders, byte[] endState, byte[] endOrders, TurnType type, int playbackFrames, int frames)
		{
			m_currentTurn = m_turns.Count;
			TurnData turnData = new TurnData(m_currentTurn, m_nrOfPlayers, playbackFrames, frames, type);
			turnData.m_startState = startState;
			turnData.m_startOrders = startOrders;
			turnData.m_startSurrenders = startSurrenders;
			turnData.m_endState = endState;
			turnData.m_endOrders = endOrders;
			m_turns.Add(turnData);
			m_turnStartDate = DateTime.Now;
		}

		private double GetCurrentTurnDuration()
		{
			return DateTime.Now.Subtract(m_turnStartDate).TotalSeconds;
		}

		private void UpdateTimeSync(float dt)
		{
			if (m_maxTurnTime <= 0.0)
			{
				return;
			}
			m_timeSyncTimer += dt;
			if (!(m_timeSyncTimer >= m_timeSyncSendDelay))
			{
				return;
			}
			m_timeSyncTimer = 0f;
			if (m_gameEnded || !m_gameStarted || !m_gotInitState)
			{
				return;
			}
			TurnData currentTurnData = GetCurrentTurnData();
			if (currentTurnData == null)
			{
				return;
			}
			double currentTurnDuration = GetCurrentTurnDuration();
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					player.GetUser().m_rpc.Invoke("TimeSync", currentTurnDuration);
				}
			}
		}

		private void UpdateAutoCommit()
		{
			if (m_maxTurnTime <= 0.0 || m_gameEnded || !m_gameStarted || !m_gotInitState)
			{
				return;
			}
			TurnData currentTurnData = GetCurrentTurnData();
			if (currentTurnData == null || AllCommited(currentTurnData))
			{
				return;
			}
			double currentTurnDuration = GetCurrentTurnDuration();
			if (!(currentTurnDuration > m_maxTurnTime))
			{
				return;
			}
			foreach (GamePlayer player in m_players)
			{
				if (!currentTurnData.Commited(player.m_id) && (!player.m_inGame || currentTurnDuration > m_maxTurnTime + (double)m_autoCommitTimeout))
				{
					byte[] orders = new byte[0];
					if (Commit(player, m_currentTurn, surrender: false, m_maxTurnTime, orders))
					{
						break;
					}
				}
			}
		}

		private void RPC_Commit(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			int turn = (int)args[0];
			bool surrender = (bool)args[1];
			float num = (float)args[2];
			byte[] orders = (byte[])args[3];
			Commit(gamePlayerByRPC, turn, surrender, num, orders);
		}

		private bool Commit(GamePlayer gplayer, int turn, bool surrender, double planningTime, byte[] orders)
		{
			if (m_gameEnded)
			{
				return false;
			}
			if (gplayer.m_surrender || gplayer.m_dead)
			{
				return false;
			}
			if (turn != m_currentTurn)
			{
				PLog.LogError("ERROR: player commited turn " + turn + " but current turn is " + m_currentTurn);
				return false;
			}
			TurnData currentTurnData = GetCurrentTurnData();
			if (currentTurnData == null || currentTurnData.GetTurnType() != 0)
			{
				return false;
			}
			if (currentTurnData.Commited(gplayer.m_id))
			{
				PLog.LogError("ERROR: player " + gplayer.GetUser().m_name + " already commited turn " + turn);
				return false;
			}
			AddUserPlanningTime(gplayer, planningTime);
			gplayer.m_surrender = surrender;
			if (surrender)
			{
				currentTurnData.SetSurrender(gplayer.m_id);
			}
			currentTurnData.m_newOrders[gplayer.m_id] = orders;
			SendPlayerStatusToAll();
			if (AllCommited(currentTurnData))
			{
				SendSimulationRequestToAll(currentTurnData);
				return true;
			}
			return false;
		}

		private bool AllCommited(TurnData turn)
		{
			foreach (GamePlayer player in m_players)
			{
				if (!PlayerCommited(turn, player))
				{
					return false;
				}
			}
			return true;
		}

		private bool PlayerCommited(TurnData turn, GamePlayer p)
		{
			return p.m_dead || p.m_leftGame || turn.Commited(p.m_id);
		}

		public bool NeedPlayerAttention(User user)
		{
			GamePlayer gamePlayerByUser = GetGamePlayerByUser(user);
			if (gamePlayerByUser == null)
			{
				return false;
			}
			return NeedPlayerAttention(gamePlayerByUser);
		}

		private bool NeedPlayerAttention(GamePlayer player)
		{
			if (player.m_leftGame)
			{
				return false;
			}
			if (m_gameEnded && !player.m_seenEndGame)
			{
				return true;
			}
			if (!m_gameStarted && !player.m_readyToStart)
			{
				return true;
			}
			TurnData currentTurnData = GetCurrentTurnData();
			if (currentTurnData != null && currentTurnData.GetTurnType() == TurnType.Normal)
			{
				if (!PlayerCommited(currentTurnData, player))
				{
					return true;
				}
				if (AllCommited(currentTurnData))
				{
					return true;
				}
			}
			return false;
		}

		public DateTime GetCreateDate()
		{
			return m_createDate;
		}

		public void SetCreateDate(DateTime date)
		{
			m_createDate = date;
		}

		private void SendSimulationRequestToAll(TurnData turn)
		{
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					RequestSimulation(player, turn);
				}
			}
		}

		private void RequestSimulation(GamePlayer gplayer, TurnData turn)
		{
			List<object> list = new List<object>();
			list.Add(turn.GetTurn());
			list.Add(turn.GetFrames());
			list.Add(turn.m_endState);
			list.Add(turn.m_endOrders);
			list.Add(turn.m_newSurrenders.ToArray());
			list.Add(turn.m_newOrders.Count);
			foreach (byte[] newOrder in turn.m_newOrders)
			{
				bool flag = newOrder != null && newOrder.Length != 0;
				list.Add(flag);
				if (flag)
				{
					list.Add(newOrder);
				}
			}
			gplayer.GetUser().m_rpc.Invoke("DoSimulation", list);
		}

		private void RPC_SimulationResults(RPC rpc, List<object> args)
		{
			GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
			int num = 0;
			int num2 = (int)args[num++];
			if (num2 == m_currentTurn)
			{
				GameOutcome gameOutcome = (GameOutcome)(int)args[num++];
				int winnerTeam = (int)args[num++];
				int num3 = (int)args[num++];
				for (int i = 0; i < num3; i++)
				{
					int id = (int)args[num++];
					int place = (int)args[num++];
					int score = (int)args[num++];
					int teamScore = (int)args[num++];
					int num4 = (int)args[num++];
					int num5 = (int)args[num++];
					bool dead = (bool)args[num++];
					int num6 = (int)args[num++];
					int num7 = (int)args[num++];
					int num8 = (int)args[num++];
					Dictionary<string, int> damages = (Dictionary<string, int>)args[num++];
					GamePlayer gamePlayerByID = GetGamePlayerByID(id);
					if (gamePlayerByID == null)
					{
						PLog.LogError("got invalid rank player");
						return;
					}
					gamePlayerByID.m_place = place;
					gamePlayerByID.m_score = score;
					gamePlayerByID.m_teamScore = teamScore;
					gamePlayerByID.m_flagshipKiller[0] = num4;
					gamePlayerByID.m_flagshipKiller[1] = num5;
					gamePlayerByID.m_dead = dead;
					if (m_gameType == GameType.Points || m_gameType == GameType.Assassination)
					{
						gamePlayerByID.GetUser().m_stats.m_vsTotalDamage += num6;
						gamePlayerByID.GetUser().m_stats.m_vsTotalFriendlyDamage += num7;
						gamePlayerByID.GetUser().m_stats.m_vsShipsSunk += num8;
						gamePlayerByID.GetUser().m_stats.AddModuleDamages(damages);
					}
				}
				byte[] array = args[num++] as byte[];
				byte[] array2 = args[num++] as byte[];
				byte[] array3 = args[num++] as byte[];
				byte[] array4 = args[num++] as byte[];
				int[] array5 = args[num++] as int[];
				int playbackFrames = (int)args[num++];
				if (array == null || array2 == null || array3 == null || array4 == null || array5 == null)
				{
					PLog.LogError("Got null data from client, BAD CLIENT ");
					return;
				}
				TurnType type = TurnType.Normal;
				if (gameOutcome != 0)
				{
					m_gameEnded = true;
					m_gameOutcome = gameOutcome;
					m_winnerTeam = winnerTeam;
					type = TurnType.EndGame;
					AddUserStats();
					if (m_onGameOver != null)
					{
						m_onGameOver(this);
					}
				}
				SetupNewTurn(array, array2, array5, array3, array4, type, playbackFrames, 300);
				SendNewTurnNotifications();
			}
			SendCurrentTurn(gamePlayerByRPC, startReplay: false);
			SendPlayerStatusToAll();
		}

		private void AddUserStats()
		{
			if (m_gameType != GameType.Points && m_gameType != GameType.Assassination)
			{
				return;
			}
			foreach (GamePlayer player in m_players)
			{
				if (m_winnerTeam != -1)
				{
					if (player.m_team == m_winnerTeam)
					{
						player.GetUser().m_stats.m_vsGamesWon++;
						if (m_gameType == GameType.Points)
						{
							player.GetUser().m_stats.m_vsPointsWon++;
						}
						if (m_gameType == GameType.Assassination)
						{
							player.GetUser().m_stats.m_vsAssWon++;
						}
					}
					else
					{
						player.GetUser().m_stats.m_vsGamesLost++;
						if (m_gameType == GameType.Points)
						{
							player.GetUser().m_stats.m_vsPointsLost++;
						}
						if (m_gameType == GameType.Assassination)
						{
							player.GetUser().m_stats.m_vsAssLost++;
						}
					}
				}
				UpdateModuleAndshipUsageStats(player);
			}
		}

		private GamePlayer GetFirstConnectedPlayer()
		{
			foreach (GamePlayer player in m_players)
			{
				if (player.m_inGame)
				{
					return player;
				}
			}
			return null;
		}

		private GamePlayer GetGamePlayerByID(int id)
		{
			foreach (GamePlayer player in m_players)
			{
				if (player.m_id == id)
				{
					return player;
				}
			}
			return null;
		}

		private GamePlayer GetGamePlayerByName(string name)
		{
			foreach (GamePlayer player in m_players)
			{
				if (player.m_userName == name)
				{
					return player;
				}
			}
			return null;
		}

		private GamePlayer GetGamePlayerByRPC(RPC rpc)
		{
			foreach (GamePlayer player in m_players)
			{
				User user = player.GetUser();
				if (user != null && user.m_rpc == rpc)
				{
					return player;
				}
			}
			return null;
		}

		private GamePlayer GetGamePlayerByUser(User user)
		{
			foreach (GamePlayer player in m_players)
			{
				if (user == player.GetUser())
				{
					return player;
				}
			}
			return null;
		}

		private void SendPlayerStatusToAll()
		{
			foreach (GamePlayer player in m_players)
			{
				SendPlayerStatusTo(player);
			}
		}

		private PlayerTurnStatus GetPlayerTurnStatus(GamePlayer p)
		{
			if (m_currentTurn >= 0)
			{
				if (p.m_dead)
				{
					return PlayerTurnStatus.Dead;
				}
				if (m_turns[m_currentTurn].Commited(p.m_id))
				{
					return PlayerTurnStatus.Done;
				}
				return PlayerTurnStatus.Planning;
			}
			return PlayerTurnStatus.Planning;
		}

		private void SendPlayerStatusTo(GamePlayer player)
		{
			if (!player.m_inGame)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(m_players.Count);
			foreach (GamePlayer player2 in m_players)
			{
				list.Add(player2.m_id);
				list.Add(player2.m_surrender);
				list.Add(player2.m_leftGame);
				list.Add((int)player2.GetPlayerPresenceStatus());
				list.Add((int)GetPlayerTurnStatus(player2));
			}
			player.GetUser().m_rpc.Invoke("PlayerStatus", list);
		}

		public void SaveData(BinaryWriter stream)
		{
			stream.Write(2);
			stream.Write(m_publicGame);
			stream.Write(m_autoJoinNextGameID);
			stream.Write(m_isNewCampaign);
			stream.Write(m_createDate.ToBinary());
			stream.Write(m_turnStartDate.ToBinary());
			stream.Write(m_gotInitState);
			stream.Write(m_gameStarted);
			stream.Write(m_gameEnded);
			stream.Write(m_disbanded);
			stream.Write(m_currentTurn);
			stream.Write(m_winnerTeam);
			stream.Write((int)m_gameOutcome);
			stream.Write(m_players.Count);
			foreach (GamePlayer player in m_players)
			{
				stream.Write(player.m_id);
				player.Save(stream);
			}
			stream.Write(m_turns.Count);
			foreach (TurnData turn in m_turns)
			{
				turn.Save(stream);
			}
			m_chatServer.Save(stream);
		}

		public void LoadData(BinaryReader stream)
		{
			int num = stream.ReadInt32();
			if (num >= 2)
			{
				m_publicGame = stream.ReadBoolean();
			}
			m_autoJoinNextGameID = stream.ReadInt32();
			m_isNewCampaign = stream.ReadBoolean();
			m_createDate = DateTime.FromBinary(stream.ReadInt64());
			m_turnStartDate = DateTime.FromBinary(stream.ReadInt64());
			m_gotInitState = stream.ReadBoolean();
			m_gameStarted = stream.ReadBoolean();
			m_gameEnded = stream.ReadBoolean();
			m_disbanded = stream.ReadBoolean();
			m_currentTurn = stream.ReadInt32();
			m_winnerTeam = stream.ReadInt32();
			m_gameOutcome = (GameOutcome)stream.ReadInt32();
			int num2 = stream.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int id = stream.ReadInt32();
				GamePlayer gamePlayer = new GamePlayer(id);
				gamePlayer.Load(stream);
				m_players.Add(gamePlayer);
			}
			int num3 = stream.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				TurnData turnData = new TurnData();
				turnData.Load(stream);
				m_turns.Add(turnData);
			}
			m_chatServer.Load(stream);
		}

		private void SendNewTurnNotifications()
		{
			foreach (GamePlayer player in m_players)
			{
				if (!player.m_inGame && !player.m_leftGame && m_onNewTurnNotification != null)
				{
					m_onNewTurnNotification(player.GetUser(), this);
				}
			}
		}

		private void RPC_RequestReplayTurn(RPC rpc, List<object> args)
		{
			int num = (int)args[0];
			if (num >= 0 && num < m_turns.Count)
			{
				m_currentTurn = num;
				GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
				SendCurrentTurn(gamePlayerByRPC, startReplay: true);
			}
		}

		private void RPC_SaveReplay(RPC rpc, List<object> args)
		{
			if (m_gameOutcome != 0)
			{
				string name = (string)args[0];
				GamePlayer gamePlayerByRPC = GetGamePlayerByRPC(rpc);
				if (m_onSaveReplay != null)
				{
					bool flag = m_onSaveReplay(this, gamePlayerByRPC.GetUser(), name);
					rpc.Invoke("SaveReplayReply", flag);
				}
			}
		}
	}
}
