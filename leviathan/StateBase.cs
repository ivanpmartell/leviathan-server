using System.Collections.Generic;
using PTech;
using UnityEngine;

internal abstract class StateBase
{
	protected GameObject m_guiCamera;

	protected MsgBox m_msgBox;

	protected UserManClient m_userManClient;

	protected MapMan m_mapMan;

	protected MusicManager m_musicMan;

	protected GDPBackend m_gdpBackend;

	private OfflineGame m_replayGame;

	private string m_watchReplayName = string.Empty;

	private byte[] m_tempReplayData;

	public StateBase(GameObject guiCamera, MusicManager musMan, GDPBackend gdpBackend)
	{
		m_guiCamera = guiCamera;
		m_musicMan = musMan;
		m_gdpBackend = gdpBackend;
	}

	public virtual void Close()
	{
		CloseMenu();
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		if (m_replayGame != null)
		{
			m_replayGame.Close();
			m_replayGame = null;
		}
	}

	public virtual void FixedUpdate()
	{
		if (m_replayGame != null)
		{
			m_replayGame.FixedUpdate();
		}
	}

	public virtual void Update()
	{
		if (m_replayGame != null)
		{
			m_replayGame.Update(Time.deltaTime);
		}
	}

	public virtual void LateUpdate()
	{
		if (m_replayGame != null)
		{
			m_replayGame.LateUpdate();
		}
	}

	public virtual void OnLevelWasLoaded()
	{
		if (m_replayGame != null)
		{
			m_replayGame.OnLevelWasLoaded();
		}
	}

	protected void ReplayData(string replayName, int majorVersion, byte[] replayData)
	{
		m_watchReplayName = replayName;
		m_tempReplayData = replayData;
		if (VersionInfo.m_majorVersion != majorVersion)
		{
			m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$replay_version_missmatch", OnReplayVersionVarningYes, OnReplayVersionVarningNo);
		}
		else
		{
			StartReplay();
		}
	}

	private void OnReplayVersionVarningYes()
	{
		m_msgBox.Close();
		StartReplay();
	}

	private void OnReplayVersionVarningNo()
	{
		m_tempReplayData = null;
		m_msgBox.Close();
	}

	private void StartReplay()
	{
		CloseMenu();
		PackMan packMan = new PackMan();
		User user = new User("player", 1);
		UserManClient userManClient = new UserManClientLocal(user, packMan, m_mapMan, m_gdpBackend);
		m_replayGame = new OfflineGame(m_watchReplayName, m_tempReplayData, replay: true, user, userManClient, m_mapMan, packMan, m_guiCamera, m_musicMan, null);
		m_replayGame.m_onExit = OnExitReplay;
		m_replayGame.m_onQuitGame = OnQuitGame;
		m_tempReplayData = null;
	}

	protected virtual void OnQuitGame()
	{
	}

	private void OnExitReplay(ExitState exitState)
	{
		SetupMenu(MenuBase.StartStatus.ShowArchiveView);
	}

	protected abstract void CloseMenu();

	protected virtual void SetupMenu(MenuBase.StartStatus status)
	{
		if (m_replayGame != null)
		{
			m_replayGame.Close();
			m_replayGame = null;
		}
	}

	protected void RegisterGDPPackets()
	{
		List<GDPBackend.GDPOwnedItem> list = m_gdpBackend.RequestOwned();
		List<string> list2 = new List<string>();
		foreach (GDPBackend.GDPOwnedItem item in list)
		{
			list2.Add(item.m_packName);
		}
		m_userManClient.SetOwnedPackages(list2);
	}
}
