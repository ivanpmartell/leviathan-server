using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class OfflineMenu : MenuBase
{
	private class PlayTab
	{
		public List<GamePost> m_games = new List<GamePost>();

		public UIScrollList m_gameList;

		public GameObject m_gameListItem;

		public UIPanel m_panel;
	}

	private class ArchiveTab
	{
		public List<GamePost> m_games = new List<GamePost>();

		public UIScrollList m_gameList;

		public GameObject m_gameListItem;
	}

	public delegate void BackHandler();

	public delegate void StartLevelHandler(GameType mode, string campaign, string levelName);

	public BackHandler m_onBack;

	public Action<int> m_onJoin;

	public Action<int, string> m_onWatchReplay;

	public StartLevelHandler m_onStartLevel;

	private MapMan m_mapMan = new ClientMapMan();

	private UserManClient m_userMan;

	private OfflineGameDB m_gameDB;

	private GameObject m_guiCamera;

	private CreateGame m_createGame;

	private GameObject m_gui;

	private UIPanelManager m_panelMan;

	private PlayTab m_playTab = new PlayTab();

	private ArchiveTab m_archiveTab = new ArchiveTab();

	private MsgBox m_msgBox;

	private Credits m_credits;

	private MusicManager m_musicMan;

	public OfflineMenu(GameObject guiCamera, UserManClient userMan, OfflineGameDB gameDB, StartStatus startStatus, MusicManager musicMan)
	{
		m_userMan = userMan;
		m_gameDB = gameDB;
		m_guiCamera = guiCamera;
		m_musicMan = musicMan;
		m_gui = GuiUtils.CreateGui("MainmenuOffline", guiCamera);
		m_panelMan = GuiUtils.FindChildOf(m_gui, "PanelMan").GetComponent<UIPanelManager>();
		GuiUtils.FindChildOf(m_gui, "VersionLbl").GetComponent<SpriteText>().Text = Localize.instance.Translate("$version " + VersionInfo.GetFullVersionString());
		GuiUtils.FindChildOf(m_gui, "CreateGameButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOpenCreateGame);
		GuiUtils.FindChildOf(m_gui, "ButtonPlay").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowPlay);
		GuiUtils.FindChildOf(m_gui, "ButtonCredits").GetComponent<UIButton>().SetValueChangedDelegate(OnShowCreditsPressed);
		GuiUtils.FindChildOf(m_gui, "ExitButton").GetComponent<UIButton>().SetValueChangedDelegate(OnBackPressed);
		GuiUtils.FindChildOf(m_gui, "OptionsButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOptionsPressed);
		GuiUtils.FindChildOf(m_gui, "ButtonArchive").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowArchive);
		GuiUtils.FindChildOf(m_gui, "HelpButton").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		GameObject gameObject = m_gui.transform.FindChild("CreateGameView").gameObject;
		m_createGame = new CreateGame(m_guiCamera, gameObject, m_mapMan, m_userMan);
		m_createGame.m_onClose = OnCreateGameClose;
		m_createGame.m_onCreateGame = OnCreateGame;
		m_playTab.m_gameListItem = GuiUtils.CreateGui("gamelist/Gamelist_listitem", m_guiCamera);
		m_playTab.m_gameListItem.transform.Translate(100000f, 0f, 0f);
		m_playTab.m_gameList = GuiUtils.FindChildOf(m_gui, "CurrentGamesScrollList").GetComponent<UIScrollList>();
		m_playTab.m_panel = GuiUtils.FindChildOf(m_gui, "PanelPlay").GetComponent<UIPanel>();
		m_archiveTab.m_gameListItem = GuiUtils.FindChildOf(m_gui, "ArchivedGameListItem");
		m_archiveTab.m_gameList = GuiUtils.FindChildOf(m_gui, "ArchivedGamesScrollList").GetComponent<UIScrollList>();
		GameObject creditsRoot = GuiUtils.FindChildOf(m_gui, "PanelCredits");
		m_credits = new Credits(creditsRoot, m_musicMan);
		if (PlayerPrefs.GetInt("playedtutorial", 0) != 1)
		{
			m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$mainmenu_starttutorial", OnStartTutorialYes, OnStartTutorialNo);
		}
		switch (startStatus)
		{
		case StartStatus.ShowArchiveView:
			m_panelMan.BringIn("PanelArchive");
			FillReplayGameList();
			break;
		case StartStatus.ShowCredits:
			m_credits.Start();
			break;
		default:
			m_panelMan.BringIn("PanelPlay");
			FillGameList();
			break;
		}
	}

	private void onHelp(IUIObject button)
	{
		ToolTipDisplay toolTip = ToolTipDisplay.GetToolTip(m_gui);
		if (!(toolTip == null))
		{
			if (button.gameObject.GetComponent<UIStateToggleBtn>().StateNum == 0)
			{
				toolTip.SetHelpMode(helpMode: false);
			}
			if (button.gameObject.GetComponent<UIStateToggleBtn>().StateNum == 1)
			{
				toolTip.SetHelpMode(helpMode: true);
			}
		}
	}

	private void OnStartTutorialYes()
	{
		m_msgBox.Close();
		m_msgBox = null;
		PlayerPrefs.SetInt("playedtutorial", 1);
		m_createGame.CreateCampaignGame("t1_name", 0, 1);
	}

	private void OnStartTutorialNo()
	{
		m_msgBox.Close();
		m_msgBox = null;
		PlayerPrefs.SetInt("playedtutorial", 1);
	}

	private void OnOpenCreateGame(IUIObject obj)
	{
		m_createGame.Show();
	}

	private void OnCreateGameClose()
	{
		m_panelMan.BringIn("PanelPlay");
	}

	private void OnCreateGame(GameType gametype, string campaign, string mapName, int players, int fleetSize, float targetScore, double turnTime, bool autoJoin)
	{
		m_onStartLevel(gametype, campaign, mapName);
	}

	private void OnShowPlay(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIPanel component = GuiUtils.FindChildOf(m_gui, "PanelPlay").GetComponent<UIPanel>();
			component.AddTempTransitionDelegate(OnPlayTransitionComplete);
		}
	}

	private void OnPlayTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		FillGameList();
	}

	private void OnShowCreditsPressed(IUIObject obj)
	{
		m_credits.Start();
	}

	private void FillGameList()
	{
		m_playTab.m_games.Clear();
		m_playTab.m_games = m_gameDB.GetGameList();
		m_playTab.m_games.Sort();
		float scrollPosition = m_playTab.m_gameList.ScrollPosition;
		m_playTab.m_gameList.ClearList(destroy: true);
		foreach (GamePost game in m_playTab.m_games)
		{
			string text = ((game.m_turn < 0) ? "-" : (game.m_turn + 1).ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate(m_playTab.m_gameListItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "NameValueLabel").GetComponent<SpriteText>().Text = game.m_gameName;
			GuiUtils.FindChildOf(gameObject, "TypeValueLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey("gametype_" + game.m_gameType.ToString().ToLower());
			GuiUtils.FindChildOf(gameObject, "MapValueLabel").GetComponent<SpriteText>().Text = CreateGame.TranslatedMapName(game.m_level);
			GuiUtils.FindChildOf(gameObject, "PlayersValueLabel").GetComponent<SpriteText>().Text = game.m_connectedPlayers + "/" + game.m_maxPlayers;
			GuiUtils.FindChildOf(gameObject, "TurnValueLabel").GetComponent<SpriteText>().Text = text;
			GuiUtils.FindChildOf(gameObject, "CreatedValueLabel").GetComponent<SpriteText>().Text = game.m_createDate.ToString("yyyy-MM-d HH:mm");
			GuiUtils.FindChildOf(gameObject, "Button").GetComponent<UIButton>().SetValueChangedDelegate(OnGameListSelection);
			UIListItemContainer component = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component);
			m_playTab.m_gameList.AddItem(component);
		}
	}

	private void OnGameListSelection(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		GamePost gamePost = m_playTab.m_games[component.Index];
		m_onJoin(gamePost.m_gameID);
	}

	private void OnShowArchive(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIPanel component = GuiUtils.FindChildOf(m_gui, "PanelArchive").GetComponent<UIPanel>();
			component.AddTempTransitionDelegate(OnArchiveTransitionComplete);
		}
	}

	private void OnArchiveTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		FillReplayGameList();
	}

	private void FillReplayGameList()
	{
		m_archiveTab.m_games = m_gameDB.GetReplayList();
		m_archiveTab.m_games.Sort();
		m_archiveTab.m_gameList.ClearList(destroy: true);
		foreach (GamePost game in m_archiveTab.m_games)
		{
			string text = ((game.m_turn < 0) ? "-" : (game.m_turn + 1).ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate(m_archiveTab.m_gameListItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "NameLbl").GetComponent<SpriteText>().Text = game.m_gameName;
			GuiUtils.FindChildOf(gameObject, "TypeLbl").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey("gametype_" + game.m_gameType.ToString().ToLower());
			GuiUtils.FindChildOf(gameObject, "MapLbl").GetComponent<SpriteText>().Text = CreateGame.TranslatedMapName(game.m_level);
			GuiUtils.FindChildOf(gameObject, "PlayersLbl").GetComponent<SpriteText>().Text = game.m_maxPlayers.ToString();
			GuiUtils.FindChildOf(gameObject, "TurnLbl").GetComponent<SpriteText>().Text = text;
			GuiUtils.FindChildOf(gameObject, "CreatedLbl").GetComponent<SpriteText>().Text = game.m_createDate.ToString("yyyy-MM-d HH:mm");
			GuiUtils.FindChildOf(gameObject, "ArchivedGameListitemRadioButton").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnReplayArchiveGameSelected);
			GuiUtils.FindChildOf(gameObject, "RemoveButton").GetComponent<UIButton>().SetValueChangedDelegate(OnRemoveReplay);
			UIListItemContainer component = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component);
			m_archiveTab.m_gameList.AddItem(component);
		}
		m_archiveTab.m_gameList.ScrollToItem(0, 0f);
	}

	private void OnReplayArchiveGameSelected(IUIObject obj)
	{
		int index = obj.transform.parent.GetComponent<UIListItemContainer>().Index;
		GamePost gamePost = m_archiveTab.m_games[index];
		m_onWatchReplay(gamePost.m_gameID, gamePost.m_gameName);
	}

	private void OnRemoveReplay(IUIObject obj)
	{
		int index = obj.transform.parent.GetComponent<UIListItemContainer>().Index;
		GamePost gamePost = m_archiveTab.m_games[index];
		m_gameDB.RemoveReplay(gamePost.m_gameID);
		FillReplayGameList();
	}

	private void OnOptionsPressed(IUIObject obj)
	{
		OptionsWindow optionsWindow = new OptionsWindow(m_guiCamera, inGame: false);
	}

	public void Close()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_credits = null;
		UnityEngine.Object.Destroy(m_gui);
		UnityEngine.Object.Destroy(m_playTab.m_gameListItem);
		UnityEngine.Object.Destroy(m_archiveTab.m_gameListItem);
		m_gui = null;
	}

	public void Update(float dt)
	{
		m_credits.Update(dt);
	}

	private void OnBackPressed(IUIObject obj)
	{
		if (m_onBack != null)
		{
			Close();
			m_onBack();
		}
	}
}
