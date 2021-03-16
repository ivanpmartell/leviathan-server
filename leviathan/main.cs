using System;
using UnityEngine;

public class main : MonoBehaviour
{
	private static string m_nextLevel = string.Empty;

	public GameObject m_guiCamera;

	private static LoadScreen m_loadScreen;

	private static GameObject m_loadScreenGuiCamera;

	private ConnectMenu m_connectMenu;

	private OnlineState m_onlineState;

	private Splash m_splash;

	private OfflineState m_offlineState;

	private MusicManager m_musicMan;

	private GDPBackend m_gdpBackend;

	private MsgBox m_msgbox;

	private PdxNews m_pdxNews;

	private int m_showSplash = 5;

	private void Start()
	{
		Application.targetFrameRate = 60;
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
		Gun.RegisterAIStates();
		Ship.RegisterAIStates();
		AudioSource[] components = GetComponents<AudioSource>();
		m_musicMan = new MusicManager(components);
		m_musicMan.SetVolume(PlayerPrefs.GetFloat("MusicVolume", 0.5f));
		AudioManager.instance.SetVolume(PlayerPrefs.GetFloat("SfxVolume", 1f));
		string @string = PlayerPrefs.GetString("Language", "english");
		Localize.instance.SetLanguage(@string);
		m_loadScreenGuiCamera = m_guiCamera;
		CheatMan cheatMan = new CheatMan();
	}

	private bool VideoModeCheck()
	{
		if (Screen.width < 1024 || Screen.height < 720)
		{
			Screen.SetResolution(1024, 720, fullscreen: false);
			return false;
		}
		return true;
	}

	private void SetupSplash()
	{
		PLog.Log("Setup splash screen");
		m_splash = new Splash(m_guiCamera, m_musicMan);
		m_splash.m_onDone = OnSplashDone;
		m_splash.m_onFadeoutComplete = OnSplashFadedOut;
	}

	private void OnSplashDone()
	{
		bool flag = SetupGDP();
		m_pdxNews = new PdxNews("leviathan", live: false);
		if (flag)
		{
			SetupConnectMenu(showLoadscreen: false);
		}
	}

	private void OnSplashFadedOut()
	{
		m_splash.Close();
		m_splash = null;
	}

	private void OnApplicationPause(bool pause)
	{
		PLog.Log("OnApplicationPause: " + pause);
	}

	private bool SetupGDP()
	{
		//Discarded unreachable code: IL_0052
		try
		{
			bool live = true;
			m_gdpBackend = new PSteam(live);
		}
		catch (Exception ex)
		{
			PLog.LogWarning("PSteam exception:" + ex.ToString());
			m_msgbox = MsgBox.CreateOkMsgBox(m_guiCamera, "$steam_init_failed ", OnNoSteamOk);
			return false;
		}
		if (!m_gdpBackend.IsBackendOnline())
		{
			m_msgbox = MsgBox.CreateOkMsgBox(m_guiCamera, "$steam_offline_mode_only", null);
		}
		return true;
	}

	private void OnNoSteamOk()
	{
		Application.Quit();
	}

	private void SetupConnectMenu(bool showLoadscreen)
	{
		if (m_offlineState != null)
		{
			m_offlineState.Close();
			m_offlineState = null;
		}
		if (m_onlineState != null)
		{
			m_onlineState.Close();
			m_onlineState = null;
		}
		if (m_connectMenu != null)
		{
			m_connectMenu.Close();
			m_connectMenu = null;
		}
		m_connectMenu = new ConnectMenu(m_guiCamera, m_musicMan, m_pdxNews, m_gdpBackend);
		m_connectMenu.m_onConnect = OnConnect;
		m_connectMenu.m_onCreateAccount = OnCreateAccount;
		m_connectMenu.m_onRequestResetPasswordCode = OnRequestResetPasswordCode;
		m_connectMenu.m_onRequestVerificationMail = OnRequestVerificationEmail;
		m_connectMenu.m_onResetPassword = OnPasswordReset;
		m_connectMenu.m_onSinglePlayer = OnSinglePlayer;
		m_connectMenu.m_onExit = OnQuit;
		LoadLevel("menu", showLoadscreen);
	}

	private void FixedUpdate()
	{
		if (m_gdpBackend != null)
		{
			m_gdpBackend.Update();
		}
		if (m_onlineState != null)
		{
			m_onlineState.FixedUpdate();
		}
		if (m_offlineState != null)
		{
			m_offlineState.FixedUpdate();
		}
	}

	private void Update()
	{
		Utils.UpdateAndroidBack();
		if (m_showSplash >= 0)
		{
			if (m_showSplash == 1)
			{
				VideoModeCheck();
			}
			if (m_showSplash == 0)
			{
				SetupSplash();
			}
			m_showSplash--;
		}
		if (m_loadScreen != null)
		{
			m_loadScreen.Update();
		}
		if (m_splash != null)
		{
			m_splash.Update();
		}
		if (m_connectMenu != null)
		{
			m_connectMenu.Update();
		}
		if (m_onlineState != null)
		{
			m_onlineState.Update();
		}
		if (m_offlineState != null)
		{
			m_offlineState.Update();
		}
		if (m_msgbox != null)
		{
			m_msgbox.Update();
		}
		AudioManager.instance.Update(Time.deltaTime);
		m_musicMan.Update(Time.deltaTime);
	}

	private void LateUpdate()
	{
		if (m_onlineState != null)
		{
			m_onlineState.LateUpdate();
		}
		if (m_offlineState != null)
		{
			m_offlineState.LateUpdate();
		}
	}

	private void OnLevelWasLoaded()
	{
		if (Application.loadedLevelName == "empty")
		{
			PLog.Log(" purging unused resources ");
			Resources.UnloadUnusedAssets();
			GC.Collect();
			PLog.Log(" loading actual scene");
			Application.LoadLevel(m_nextLevel);
			m_nextLevel = string.Empty;
			return;
		}
		PLog.Log(" done loading " + Application.loadedLevelName);
		if (m_onlineState != null)
		{
			m_onlineState.OnLevelWasLoaded();
		}
		if (m_offlineState != null)
		{
			m_offlineState.OnLevelWasLoaded();
		}
		if (m_loadScreen != null)
		{
			m_loadScreen.SetVisible(visible: false);
		}
	}

	public static void LoadLevel(string name, bool showLoadScreen)
	{
		if (!(Application.loadedLevelName == name))
		{
			PLog.Log("Level change started: " + name);
			if (m_loadScreenGuiCamera != null && showLoadScreen)
			{
				m_loadScreen = new LoadScreen(m_loadScreenGuiCamera);
				m_loadScreen.SetImage(string.Empty);
				m_loadScreen.SetVisible(visible: true);
			}
			m_nextLevel = name;
			Application.LoadLevel("empty");
		}
	}

	private void OnGUI()
	{
		if (m_onlineState != null)
		{
			m_onlineState.OnGui();
		}
		if (m_offlineState != null)
		{
			m_offlineState.OnGui();
		}
	}

	private void OnConnect(string hostVisualName, string host, int port, string userName, string password, string token)
	{
		if (m_onlineState != null)
		{
			PLog.Log("Already trying to connect");
			return;
		}
		m_onlineState = new OnlineState(m_guiCamera, m_musicMan, m_gdpBackend, m_pdxNews);
		m_onlineState.m_onLoginFailed = OnLoginFail;
		m_onlineState.m_onLoginSuccess = OnLoginSuccess;
		m_onlineState.m_onDisconnect = OnDisconnected;
		m_onlineState.m_onQuitGame = OnQuit;
		if (!m_onlineState.Login(hostVisualName, host, port, userName, password, token))
		{
			m_onlineState.Close();
			m_onlineState = null;
			m_connectMenu.OnConnectFailed();
		}
	}

	private void OnCreateAccount(string host, int port, string userName, string password, string email)
	{
		if (m_onlineState != null)
		{
			PLog.Log("Already trying to connect");
			return;
		}
		m_onlineState = new OnlineState(m_guiCamera, m_musicMan, m_gdpBackend, m_pdxNews);
		m_onlineState.m_onCreateFailed = OnCreateFail;
		m_onlineState.m_onCreateSuccess = OnCreateSuccess;
		m_onlineState.m_onDisconnect = OnDisconnected;
		if (!m_onlineState.CreateAccount(host, port, userName, password, email))
		{
			m_onlineState.Close();
			m_onlineState = null;
			m_connectMenu.OnConnectFailed();
		}
	}

	private void OnRequestVerificationEmail(string host, int port, string email)
	{
		if (m_onlineState != null)
		{
			PLog.Log("Already trying to connect");
			return;
		}
		m_onlineState = new OnlineState(m_guiCamera, m_musicMan, m_gdpBackend, m_pdxNews);
		m_onlineState.m_onRequestVerificationRespons = OnRequestVerificationRespons;
		m_onlineState.m_onDisconnect = OnDisconnected;
		if (!m_onlineState.RequestVerificationEmail(host, port, email))
		{
			m_onlineState.Close();
			m_onlineState = null;
			m_connectMenu.OnConnectFailed();
		}
	}

	private void OnRequestVerificationRespons(ErrorCode errorCode)
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnRequestVerificaionRespons(errorCode);
	}

	private void OnRequestResetPasswordCode(string host, int port, string email)
	{
		if (m_onlineState != null)
		{
			PLog.Log("Already trying to connect");
			return;
		}
		m_onlineState = new OnlineState(m_guiCamera, m_musicMan, m_gdpBackend, m_pdxNews);
		if (!m_onlineState.RequestPasswordReset(host, port, email))
		{
			m_onlineState.Close();
			m_onlineState = null;
			m_connectMenu.OnConnectFailed();
		}
		else
		{
			m_onlineState.Close();
			m_onlineState = null;
		}
	}

	private void OnPasswordReset(string host, int port, string email, string token, string password)
	{
		if (m_onlineState != null)
		{
			PLog.Log("Already trying to connect");
			return;
		}
		m_onlineState = new OnlineState(m_guiCamera, m_musicMan, m_gdpBackend, m_pdxNews);
		m_onlineState.m_onResetPasswordOk = OnResetPasswordOk;
		m_onlineState.m_onResetPasswordFail = OnResetPasswordFail;
		m_onlineState.m_onDisconnect = OnDisconnected;
		if (!m_onlineState.ResetPassword(host, port, email, token, password))
		{
			m_onlineState.Close();
			m_onlineState = null;
			m_connectMenu.OnConnectFailed();
		}
	}

	private void OnResetPasswordFail()
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnResetPasswordFail();
	}

	private void OnResetPasswordOk()
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnResetPasswordConfirmed();
	}

	private void OnLoginFail(ErrorCode errorCode)
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnLoginFailed(errorCode);
	}

	private void OnCreateFail(ErrorCode errorCode)
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnCreateFailed(errorCode);
	}

	private void OnCreateSuccess()
	{
		m_onlineState.Close();
		m_onlineState = null;
		m_connectMenu.OnCreateSuccess();
	}

	private void OnLoginSuccess()
	{
		m_connectMenu.Close();
		m_connectMenu = null;
	}

	private void OnDisconnected(bool error)
	{
		PLog.Log("Disconnected");
		if (error)
		{
			m_msgbox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_connectfail", null);
		}
		SetupConnectMenu(showLoadscreen: true);
	}

	private void OnQuit()
	{
		PLog.Log("On quit");
		if (m_gdpBackend != null)
		{
			m_gdpBackend.Close();
			m_gdpBackend = null;
		}
		PlayerPrefs.SetString("Language", Localize.instance.GetLanguage());
		PLog.Log("Application QUIT");
		Application.Quit();
	}

	private void OnSinglePlayer()
	{
		m_connectMenu.Close();
		m_connectMenu = null;
		m_offlineState = new OfflineState(m_guiCamera, m_musicMan, m_gdpBackend);
		m_offlineState.m_onBack = OnDisconnected;
		m_offlineState.m_onQuitGame = OnQuit;
	}
}
