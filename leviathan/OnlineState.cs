#define DEBUG
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using PTech;
using UnityEngine;

internal class OnlineState : StateBase
{
	public delegate void LoginFailHandler(ErrorCode errorCode);

	public delegate void LoginSuccessHandler();

	public delegate void DisconnectHandler(bool error);

	private ChooseFleetMenu m_chooseFleetDialog;

	private FleetMenu m_fleetEditor;

	public LoginFailHandler m_onLoginFailed;

	public LoginSuccessHandler m_onLoginSuccess;

	public DisconnectHandler m_onDisconnect;

	public Action<ErrorCode> m_onCreateFailed;

	public Action m_onCreateSuccess;

	public Action m_onResetPasswordOk;

	public Action m_onResetPasswordFail;

	public Action m_onQuitGame;

	public Action<ErrorCode> m_onRequestVerificationRespons;

	private Socket m_socket;

	private PTech.RPC m_rpc;

	private string m_localUserName;

	private string m_hostName;

	private ClientGame m_game;

	private OnlineMenu m_menu;

	private ToastMaster m_toastMaster;

	private PdxNews m_pdxNews;

	private bool m_waitingForJoinResponse;

	private string m_watchReplayName = string.Empty;

	public OnlineState(GameObject guiCamera, MusicManager musMan, GDPBackend gdpBackend, PdxNews pdxNews)
		: base(guiCamera, musMan, gdpBackend)
	{
		m_guiCamera = guiCamera;
		m_musicMan = musMan;
		m_pdxNews = pdxNews;
	}

	public override void Close()
	{
		base.Close();
		CloseMenu();
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Close();
			m_fleetEditor = null;
		}
		if (m_toastMaster != null)
		{
			m_toastMaster.Close();
			m_toastMaster = null;
		}
		if (m_rpc != null)
		{
			m_rpc.Close();
		}
	}

	public bool Login(string hostVisalName, string host, int port, string user, string passwd, string token)
	{
		if (!Connect(host, port))
		{
			return false;
		}
		m_hostName = hostVisalName;
		m_localUserName = user;
		PlatformType platform = Utils.GetPlatform();
		string text = PwdUtils.GenerateWeakPasswordHash(passwd);
		m_rpc.Invoke("Login", user, text, token, VersionInfo.GetMajorVersionString(), (int)platform, Localize.instance.GetLanguage());
		m_rpc.Register("LoginOK", RPC_LoginOK);
		m_rpc.Register("LoginFail", RPC_LoginFail);
		m_rpc.Register("JoinOK", RPC_JoinOK);
		m_rpc.Register("JoinFail", RPC_JoinFail);
		return true;
	}

	public bool CreateAccount(string host, int port, string user, string passwd, string email)
	{
		if (!Connect(host, port))
		{
			return false;
		}
		string text = PwdUtils.GenerateWeakPasswordHash(passwd);
		m_rpc.Invoke("CreateAccount", user, text, email, VersionInfo.GetMajorVersionString(), Localize.instance.GetLanguage());
		m_rpc.Register("CreateSuccess", RPC_CreateSuccess);
		m_rpc.Register("CreateFail", RPC_CreateFail);
		return true;
	}

	public bool RequestVerificationEmail(string host, int port, string user)
	{
		if (!Connect(host, port))
		{
			return false;
		}
		m_rpc.Invoke("RequestVerificationEmail", user);
		m_rpc.Register("RequestVerificationRespons", RPC_RequestVerificationRespons);
		return true;
	}

	public bool RequestPasswordReset(string host, int port, string email)
	{
		if (!Connect(host, port))
		{
			return false;
		}
		m_rpc.Invoke("RequestPasswordReset", email);
		return true;
	}

	public bool ResetPassword(string host, int port, string email, string token, string newPassword)
	{
		if (!Connect(host, port))
		{
			return false;
		}
		string text = PwdUtils.GenerateWeakPasswordHash(newPassword);
		m_rpc.Invoke("ResetPassword", email, token, text);
		m_rpc.Register("ResetPasswordOK", RPC_ResetPasswordOk);
		m_rpc.Register("ResetPasswordFail", RPC_ResetPasswordFail);
		return true;
	}

	private bool Connect(string host, int port)
	{
		//Discarded unreachable code: IL_008c
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		m_socket.NoDelay = true;
		PLog.LogWarning("(not really a warning) Connecting to " + host + " : " + port);
		IAsyncResult asyncResult;
		try
		{
			asyncResult = m_socket.BeginConnect(host, port, null, null);
		}
		catch (SocketException ex)
		{
			m_socket.Close();
			m_socket = null;
			PLog.LogWarning("Failed to connect (dns lookup) " + ex.ToString());
			return false;
		}
		bool flag = asyncResult.AsyncWaitHandle.WaitOne(10000, exitContext: true);
		if (!m_socket.Connected)
		{
			m_socket.Close();
			m_socket = null;
			PLog.LogWarning("Socket connection timed out");
			return false;
		}
		PacketSocket socket = new PacketSocket(m_socket);
		m_rpc = new PTech.RPC(socket);
		PLog.Log("Connected ");
		return true;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (m_game != null)
		{
			m_game.FixedUpdate();
		}
	}

	public override void Update()
	{
		if (m_rpc != null && !m_rpc.Update(recvAll: false))
		{
			m_rpc.Close();
			if (m_onDisconnect != null)
			{
				m_onDisconnect(error: true);
			}
			return;
		}
		base.Update();
		if (m_game != null)
		{
			m_game.Update();
		}
		if (m_menu != null)
		{
			m_menu.Update();
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Update();
		}
		if (m_toastMaster != null)
		{
			m_toastMaster.Update(Time.deltaTime);
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (m_game != null)
		{
			m_game.LateUpdate();
		}
	}

	public override void OnLevelWasLoaded()
	{
		base.OnLevelWasLoaded();
		if (m_game != null)
		{
			m_game.OnLevelWasLoaded();
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.OnLevelWasLoaded();
		}
	}

	public void OnGui()
	{
	}

	private void RPC_LoginOK(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("Login ok");
		if (m_onLoginSuccess != null)
		{
			m_onLoginSuccess();
		}
		rpc.Register("ReplayData", RPC_ReplayData);
		rpc.Register("ReplayMissing", RPC_ReplayMissing);
		m_userManClient = new UserManClientRemote(rpc, m_gdpBackend);
		m_mapMan = new ClientMapMan();
		m_toastMaster = new ToastMaster(m_rpc, m_guiCamera, m_userManClient);
		if (m_gdpBackend != null)
		{
			RegisterGDPPackets();
		}
		SetupMenu(MenuBase.StartStatus.Normal);
	}

	protected override void SetupMenu(MenuBase.StartStatus startStatus)
	{
		base.SetupMenu(startStatus);
		m_musicMan.SetMusic("menu");
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Close();
			m_fleetEditor = null;
		}
		m_menu = new OnlineMenu(m_guiCamera, m_rpc, m_hostName, m_localUserName, m_mapMan, m_userManClient, startStatus, m_musicMan, m_gdpBackend, m_pdxNews);
		m_menu.m_onJoin = OnJoinGame;
		m_menu.m_onWatchReplay = OnMenuWatchReplay;
		m_menu.m_onLogout = OnLogout;
		m_menu.m_onStartFleetEditor = SetupChooseFleet;
		m_menu.m_onProceed = OnChooseFleetDialogProceed;
		m_menu.m_onItemBought = OnItemBought;
		main.LoadLevel("menu", showLoadScreen: true);
	}

	private void RPC_LoginFail(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("Failed to login");
		ErrorCode errorCode = (ErrorCode)(int)args[0];
		if (m_onLoginFailed != null)
		{
			m_onLoginFailed(errorCode);
		}
	}

	private void RPC_CreateSuccess(PTech.RPC rpc, List<object> args)
	{
		if (m_onCreateSuccess != null)
		{
			m_onCreateSuccess();
		}
	}

	private void RPC_CreateFail(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("Failed to create");
		ErrorCode obj = (ErrorCode)(int)args[0];
		if (m_onCreateFailed != null)
		{
			m_onCreateFailed(obj);
		}
	}

	private void RPC_RequestVerificationRespons(PTech.RPC rpc, List<object> args)
	{
		ErrorCode obj = (ErrorCode)(int)args[0];
		if (m_onRequestVerificationRespons != null)
		{
			m_onRequestVerificationRespons(obj);
		}
	}

	private void RPC_ResetPasswordOk(PTech.RPC rpc, List<object> args)
	{
		m_onResetPasswordOk();
	}

	private void RPC_ResetPasswordFail(PTech.RPC rpc, List<object> args)
	{
		m_onResetPasswordFail();
	}

	private void RPC_ReplayData(PTech.RPC rpc, List<object> args)
	{
		int majorVersion = (int)args[0];
		byte[] replayData = (byte[])args[1];
		ReplayData(m_watchReplayName, majorVersion, replayData);
	}

	private void RPC_ReplayMissing(PTech.RPC rpc, List<object> args)
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$replay_missing", OnMsgBoxOk);
	}

	protected override void CloseMenu()
	{
		if (m_menu != null)
		{
			m_menu.Close();
			m_menu = null;
		}
	}

	private void OnJoinGame(int gameID)
	{
		if (m_game == null && !m_waitingForJoinResponse)
		{
			m_rpc.Invoke("Join", gameID);
			m_waitingForJoinResponse = true;
		}
	}

	private void OnMenuWatchReplay(int gameID, string replayName)
	{
		WatchReplay(gameID, replayName);
	}

	private void WatchReplay(int gameID, string replayName)
	{
		m_watchReplayName = replayName;
		m_rpc.Invoke("WatchReplay", gameID);
	}

	private void RPC_JoinOK(PTech.RPC rpc, List<object> args)
	{
		if (m_game == null)
		{
			CloseMenu();
			m_game = new ClientGame(m_rpc, m_guiCamera, m_userManClient, m_mapMan, m_musicMan, replayMode: false, onlineGame: true, m_hostName);
			m_game.m_onExit = OnExitGame;
			m_game.m_onQuitGame = OnQuitGame;
			m_waitingForJoinResponse = false;
		}
	}

	private void RPC_JoinFail(PTech.RPC rpc, List<object> args)
	{
		m_waitingForJoinResponse = false;
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$JoinGameFailed", OnMsgBoxOk);
		if (m_menu == null)
		{
			SetupMenu(MenuBase.StartStatus.ShowGameView);
		}
	}

	private void OnMsgBoxOk()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void OnExitGame(ExitState exitState, int joinGameID)
	{
		switch (exitState)
		{
		case ExitState.Normal:
		case ExitState.Kicked:
			SetupMenu(MenuBase.StartStatus.ShowGameView);
			if (exitState == ExitState.Kicked)
			{
				m_menu.OnKicked();
			}
			return;
		case ExitState.ShowCredits:
			SetupMenu(MenuBase.StartStatus.ShowCredits);
			return;
		}
		DebugUtils.Assert(joinGameID > 0);
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		main.LoadLevel("menu", showLoadScreen: true);
		if (exitState == ExitState.JoinGame)
		{
			m_rpc.Invoke("Join", joinGameID);
			m_waitingForJoinResponse = true;
		}
	}

	protected override void OnQuitGame()
	{
		PLog.LogWarning("onlinestate recives delegate that we should quit the game. Stacktrace:\n" + StackTraceUtility.ExtractStackTrace());
		Close();
		m_onQuitGame();
	}

	private void OnLogout()
	{
		m_rpc.Close();
		m_onDisconnect(error: false);
	}

	private void OnItemBought()
	{
		PLog.Log("Updating owned items");
		RegisterGDPPackets();
	}

	private void SetupChooseFleet()
	{
		CloseMenu();
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		Thread.Sleep(0);
		m_chooseFleetDialog = new ChooseFleetMenu(m_guiCamera, m_userManClient, FleetSizeClass.None, null, "online", 0);
		m_chooseFleetDialog.m_onExit = OnChooseFleetDialogExit;
		m_chooseFleetDialog.m_onProceed = OnChooseFleetDialogProceed;
	}

	private void OnChooseFleetDialogExit()
	{
		if (m_chooseFleetDialog != null)
		{
			ChooseFleetMenu chooseFleetDialog = m_chooseFleetDialog;
			chooseFleetDialog.m_onExit = (ChooseFleetMenu.OnExitDelegate)Delegate.Remove(chooseFleetDialog.m_onExit, new ChooseFleetMenu.OnExitDelegate(OnChooseFleetDialogExit));
			m_chooseFleetDialog.Close();
			m_chooseFleetDialog = null;
		}
	}

	private void OnChooseFleetDialogProceed(string selectedFleetName, int campaignID)
	{
		CloseMenu();
		if (m_game != null)
		{
			m_game.Close();
			m_game = null;
		}
		m_fleetEditor = new FleetMenu(m_guiCamera, m_userManClient, selectedFleetName, campaignID, new FleetSize(0, 8000), oneFleetOnly: false, m_musicMan);
		m_fleetEditor.m_onExit = OnFleetEditorExit;
	}

	private void OnFleetEditorExit()
	{
		if (m_fleetEditor != null)
		{
			FleetMenu fleetEditor = m_fleetEditor;
			fleetEditor.m_onExit = (FleetMenu.OnExitDelegate)Delegate.Remove(fleetEditor.m_onExit, new FleetMenu.OnExitDelegate(OnFleetEditorExit));
			m_fleetEditor.Close();
			m_fleetEditor = null;
		}
		SetupMenu(MenuBase.StartStatus.ShowShipyard);
	}
}
