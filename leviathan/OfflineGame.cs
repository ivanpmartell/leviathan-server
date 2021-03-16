#define DEBUG
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using PTech;
using UnityEngine;

internal class OfflineGame : ServerBase
{
	private enum JoinState
	{
		None,
		CloseGame,
		LoadMenuLevel,
		JoinGame
	}

	public Action<ExitState> m_onExit;

	public Action m_onQuitGame;

	public Action m_onSaveUserRequest;

	private PTech.RPC m_serverRpc;

	private PTech.RPC m_clientRpc;

	private List<Game> m_serverGames = new List<Game>();

	private ClientGame m_game;

	private GameObject m_guiCamera;

	private UserManClient m_userManClient;

	private MusicManager m_musicMan;

	private PackMan m_pacMan;

	private OfflineGameDB m_gameDB;

	private User m_user;

	private int m_joinGame;

	private JoinState m_joinTransitionState;

	private bool m_replayMode;

	public OfflineGame(string overrideGameName, byte[] data, bool replay, User user, UserManClient userManClient, MapMan mapman, PackMan pacMan, GameObject guiCamera, MusicManager musMan, OfflineGameDB gameDB)
		: base(mapman)
	{
		m_userManClient = userManClient;
		m_musicMan = musMan;
		m_pacMan = pacMan;
		m_guiCamera = guiCamera;
		m_user = user;
		m_gameDB = gameDB;
		m_replayMode = replay;
		SetupSockets();
		Game game = CreateGameFromArray(data, overrideGameName);
		List<string> userNames = game.GetUserNames();
		game.InternalSetUser(userNames[0], m_user);
		if (replay)
		{
			game.SetupReplayMode();
		}
		JoinGame(game, replay);
	}

	public OfflineGame(string campaign, string levelName, string gameName, GameType gameMode, User user, UserManClient userManClient, MapMan mapman, PackMan pacMan, GameObject guiCamera, MusicManager musMan, int nrOfPlayers, FleetSizeClass fleetSize, float targetScore, OfflineGameDB gameDB)
		: base(mapman)
	{
		m_userManClient = userManClient;
		m_musicMan = musMan;
		m_pacMan = pacMan;
		m_guiCamera = guiCamera;
		m_user = user;
		m_gameDB = gameDB;
		SetupSockets();
		MapInfo mapByName = mapman.GetMapByName(gameMode, campaign, levelName);
		DebugUtils.Assert(mapByName != null, "map info not found for " + campaign + " " + levelName);
		Game game = CreateGame(gameName, 0, 0, gameMode, campaign, levelName, 1, fleetSize, targetScore, -1.0, mapByName);
		game.AddUserToGame(user, admin: true);
		JoinGame(game, replayMode: false);
	}

	private void SetupSockets()
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		socket.NoDelay = true;
		int num = FindValidPort(ref socket, 12345);
		DebugUtils.Assert(num > 0);
		PLog.Log("Started server on port " + num);
		socket.Listen(100);
		socket.Blocking = false;
		Socket socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		socket2.Connect("localhost", num);
		socket2.Blocking = false;
		socket2.NoDelay = true;
		Socket socket3 = null;
		while (socket3 == null)
		{
			try
			{
				socket3 = socket.Accept();
				socket3.Blocking = false;
			}
			catch (SocketException)
			{
			}
		}
		PacketSocket socket4 = new PacketSocket(socket3);
		m_serverRpc = new PTech.RPC(socket4);
		PacketSocket socket5 = new PacketSocket(socket2);
		m_clientRpc = new PTech.RPC(socket5);
		m_user.Connect(m_serverRpc, Utils.GetPlatform());
	}

	private void JoinGame(Game game, bool replayMode)
	{
		if (m_game != null)
		{
			m_game.Close();
		}
		m_game = new ClientGame(m_clientRpc, m_guiCamera, m_userManClient, m_mapMan, m_musicMan, replayMode, onlineGame: false, string.Empty);
		m_game.m_onExit = OnExit;
		m_game.m_onQuitGame = OnQuitGame;
		game.JoinGame(m_user);
	}

	protected override Game CreateGame(string gameName, int gameID, int campaignID, GameType gameType, string campaignName, string levelName, int nrOfPlayers, FleetSizeClass fleetSizeClass, float targetScore, double turnTime, MapInfo mapinfo)
	{
		Game game = new Game(gameName, gameID, campaignID, gameType, campaignName, levelName, nrOfPlayers, fleetSizeClass, targetScore, turnTime, mapinfo, m_pacMan);
		m_serverGames.Add(game);
		game.m_onGameOver = base.OnGameOver;
		game.m_onSaveReplay = OnSaveReplay;
		if (m_onSaveUserRequest != null)
		{
			m_onSaveUserRequest();
		}
		return game;
	}

	private int FindValidPort(ref Socket socket, int startPort)
	{
		//Discarded unreachable code: IL_002a
		for (int i = 0; i < 1000; i++)
		{
			int num = startPort + i;
			IPEndPoint local_end = new IPEndPoint(IPAddress.Any, num);
			try
			{
				socket.Bind(local_end);
				return num;
			}
			catch (SocketException)
			{
			}
		}
		return -1;
	}

	public void OnLevelWasLoaded()
	{
		if (m_game != null)
		{
			m_game.OnLevelWasLoaded();
		}
		if (m_joinTransitionState == JoinState.LoadMenuLevel)
		{
			m_joinTransitionState = JoinState.JoinGame;
		}
	}

	public void FixedUpdate()
	{
		if (m_game != null)
		{
			m_game.FixedUpdate();
		}
	}

	public void Update(float dt)
	{
		ServerUpdate(dt);
		ClientUpdates(dt);
	}

	private void ClientUpdates(float dt)
	{
		m_clientRpc.Update(recvAll: false);
		if (m_game != null)
		{
			m_game.Update();
		}
		if (m_joinTransitionState == JoinState.JoinGame && m_joinGame != 0)
		{
			Game serverGame = GetServerGame(m_joinGame);
			JoinGame(serverGame, replayMode: false);
			m_joinGame = 0;
			m_joinTransitionState = JoinState.None;
		}
		if (m_joinTransitionState == JoinState.CloseGame)
		{
			m_joinTransitionState = JoinState.LoadMenuLevel;
			m_game.Close();
			m_game = null;
			main.LoadLevel("menu", showLoadScreen: true);
		}
	}

	private void ServerUpdate(float dt)
	{
		m_serverRpc.Update(recvAll: true);
		foreach (Game serverGame in m_serverGames)
		{
			serverGame.Update(dt);
		}
		RemoveOldGames();
	}

	private void RemoveOldGames()
	{
		foreach (Game serverGame in m_serverGames)
		{
			if (serverGame.IsFinished())
			{
				if (m_gameDB != null)
				{
					m_gameDB.RemoveGame(serverGame.GetGameID());
				}
				m_serverGames.Remove(serverGame);
				break;
			}
		}
	}

	public void LateUpdate()
	{
		if (m_game != null)
		{
			m_game.LateUpdate();
		}
	}

	public void Close()
	{
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		ServerUpdate(0.1f);
		SaveGames();
		foreach (Game serverGame in m_serverGames)
		{
			serverGame.Close();
		}
		m_serverGames.Clear();
		m_serverRpc.Close();
		m_clientRpc.Close();
	}

	private void OnExit(ExitState exitState, int joinGameID)
	{
		if (exitState == ExitState.JoinGame)
		{
			DebugUtils.Assert(joinGameID != 0);
			m_joinGame = joinGameID;
			m_joinTransitionState = JoinState.CloseGame;
		}
		else
		{
			m_onExit(exitState);
		}
	}

	private void OnQuitGame()
	{
		if (m_onQuitGame != null)
		{
			m_onQuitGame();
		}
	}

	private bool OnSaveReplay(Game game, User user, string replayName)
	{
		return SaveGame(game, replay: true, replayName);
	}

	private Game GetServerGame(int gameID)
	{
		foreach (Game serverGame in m_serverGames)
		{
			if (serverGame.GetGameID() == gameID)
			{
				return serverGame;
			}
		}
		return null;
	}

	private void SaveGames()
	{
		foreach (Game serverGame in m_serverGames)
		{
			if (!serverGame.IsFinished())
			{
				SaveGame(serverGame, replay: false, string.Empty);
			}
		}
	}

	private bool SaveGame(Game game, bool replay, string replayName)
	{
		if (m_gameDB == null)
		{
			return false;
		}
		GamePost gamePost = new GamePost();
		gamePost.m_campaign = game.GetCampaign();
		gamePost.m_connectedPlayers = 1;
		gamePost.m_createDate = game.GetCreateDate();
		gamePost.m_fleetSizeClass = game.GetFleetSizeClass();
		gamePost.m_gameID = game.GetGameID();
		gamePost.m_gameName = ((!replay) ? game.GetName() : replayName);
		gamePost.m_gameType = game.GetGameType();
		gamePost.m_level = game.GetLevelName();
		gamePost.m_maxPlayers = 1;
		gamePost.m_nrOfPlayers = 1;
		gamePost.m_turn = game.GetTurn();
		byte[] gameData = GameToArray(game);
		return m_gameDB.SaveGame(gamePost, gameData, replay);
	}
}
