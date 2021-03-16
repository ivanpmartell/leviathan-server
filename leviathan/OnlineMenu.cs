#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class OnlineMenu : MenuBase
{
	private class NewsTab
	{
		public GameObject m_newsItem;

		public UIScrollList m_newsList;
	}

	private class PlayTab
	{
		public List<GamePost> m_games = new List<GamePost>();

		public UIScrollList m_gameList;

		public GameObject m_gameListItem;

		public UIPanel m_panel;

		public UIStateToggleBtn m_listPublicGames;
	}

	private class ArchiveTab
	{
		public List<GamePost> m_games = new List<GamePost>();

		public UIScrollList m_gameList;

		public GameObject m_gameListItem;

		public GameObject m_playReplayDialog;
	}

	private class FriendsTab
	{
		public UIPanel m_panel;

		public List<FriendData> m_friends = new List<FriendData>();

		public UIScrollList m_friendList;

		public GameObject m_friendListItem;

		public GameObject m_confirmedFriendListItem;

		public GameObject m_pendingFriendListItem;

		public GameObject m_addFriendDialog;
	}

	private class FleetTab
	{
		public GameObject m_gameFleetItem;

		public List<string> m_fleets = new List<string>();

		public UIScrollList m_fleetList;

		public string m_selectedFleet;
	}

	private class ProfileTab
	{
		public List<int> m_flags;
	}

	private class MatchMaking
	{
		public GameObject m_progressDialog;

		public GameObject m_panel;

		public UIRadioBtn m_gameTypeCampaign;

		public UIRadioBtn m_gameTypeChallenge;

		public UIRadioBtn m_gameTypePoints;

		public UIRadioBtn m_gameTypeAssassination;

		public UIRadioBtn m_playersAny;

		public UIRadioBtn m_playersTwo;

		public UIRadioBtn m_playersThree;

		public UIRadioBtn m_playersFour;

		public UIRadioBtn m_fleetSizeAny;

		public UIRadioBtn m_fleetSizeSmall;

		public UIRadioBtn m_fleetSizeMedium;

		public UIRadioBtn m_fleetSizeLarge;

		public UIRadioBtn m_targetPointsEnabled;

		public UIRadioBtn m_targetPointsDisabled;

		public UIButton m_targetPointsMinus;

		public UIButton m_targetPointsPlus;

		public SpriteText m_targetPointsPercentage;

		public SpriteText m_targetPointsConversion;

		public UIStateToggleBtn m_anyTurnTime;

		public List<KeyValuePair<UIStateToggleBtn, double>> m_turnTimeCheckBoxes = new List<KeyValuePair<UIStateToggleBtn, double>>();

		public int m_targetPointsValue = 60;

		public List<double> m_turnTimes = new List<double>();
	}

	public Action<string, int> m_onProceed;

	public Action<int> m_onJoin;

	public Action<int, string> m_onWatchReplay;

	public Action m_onLogout;

	public Action m_onStartFleetEditor;

	public Action m_onItemBought;

	private PTech.RPC m_rpc;

	private GameObject m_guiCamera;

	private CreateGame m_createGame;

	private MapMan m_mapMan;

	private UserManClient m_userMan;

	private MsgBox m_msgBox;

	private NewsTicker m_newsTicker;

	private GameObject m_gui;

	private UIPanelManager m_panelMan;

	private NewsTab m_newsTab = new NewsTab();

	private PlayTab m_playTab = new PlayTab();

	private ArchiveTab m_archiveTab = new ArchiveTab();

	private FriendsTab m_friendsTab = new FriendsTab();

	private FleetTab m_fleetTab = new FleetTab();

	private ProfileTab m_profileTab = new ProfileTab();

	private MatchMaking m_matchMaking = new MatchMaking();

	private string m_localUserName;

	private float m_refreshTimer;

	private Credits m_credits;

	private Shop m_shop;

	private FriendData m_removeFriendTempData;

	private MusicManager m_musicMan;

	private GDPBackend m_gdpBackend;

	public OnlineMenu(GameObject guiCamera, PTech.RPC rpc, string hostName, string localUserName, MapMan mapMan, UserManClient userMan, StartStatus startStatus, MusicManager musMan, GDPBackend gdpBackend, PdxNews pdxNews)
	{
		m_guiCamera = guiCamera;
		m_rpc = rpc;
		m_localUserName = localUserName;
		m_mapMan = mapMan;
		m_userMan = userMan;
		m_musicMan = musMan;
		m_gdpBackend = gdpBackend;
		m_newsTicker = new NewsTicker(pdxNews, gdpBackend, m_guiCamera);
		m_rpc.Register("CreateOK", RPC_CreateOK);
		m_rpc.Register("CreateFail", RPC_CreateFail);
		m_rpc.Register("GameList", RPC_GameList);
		m_rpc.Register("ArchivedGameList", RPC_ArchivedGameList);
		m_rpc.Register("Friends", RPC_Friends);
		m_rpc.Register("FriendRequestReply", RPC_FriendRequestReply);
		m_rpc.Register("NrOfFriendRequests", RPC_NrOfFriendRequests);
		m_rpc.Register("Profile", RPC_Profile);
		m_rpc.Register("News", RPC_News);
		m_gui = GuiUtils.CreateGui("MainmenuOnline", guiCamera);
		m_panelMan = GuiUtils.FindChildOf(m_gui, "PanelMan").GetComponent<UIPanelManager>();
		GuiUtils.FindChildOf(m_gui, "VersionLbl").GetComponent<SpriteText>().Text = Localize.instance.Translate("$version " + VersionInfo.GetFullVersionString());
		GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "ActiveUserLabel").Text = m_localUserName;
		GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "ActiveHostLabel").Text = hostName;
		GuiUtils.FindChildOf(m_gui, "CreateGameButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOpenCreateGame);
		GuiUtils.FindChildOf(m_gui, "ButtonPlay").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowPlay);
		GuiUtils.FindChildOf(m_gui, "ButtonProfile").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowProfile);
		GuiUtils.FindChildOf(m_gui, "ButtonArchive").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowArchive);
		GuiUtils.FindChildOf(m_gui, "ButtonFriends").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowFriends);
		GuiUtils.FindChildOf(m_gui, "ButtonNews").GetComponent<UIPanelTab>().SetValueChangedDelegate(OnShowNews);
		GuiUtils.FindChildOf(m_gui, "ButtonCredits").GetComponent<UIButton>().SetValueChangedDelegate(OnShowCreditsPressed);
		GuiUtils.FindChildOf(m_gui, "ExitButton").GetComponent<UIButton>().SetValueChangedDelegate(OnBackPressed);
		GuiUtils.FindChildOf(m_gui, "OptionsButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOptionsPressed);
		GuiUtils.FindChildOf(m_gui, "NewFleetButton").GetComponent<UIButton>().SetValueChangedDelegate(OnNewFleet);
		GuiUtils.FindChildOf(m_gui, "DeleteFleetButton").GetComponent<UIButton>().SetValueChangedDelegate(OnDeleteFleet);
		GuiUtils.FindChildOf(m_gui, "EditFleetButton").GetComponent<UIButton>().SetValueChangedDelegate(OnEditleet);
		GuiUtils.FindChildOf(m_gui, "HelpButton").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "facebookButton").SetValueChangedDelegate(OnOpenFaceBook);
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "twitterButton").SetValueChangedDelegate(OnOpenTwitter);
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "paradoxButton").SetValueChangedDelegate(OnOpenForum);
		SetModifyFleetButtonsStatus(enable: false);
		GameObject gameObject = m_gui.transform.FindChild("CreateGameView").gameObject;
		m_createGame = new CreateGame(m_guiCamera, gameObject, m_mapMan, m_userMan);
		m_createGame.m_onClose = OnCreateGameClose;
		m_createGame.m_onCreateGame = OnCreateGame;
		m_newsTab.m_newsItem = GuiUtils.FindChildOf(m_gui, "NewsListItem");
		m_newsTab.m_newsList = GuiUtils.FindChildOf(m_gui, "NewsScrollList").GetComponent<UIScrollList>();
		m_playTab.m_gameListItem = GuiUtils.CreateGui("gamelist/Gamelist_listitem", m_guiCamera);
		m_playTab.m_gameListItem.transform.Translate(100000f, 0f, 0f);
		m_playTab.m_gameList = GuiUtils.FindChildOf(m_gui, "CurrentGamesScrollList").GetComponent<UIScrollList>();
		m_playTab.m_panel = GuiUtils.FindChildOf(m_gui, "PanelPlay").GetComponent<UIPanel>();
		m_playTab.m_listPublicGames = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_gui, "CheckboxPublicGames");
		m_playTab.m_listPublicGames.SetValueChangedDelegate(OnListPublicGamesChanged);
		m_archiveTab.m_gameListItem = GuiUtils.FindChildOf(m_gui, "ArchivedGameListItem");
		m_archiveTab.m_gameList = GuiUtils.FindChildOf(m_gui, "ArchivedGamesScrollList").GetComponent<UIScrollList>();
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "FindReplayButton").SetValueChangedDelegate(OnPlayReplayID);
		m_friendsTab.m_panel = GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "PanelFriends");
		m_friendsTab.m_friendListItem = GuiUtils.FindChildOf(m_gui, "FriendListItem");
		m_friendsTab.m_confirmedFriendListItem = GuiUtils.FindChildOf(m_gui, "ConfirmedFriendListItem");
		m_friendsTab.m_pendingFriendListItem = GuiUtils.FindChildOf(m_gui, "PendingFriendListItem");
		m_friendsTab.m_friendList = GuiUtils.FindChildOf(m_gui, "FriendsScrollList").GetComponent<UIScrollList>();
		GuiUtils.FindChildOf(m_gui, "FriendsNotificationWidget").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui, "AddFriendButton").GetComponent<UIButton>().SetValueChangedDelegate(OnAddFriendPressed);
		m_fleetTab.m_gameFleetItem = Resources.Load("gui/Shipyard/FleetListItem") as GameObject;
		m_fleetTab.m_fleetList = GuiUtils.FindChildOf(m_gui, "FleetList").GetComponent<UIScrollList>();
		GameObject shopPanel = GuiUtils.FindChildOf(m_gui, "PanelShop");
		m_shop = new Shop(m_guiCamera, shopPanel, m_gdpBackend, m_rpc, m_userMan);
		m_shop.m_onItemBought = OnItemBought;
		GuiUtils.FindChildOf(m_gui, "ButtonShop").GetComponent<UIPanelTab>().SetValueChangedDelegate(m_shop.OnShowShop);
		GameObject creditsRoot = GuiUtils.FindChildOf(m_gui, "PanelCredits");
		m_credits = new Credits(creditsRoot, m_musicMan);
		m_matchMaking.m_panel = GuiUtils.FindChildOf(m_gui, "MatchmakingPanel");
		m_matchMaking.m_gameTypeCampaign = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Campaign");
		m_matchMaking.m_gameTypeChallenge = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Challenge");
		m_matchMaking.m_gameTypePoints = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Points");
		m_matchMaking.m_gameTypeAssassination = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Assassination");
		m_matchMaking.m_playersAny = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "CheckboxAnyPlayers");
		m_matchMaking.m_playersTwo = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Two");
		m_matchMaking.m_playersThree = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Three");
		m_matchMaking.m_playersFour = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Four");
		m_matchMaking.m_fleetSizeAny = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "CheckboxAnyFleetsize");
		m_matchMaking.m_fleetSizeSmall = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Small");
		m_matchMaking.m_fleetSizeMedium = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Medium");
		m_matchMaking.m_fleetSizeLarge = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "Large");
		m_matchMaking.m_targetPointsEnabled = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "CheckboxOn");
		m_matchMaking.m_targetPointsDisabled = GuiUtils.FindChildOfComponent<UIRadioBtn>(m_matchMaking.m_panel, "CheckboxAnyPointsgoal");
		m_matchMaking.m_targetPointsMinus = GuiUtils.FindChildOfComponent<UIActionBtn>(m_matchMaking.m_panel, "MinusButton");
		m_matchMaking.m_targetPointsPlus = GuiUtils.FindChildOfComponent<UIActionBtn>(m_matchMaking.m_panel, "PlusButton");
		m_matchMaking.m_targetPointsPercentage = GuiUtils.FindChildOfComponent<SpriteText>(m_matchMaking.m_panel, "PointsPercentage");
		m_matchMaking.m_targetPointsConversion = GuiUtils.FindChildOfComponent<SpriteText>(m_matchMaking.m_panel, "PointsConversion");
		GuiUtils.FindChildOfComponent<UIButton>(m_matchMaking.m_panel, "ButtonMatchmake").SetValueChangedDelegate(OnFindGame);
		m_matchMaking.m_gameTypeCampaign.SetValueChangedDelegate(OnMatchmakeGameTypeChanged);
		m_matchMaking.m_gameTypeChallenge.SetValueChangedDelegate(OnMatchmakeGameTypeChanged);
		m_matchMaking.m_gameTypePoints.SetValueChangedDelegate(OnMatchmakeGameTypeChanged);
		m_matchMaking.m_gameTypeAssassination.SetValueChangedDelegate(OnMatchmakeGameTypeChanged);
		m_matchMaking.m_playersAny.SetValueChangedDelegate(OnMatchmakePlayersChanged);
		m_matchMaking.m_playersTwo.SetValueChangedDelegate(OnMatchmakePlayersChanged);
		m_matchMaking.m_playersThree.SetValueChangedDelegate(OnMatchmakePlayersChanged);
		m_matchMaking.m_playersFour.SetValueChangedDelegate(OnMatchmakePlayersChanged);
		m_matchMaking.m_fleetSizeAny.SetValueChangedDelegate(OnMatchmakeFleetSizeChanged);
		m_matchMaking.m_fleetSizeSmall.SetValueChangedDelegate(OnMatchmakeFleetSizeChanged);
		m_matchMaking.m_fleetSizeMedium.SetValueChangedDelegate(OnMatchmakeFleetSizeChanged);
		m_matchMaking.m_fleetSizeLarge.SetValueChangedDelegate(OnMatchmakeFleetSizeChanged);
		m_matchMaking.m_targetPointsMinus.SetValueChangedDelegate(OnMatchmakeTargetPointsMinus);
		m_matchMaking.m_targetPointsPlus.SetValueChangedDelegate(OnMatchmakeTargetPointsPlus);
		m_matchMaking.m_targetPointsEnabled.SetValueChangedDelegate(OnMatchmakeTargetPointsStatusChanged);
		m_matchMaking.m_targetPointsDisabled.SetValueChangedDelegate(OnMatchmakeTargetPointsStatusChanged);
		m_matchMaking.m_anyTurnTime = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_matchMaking.m_panel, "CheckboxAnyPlanningTime");
		m_matchMaking.m_anyTurnTime.SetValueChangedDelegate(OnAnyTurnTimeStatusChangeD);
		for (int i = 0; i < 10; i++)
		{
			UIStateToggleBtn uIStateToggleBtn = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_matchMaking.m_panel, "TurnTime" + i);
			if (uIStateToggleBtn != null)
			{
				uIStateToggleBtn.Text = Localize.instance.Translate(Utils.FormatTimeLeftString(Constants.m_turnTimeLimits[i]));
				m_matchMaking.m_turnTimeCheckBoxes.Add(new KeyValuePair<UIStateToggleBtn, double>(uIStateToggleBtn, Constants.m_turnTimeLimits[i]));
			}
		}
		UpdateMatchmakingTargetScoreWidgets();
		switch (startStatus)
		{
		case StartStatus.ShowGameView:
			m_panelMan.BringIn("PanelPlay");
			RequestGameList();
			break;
		case StartStatus.ShowArchiveView:
			m_panelMan.BringIn("PanelArchive");
			RequestArchivedGames();
			break;
		case StartStatus.ShowShipyard:
			m_panelMan.BringIn("PanelShipyard");
			break;
		case StartStatus.ShowCredits:
			StartCredits();
			break;
		default:
			m_panelMan.BringIn("PanelNews");
			RequestNews();
			break;
		}
		UserManClient userMan2 = m_userMan;
		userMan2.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Combine(userMan2.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		FillFleetList();
		UpdateNotifications();
		LoadMatchmakingSettings();
		if (PlayerPrefs.GetInt("playedtutorial", 0) != 1)
		{
			m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$mainmenu_starttutorial", OnStartTutorialYes, OnStartTutorialNo);
		}
	}

	public void Close()
	{
		UserManClient userMan = m_userMan;
		userMan.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userMan.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		m_rpc.Unregister("CreateOK");
		m_rpc.Unregister("CreateFail");
		m_rpc.Unregister("GameList");
		m_rpc.Unregister("ArchivedGameList");
		m_rpc.Unregister("Profile");
		m_rpc.Unregister("Friends");
		m_rpc.Unregister("FriendRequestReply");
		m_rpc.Unregister("NrOfFriendRequests");
		m_rpc.Unregister("News");
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_credits = null;
		if (m_shop != null)
		{
			m_shop.Close();
			m_shop = null;
		}
		if (m_newsTicker != null)
		{
			m_newsTicker.Close();
			m_newsTicker = null;
		}
		if (m_archiveTab.m_playReplayDialog != null)
		{
			UnityEngine.Object.Destroy(m_archiveTab.m_playReplayDialog);
		}
		if (m_matchMaking.m_progressDialog != null)
		{
			UnityEngine.Object.Destroy(m_matchMaking.m_progressDialog);
		}
		UnityEngine.Object.Destroy(m_playTab.m_gameListItem);
		UnityEngine.Object.Destroy(m_archiveTab.m_gameListItem);
		UnityEngine.Object.Destroy(m_gui);
		m_gui = null;
	}

	public void OnKicked()
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_kicked", OnMsgBoxOK);
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

	private void UpdateNotifications()
	{
		m_rpc.Invoke("RequestNrOfFriendRequests");
	}

	private void SetModifyFleetButtonsStatus(bool enable)
	{
		GuiUtils.FindChildOf(m_gui, "DeleteFleetButton").GetComponent<UIButton>().controlIsEnabled = enable;
		GuiUtils.FindChildOf(m_gui, "EditFleetButton").GetComponent<UIButton>().controlIsEnabled = enable;
	}

	private void OnMatchmakeGameTypeChanged(IUIObject button)
	{
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnMatchmakePlayersChanged(IUIObject button)
	{
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnMatchmakeFleetSizeChanged(IUIObject button)
	{
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnMatchmakeTargetPointsMinus(IUIObject button)
	{
		int num = 10;
		m_matchMaking.m_targetPointsValue -= 10;
		if (m_matchMaking.m_targetPointsValue < num)
		{
			m_matchMaking.m_targetPointsValue = num;
		}
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnMatchmakeTargetPointsPlus(IUIObject button)
	{
		m_matchMaking.m_targetPointsValue += 10;
		if (m_matchMaking.m_targetPointsValue > 100)
		{
			m_matchMaking.m_targetPointsValue = 100;
		}
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnMatchmakeTargetPointsStatusChanged(IUIObject button)
	{
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnAnyTurnTimeStatusChangeD(IUIObject button)
	{
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void UpdateFleetSizes()
	{
		GameType selectedMatchmakingGameType = GetSelectedMatchmakingGameType();
		if (selectedMatchmakingGameType == GameType.Assassination || selectedMatchmakingGameType == GameType.Points)
		{
			m_matchMaking.m_fleetSizeAny.controlIsEnabled = true;
			m_matchMaking.m_fleetSizeLarge.controlIsEnabled = true;
			m_matchMaking.m_fleetSizeMedium.controlIsEnabled = true;
			m_matchMaking.m_fleetSizeSmall.controlIsEnabled = true;
		}
		else
		{
			m_matchMaking.m_fleetSizeAny.controlIsEnabled = false;
			m_matchMaking.m_fleetSizeLarge.controlIsEnabled = false;
			m_matchMaking.m_fleetSizeMedium.controlIsEnabled = false;
			m_matchMaking.m_fleetSizeSmall.controlIsEnabled = false;
		}
		m_matchMaking.m_targetPointsEnabled.controlIsEnabled = selectedMatchmakingGameType == GameType.Points;
		m_matchMaking.m_targetPointsDisabled.controlIsEnabled = selectedMatchmakingGameType == GameType.Points;
		m_matchMaking.m_targetPointsMinus.controlIsEnabled = selectedMatchmakingGameType == GameType.Points;
		m_matchMaking.m_targetPointsPlus.controlIsEnabled = selectedMatchmakingGameType == GameType.Points;
		int selectedMatchmakingPlayers = GetSelectedMatchmakingPlayers();
		if ((selectedMatchmakingGameType == GameType.Points || selectedMatchmakingGameType == GameType.Assassination) && selectedMatchmakingPlayers == 3)
		{
			m_matchMaking.m_fleetSizeLarge.controlIsEnabled = false;
			m_matchMaking.m_fleetSizeMedium.controlIsEnabled = false;
		}
	}

	private void UpdateMatchmakingTurnTimes()
	{
		m_matchMaking.m_turnTimes.Clear();
		bool flag = m_matchMaking.m_anyTurnTime.StateNum == 1;
		foreach (KeyValuePair<UIStateToggleBtn, double> turnTimeCheckBox in m_matchMaking.m_turnTimeCheckBoxes)
		{
			turnTimeCheckBox.Key.controlIsEnabled = !flag;
			if (turnTimeCheckBox.Key.StateNum == 1 || flag)
			{
				m_matchMaking.m_turnTimes.Add(turnTimeCheckBox.Value);
			}
		}
	}

	private void UpdateMatchmakingTargetScoreWidgets()
	{
		GameType selectedMatchmakingGameType = GetSelectedMatchmakingGameType();
		int num = 10;
		bool flag = m_matchMaking.m_targetPointsEnabled.Value && selectedMatchmakingGameType == GameType.Points;
		m_matchMaking.m_targetPointsMinus.controlIsEnabled = m_matchMaking.m_targetPointsValue > num && flag;
		m_matchMaking.m_targetPointsPlus.controlIsEnabled = m_matchMaking.m_targetPointsValue < 100 && flag;
		int selectedMatchmakingPlayers = GetSelectedMatchmakingPlayers();
		FleetSizeClass fleetSizeClass = GetSelectedMatchmakingFleetSize();
		if ((selectedMatchmakingGameType == GameType.Points || selectedMatchmakingGameType == GameType.Assassination) && selectedMatchmakingPlayers == 3 && (fleetSizeClass == FleetSizeClass.Heavy || fleetSizeClass == FleetSizeClass.Medium))
		{
			m_matchMaking.m_fleetSizeSmall.Value = true;
			fleetSizeClass = FleetSizeClass.Small;
		}
		UpdateFleetSizes();
		m_matchMaking.m_targetPointsPercentage.Text = m_matchMaking.m_targetPointsValue + "%";
		if (selectedMatchmakingPlayers < 2 || fleetSizeClass == FleetSizeClass.None || selectedMatchmakingGameType != GameType.Points)
		{
			m_matchMaking.m_targetPointsConversion.Text = string.Empty;
		}
		else
		{
			int num2 = (int)((float)FleetSizes.sizes[(int)fleetSizeClass].max * ((float)m_matchMaking.m_targetPointsValue / 100f));
			if (selectedMatchmakingPlayers >= 3)
			{
				num2 *= 2;
			}
			m_matchMaking.m_targetPointsConversion.Text = num2 + Localize.instance.Translate(" $creategame_pointstowin");
		}
		UpdateMatchmakingTurnTimes();
	}

	private GameType GetSelectedMatchmakingGameType()
	{
		GameType result = GameType.Points;
		if (m_matchMaking.m_gameTypeAssassination.Value)
		{
			result = GameType.Assassination;
		}
		if (m_matchMaking.m_gameTypeCampaign.Value)
		{
			result = GameType.Campaign;
		}
		if (m_matchMaking.m_gameTypeChallenge.Value)
		{
			result = GameType.Challenge;
		}
		if (m_matchMaking.m_gameTypePoints.Value)
		{
			result = GameType.Points;
		}
		return result;
	}

	private int GetSelectedMatchmakingPlayers()
	{
		int result = -1;
		if (m_matchMaking.m_playersTwo.Value)
		{
			result = 2;
		}
		if (m_matchMaking.m_playersThree.Value)
		{
			result = 3;
		}
		if (m_matchMaking.m_playersFour.Value)
		{
			result = 4;
		}
		return result;
	}

	private FleetSizeClass GetSelectedMatchmakingFleetSize()
	{
		FleetSizeClass result = FleetSizeClass.None;
		if (m_matchMaking.m_fleetSizeLarge.Value)
		{
			result = FleetSizeClass.Heavy;
		}
		if (m_matchMaking.m_fleetSizeMedium.Value)
		{
			result = FleetSizeClass.Medium;
		}
		if (m_matchMaking.m_fleetSizeSmall.Value)
		{
			result = FleetSizeClass.Small;
		}
		return result;
	}

	public static string DoubleToString(double f)
	{
		return f.ToString();
	}

	public static double StringToDouble(string s)
	{
		return double.Parse(s);
	}

	private void SaveMatchmakingSettings(GameType gameType, int players, FleetSizeClass fleetSize, bool targetScoreEnabled, int targetScore, double[] turnTimes)
	{
		PlayerPrefs.SetInt("Match_GameType", (int)gameType);
		PlayerPrefs.SetInt("Match_Players", players);
		PlayerPrefs.SetInt("Match_FleetSize", (int)fleetSize);
		PlayerPrefs.SetInt("Match_TargetScoreEnabled", targetScoreEnabled ? 1 : 0);
		PlayerPrefs.SetInt("Match_TargetScore", targetScore);
		string[] value = Array.ConvertAll(turnTimes, DoubleToString);
		string value2 = string.Join(",", value);
		PlayerPrefs.SetString("Match_TurnTimes", value2);
	}

	private void LoadMatchmakingSettings()
	{
		GameType @int = (GameType)PlayerPrefs.GetInt("Match_GameType", 5);
		m_matchMaking.m_gameTypeAssassination.Value = false;
		m_matchMaking.m_gameTypeCampaign.Value = false;
		m_matchMaking.m_gameTypePoints.Value = false;
		m_matchMaking.m_gameTypeChallenge.Value = false;
		switch (@int)
		{
		case GameType.Assassination:
			m_matchMaking.m_gameTypeAssassination.Value = true;
			break;
		case GameType.Campaign:
			m_matchMaking.m_gameTypeCampaign.Value = true;
			break;
		case GameType.Challenge:
			m_matchMaking.m_gameTypeChallenge.Value = true;
			break;
		case GameType.Points:
			m_matchMaking.m_gameTypePoints.Value = true;
			break;
		}
		int int2 = PlayerPrefs.GetInt("Match_Players", -1);
		m_matchMaking.m_playersAny.Value = false;
		m_matchMaking.m_playersTwo.Value = false;
		m_matchMaking.m_playersThree.Value = false;
		m_matchMaking.m_playersFour.Value = false;
		switch (int2)
		{
		case -1:
			m_matchMaking.m_playersAny.Value = true;
			break;
		case 2:
			m_matchMaking.m_playersTwo.Value = true;
			break;
		case 3:
			m_matchMaking.m_playersThree.Value = true;
			break;
		case 4:
			m_matchMaking.m_playersFour.Value = true;
			break;
		}
		FleetSizeClass int3 = (FleetSizeClass)PlayerPrefs.GetInt("Match_FleetSize", 5);
		m_matchMaking.m_fleetSizeAny.Value = false;
		m_matchMaking.m_fleetSizeLarge.Value = false;
		m_matchMaking.m_fleetSizeMedium.Value = false;
		m_matchMaking.m_fleetSizeSmall.Value = false;
		switch (int3)
		{
		case FleetSizeClass.None:
			m_matchMaking.m_fleetSizeAny.Value = true;
			break;
		case FleetSizeClass.Small:
			m_matchMaking.m_fleetSizeSmall.Value = true;
			break;
		case FleetSizeClass.Medium:
			m_matchMaking.m_fleetSizeMedium.Value = true;
			break;
		case FleetSizeClass.Heavy:
			m_matchMaking.m_fleetSizeLarge.Value = true;
			break;
		}
		bool value = ((PlayerPrefs.GetInt("Match_TargetScoreEnabled", 0) == 1) ? true : false);
		m_matchMaking.m_targetPointsEnabled.Value = value;
		m_matchMaking.m_targetPointsValue = PlayerPrefs.GetInt("Match_TargetScore", m_matchMaking.m_targetPointsValue);
		List<double> list = new List<double>();
		string @string = PlayerPrefs.GetString("Match_TurnTimes", string.Empty);
		if (@string != string.Empty)
		{
			string[] array = @string.Split(',');
			list = new List<double>(Array.ConvertAll(array, StringToDouble));
		}
		bool flag = list.Count == 6 || list.Count == 0;
		m_matchMaking.m_anyTurnTime.SetToggleState(flag ? 1 : 0);
		foreach (KeyValuePair<UIStateToggleBtn, double> turnTimeCheckBox in m_matchMaking.m_turnTimeCheckBoxes)
		{
			bool flag2 = flag || list.Contains(turnTimeCheckBox.Value);
			turnTimeCheckBox.Key.SetToggleState(flag2 ? 1 : 0);
		}
		m_matchMaking.m_turnTimes = list;
		UpdateMatchmakingTargetScoreWidgets();
	}

	private void OnFindGame(IUIObject obj)
	{
		UpdateMatchmakingTurnTimes();
		m_matchMaking.m_progressDialog = GuiUtils.CreateGui("dialogs/Dialog_Progress_Interruptable", m_guiCamera);
		GuiUtils.FindChildOfComponent<UIButton>(m_matchMaking.m_progressDialog, "CancelButton").SetValueChangedDelegate(OnFindGameCancel);
		GameType selectedMatchmakingGameType = GetSelectedMatchmakingGameType();
		int selectedMatchmakingPlayers = GetSelectedMatchmakingPlayers();
		FleetSizeClass selectedMatchmakingFleetSize = GetSelectedMatchmakingFleetSize();
		float num = (float)m_matchMaking.m_targetPointsValue / 100f;
		bool flag = !m_matchMaking.m_targetPointsDisabled.Value;
		if (!flag)
		{
			num = -1f;
		}
		SaveMatchmakingSettings(selectedMatchmakingGameType, selectedMatchmakingPlayers, selectedMatchmakingFleetSize, flag, m_matchMaking.m_targetPointsValue, m_matchMaking.m_turnTimes.ToArray());
		m_rpc.Invoke("FindGame", (int)selectedMatchmakingGameType, selectedMatchmakingPlayers, num, (int)selectedMatchmakingFleetSize, m_matchMaking.m_turnTimes.ToArray());
	}

	private void OnFindGameCancel(IUIObject button)
	{
		UnityEngine.Object.Destroy(m_matchMaking.m_progressDialog);
		m_matchMaking.m_progressDialog = null;
		m_rpc.Invoke("CancelFindGame");
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
		m_rpc.Invoke("CreateGame", (int)gametype, campaign, mapName, players, fleetSize, targetScore, turnTime, autoJoin);
	}

	private void OnNewFleet(IUIObject obj)
	{
		if (m_onProceed != null)
		{
			m_onProceed(string.Empty, 0);
		}
	}

	private void OnDeleteFleet(IUIObject obj)
	{
		if (m_onProceed != null && m_fleetTab.m_selectedFleet.Length != 0)
		{
			string text = Localize.instance.Translate("$fleetedit_delete");
			m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, text, OnConfirmDelete, null);
		}
	}

	private void OnConfirmDelete()
	{
		m_userMan.RemoveFleet(m_fleetTab.m_selectedFleet);
		m_fleetTab.m_selectedFleet = string.Empty;
		SetModifyFleetButtonsStatus(enable: false);
	}

	private void OnEditleet(IUIObject obj)
	{
		if (m_onProceed != null && m_fleetTab.m_selectedFleet.Length != 0)
		{
			m_onProceed(m_fleetTab.m_selectedFleet, 0);
		}
	}

	private void OnListPublicGamesChanged(IUIObject obj)
	{
		RequestGameList();
	}

	private void RequestGameList()
	{
		bool flag = m_playTab.m_listPublicGames.StateNum == 1;
		GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "CurrentGamesLbl").gameObject.SetActiveRecursively(!flag);
		GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "PublicGamesLbl").gameObject.SetActiveRecursively(flag);
		m_rpc.Invoke("RequestGameList", flag);
	}

	private void RPC_CreateOK(PTech.RPC rpc, List<object> args)
	{
		m_createGame.Hide();
	}

	private void RPC_CreateFail(PTech.RPC rpc, List<object> args)
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "Failed to create game", null);
	}

	private void OnAddFriendPressed(IUIObject obj)
	{
		m_friendsTab.m_addFriendDialog = GuiUtils.OpenInputDialog(m_guiCamera, "$add_friend", string.Empty, OnAddFriendCancel, OnAddFriendOk);
	}

	private void OnAddFriendCancel()
	{
		UnityEngine.Object.Destroy(m_friendsTab.m_addFriendDialog);
	}

	private void OnAddFriendOk(string text)
	{
		UnityEngine.Object.Destroy(m_friendsTab.m_addFriendDialog);
		m_rpc.Invoke("FriendRequest", text);
	}

	private void RPC_FriendRequestReply(PTech.RPC rpc, List<object> args)
	{
		ErrorCode errorCode = (ErrorCode)(int)args[0];
		string text = ((errorCode != ErrorCode.FriendUserDoesNotExist) ? "$already_friends" : "$user_does_not_exist");
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, text, null);
	}

	private void RPC_Friends(PTech.RPC rpc, List<object> args)
	{
		m_friendsTab.m_friends.Clear();
		foreach (object arg in args)
		{
			FriendData friendData = new FriendData();
			friendData.FromArray((byte[])arg);
			m_friendsTab.m_friends.Add(friendData);
		}
		float scrollPosition = m_friendsTab.m_friendList.ScrollPosition;
		m_friendsTab.m_friendList.ClearList(destroy: true);
		bool flag = false;
		foreach (FriendData friend in m_friendsTab.m_friends)
		{
			UIListItemContainer item = null;
			if (friend.m_status == FriendData.FriendStatus.IsFriend)
			{
				flag = true;
				GameObject gameObject = UnityEngine.Object.Instantiate(m_friendsTab.m_confirmedFriendListItem) as GameObject;
				gameObject.transform.FindChild("FriendRemoveButton").GetComponent<UIButton>().SetValueChangedDelegate(OnFriendRemove);
				SimpleSprite component = gameObject.transform.FindChild("FriendOnlineStatus").GetComponent<SimpleSprite>();
				SimpleSprite component2 = gameObject.transform.FindChild("FriendOfflineStatus").GetComponent<SimpleSprite>();
				if (friend.m_online)
				{
					component2.transform.Translate(new Vector3(10000f, 0f, 0f));
				}
				if (!friend.m_online)
				{
					component.transform.Translate(new Vector3(10000f, 0f, 0f));
				}
				SimpleSprite component3 = gameObject.transform.FindChild("FriendFlag").GetComponent<SimpleSprite>();
				Texture2D flagTexture = GuiUtils.GetFlagTexture(friend.m_flagID);
				GuiUtils.SetImage(component3, flagTexture);
				GuiUtils.FindChildOf(gameObject, "FriendNameLabel").GetComponent<SpriteText>().Text = friend.m_name;
				item = gameObject.GetComponent<UIListItemContainer>();
			}
			else if (friend.m_status == FriendData.FriendStatus.NeedAccept)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(m_friendsTab.m_friendListItem) as GameObject;
				DebugUtils.Assert(gameObject2 != null);
				gameObject2.transform.FindChild("FriendDeclineButton").GetComponent<UIButton>().SetValueChangedDelegate(OnFriendRemove);
				gameObject2.transform.FindChild("FriendAcceptButton").GetComponent<UIButton>().SetValueChangedDelegate(OnFriendAccept);
				item = gameObject2.GetComponent<UIListItemContainer>();
				SpriteText component4 = gameObject2.transform.FindChild("FriendRequestLabel").GetComponent<SpriteText>();
				component4.Text = Localize.instance.Translate(friend.m_name + " $label_friend_request");
			}
			else if (friend.m_status == FriendData.FriendStatus.Requested)
			{
				GameObject gameObject3 = UnityEngine.Object.Instantiate(m_friendsTab.m_pendingFriendListItem) as GameObject;
				gameObject3.transform.FindChild("FriendCancelButton").GetComponent<UIButton>().SetValueChangedDelegate(OnFriendRemove);
				item = gameObject3.GetComponent<UIListItemContainer>();
				SpriteText component5 = gameObject3.transform.FindChild("FriendPendingLabel").GetComponent<SpriteText>();
				component5.Text = Localize.instance.Translate("$label_friend_request_pending " + friend.m_name + ".");
			}
			GuiUtils.FixedItemContainerInstance(item);
			m_friendsTab.m_friendList.AddItem(item);
		}
		m_friendsTab.m_friendList.ScrollPosition = scrollPosition;
		if (flag)
		{
			m_userMan.UnlockAchievement(3);
		}
	}

	private void OnFriendAccept(IUIObject button)
	{
		UIListItemContainer component = button.transform.parent.GetComponent<UIListItemContainer>();
		FriendData friendData = m_friendsTab.m_friends[component.Index];
		m_userMan.UnlockAchievement(3);
		m_rpc.Invoke("AcceptFriendRequest", friendData.m_friendID);
	}

	private void OnFriendRemove(IUIObject button)
	{
		UIListItemContainer component = button.transform.parent.GetComponent<UIListItemContainer>();
		m_removeFriendTempData = m_friendsTab.m_friends[component.Index];
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$label_defriend", OnRemoveFriendConfirm, OnMsgBoxOK);
	}

	private void OnRemoveFriendConfirm()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_rpc.Invoke("RemoveFriend", m_removeFriendTempData.m_friendID);
	}

	private void OnPlayReplayID(IUIObject button)
	{
		if (m_archiveTab.m_playReplayDialog == null)
		{
			m_archiveTab.m_playReplayDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$replay_enterreplayid"), string.Empty, OnPlayReplayCancel, OnPlayReplayOk);
		}
	}

	private void OnPlayReplayCancel()
	{
		UnityEngine.Object.Destroy(m_archiveTab.m_playReplayDialog);
		m_archiveTab.m_playReplayDialog = null;
	}

	private void OnPlayReplayOk(string id)
	{
		UnityEngine.Object.Destroy(m_archiveTab.m_playReplayDialog);
		m_archiveTab.m_playReplayDialog = null;
		try
		{
			int arg = int.Parse(id);
			m_onWatchReplay(arg, "Replay " + arg);
		}
		catch
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$replay_invalidid", OnMsgBoxOK);
		}
	}

	private void OnMsgBoxOK()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
	}

	private void RPC_ArchivedGameList(PTech.RPC rpc, List<object> args)
	{
		m_archiveTab.m_games.Clear();
		foreach (object arg in args)
		{
			GamePost gamePost = new GamePost();
			gamePost.FromArray((byte[])arg);
			m_archiveTab.m_games.Add(gamePost);
		}
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
			GuiUtils.FindChildOf(gameObject, "IdLbl").GetComponent<SpriteText>().Text = game.m_gameID.ToString();
			GuiUtils.FindChildOf(gameObject, "CreatedLbl").GetComponent<SpriteText>().Text = game.m_createDate.ToString("yyyy-MM-d HH:mm");
			GuiUtils.FindChildOf(gameObject, "ArchivedGameListitemRadioButton").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnReplayArchiveGameSelected);
			GuiUtils.FindChildOf(gameObject, "RemoveButton").GetComponent<UIButton>().SetValueChangedDelegate(OnRemoveReplay);
			UIListItemContainer component = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component);
			m_archiveTab.m_gameList.AddItem(component);
		}
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
		m_rpc.Invoke("RemoveReplay", gamePost.m_gameName, gamePost.m_gameID);
	}

	private void RPC_GameList(PTech.RPC rpc, List<object> args)
	{
		m_playTab.m_games.Clear();
		foreach (object arg in args)
		{
			GamePost gamePost = new GamePost();
			gamePost.FromArray((byte[])arg);
			m_playTab.m_games.Add(gamePost);
		}
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
			GuiUtils.FindChildOf(gameObject, "CreatedValueLabel").GetComponent<SpriteText>().Text = game.m_createDate.ToString("yyyy-MM-dd HH:mm");
			if (game.m_needAttention)
			{
				GuiUtils.FindChildOf(gameObject, "StateIcon_Default").transform.Translate(0f, 0f, 20f);
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "StateIcon_NewTurn").transform.Translate(0f, 0f, 20f);
			}
			GuiUtils.FindChildOf(gameObject, "Button").GetComponent<UIButton>().SetValueChangedDelegate(OnGameListSelection);
			UIListItemContainer component = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component);
			m_playTab.m_gameList.AddItem(component);
		}
		m_playTab.m_gameList.ScrollPosition = scrollPosition;
	}

	private void FillFleetList()
	{
		m_fleetTab.m_selectedFleet = string.Empty;
		m_fleetTab.m_fleets.Clear();
		m_fleetTab.m_fleetList.ClearList(destroy: true);
		List<FleetDef> fleetDefs = m_userMan.GetFleetDefs(0);
		foreach (FleetDef item in fleetDefs)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_fleetTab.m_gameFleetItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Name").GetComponent<SpriteText>().Text = item.m_name;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Ships").GetComponent<SpriteText>().Text = item.m_ships.Count.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Points").GetComponent<SpriteText>().Text = item.m_value.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Size").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey(FleetSizes.GetSizeClassName(item.m_value));
			UIRadioBtn component = GuiUtils.FindChildOf(gameObject, "RadioButton").GetComponent<UIRadioBtn>();
			component.SetValueChangedDelegate(OnFleetSelected);
			UIListItemContainer component2 = gameObject.GetComponent<UIListItemContainer>();
			if (!item.m_available)
			{
				component.controlIsEnabled = false;
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "LblDLCNotAvailable").transform.Translate(new Vector3(0f, 0f, 20f));
			}
			m_fleetTab.m_fleetList.AddItem(component2);
			m_fleetTab.m_fleets.Add(item.m_name);
		}
		SetModifyFleetButtonsStatus(enable: false);
	}

	private void OnFleetSelected(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		m_fleetTab.m_selectedFleet = m_fleetTab.m_fleets[component.Index];
		SetModifyFleetButtonsStatus(enable: true);
	}

	public void Update()
	{
		m_refreshTimer += Time.deltaTime;
		if (m_refreshTimer > 5f)
		{
			m_refreshTimer = 0f;
			if (m_playTab.m_panel.gameObject.active)
			{
				RequestGameList();
			}
			if (m_friendsTab.m_panel.gameObject.active)
			{
				RequestFriends();
			}
		}
		m_credits.Update(Time.deltaTime);
		m_shop.Update(Time.deltaTime);
		m_newsTicker.Update(Time.deltaTime);
	}

	private void OnBackPressed(IUIObject obj)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$mainmenu_logout", OnExitYes, OnExitNo);
		}
		else if (m_onLogout != null)
		{
			Close();
			m_onLogout();
		}
	}

	private void OnExitYes()
	{
		m_msgBox.Close();
		m_msgBox = null;
		if (m_onLogout != null)
		{
			Close();
			m_onLogout();
		}
	}

	private void OnExitNo()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void OnOptionsPressed(IUIObject obj)
	{
		OptionsWindow optionsWindow = new OptionsWindow(m_guiCamera, inGame: false);
	}

	private void OnUserManUpdate()
	{
		FillFleetList();
	}

	private void OnFleetEditorPressed(IUIObject obj)
	{
		if (m_onStartFleetEditor != null)
		{
			m_onStartFleetEditor();
		}
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
		RequestArchivedGames();
	}

	private void OnShowNews(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIPanel component = GuiUtils.FindChildOf(m_gui, "PanelNews").GetComponent<UIPanel>();
			component.AddTempTransitionDelegate(OnNewsTransitionComplete);
		}
	}

	private void OnNewsTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		RequestNews();
	}

	private void OnShowCreditsPressed(IUIObject obj)
	{
		StartCredits();
	}

	private void StartCredits()
	{
		m_credits.Start();
	}

	private void RequestNews()
	{
		m_rpc.Invoke("RequestNews");
	}

	private void RPC_News(PTech.RPC rpc, List<object> args)
	{
		m_newsTab.m_newsList.ClearList(destroy: true);
		foreach (object arg in args)
		{
			byte[] data = (byte[])arg;
			NewsPost newsPost = new NewsPost(data);
			GameObject gameObject = UnityEngine.Object.Instantiate(m_newsTab.m_newsItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "NewsTitleLabel").GetComponent<SpriteText>().Text = newsPost.m_title;
			GuiUtils.FindChildOf(gameObject, "NewsCategoryLabel").GetComponent<SpriteText>().Text = newsPost.m_category;
			GuiUtils.FindChildOf(gameObject, "NewsDateLabel").GetComponent<SpriteText>().Text = newsPost.m_date.ToString("yyyy-MM-d HH:mm");
			GuiUtils.FindChildOf(gameObject, "NewsContentLabel").GetComponent<SpriteText>().Text = newsPost.m_content;
			UIListItemContainer component = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component);
			m_newsTab.m_newsList.AddItem(component);
		}
	}

	private void OnShowFriends(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIPanel component = GuiUtils.FindChildOf(m_gui, "PanelFriends").GetComponent<UIPanel>();
			component.AddTempTransitionDelegate(OnFriendsTransitionComplete);
			m_friendsTab.m_friendList.ClearList(destroy: true);
		}
	}

	private void OnFriendsTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		RequestFriends();
	}

	private void RequestFriends()
	{
		m_rpc.Invoke("RequestFriends");
	}

	private void RequestArchivedGames()
	{
		m_rpc.Invoke("RequestArchivedGames");
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
		RequestGameList();
	}

	private void OnShowProfile(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIPanel component = GuiUtils.FindChildOf(m_gui, "PanelProfile").GetComponent<UIPanel>();
			component.AddTempTransitionDelegate(OnProfileTransitionComplete);
		}
	}

	private void OnProfileTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		RequestProfileData();
	}

	private void RequestProfileData()
	{
		m_rpc.Invoke("RequestProfile");
	}

	private void RPC_NrOfFriendRequests(PTech.RPC rpc, List<object> args)
	{
		int num = (int)args[0];
		GameObject gameObject = GuiUtils.FindChildOf(m_gui, "FriendsNotificationWidget");
		SpriteText component = GuiUtils.FindChildOf(m_gui, "FriendsNotificationLabel").GetComponent<SpriteText>();
		if (num > 0)
		{
			component.Text = num.ToString();
			gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			gameObject.SetActiveRecursively(state: false);
		}
	}

	private void RPC_Profile(PTech.RPC rpc, List<object> args)
	{
		int num = 0;
		string userName = (string)args[num++];
		int flag = (int)args[num++];
		int mailFlags = (int)args[num++];
		int totalNrOfFlags = (int)args[num++];
		string email = (string)args[num++];
		byte[] data = (byte[])args[num++];
		UserStats userStats = new UserStats();
		userStats.FromArray(data);
		SetProfileData(userName, flag, mailFlags, totalNrOfFlags, email, userStats);
	}

	private string GetTimeString(long seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		string text = string.Empty;
		if (timeSpan.Days > 0)
		{
			text = text + timeSpan.Days + " $time_days ";
		}
		if (timeSpan.Hours > 0)
		{
			text = text + timeSpan.Hours + " $time_hours ";
		}
		if (timeSpan.Minutes > 0)
		{
			text = text + timeSpan.Minutes + " $time_minutes ";
		}
		return Localize.instance.Translate(text);
	}

	private string GetMostUsedShip(Dictionary<string, int> shipUsage)
	{
		string result = string.Empty;
		int num = 0;
		foreach (KeyValuePair<string, int> item in shipUsage)
		{
			if (item.Value > num)
			{
				result = item.Key;
				num = item.Value;
			}
		}
		return result;
	}

	private string GetBestGun(Dictionary<string, UserStats.ModuleStat> moduleData)
	{
		string result = string.Empty;
		int num = 0;
		foreach (KeyValuePair<string, UserStats.ModuleStat> moduleDatum in moduleData)
		{
			if (moduleDatum.Value.m_damage > num)
			{
				result = moduleDatum.Key;
				num = moduleDatum.Value.m_damage;
			}
		}
		return result;
	}

	private string GetMostUsedUtilityModulen(Dictionary<string, UserStats.ModuleStat> moduleData)
	{
		string result = string.Empty;
		int num = 0;
		foreach (KeyValuePair<string, UserStats.ModuleStat> moduleDatum in moduleData)
		{
			if (moduleDatum.Value.m_uses > num)
			{
				GameObject prefab = ObjectFactory.instance.GetPrefab(moduleDatum.Key);
				if (prefab.GetComponent<HPModule>().m_type == HPModule.HPModuleType.Defensive)
				{
					result = moduleDatum.Key;
					num = moduleDatum.Value.m_uses;
				}
			}
		}
		return result;
	}

	private void SetProfileData(string userName, int flag, int mailFlags, int totalNrOfFlags, string email, UserStats stats)
	{
		GameObject gameObject = GuiUtils.FindChildOf(m_gui, "PanelProfile");
		SpriteText component = GuiUtils.FindChildOf(gameObject, "UserNameLabel").GetComponent<SpriteText>();
		SpriteText component2 = GuiUtils.FindChildOf(gameObject, "UserEmailLabel").GetComponent<SpriteText>();
		SimpleSprite component3 = GuiUtils.FindChildOf(gameObject, "FlagImage").GetComponent<SimpleSprite>();
		SpriteText component4 = GuiUtils.FindChildOf(gameObject, "GamesPlayedValueLabel").GetComponent<SpriteText>();
		SpriteText component5 = GuiUtils.FindChildOf(gameObject, "TotalDamageAmountValueLabel").GetComponent<SpriteText>();
		SpriteText component6 = GuiUtils.FindChildOf(gameObject, "FFRatioValueLabel").GetComponent<SpriteText>();
		SpriteText component7 = GuiUtils.FindChildOf(gameObject, "TotalTimeValueLabel").GetComponent<SpriteText>();
		SpriteText component8 = GuiUtils.FindChildOf(gameObject, "PlanningTimeValueLabel").GetComponent<SpriteText>();
		SpriteText component9 = GuiUtils.FindChildOf(gameObject, "ShipyardTimeValueLabel").GetComponent<SpriteText>();
		SimpleSprite component10 = GuiUtils.FindChildOf(gameObject, "PreferredClassImage").GetComponent<SimpleSprite>();
		SpriteText component11 = GuiUtils.FindChildOf(gameObject, "PreferredClassValueLabel").GetComponent<SpriteText>();
		SimpleSprite component12 = GuiUtils.FindChildOf(gameObject, "DeadliestWeaponImage").GetComponent<SimpleSprite>();
		SpriteText component13 = GuiUtils.FindChildOf(gameObject, "DeadliestWeaponValueLabel").GetComponent<SpriteText>();
		SimpleSprite component14 = GuiUtils.FindChildOf(gameObject, "PreferredUtilityImage").GetComponent<SimpleSprite>();
		SpriteText component15 = GuiUtils.FindChildOf(gameObject, "PreferredUtilityValueLabel").GetComponent<SpriteText>();
		UIStateToggleBtn uIStateToggleBtn = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(gameObject, "EmailNotificationsCheckbox");
		uIStateToggleBtn.SetValueChangedDelegate(OnMailCheckboxChanged);
		component.Text = userName;
		component2.Text = email;
		bool flag2 = (mailFlags & 1) != 0;
		uIStateToggleBtn.SetState(flag2 ? 1 : 0);
		float num = ((stats.m_vsTotalDamage <= 0) ? 0f : ((float)stats.m_vsTotalFriendlyDamage / (float)stats.m_vsTotalDamage));
		component4.Text = stats.m_vsGamesWon + "/" + stats.m_vsGamesLost;
		component5.Text = stats.m_vsTotalDamage.ToString();
		component6.Text = (int)(num * 100f) + "%";
		component7.Text = GetTimeString(stats.m_totalPlayTime);
		component8.Text = GetTimeString(stats.m_totalPlanningTime);
		component9.Text = GetTimeString(stats.m_totalShipyardTime);
		string mostUsedShip = GetMostUsedShip(stats.m_vsShipUsage);
		if (mostUsedShip != string.Empty)
		{
			GuiUtils.SetImage(component10, GuiUtils.GetProfileShipSilhouette(mostUsedShip));
			component11.Text = Localize.instance.TranslateKey(mostUsedShip + "_name");
		}
		else
		{
			component10.gameObject.SetActiveRecursively(state: false);
			component11.Text = string.Empty;
		}
		string bestGun = GetBestGun(stats.m_vsModuleStats);
		if (bestGun != string.Empty)
		{
			Texture2D profileArmamentThumbnail = GuiUtils.GetProfileArmamentThumbnail(bestGun);
			if (profileArmamentThumbnail != null)
			{
				GuiUtils.SetImage(component12, profileArmamentThumbnail);
			}
			component13.Text = Localize.instance.TranslateKey(bestGun + "_name");
		}
		else
		{
			component12.gameObject.SetActiveRecursively(state: false);
			component13.Text = string.Empty;
		}
		string mostUsedUtilityModulen = GetMostUsedUtilityModulen(stats.m_vsModuleStats);
		if (mostUsedUtilityModulen != string.Empty)
		{
			Texture2D profileArmamentThumbnail2 = GuiUtils.GetProfileArmamentThumbnail(mostUsedUtilityModulen);
			if (profileArmamentThumbnail2 != null)
			{
				GuiUtils.SetImage(component14, profileArmamentThumbnail2);
			}
			component15.Text = Localize.instance.TranslateKey(mostUsedUtilityModulen + "_name");
		}
		else
		{
			component14.gameObject.SetActiveRecursively(state: false);
			component15.Text = string.Empty;
		}
		Texture2D flagTexture = GuiUtils.GetFlagTexture(flag);
		if (flagTexture != null)
		{
			component3.SetTexture(flagTexture);
			component3.Setup(flagTexture.width, flagTexture.height, new Vector2(0f, flagTexture.height), new Vector2(flagTexture.width, flagTexture.height));
		}
		FillAchivementList(gameObject, stats.m_achievements);
		FillFlagList(gameObject, totalNrOfFlags);
	}

	private void OnMailCheckboxChanged(IUIObject button)
	{
		UIStateToggleBtn uIStateToggleBtn = button as UIStateToggleBtn;
		int num = 0;
		if (uIStateToggleBtn.StateNum == 1)
		{
			num |= 1;
		}
		PLog.Log("Mail " + num);
		m_rpc.Invoke("SetMailFlags", num);
	}

	private void FillAchivementList(GameObject panelProfile, Dictionary<int, long> unlocked)
	{
		GameObject original = GuiUtils.FindChildOf(m_gui, "AchievementListItem_Unlocked");
		GameObject original2 = GuiUtils.FindChildOf(m_gui, "AchievementListItem_Locked");
		UIScrollList uIScrollList = GuiUtils.FindChildOfComponent<UIScrollList>(m_gui, "AchievementList");
		uIScrollList.ClearList(destroy: true);
		List<KeyValuePair<int, DateTime>> list = new List<KeyValuePair<int, DateTime>>();
		foreach (KeyValuePair<int, long> item in unlocked)
		{
			list.Add(new KeyValuePair<int, DateTime>(item.Key, DateTime.FromBinary(item.Value)));
		}
		list.Sort((KeyValuePair<int, DateTime> a, KeyValuePair<int, DateTime> b) => b.Value.CompareTo(a.Value));
		foreach (KeyValuePair<int, DateTime> item2 in list)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			SpriteText spriteText = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "AchievementName");
			spriteText.Text = Localize.instance.TranslateKey("achievement_name" + item2.Key);
			SpriteText spriteText2 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "AchievementDescription");
			spriteText2.Text = Localize.instance.TranslateKey("achievement_desc" + item2.Key);
			SpriteText spriteText3 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "AchievementUnlockDate");
			spriteText3.Text = item2.Value.ToString("yyyy-MM-d HH:mm");
			SimpleSprite sprite = GuiUtils.FindChildOfComponent<SimpleSprite>(gameObject, "AchievementIcon");
			Texture2D achievementIconTexture = GuiUtils.GetAchievementIconTexture(item2.Key, unlocked: true);
			if (achievementIconTexture != null)
			{
				GuiUtils.SetImage(sprite, achievementIconTexture);
			}
			GuiUtils.FixedItemContainerInstance(gameObject.GetComponent<UIListItemContainer>());
			uIScrollList.AddItem(gameObject);
		}
		int[] achivements = Constants.m_achivements;
		for (int i = 0; i < achivements.Length; i++)
		{
			int num = achivements[i];
			if (!unlocked.ContainsKey(num) && !Constants.m_achivementHidden[num])
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(original2) as GameObject;
				SpriteText spriteText4 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject2, "AchievementName");
				spriteText4.Text = Localize.instance.TranslateKey("achievement_name" + num);
				SpriteText spriteText5 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject2, "AchievementDescription");
				spriteText5.Text = Localize.instance.TranslateKey("achievement_desc" + num);
				SimpleSprite sprite2 = GuiUtils.FindChildOfComponent<SimpleSprite>(gameObject2, "AchievementIcon");
				Texture2D achievementIconTexture2 = GuiUtils.GetAchievementIconTexture(num, unlocked: false);
				if (achievementIconTexture2 != null)
				{
					GuiUtils.SetImage(sprite2, achievementIconTexture2);
				}
				GuiUtils.FixedItemContainerInstance(gameObject2.GetComponent<UIListItemContainer>());
				uIScrollList.AddItem(gameObject2);
			}
		}
	}

	private void FillFlagList(GameObject panelProfile, int totalNrOfFlags)
	{
		m_profileTab.m_flags = m_userMan.GetAvailableFlags();
		UIScrollList uIScrollList = GuiUtils.FindChildOfComponent<UIScrollList>(m_gui, "FlagList");
		uIScrollList.ClearList(destroy: true);
		int num = 6;
		int num2 = 1 + totalNrOfFlags / num;
		for (int i = 0; i < num2; i++)
		{
			GameObject gameObject = GuiUtils.CreateGui("FlagListItem", m_guiCamera);
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				string text = "Flag " + (j + 1);
				UIButton uIButton = GuiUtils.FindChildOfComponent<UIButton>(gameObject, text);
				if (num3 < m_profileTab.m_flags.Count)
				{
					int num4 = m_profileTab.m_flags[num3];
					Texture2D flagTexture = GuiUtils.GetFlagTexture(num4);
					DebugUtils.Assert(flagTexture != null, "Missing flag " + num4);
					DebugUtils.Assert(uIButton != null, "Missing button " + text);
					GuiUtils.SetButtonImage(uIButton, flagTexture);
					uIButton.SetValueChangedDelegate(OnFlagListSelection);
					uIButton.name = num4.ToString();
				}
				else if (num3 >= totalNrOfFlags)
				{
					uIButton.transform.Translate(new Vector3(10000f, 0f, 0f));
				}
			}
			uIScrollList.AddItem(gameObject);
		}
	}

	private void OnFlagListSelection(IUIObject obj)
	{
		int flag = int.Parse(obj.name);
		m_userMan.SetFlag(flag);
		RequestProfileData();
		UIPanelManager uIPanelManager = GuiUtils.FindChildOfComponent<UIPanelManager>(m_gui, "FlagSelectionPanelMan");
		UIPanelBase currentPanel = uIPanelManager.CurrentPanel;
		uIPanelManager.Dismiss();
		currentPanel.Dismiss();
	}

	public string GetLocalUserName()
	{
		return m_localUserName;
	}

	private void OnGameListSelection(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		GamePost gamePost = m_playTab.m_games[component.Index];
		m_onJoin(gamePost.m_gameID);
	}

	private void OnOpenFaceBook(IUIObject obj)
	{
		Application.OpenURL("http://www.facebook.com/LeviathanWarships");
	}

	private void OnOpenTwitter(IUIObject obj)
	{
		Application.OpenURL("http://www.twitter.com/LeviathanGame");
	}

	private void OnOpenForum(IUIObject obj)
	{
		Application.OpenURL("http://forum.paradoxplaza.com/forum/forumdisplay.php?771-Leviathan-Warships");
	}

	private void OnItemBought()
	{
		if (m_onItemBought != null)
		{
			m_onItemBought();
		}
	}
}
