#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class ClientGame
{
	private class InitializeData
	{
		public List<PlayerInitData> m_players = new List<PlayerInitData>();

		public byte[] m_startState;

		public byte[] m_StartOrders;
	}

	private class SimulationData
	{
		public int m_simulationTurn;

		public int m_frames;

		public byte[] m_startState;

		public byte[] m_combinedStartOrders;

		public List<byte[]> m_playerOrders;

		public byte[] m_combinedOrders;

		public int[] m_surrenders;
	}

	private class TurnData
	{
		public int m_turn = -1;

		public bool m_needCommit;

		public bool m_localSurrender;

		public byte[] m_startState;

		public byte[] m_orders;

		public byte[] m_endState;

		public byte[] m_endOrders;

		public byte[] m_myOrders;

		public int[] m_surrenders;

		public bool m_waitingForLoad;

		public int m_playbackFrames;

		public int m_frames;

		public TurnType m_turnType;

		public byte[] m_tempOrders;

		public float m_planningTime;

		public double m_turnDuration;
	}

	private enum State
	{
		None,
		Simulating,
		Initializing,
		Planning,
		Outcome,
		Test,
		Waiting
	}

	public Action<ExitState, int> m_onExit;

	public Action m_onQuitGame;

	private GameObject m_guiCamera;

	private PTech.RPC m_rpc;

	private Hud m_hud;

	private Dialog m_dialog;

	private UserManClient m_userManClient;

	private MapMan m_mapMan;

	private HitText m_hitText;

	private LosDrawer m_losDrawer;

	private LobbyMenu m_lobbyMenu;

	private EndGameMenu m_endGameMenu;

	private LoadScreen m_loadScreen;

	private MusicManager m_musMan;

	private TurnMan m_turnMan;

	private ChatClient m_chatClient;

	private VOSystem m_voSystem;

	private GameState m_gameState;

	private GameSettings m_gameSettings;

	private InitializeData m_initializeData;

	private TurnData m_turnData;

	private SimulationData m_simData;

	private EndGameData m_endGameData;

	private GameObject m_endGameMsg;

	private GameObject m_countdownPrefab;

	private GameObject m_autoCommitPrefab;

	private bool m_firstLook = true;

	private int m_focusCamera = -1;

	private bool m_replayMode;

	private bool m_isOutcomeReplay;

	private bool m_onlineGame = true;

	private string m_hostName = string.Empty;

	private State m_state;

	private List<ClientPlayer> m_players = new List<ClientPlayer>();

	private static ClientGame m_instance;

	public static ClientGame instance => m_instance;

	public ClientGame(PTech.RPC rpc, GameObject guiCamera, UserManClient userManClient, MapMan mapman, MusicManager musMan, bool replayMode, bool onlineGame, string hostName)
	{
		m_instance = this;
		m_userManClient = userManClient;
		m_mapMan = mapman;
		m_guiCamera = guiCamera;
		m_rpc = rpc;
		m_musMan = musMan;
		m_replayMode = replayMode;
		m_onlineGame = onlineGame;
		m_hostName = hostName;
		m_turnMan = new TurnMan();
		m_chatClient = new ChatClient(rpc);
		m_loadScreen = new LoadScreen(m_guiCamera);
		m_hitText = new HitText(guiCamera);
		m_losDrawer = new LosDrawer();
		m_voSystem = new VOSystem();
		m_rpc.Register("GameSettings", RPC_GameSettings);
		m_rpc.Register("DoInitiation", RPC_DoInitiation);
		m_rpc.Register("DoSimulation", RPC_DoSimulation);
		m_rpc.Register("TurnData", RPC_TurnData);
		m_rpc.Register("EndGame", RPC_EndGame);
		m_rpc.Register("PlayerStatus", RPC_PlayerStatus);
		m_rpc.Register("Lobby", RPC_Lobby);
		m_rpc.Register("GameStarted", RPC_GameStarted);
		m_rpc.Register("Kicked", RPC_Kicked);
		m_rpc.Register("TimeSync", RPC_TimeSync);
		m_hud = new Hud(m_rpc, m_guiCamera, m_turnMan, m_chatClient, replayMode, m_onlineGame);
		m_hud.m_onCommit = OnHudCommit;
		m_hud.m_onPlayPause = OnPlayPause;
		m_hud.m_onStartTest = StartTest;
		m_hud.m_onStopTest = StopTest;
		m_hud.m_onStopOutcome = StopOutcome;
		m_hud.m_onNextTurn = OnNextReplayTurn;
		m_hud.m_onPrevTurn = OnPrevReplayTurn;
		m_hud.m_onSurrender = OnSurrender;
		m_hud.m_onExit = OnHudExit;
		m_hud.m_onQuitGame = OnHudQuitGame;
		m_hud.SetMode(Hud.Mode.Waiting);
		m_dialog = new Dialog(m_rpc, m_guiCamera, m_hud, m_turnMan);
		m_dialog.m_onPlayDialog = OnPlayDialog;
		m_dialog.m_onEndDialog = OnEndDialog;
		ObjectFactory.ResetInstance();
		ShipFactory.ResetInstance();
		ShipFactory.instance.RegisterShips("shared_settings/ship_defs/default_enemy_ships");
		m_countdownPrefab = Resources.Load("DownCountBlipp") as GameObject;
		m_autoCommitPrefab = Resources.Load("AutoCommitSound") as GameObject;
	}

	public void Close()
	{
		m_instance = null;
		float num = ((m_state != State.Planning || m_turnData == null) ? 0f : m_turnData.m_planningTime);
		m_rpc.Invoke("LeaveGame", num);
		m_rpc.Unregister("GameSettings");
		m_rpc.Unregister("DoInitiation");
		m_rpc.Unregister("DoSimulation");
		m_rpc.Unregister("TurnData");
		m_rpc.Unregister("EndGame");
		m_rpc.Unregister("PlayerStatus");
		m_rpc.Unregister("Lobby");
		m_rpc.Unregister("GameStarted");
		m_rpc.Unregister("Kicked");
		m_rpc.Unregister("TimeSync");
		if (m_voSystem != null)
		{
			m_voSystem.Close();
		}
		if (m_lobbyMenu != null)
		{
			m_lobbyMenu.Close();
		}
		if (m_hud != null)
		{
			m_hud.Close();
		}
		if (m_hitText != null)
		{
			m_hitText.Close();
		}
		if (m_dialog != null)
		{
			m_dialog.Close();
		}
		if (m_endGameMenu != null)
		{
			m_endGameMenu.Close();
		}
		if (m_endGameMsg != null)
		{
			UnityEngine.Object.Destroy(m_endGameMsg);
		}
		if (m_loadScreen != null)
		{
			m_loadScreen.Close();
			m_loadScreen = null;
		}
		m_chatClient.Close();
		NetObj.ResetObjectDB();
	}

	public void FixedUpdate()
	{
		if (m_gameState != null)
		{
			m_gameState.FixedUpdate();
		}
		UpdateHudPlaybackData();
		if (m_endGameMenu == null)
		{
			if (m_gameState != null && m_gameSettings != null)
			{
				bool inPlanning = m_state == State.Planning;
				m_hud.UpdatePlayerStates(m_players, m_gameSettings.m_localPlayerID, inPlanning, m_replayMode, m_gameSettings.m_localAdmin);
			}
			m_hud.FixedUpdate();
			m_dialog.FixedUpdate();
		}
		if (m_focusCamera >= 0)
		{
			m_focusCamera--;
			if (m_focusCamera < 0 && !m_replayMode && GameUtils.FindCameraStartPos(m_gameSettings.m_localPlayerID, out var pos))
			{
				m_gameState.GetGameCamera().SetFocus(pos, 450f);
			}
		}
		if (m_state == State.Planning && m_endGameMenu == null && m_lobbyMenu == null && m_turnData != null)
		{
			m_turnData.m_planningTime += Time.fixedDeltaTime;
		}
		UpdateAutoCommit(Time.fixedDeltaTime);
	}

	private void UpdateAutoCommit(float dt)
	{
		if (m_turnData != null && !(m_gameSettings.m_maxTurnTime <= 0.0) && !(m_turnData.m_turnDuration > m_gameSettings.m_maxTurnTime))
		{
			double turnDuration = m_turnData.m_turnDuration;
			m_turnData.m_turnDuration += dt;
			double num = m_gameSettings.m_maxTurnTime - m_turnData.m_turnDuration;
			if (num < 6.0 && num > 1.0 && (long)turnDuration != (long)m_turnData.m_turnDuration)
			{
				UnityEngine.Object.Instantiate(m_countdownPrefab);
			}
			if (num <= 0.0 && m_state == State.Planning && m_turnData.m_needCommit)
			{
				PLog.Log("autocommiting");
				UnityEngine.Object.Instantiate(m_autoCommitPrefab);
				Commit();
			}
		}
	}

	private void UpdateHudPlaybackData()
	{
		int frame = 0;
		int totalFrames = 0;
		int num = 0;
		if (m_gameState != null)
		{
			frame = m_gameState.GetCurrentFrame();
			totalFrames = m_gameState.GetTotalFrames();
		}
		if (m_state == State.Simulating && m_simData != null)
		{
			num = m_simData.m_simulationTurn;
		}
		else if (m_turnData != null)
		{
			num = ((m_state != State.Outcome) ? m_turnData.m_turn : (m_turnData.m_turn - 1));
		}
		double turnTimeLeft = -1.0;
		if (!m_replayMode && m_turnData != null && m_gameSettings.m_maxTurnTime > 0.0)
		{
			turnTimeLeft = m_gameSettings.m_maxTurnTime - m_turnData.m_turnDuration;
		}
		num++;
		m_hud.SetPlaybackData(frame, totalFrames, num, turnTimeLeft);
	}

	public void Update()
	{
		if (m_loadScreen != null)
		{
			m_loadScreen.Update();
		}
		if (m_gameState != null && m_gameState.GetGameModeScript() != null)
		{
			m_hud.SetTargetScore(m_gameState.GetGameModeScript().GetTargetScore());
			m_hud.m_showObjectives = m_gameState.GetGameModeScript().HasObjectives();
		}
		if (m_gameState != null && m_gameState.GetGameCamera() != null)
		{
			m_hud.Update(m_gameState.GetGameCamera().camera);
		}
		if (!m_replayMode && !CheatMan.instance.GetNoFogOfWar())
		{
			m_losDrawer.Draw();
		}
		m_hitText.Update(Time.deltaTime);
		m_dialog.Update(m_players);
		m_voSystem.Update(Time.deltaTime);
		if (m_lobbyMenu != null)
		{
			m_lobbyMenu.Update();
		}
		if (m_endGameMenu != null)
		{
			m_endGameMenu.Update();
		}
		if (Utils.IsAndroidBack() && !m_hud.DismissAnyPopup())
		{
			PLog.Log("Now Quit Game");
			Utils.AndroidBack();
			OnHudExit(ExitState.Normal, 0);
		}
	}

	public void LateUpdate()
	{
		if (m_gameState != null && m_gameState.GetGameCamera() != null)
		{
			m_hitText.LateUpdate(m_gameState.GetGameCamera().GetComponent<Camera>());
		}
	}

	public void OnLevelWasLoaded()
	{
		TryCloseEndGameMenu();
		if (m_gameState != null)
		{
			m_gameState.OnLevelWasLoaded();
		}
		if (m_lobbyMenu != null)
		{
			m_lobbyMenu.OnLevelWasLoaded();
		}
	}

	private void RPC_GameSettings(PTech.RPC rpc, List<object> args)
	{
		m_gameSettings = new GameSettings();
		int num = 0;
		m_gameSettings.m_localPlayerID = (int)args[num++];
		m_gameSettings.m_localAdmin = (bool)args[num++];
		m_gameSettings.m_campaignID = (int)args[num++];
		m_gameSettings.m_gameID = (int)args[num++];
		m_gameSettings.m_gameName = (string)args[num++];
		m_gameSettings.m_gameType = (GameType)(int)args[num++];
		m_gameSettings.m_campaign = (string)args[num++];
		m_gameSettings.m_level = (string)args[num++];
		m_gameSettings.m_fleetSizeClass = (FleetSizeClass)(int)args[num++];
		m_gameSettings.m_fleetSizeLimits.min = (int)args[num++];
		m_gameSettings.m_fleetSizeLimits.max = (int)args[num++];
		m_gameSettings.m_targetScore = (float)args[num++];
		m_gameSettings.m_maxTurnTime = (double)args[num++];
		m_gameSettings.m_nrOfPlayers = (int)args[num++];
		m_gameSettings.m_mapInfo = m_mapMan.GetMapByName(m_gameSettings.m_gameType, m_gameSettings.m_campaign, m_gameSettings.m_level);
		m_gameSettings.m_campaignInfo = m_mapMan.GetCampaign(m_gameSettings.m_campaign);
		if (m_replayMode)
		{
			m_gameSettings.m_localPlayerID = -1;
		}
		if (m_loadScreen != null)
		{
			m_loadScreen.SetImage(m_gameSettings.m_mapInfo.m_loadscreen);
		}
		m_hud.SetGameInfo(m_gameSettings.m_gameName, m_gameSettings.m_gameType, m_gameSettings.m_gameID, m_gameSettings.m_localAdmin);
	}

	private void RPC_GameStarted(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("game started");
		DebugUtils.Assert(m_gameSettings != null);
		m_musMan.SetMusic(string.Empty);
		m_hud.SetVisible(visible: true, hideAll: true, affectBattleBar: true);
		m_loadScreen.SetVisible(visible: true);
		CloseLobby();
	}

	private void OnLobbyExit()
	{
		PLog.Log("clientgame:OnLobbyExit()");
		CloseLobby();
		if (m_onExit != null)
		{
			m_onExit(ExitState.Normal, 0);
		}
	}

	private void Lobby_OnInviteClicked(LobbyPlayer sender)
	{
		PLog.Log("ClientGame recived from Lobby that " + sender.m_name + " wants to invite friends.");
	}

	private void RPC_Lobby(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("Got lobby data");
		int num = 0;
		string localName = (string)args[num++];
		bool allowMatchmaking = (bool)args[num++];
		bool noFleet = (bool)args[num++];
		int num2 = (int)args[num++];
		List<LobbyPlayer> list = new List<LobbyPlayer>();
		for (int i = 0; i < num2; i++)
		{
			LobbyPlayer lobbyPlayer = new LobbyPlayer();
			lobbyPlayer.m_id = (int)args[num++];
			lobbyPlayer.m_team = (int)args[num++];
			lobbyPlayer.m_name = (string)args[num++];
			lobbyPlayer.m_flag = (int)args[num++];
			lobbyPlayer.m_admin = (bool)args[num++];
			lobbyPlayer.m_readyToStart = (bool)args[num++];
			lobbyPlayer.m_status = (PlayerPresenceStatus)(int)args[num++];
			lobbyPlayer.m_fleet = (string)args[num++];
			lobbyPlayer.m_fleetValue = (int)args[num++];
			list.Add(lobbyPlayer);
		}
		SetupLobby(localName, m_gameSettings, noFleet, allowMatchmaking, list);
	}

	private void SetupLobby(string localName, GameSettings gameSettings, bool noFleet, bool allowMatchmaking, List<LobbyPlayer> playerList)
	{
		m_hud.SetVisible(visible: false, hideAll: true, affectBattleBar: true);
		if (m_lobbyMenu == null)
		{
			m_lobbyMenu = new LobbyMenu(m_guiCamera, m_gameSettings.m_mapInfo, m_rpc, m_userManClient, m_gameSettings.m_campaignID, m_chatClient, m_musMan, m_hostName);
			m_lobbyMenu.m_onExit = OnLobbyExit;
			m_lobbyMenu.m_playerRemovedDelegate = Lobby_RemovedPlayer;
		}
		m_lobbyMenu.Setup(noFleet, allowMatchmaking, m_gameSettings, playerList);
	}

	private void Lobby_RemovedPlayer(string playerName)
	{
		PLog.Log("ClientGame recived from Lobby that " + playerName + " wants to leave the loby friends. (or be kicked).");
		m_rpc.Invoke("KickPlayer", playerName);
	}

	private void CloseLobby()
	{
		if (m_lobbyMenu != null)
		{
			LobbyMenu lobbyMenu = m_lobbyMenu;
			lobbyMenu.m_onExit = (Action)Delegate.Remove(lobbyMenu.m_onExit, new Action(OnLobbyExit));
			m_lobbyMenu.Close();
			m_lobbyMenu = null;
		}
	}

	private void SetupEndgameMenu()
	{
		DebugUtils.Assert(m_endGameData != null);
		m_hitText.Clear();
		m_hud.SetVisible(visible: false, hideAll: true, affectBattleBar: true);
		m_dialog.Hide();
		if (m_gameState != null && m_gameState.GetGameCamera() != null)
		{
			m_gameState.GetGameCamera().SetEnabled(enabled: false);
		}
		if (m_gameState != null && m_gameState.GetGameCamera() != null)
		{
			m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Disabled);
		}
		m_endGameMenu = new EndGameMenu(m_guiCamera, m_endGameData, m_gameSettings, m_rpc, m_chatClient, m_musMan);
		m_endGameMenu.m_onLeavePressed = OnEndGameExit;
	}

	private void OnEndGameExit(int joinGameID)
	{
		PLog.Log("** OnEndGame_Leave **");
		if (joinGameID > 0)
		{
			m_onExit(ExitState.JoinGame, joinGameID);
		}
		else if (m_gameSettings.m_gameType == GameType.Campaign && m_gameSettings.m_campaignInfo != null && !m_gameSettings.m_campaignInfo.m_tutorial)
		{
			m_onExit(ExitState.ShowCredits, 0);
		}
		else
		{
			m_onExit(ExitState.Normal, 0);
		}
	}

	private void TryCloseEndGameMenu()
	{
		m_endGameData = null;
		if (m_endGameMenu != null)
		{
			m_endGameMenu.Close();
			m_endGameMenu = null;
		}
	}

	private void OnHudExit(ExitState state, int gameID)
	{
		PLog.LogWarning("Clientgame::OnHudExit()");
		m_onExit(state, gameID);
	}

	private void OnHudQuitGame()
	{
		if (m_onQuitGame != null)
		{
			m_onQuitGame();
		}
	}

	private void RPC_DoInitiation(PTech.RPC rpc, List<object> args)
	{
		DebugUtils.Assert(m_gameSettings != null);
		int num = 0;
		m_initializeData = new InitializeData();
		for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
		{
			PlayerInitData playerInitData = new PlayerInitData();
			if ((bool)args[num++])
			{
				byte[] data = (byte[])args[num++];
				FleetDef fleetDef = (playerInitData.m_fleet = new FleetDef(data));
			}
			playerInitData.m_id = (int)args[num++];
			playerInitData.m_name = (string)args[num++];
			playerInitData.m_team = (int)args[num++];
			playerInitData.m_flag = (int)args[num++];
			m_initializeData.m_players.Add(playerInitData);
		}
		int frames = 60;
		m_gameState = new GameState(m_guiCamera, m_gameSettings, m_turnMan, TurnPhase.Simulating, fastSimulation: false, null, -1, frames, OnInitiationSetupComplete, OnInitiationComplete);
		m_turnMan.SetNrOfPlayers(m_gameSettings.m_nrOfPlayers);
		m_turnMan.SetGameType(m_gameSettings.m_gameType);
		foreach (PlayerInitData player in m_initializeData.m_players)
		{
			m_turnMan.SetPlayerName(player.m_id, player.m_name);
			m_turnMan.SetPlayerTeam(player.m_id, player.m_team);
			m_turnMan.SetPlayerHuman(player.m_id, ishuman: true);
			m_turnMan.SetPlayerFlag(player.m_id, player.m_flag);
		}
		int num2 = 0;
		int num3 = 0;
		foreach (PlayerInitData player2 in m_initializeData.m_players)
		{
			if (m_gameSettings.m_gameType == GameType.Campaign || m_gameSettings.m_gameType == GameType.Challenge)
			{
				m_turnMan.SetPlayerColors(player2.m_id, Constants.m_coopColors[player2.m_id]);
				continue;
			}
			Color color = ((player2.m_team != 0) ? Constants.m_teamColors2[num3++] : Constants.m_teamColors1[num2++]);
			m_turnMan.SetPlayerColors(player2.m_id, color);
		}
		CheatMan.instance.ResetCheats();
	}

	private void OnInitiationSetupComplete()
	{
		PLog.Log(" OnInitiationSetupComplete");
		m_gameState.GetGameModeScript().InitializeGame(m_initializeData.m_players);
		m_initializeData.m_startState = m_gameState.GetState();
		m_initializeData.m_StartOrders = m_gameState.GetOrders(-1);
		m_gameState.SetSimulating(enabled: true);
		m_gameState.GetGameCamera().SetEnabled(enabled: false);
	}

	private void OnInitiationComplete()
	{
		PLog.Log(" OnInitiationComplete");
		int totalFrames = m_gameState.GetTotalFrames();
		byte[] state = m_gameState.GetState();
		byte[] orders = m_gameState.GetOrders(-1);
		m_rpc.Invoke("InitialState", CompressUtils.Compress(m_initializeData.m_startState), CompressUtils.Compress(m_initializeData.m_StartOrders), CompressUtils.Compress(state), CompressUtils.Compress(orders), totalFrames);
		m_gameState = null;
	}

	private void RPC_DoSimulation(PTech.RPC rpc, List<object> args)
	{
		m_loadScreen.SetVisible(visible: false);
		m_simData = new SimulationData();
		int num = 0;
		m_simData.m_simulationTurn = (int)args[num++];
		m_simData.m_frames = (int)args[num++];
		m_simData.m_startState = CompressUtils.Decompress((byte[])args[num++]);
		m_simData.m_combinedStartOrders = CompressUtils.Decompress((byte[])args[num++]);
		m_simData.m_surrenders = (int[])args[num++];
		int num2 = (int)args[num++];
		m_simData.m_playerOrders = new List<byte[]>();
		for (int i = 0; i < num2; i++)
		{
			if ((bool)args[num++])
			{
				m_simData.m_playerOrders.Add(CompressUtils.Decompress((byte[])args[num++]));
			}
			else
			{
				m_simData.m_playerOrders.Add(null);
			}
		}
		UpdateHudPlaybackData();
		m_hud.SetMode(Hud.Mode.Outcome);
		StartSimulation();
	}

	private void StartSimulation()
	{
		m_gameState = new GameState(m_guiCamera, m_gameSettings, m_turnMan, TurnPhase.Playback, fastSimulation: false, m_simData.m_startState, m_gameSettings.m_localPlayerID, m_simData.m_frames, OnSimulationSetupComplete, OnSimulationComplete);
		m_state = State.Simulating;
		NetObj.SetDrawOrders(enabled: false);
	}

	private void OnSimulationSetupComplete()
	{
		m_gameState.SetOrders(-1, m_simData.m_combinedStartOrders);
		for (int i = 0; i < m_simData.m_playerOrders.Count; i++)
		{
			byte[] array = m_simData.m_playerOrders[i];
			if (array != null)
			{
				m_gameState.SetOrders(i, array);
			}
		}
		m_simData.m_combinedOrders = m_gameState.GetOrders(-1);
		m_gameState.SetSimulating(enabled: true);
		m_gameState.GetGameCamera().GetComponent<GameCamera>().SetMode(GameCamera.Mode.Passive);
		SetupTurnMusic();
		ShowSurrenderMessages(m_simData.m_surrenders);
		VOSystem.instance.ResetTurnflags();
	}

	private void OnSimulationComplete()
	{
		GameMode gameModeScript = m_gameState.GetGameModeScript();
		TurnMan turnMan = m_gameState.GetTurnMan();
		if (!m_replayMode && gameModeScript.GetOutcome() != 0)
		{
			CheckLocalEndGameAchivements();
		}
		List<object> list = new List<object>();
		list.Add(m_simData.m_simulationTurn);
		list.Add((int)gameModeScript.GetOutcome());
		list.Add(gameModeScript.GetWinnerTeam(m_gameSettings.m_nrOfPlayers));
		list.Add(m_gameSettings.m_nrOfPlayers);
		for (int i = 0; i < m_gameSettings.m_nrOfPlayers; i++)
		{
			int num = i;
			int playerPlace = gameModeScript.GetPlayerPlace(num, m_gameSettings.m_nrOfPlayers);
			int playerScore = turnMan.GetPlayerScore(num);
			int teamScoreForPlayer = turnMan.GetTeamScoreForPlayer(num);
			int flagshipKiller = turnMan.GetFlagshipKiller(num, 0);
			int flagshipKiller2 = turnMan.GetFlagshipKiller(num, 1);
			list.Add(num);
			list.Add(playerPlace);
			list.Add(playerScore);
			list.Add(teamScoreForPlayer);
			list.Add(flagshipKiller);
			list.Add(flagshipKiller2);
			list.Add(gameModeScript.IsPlayerDead(i));
			TurnMan.PlayerTurnData player = turnMan.GetPlayer(num);
			list.Add(player.m_turnDamage);
			list.Add(player.m_turnFriendlyDamage);
			list.Add(player.m_turnShipsSunk);
			list.Add(player.m_turnGunDamage);
		}
		list.Add(CompressUtils.Compress(m_simData.m_startState));
		list.Add(CompressUtils.Compress(m_simData.m_combinedOrders));
		list.Add(CompressUtils.Compress(m_gameState.GetState()));
		list.Add(CompressUtils.Compress(m_gameState.GetOrders(-1)));
		list.Add(m_simData.m_surrenders);
		list.Add(m_gameState.GetCurrentFrame());
		m_rpc.Invoke("SimulationResults", list);
		m_simData = null;
	}

	private void RPC_TimeSync(PTech.RPC rpc, List<object> args)
	{
		double turnDuration = (double)args[0];
		if (m_turnData != null)
		{
			m_turnData.m_turnDuration = turnDuration;
		}
	}

	private void RPC_TurnData(PTech.RPC rpc, List<object> args)
	{
		m_loadScreen.SetVisible(visible: false);
		int num = 0;
		m_turnData = new TurnData();
		m_turnData.m_turn = (int)args[num++];
		m_turnData.m_turnType = (TurnType)(int)args[num++];
		m_turnData.m_needCommit = (bool)args[num++];
		m_turnData.m_turnDuration = (double)args[num++];
		m_turnData.m_myOrders = CompressUtils.Decompress((byte[])args[num++]);
		m_turnData.m_startState = CompressUtils.Decompress((byte[])args[num++]);
		m_turnData.m_orders = CompressUtils.Decompress((byte[])args[num++]);
		m_turnData.m_endState = CompressUtils.Decompress((byte[])args[num++]);
		m_turnData.m_endOrders = CompressUtils.Decompress((byte[])args[num++]);
		m_turnData.m_playbackFrames = (int)args[num++];
		m_turnData.m_frames = (int)args[num++];
		m_turnData.m_surrenders = (int[])args[num++];
		if ((bool)args[num++])
		{
			PLog.Log("Got replay data, starting replay");
			StartOutcomePlayback(isReplay: false);
		}
		else
		{
			m_state = State.Outcome;
			OnOutcomeComplete();
		}
	}

	private void StartOutcomePlayback(bool isReplay)
	{
		m_state = State.Outcome;
		m_isOutcomeReplay = isReplay;
		m_gameState = new GameState(m_guiCamera, m_gameSettings, m_turnMan, TurnPhase.Playback, fastSimulation: false, m_turnData.m_startState, m_gameSettings.m_localPlayerID, m_turnData.m_playbackFrames, OnOutcomeSetupComplete, OnOutcomeComplete);
		NetObj.SetDrawOrders(enabled: false);
	}

	private void OnOutcomeSetupComplete()
	{
		m_gameState.SetOrders(-1, m_turnData.m_orders);
		m_gameState.SetSimulating(enabled: true);
		UpdateHudPlaybackData();
		if (m_replayMode)
		{
			m_hud.SetMode(Hud.Mode.Replay);
		}
		else
		{
			m_hud.SetMode(m_isOutcomeReplay ? Hud.Mode.ReplayOutcome : Hud.Mode.Outcome);
		}
		m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Passive);
		m_gameState.GetGameCamera().SetEnabled(enabled: true);
		if (m_firstLook)
		{
			m_firstLook = false;
			m_focusCamera = 10;
		}
		ShowSurrenderMessages(m_turnData.m_surrenders);
		SetupTurnMusic();
		VOSystem.instance.ResetTurnflags();
	}

	private void ShowSurrenderMessages(int[] surrenders)
	{
		foreach (int playerID in surrenders)
		{
			string playerName = m_turnMan.GetPlayerName(playerID);
			MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, playerName + " $label_surrender_newsflash", string.Empty, "NewsflashMessage", 2f);
		}
	}

	public void AwardAchievement(int owner, int id)
	{
		if (owner == m_gameSettings.m_localPlayerID)
		{
			PLog.Log("AwardAchievement " + id);
			m_userManClient.UnlockAchievement(id);
		}
	}

	private void CheckLocalEndGameAchivements()
	{
		m_gameState.GetGameModeScript().CheckAchivements(m_userManClient);
		if (m_gameSettings.m_nrOfPlayers == 4)
		{
			m_userManClient.UnlockAchievement(2);
		}
	}

	private void SetupTurnMusic()
	{
		if (m_gameSettings.m_gameType == GameType.Points || m_gameSettings.m_gameType == GameType.Assassination)
		{
			string @string = PlayerPrefs.GetString("CustomVSMusic", string.Empty);
			if (@string != string.Empty)
			{
				m_musMan.SetMusic(@string);
				return;
			}
		}
		if (m_replayMode)
		{
			m_musMan.SetMusic("replay");
		}
		else
		{
			m_musMan.SetMusic(m_turnMan.GetTurnMusic());
		}
	}

	private void OnOutcomeComplete()
	{
		GameMode gameModeScript = m_gameState.GetGameModeScript();
		if (!m_replayMode && gameModeScript.GetOutcome() != 0)
		{
			CheckLocalEndGameAchivements();
		}
		if (m_endGameData != null)
		{
			ShowEndGameMessage();
		}
		else if (m_replayMode)
		{
			RequestNextTurn();
		}
		else
		{
			SetupPlanning();
		}
	}

	private void SetupPlanning()
	{
		m_gameState = new GameState(m_guiCamera, m_gameSettings, m_turnMan, TurnPhase.Planning, fastSimulation: false, m_turnData.m_endState, m_gameSettings.m_localPlayerID, m_turnData.m_frames, OnPlanningSetupComplete, null);
		m_state = State.Planning;
	}

	private void OnPlanningSetupComplete()
	{
		NetObj.SetDrawOrders(enabled: true);
		m_gameState.SetOrders(-1, m_turnData.m_endOrders);
		if (m_turnData.m_myOrders.Length > 0)
		{
			m_gameState.SetOrders(m_gameSettings.m_localPlayerID, m_turnData.m_myOrders);
		}
		if (m_turnData.m_tempOrders != null)
		{
			m_gameState.SetOrders(m_gameSettings.m_localPlayerID, m_turnData.m_tempOrders);
		}
		m_gameState.ClearNonLocalOrders();
		SetupTurnMusic();
		if (m_turnData.m_turn == 0)
		{
			VOSystem.instance.DoEvent("Match start");
		}
		if (m_turnData.m_needCommit)
		{
			UpdateHudPlaybackData();
			m_hud.SetMode(Hud.Mode.Planning);
			m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Active);
		}
		else
		{
			SetupWaiting();
		}
		if (m_turnData.m_turnType == TurnType.EndGame)
		{
			SetupEndgame();
		}
	}

	private void OnPlayDialog(bool hideBattlebar)
	{
		m_dialog.Show();
		if (m_state != State.Planning)
		{
			OnPlayPause();
		}
		m_hud.SetVisible(visible: false, hideAll: true, hideBattlebar);
	}

	private void OnEndDialog()
	{
		if (TurnMan.instance.m_endGame == GameOutcome.None)
		{
			m_dialog.Hide();
			m_hud.SetVisible(visible: true, hideAll: true, affectBattleBar: true);
		}
		if (m_state != State.Planning)
		{
			OnPlayPause();
		}
	}

	private void OnHudCommit()
	{
		Commit();
	}

	private void Commit()
	{
		if (m_state == State.Planning && m_turnData.m_needCommit)
		{
			List<object> list = new List<object>();
			list.Add(m_turnData.m_turn);
			list.Add(m_turnData.m_localSurrender);
			list.Add(m_turnData.m_planningTime);
			list.Add(CompressUtils.Compress(m_gameState.GetOrders(m_gameSettings.m_localPlayerID)));
			m_rpc.Invoke("Commit", list);
			m_turnData.m_needCommit = false;
			SetupWaiting();
			VOSystem.instance.DoEvent("Press commit");
		}
	}

	private void SetupEndgame()
	{
		m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Passive);
		m_hud.SetMode(m_replayMode ? Hud.Mode.Replay : Hud.Mode.Outcome);
		m_state = State.Waiting;
	}

	private void SetupWaiting()
	{
		m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Passive);
		m_hud.SetMode(Hud.Mode.Waiting);
		m_state = State.Waiting;
	}

	private void ClearNonLocalOrders(int playerID)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Unit");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			Unit unit = gameObject.GetComponent<NetObj>() as Unit;
			if (unit != null && unit.GetOwner() != playerID)
			{
				unit.ClearOrders();
			}
		}
	}

	private void OnPlayPause()
	{
		if (m_state == State.Outcome || m_state == State.Simulating)
		{
			m_gameState.SetSimulating(!m_gameState.IsSimulating());
		}
		else if (m_state == State.Planning)
		{
			m_turnData.m_tempOrders = m_gameState.GetOrders(m_gameSettings.m_localPlayerID);
			StartOutcomePlayback(isReplay: true);
		}
	}

	private void StartTest()
	{
		if (m_state == State.Planning)
		{
			m_state = State.Test;
			m_turnData.m_tempOrders = m_gameState.GetOrders(m_gameSettings.m_localPlayerID);
			m_gameState = new GameState(m_guiCamera, m_gameSettings, m_turnMan, TurnPhase.Testing, fastSimulation: false, m_turnData.m_endState, m_gameSettings.m_localPlayerID, m_turnData.m_frames, OnTestingSetupComplete, OnTestingComplete);
		}
	}

	private void OnTestingSetupComplete()
	{
		m_gameState.SetOrders(-1, m_turnData.m_endOrders);
		m_gameState.SetOrders(-1, m_turnData.m_tempOrders);
		m_gameState.SetSimulating(enabled: true);
		NetObj.SetDrawOrders(enabled: false);
		m_gameState.GetGameCamera().SetMode(GameCamera.Mode.Passive);
		UpdateHudPlaybackData();
		m_hud.SetMode(Hud.Mode.Outcome);
	}

	private void OnTestingComplete()
	{
	}

	private void StopOutcome()
	{
		if (m_state == State.Outcome && m_isOutcomeReplay)
		{
			SetupPlanning();
		}
	}

	private void StopTest()
	{
		if (m_state == State.Test)
		{
			SetupPlanning();
		}
	}

	private void RequestNextTurn()
	{
		if (m_turnData != null && m_turnData.m_turn != -1)
		{
			m_rpc.Invoke("RequestReplayTurn", m_turnData.m_turn + 1);
		}
	}

	private void OnNextReplayTurn()
	{
		RequestNextTurn();
	}

	private void OnPrevReplayTurn()
	{
		if (m_turnData != null && m_turnData.m_turn != -1)
		{
			m_rpc.Invoke("RequestReplayTurn", m_turnData.m_turn - 1);
		}
	}

	private int CheckNrOfUnits(int owner)
	{
		int num = 0;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Unit");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			Unit component = gameObject.GetComponent<Unit>();
			if (component != null && !component.IsDead() && component.GetOwner() == owner)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsSimulating()
	{
		return NetObj.IsSimulating();
	}

	private void RPC_EndGame(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("RPC_EndGame");
		if (m_endGameMenu != null)
		{
			return;
		}
		m_endGameData = new EndGameData();
		int num = 0;
		m_endGameData.m_localPlayerID = (int)args[num++];
		m_endGameData.m_outcome = (GameOutcome)(int)args[num++];
		m_endGameData.m_winnerTeam = (int)args[num++];
		m_endGameData.m_autoJoinGameID = (int)args[num++];
		m_endGameData.m_turns = (int)args[num++];
		int num2 = (int)args[num++];
		for (int i = 0; i < num2; i++)
		{
			EndGame_PlayerStatistics endGame_PlayerStatistics = new EndGame_PlayerStatistics();
			endGame_PlayerStatistics.m_playerID = (int)args[num++];
			endGame_PlayerStatistics.m_team = (int)args[num++];
			endGame_PlayerStatistics.m_name = (string)args[num++];
			endGame_PlayerStatistics.m_flag = (int)args[num++];
			endGame_PlayerStatistics.m_place = (int)args[num++];
			endGame_PlayerStatistics.m_score = (int)args[num++];
			endGame_PlayerStatistics.m_teamScore = (int)args[num++];
			endGame_PlayerStatistics.m_flagshipKiller0 = (int)args[num++];
			endGame_PlayerStatistics.m_flagshipKiller1 = (int)args[num++];
			endGame_PlayerStatistics.m_shipsSunk = -1;
			endGame_PlayerStatistics.m_shipsLost = -1;
			endGame_PlayerStatistics.m_shipsDamaged = -1;
			EndGame_PlayerStatistics endGame_PlayerStatistics2 = endGame_PlayerStatistics;
			endGame_PlayerStatistics2.m_shipsSunk = m_turnMan.GetTotalShipsSunk(endGame_PlayerStatistics2.m_playerID);
			endGame_PlayerStatistics2.m_shipsLost = m_turnMan.GetTotalShipsLost(endGame_PlayerStatistics2.m_playerID);
			if (endGame_PlayerStatistics2.m_playerID == m_endGameData.m_localPlayerID)
			{
				m_endGameData.m_localPlayer = endGame_PlayerStatistics2;
			}
			m_endGameData.m_players.Add(endGame_PlayerStatistics2);
		}
		TurnMan.PlayerTurnData accoladeDestory = TurnMan.instance.GetAccoladeDestory(highest: true);
		if (accoladeDestory.m_totalDamageInflicted == 0)
		{
			m_endGameData.m_AccoladeDestroy = Localize.instance.Translate("$label_notapplicable");
		}
		else
		{
			m_endGameData.m_AccoladeDestroy = accoladeDestory.m_name + ": " + accoladeDestory.m_totalDamageInflicted + " " + Localize.instance.Translate("$accolade_destroyer");
		}
		TurnMan.PlayerTurnData accoladeDestory2 = TurnMan.instance.GetAccoladeDestory(highest: false);
		if (accoladeDestory2.m_totalDamageInflicted == 0)
		{
			m_endGameData.m_AccoladeHarmless = Localize.instance.Translate("$label_notapplicable");
		}
		else
		{
			m_endGameData.m_AccoladeHarmless = accoladeDestory2.m_name + ": " + accoladeDestory2.m_totalDamageInflicted + " " + Localize.instance.Translate("$accolade_destroyer");
		}
		TurnMan.PlayerTurnData accoladeAbsorbed = TurnMan.instance.GetAccoladeAbsorbed();
		if (accoladeAbsorbed.m_totalDamageAbsorbed == 0)
		{
			m_endGameData.m_AccoladeShields = Localize.instance.Translate("$label_notapplicable");
		}
		else
		{
			m_endGameData.m_AccoladeShields = accoladeAbsorbed.m_name + ": " + accoladeAbsorbed.m_totalDamageAbsorbed + " " + Localize.instance.Translate("$accolade_absorbed");
		}
		DebugUtils.Assert(m_endGameData.m_localPlayer != null);
		if (m_state == State.Planning || m_state == State.Waiting || m_state == State.None)
		{
			ShowEndGameMessage();
		}
	}

	private void RPC_PlayerStatus(PTech.RPC rpc, List<object> args)
	{
		int num = 0;
		int num2 = (int)args[num++];
		m_players.Clear();
		for (int i = 0; i < num2; i++)
		{
			ClientPlayer clientPlayer = new ClientPlayer();
			clientPlayer.m_id = (int)args[num++];
			clientPlayer.m_surrender = (bool)args[num++];
			clientPlayer.m_left = (bool)args[num++];
			clientPlayer.m_status = (PlayerPresenceStatus)(int)args[num++];
			clientPlayer.m_turnStatus = (PlayerTurnStatus)(int)args[num++];
			m_players.Add(clientPlayer);
		}
	}

	private void RPC_Kicked(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("You have been kicked");
		m_onExit(ExitState.Kicked, 0);
	}

	private void OnSurrender()
	{
		Surrender(m_gameSettings.m_localPlayerID);
	}

	private void Surrender(int playerID)
	{
		if (m_state == State.Planning)
		{
			List<NetObj> all = NetObj.GetAll();
			foreach (NetObj item in all)
			{
				Ship ship = item as Ship;
				if (ship != null && ship.GetOwner() == playerID)
				{
					ship.SelfDestruct();
				}
			}
		}
		m_turnData.m_localSurrender = true;
		Commit();
	}

	private void ShowEndGameMessage()
	{
		if (!(m_endGameMsg != null))
		{
			bool flag = m_endGameData.m_outcome == GameOutcome.Victory || (m_endGameData.m_outcome == GameOutcome.GameOver && m_endGameData.m_winnerTeam == m_endGameData.m_localPlayer.m_team);
			bool flag2 = m_endGameData.m_outcome == GameOutcome.GameOver && m_endGameData.m_winnerTeam == -1;
			if (flag)
			{
				PLog.Log("Show winner splash");
				m_endGameMsg = GuiUtils.CreateGui("IngameGui/VictoryMessage", m_guiCamera);
				m_musMan.SetMusic("victory");
				VOSystem.instance.DoEvent("Match won");
			}
			else if (flag2)
			{
				PLog.Log("Show tie splash");
				m_endGameMsg = GuiUtils.CreateGui("IngameGui/DrawMessage", m_guiCamera);
				m_musMan.SetMusic("victory");
			}
			else
			{
				PLog.Log("Show looser splash");
				m_endGameMsg = GuiUtils.CreateGui("IngameGui/DefeatMessage", m_guiCamera);
				m_musMan.SetMusic("defeat");
				VOSystem.instance.DoEvent("Match lost");
			}
			GuiUtils.FindChildOf(m_endGameMsg, "ContinueButton").GetComponent<UIButton>().SetValueChangedDelegate(OnEndGameMsgContinue);
			m_endGameMsg.GetComponent<UIPanel>().BringIn();
			m_hud.SetVisible(visible: false, hideAll: true, affectBattleBar: true);
		}
	}

	private void OnEndGameMsgContinue(IUIObject button)
	{
		UnityEngine.Object.Destroy(m_endGameMsg);
		m_endGameMsg = null;
		if (m_endGameData.m_outcome == GameOutcome.Victory && (m_gameSettings.m_gameType == GameType.Campaign || m_gameSettings.m_gameType == GameType.Challenge))
		{
			m_dialog.ForceEndDialog();
			m_dialog.m_onEndDialog = SetupEndgameMenu;
			m_turnMan.PlayBriefing("leveldata/campaign/briefings/" + m_gameSettings.m_level + "_debriefing");
		}
		else
		{
			SetupEndgameMenu();
		}
	}

	public GameType GetGameType()
	{
		return m_gameSettings.m_gameType;
	}

	public bool IsReplayMode()
	{
		return m_replayMode;
	}
}
