using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using PTech;
using UnityEngine;

internal class OfflineState : StateBase
{
	public Action<bool> m_onBack;

	public Action m_onQuitGame;

	private OfflineGame m_offlineGame;

	private OfflineMenu m_menu;

	private User m_user;

	private PackMan m_packMan;

	private OfflineGameDB m_gameDB;

	public OfflineState(GameObject guiCamera, MusicManager musMan, GDPBackend gdpBackend)
		: base(guiCamera, musMan, gdpBackend)
	{
		m_guiCamera = guiCamera;
		m_mapMan = new ClientMapMan();
		m_packMan = new PackMan();
		m_gameDB = new OfflineGameDB();
		m_user = new User("Admiral", 1);
		LoadUser();
		m_userManClient = new UserManClientLocal(m_user, m_packMan, m_mapMan, m_gdpBackend);
		if (m_gdpBackend != null)
		{
			RegisterGDPPackets();
		}
		else
		{
			AddAllContentPacks(m_user);
		}
		SetupMenu(MenuBase.StartStatus.Normal);
	}

	protected override void SetupMenu(MenuBase.StartStatus status)
	{
		base.SetupMenu(status);
		if (m_offlineGame != null)
		{
			m_offlineGame.Close();
			m_offlineGame = null;
		}
		m_musicMan.SetMusic("menu");
		m_menu = new OfflineMenu(m_guiCamera, m_userManClient, m_gameDB, status, m_musicMan);
		m_menu.m_onBack = OnMenuBack;
		m_menu.m_onStartLevel = OnMenuStartLevel;
		m_menu.m_onJoin = OnMenuJoinGame;
		m_menu.m_onWatchReplay = OnMenuWatchReplay;
		main.LoadLevel("menu", showLoadScreen: true);
	}

	public override void Close()
	{
		base.Close();
		SaveUser();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (m_offlineGame != null)
		{
			m_offlineGame.FixedUpdate();
		}
	}

	public override void OnLevelWasLoaded()
	{
		base.OnLevelWasLoaded();
		if (m_offlineGame != null)
		{
			m_offlineGame.OnLevelWasLoaded();
		}
	}

	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		if (m_offlineGame != null)
		{
			m_offlineGame.Update(deltaTime);
		}
		if (m_menu != null)
		{
			m_menu.Update(deltaTime);
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (m_offlineGame != null)
		{
			m_offlineGame.LateUpdate();
		}
	}

	public void OnGui()
	{
	}

	private void OnMenuBack()
	{
		if (m_onBack != null)
		{
			m_onBack(obj: false);
		}
	}

	protected override void OnQuitGame()
	{
		m_offlineGame.Close();
		m_offlineGame = null;
		if (m_onQuitGame != null)
		{
			m_onQuitGame();
		}
	}

	private void OnSaveUserRequest()
	{
		SaveUser();
	}

	private void OnMenuStartLevel(GameType mode, string campaign, string levelName)
	{
		CloseMenu();
		int num = m_user.m_gamesCreated + 1;
		m_user.m_gamesCreated++;
		string gameName = "Game " + num;
		m_offlineGame = new OfflineGame(campaign, levelName, gameName, mode, m_user, m_userManClient, m_mapMan, m_packMan, m_guiCamera, m_musicMan, 1, FleetSizeClass.Heavy, 1f, m_gameDB);
		m_offlineGame.m_onExit = OnExitGame;
		m_offlineGame.m_onQuitGame = OnQuitGame;
		m_offlineGame.m_onSaveUserRequest = OnSaveUserRequest;
	}

	private void OnMenuJoinGame(int gameID)
	{
		CloseMenu();
		byte[] data = m_gameDB.LoadGame(gameID, replay: false);
		m_offlineGame = new OfflineGame(string.Empty, data, replay: false, m_user, m_userManClient, m_mapMan, m_packMan, m_guiCamera, m_musicMan, m_gameDB);
		m_offlineGame.m_onExit = OnExitGame;
		m_offlineGame.m_onQuitGame = OnQuitGame;
		m_offlineGame.m_onSaveUserRequest = OnSaveUserRequest;
	}

	private void OnMenuWatchReplay(int gameID, string replayName)
	{
		int majorVersion = VersionInfo.m_majorVersion;
		byte[] replayData = m_gameDB.LoadGame(gameID, replay: true);
		ReplayData(replayName, majorVersion, replayData);
	}

	private void OnExitGame(ExitState exitState)
	{
		PLog.Log("Disconnect");
		m_offlineGame.Close();
		m_offlineGame = null;
		SaveUser();
		switch (exitState)
		{
		case ExitState.Normal:
		case ExitState.Kicked:
			SetupMenu(MenuBase.StartStatus.ShowGameView);
			break;
		case ExitState.ShowCredits:
			SetupMenu(MenuBase.StartStatus.ShowCredits);
			break;
		}
	}

	private void AddAllContentPacks(User user)
	{
		//Discarded unreachable code: IL_0087
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		UnityEngine.Object[] array = Resources.LoadAll("shared_settings/packs");
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			TextAsset textAsset = @object as TextAsset;
			if (!(textAsset == null))
			{
				XmlReader xmlReader = XmlReader.Create(new StringReader(textAsset.text), xmlReaderSettings);
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.Load(xmlReader);
				}
				catch (XmlException ex)
				{
					PLog.LogError("Parse error " + ex.ToString());
					continue;
				}
				ContentPack contentPack = new ContentPack();
				contentPack.Load(xmlDocument);
				bool unlockAllMaps = false;
				if (contentPack.m_dev)
				{
					user.AddContentPack(contentPack, m_mapMan, unlockAllMaps);
				}
				else
				{
					user.AddContentPack(contentPack, m_mapMan, unlockAllMaps);
				}
			}
		}
	}

	private void LoadUser()
	{
		string path = Application.persistentDataPath + "/user.dat";
		try
		{
			FileStream fileStream = new FileStream(path, FileMode.Open);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			int nextGameID = binaryReader.ReadInt32();
			Game.SetNextGameID(nextGameID);
			int nextCampaignID = binaryReader.ReadInt32();
			Game.SetNextCampaignID(nextCampaignID);
			m_user.OfflineLoad(binaryReader, m_packMan);
			fileStream.Close();
			PLog.Log("Loaded user");
		}
		catch (FileNotFoundException)
		{
			PLog.Log("No user.dat found, assuming new install");
		}
		catch (IsolatedStorageException)
		{
			PLog.Log("No user.dat found, assuming new install");
		}
		catch (IOException)
		{
			PLog.LogError("IOerror while loading user, try clearing your application data");
		}
	}

	private void SaveUser()
	{
		try
		{
			string path = Application.persistentDataPath + "/user.dat";
			FileStream fileStream = new FileStream(path, FileMode.Create);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(Game.GetNextGameID());
			binaryWriter.Write(Game.GetNextCampaignID());
			m_user.OfflineSave(binaryWriter);
			fileStream.Close();
			PLog.Log("SAVED user");
		}
		catch (IOException)
		{
			PLog.LogError("IOerror while saving user");
		}
	}

	protected override void CloseMenu()
	{
		if (m_menu != null)
		{
			m_menu.Close();
			m_menu = null;
		}
	}
}
