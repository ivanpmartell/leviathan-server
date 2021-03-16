#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class LobbyMenu
{
	private class InvitePanel
	{
		public GameObject m_inviteGui;

		public UIPanel m_inviteFriendDialog;

		public List<FriendData> m_friends = new List<FriendData>();

		public UIScrollList m_friendList;

		public GameObject m_friendListItem;

		public int m_selectedIndex = -1;
	}

	private class Chat
	{
		public UITextField m_textInput;

		public UIScrollList m_messageList;

		public GameObject m_listItem;
	}

	public Action m_onExit;

	public Action<string> m_playerRemovedDelegate;

	private GameObject m_gui;

	private PTech.RPC m_rpc;

	private ChatClient m_chatClient;

	private UserManClient m_userManClient;

	private bool m_visible = true;

	private LobbyPlayer m_currentPlayer;

	private MapInfo m_mapInfo;

	private GameSettings m_settings;

	private GameObject m_guiCamera;

	private int m_campaignID;

	private string m_selectedFleet;

	private bool m_noFleet;

	private bool m_allowMatchmaking;

	private SpriteText m_lblPointGoalTitel;

	private SpriteText m_lblPointGoalValue;

	private SpriteText m_lblGameName;

	private SpriteText m_lblMapName;

	private SpriteText m_lblMapSize;

	private SimpleSprite m_mapIcon;

	private SpriteText m_lblFleetSize;

	private SpriteText m_lblNrOfPlayers;

	private SpriteText m_lblMaxTurnTime;

	private SpriteText m_lblGameType;

	private SpriteText m_lblMissionDescription;

	private UIScrollList m_team1List;

	private UIScrollList m_team2List;

	private UIButton m_btnDisband;

	private UIButton m_btnLeave;

	private UIButton m_btnRename;

	private UIButton m_btnReady;

	private UIButton m_btnSwitchTeam;

	private UIButton m_btnFleetEdit;

	private UIButton m_btnSelectFleet;

	private UIButton m_btnInvite;

	private UIStateToggleBtn m_matchmakeCheckbox;

	private PackedSprite m_matchmakingSearchIcon;

	private SpriteText m_matchmakingSearchText;

	private InvitePanel m_invitePanel = new InvitePanel();

	private Chat m_chat = new Chat();

	private List<LobbyPlayer> m_playerList = new List<LobbyPlayer>();

	private ChooseFleetMenu m_chooseFleetMenu;

	private FleetMenu m_fleetEditor;

	private MsgBox m_msgBox;

	private GameObject m_renameDialog;

	private GameObject m_selectFleetGui;

	private UIScrollList m_fleetList;

	private GameObject m_gameFleetItem;

	private List<string> m_fleets = new List<string>();

	private List<int> m_fleetsSize = new List<int>();

	private string m_selectedOpenFleet;

	private bool m_fleetSelectorWasOpen;

	private MusicManager m_musMan;

	public LobbyMenu(GameObject guiCamera, MapInfo map, PTech.RPC rpc, UserManClient userman, int campaignID, ChatClient chatClient, MusicManager musMan, string hostName)
	{
		m_musMan = musMan;
		m_guiCamera = guiCamera;
		m_mapInfo = map;
		m_rpc = rpc;
		m_userManClient = userman;
		m_campaignID = campaignID;
		m_chatClient = chatClient;
		switch (map.m_gameMode)
		{
		case GameType.Challenge:
		case GameType.Campaign:
			m_gui = GuiUtils.CreateGui("Lobby/LobbyWindow_CampaignChallenge", guiCamera);
			break;
		case GameType.Points:
		case GameType.Assassination:
			m_gui = GuiUtils.CreateGui("Lobby/LobbyWindow_AssassinPoints", guiCamera);
			break;
		}
		m_btnReady = GuiUtils.FindChildOf(m_gui, "btnReady").GetComponent<UIButton>();
		m_btnReady.SetValueChangedDelegate(OnReadyPressed);
		m_btnSwitchTeam = GuiUtils.FindChildOf(m_gui, "btnSwitchTeam").GetComponent<UIButton>();
		m_btnSwitchTeam.AddValueChangedDelegate(OnSwitchTeam);
		m_btnSelectFleet = GuiUtils.FindChildOf(m_gui, "btnSelectFleet").GetComponent<UIButton>();
		m_btnSelectFleet.AddValueChangedDelegate(OnSelectFleet);
		m_btnFleetEdit = GuiUtils.FindChildOf(m_gui, "btnFleetEdit").GetComponent<UIButton>();
		m_btnFleetEdit.AddValueChangedDelegate(OnEditFleet);
		m_btnInvite = GuiUtils.FindChildOf(m_gui, "btnInviteFriend").GetComponent<UIButton>();
		m_btnInvite.SetValueChangedDelegate(OnInviteClicked);
		m_matchmakingSearchIcon = GuiUtils.FindChildOfComponent<PackedSprite>(m_gui, "loadingIconAnimated");
		m_matchmakingSearchText = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "LblSearchingForPlayers");
		m_matchmakeCheckbox = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_gui, "MatchmakeCheckbox");
		m_matchmakeCheckbox.SetValueChangedDelegate(OnMatchmakeChanged);
		m_lblGameName = GuiUtils.FindChildOf(m_gui, "LobbyTitleLabel").GetComponent<SpriteText>();
		GuiUtils.FindChildOf(m_gui, "ActiveHostLabel").GetComponent<SpriteText>().Text = hostName;
		GameObject gameObject = GuiUtils.FindChildOf(m_gui, "Team1List");
		GameObject gameObject2 = GuiUtils.FindChildOf(m_gui, "Team2List");
		if (gameObject != null)
		{
			m_team1List = gameObject.GetComponent<UIScrollList>();
		}
		if (gameObject2 != null)
		{
			m_team2List = gameObject2.GetComponent<UIScrollList>();
		}
		m_mapIcon = GuiUtils.FindChildOf(m_gui, "Map").GetComponent<SimpleSprite>();
		m_lblMapName = GuiUtils.FindChildOf(m_gui, "MapValueLabel").GetComponent<SpriteText>();
		m_lblMapSize = GuiUtils.FindChildOf(m_gui, "MapSizeValueLabel").GetComponent<SpriteText>();
		m_btnDisband = GuiUtils.FindChildOf(m_gui, "btnDisband").GetComponent<UIButton>();
		m_btnLeave = GuiUtils.FindChildOf(m_gui, "btnLeave").GetComponent<UIButton>();
		m_btnRename = GuiUtils.FindChildOf(m_gui, "RenameGameButton").GetComponent<UIButton>();
		m_lblFleetSize = GuiUtils.FindChildOf(m_gui, "FleetSizeValueLabel").GetComponent<SpriteText>();
		m_lblNrOfPlayers = GuiUtils.FindChildOf(m_gui, "PlayersValueLabel").GetComponent<SpriteText>();
		m_lblMaxTurnTime = GuiUtils.FindChildOf(m_gui, "PlanningTimerValueLabel").GetComponent<SpriteText>();
		m_lblGameType = GuiUtils.FindChildOf(m_gui, "GametypeValueLabel").GetComponent<SpriteText>();
		m_lblMissionDescription = GuiUtils.FindChildOf(m_gui, "GametypeMissionDescriptionValueLabel").GetComponent<SpriteText>();
		GuiUtils.FindChildOf(m_gui, "btnBack").GetComponent<UIButton>().SetValueChangedDelegate(OnBackPressed);
		m_btnDisband.SetValueChangedDelegate(OnDisbandGamePressed);
		m_btnLeave.SetValueChangedDelegate(OnLeaveGamePressed);
		m_btnRename.SetValueChangedDelegate(OnRenameGamePressed);
		GameObject gameObject3 = GuiUtils.FindChildOf(m_gui, "PointGoalTitleLabel");
		if (gameObject3 != null)
		{
			m_lblPointGoalTitel = gameObject3.GetComponent<SpriteText>();
		}
		GameObject gameObject4 = GuiUtils.FindChildOf(m_gui, "PointGoalValueLabel");
		if (gameObject4 != null)
		{
			m_lblPointGoalValue = gameObject4.GetComponent<SpriteText>();
		}
		m_invitePanel.m_inviteGui = GuiUtils.CreateGui("Lobby/LobbyInviteFriendDialog", guiCamera);
		m_invitePanel.m_inviteFriendDialog = m_invitePanel.m_inviteGui.GetComponent<UIPanel>();
		m_invitePanel.m_friendList = GuiUtils.FindChildOf(m_invitePanel.m_inviteFriendDialog.gameObject, "InviteFriendDialogList").GetComponent<UIScrollList>();
		m_invitePanel.m_friendListItem = GuiUtils.FindChildOf(m_invitePanel.m_inviteFriendDialog.gameObject, "InviteFriendListItem");
		GuiUtils.FindChildOf(m_invitePanel.m_inviteFriendDialog.gameObject, "InviteFriendDialog_Cancel_Button").GetComponent<UIButton>().SetValueChangedDelegate(OnInviteCancel);
		GuiUtils.FindChildOf(m_invitePanel.m_inviteFriendDialog.gameObject, "InviteFriendDialog_Select_Button").GetComponent<UIButton>().SetValueChangedDelegate(OnInviteSelected);
		m_invitePanel.m_inviteFriendDialog.Dismiss();
		m_selectFleetGui = GuiUtils.CreateGui("Lobby/LobbyOpenFleetDialog", guiCamera);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Cancel_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnCancelOpenFleet);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Edit_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnEditOpenFleet);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Select_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnSelectOpenFleet);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Createnewfleet_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnNewFleet);
		SetModifyFleetButtonsStatus(enable: false);
		m_fleetList = GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialogList").GetComponent<UIScrollList>();
		m_gameFleetItem = Resources.Load("gui/Shipyard/FleetListItem") as GameObject;
		DebugUtils.Assert(m_gameFleetItem != null);
		SetupChat();
		FillFleetList();
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Combine(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		m_selectFleetGui.GetComponent<UIPanel>().Dismiss();
	}

	private void OnRenameGamePressed(IUIObject obj)
	{
		PLog.Log("rename");
		if (m_renameDialog == null)
		{
			m_renameDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$label_renamegame"), m_settings.m_gameName, OnRenameCancel, OnRenameOk);
		}
	}

	private void OnRenameOk(string name)
	{
		m_rpc.Invoke("RenameGame", name);
		UnityEngine.Object.Destroy(m_renameDialog);
	}

	private void OnRenameCancel()
	{
		UnityEngine.Object.Destroy(m_renameDialog);
	}

	private void OnUserManUpdate()
	{
		FillFleetList();
	}

	private int GetPlayersInTeam(int team)
	{
		int num = 0;
		foreach (LobbyPlayer player in m_playerList)
		{
			if (player.m_team == team)
			{
				num++;
			}
		}
		return num;
	}

	private void OnMatchmakeChanged(IUIObject obj)
	{
		m_rpc.Invoke("AllowMatchmaking", m_matchmakeCheckbox.StateNum == 0);
	}

	public void Setup(bool noFleet, bool allowMatchmaking, GameSettings settings, List<LobbyPlayer> players)
	{
		DebugUtils.Assert(players != null);
		DebugUtils.Assert(players.Count > 0);
		m_noFleet = noFleet;
		m_allowMatchmaking = allowMatchmaking;
		m_settings = settings;
		m_playerList = players;
		if (!m_visible)
		{
			return;
		}
		if (settings.m_nrOfPlayers == 1 || !settings.m_localAdmin)
		{
			m_matchmakeCheckbox.controlIsEnabled = false;
		}
		else
		{
			m_matchmakeCheckbox.SetToggleState((!allowMatchmaking) ? 1 : 0);
		}
		if (settings.m_nrOfPlayers > 1 && players.Count < settings.m_nrOfPlayers && allowMatchmaking)
		{
			m_matchmakingSearchIcon.gameObject.SetActiveRecursively(state: true);
			m_matchmakingSearchText.gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			m_matchmakingSearchIcon.gameObject.SetActiveRecursively(state: false);
			m_matchmakingSearchText.gameObject.SetActiveRecursively(state: false);
		}
		foreach (LobbyPlayer player in players)
		{
			if (player.m_id == m_settings.m_localPlayerID)
			{
				m_currentPlayer = player;
				m_selectedFleet = player.m_fleet;
				break;
			}
		}
		DebugUtils.Assert(m_currentPlayer != null);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialogSubHeader").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey("gamelobby_fleetsize") + " " + GetFleetSize(m_currentPlayer);
		m_lblGameName.Text = m_settings.m_gameName;
		SetupMapIfno(m_mapInfo);
		if (m_team1List != null)
		{
			m_team1List.ClearList(destroy: true);
		}
		if (m_team2List != null)
		{
			m_team2List.ClearList(destroy: true);
		}
		foreach (LobbyPlayer player2 in players)
		{
			AddPlayer(player2);
		}
		m_btnReady.Text = Localize.instance.Translate((!m_currentPlayer.m_readyToStart) ? "$gamelobby_ready" : "$gamelobby_unready");
		if (m_noFleet)
		{
			m_btnSelectFleet.gameObject.SetActiveRecursively(state: false);
			m_btnFleetEdit.gameObject.SetActiveRecursively(state: false);
		}
		else if (m_settings.m_gameType == GameType.Campaign || m_settings.m_gameType == GameType.Challenge)
		{
			m_btnSelectFleet.gameObject.SetActiveRecursively(state: false);
			m_btnFleetEdit.gameObject.SetActiveRecursively(state: true);
			m_btnFleetEdit.controlIsEnabled = m_selectedFleet != string.Empty && !m_currentPlayer.m_readyToStart;
		}
		else
		{
			m_btnSelectFleet.gameObject.SetActiveRecursively(state: true);
			m_btnFleetEdit.gameObject.SetActiveRecursively(state: false);
			bool activeRecursively = !HasValidFleet(m_currentPlayer);
			m_btnSelectFleet.transform.FindChild("glow").gameObject.SetActiveRecursively(activeRecursively);
		}
		if (m_settings.m_gameType == GameType.Points || m_settings.m_gameType == GameType.Assassination)
		{
			m_btnSwitchTeam.gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			m_btnSwitchTeam.gameObject.SetActiveRecursively(state: false);
		}
		m_btnDisband.gameObject.SetActiveRecursively(m_currentPlayer.m_admin);
		m_btnLeave.gameObject.SetActiveRecursively(!m_currentPlayer.m_admin);
		m_btnRename.gameObject.SetActiveRecursively(m_currentPlayer.m_admin);
		m_btnInvite.gameObject.SetActiveRecursively(m_currentPlayer.m_admin && m_settings.m_nrOfPlayers > 1 && players.Count < m_settings.m_nrOfPlayers);
		if (noFleet)
		{
			m_btnReady.gameObject.GetComponent<UIButton>().controlIsEnabled = true;
		}
		else if (HasValidFleet(m_currentPlayer))
		{
			m_btnReady.gameObject.GetComponent<UIButton>().controlIsEnabled = true;
		}
		else
		{
			m_btnReady.gameObject.GetComponent<UIButton>().controlIsEnabled = false;
		}
		if (m_currentPlayer.m_readyToStart)
		{
			m_btnSelectFleet.gameObject.GetComponent<UIButton>().controlIsEnabled = false;
			m_btnSwitchTeam.gameObject.GetComponent<UIButton>().controlIsEnabled = false;
			m_btnReady.transform.FindChild("glow").gameObject.SetActiveRecursively(state: false);
		}
		else
		{
			m_btnSelectFleet.gameObject.GetComponent<UIButton>().controlIsEnabled = true;
			m_btnSwitchTeam.gameObject.GetComponent<UIButton>().controlIsEnabled = true;
			m_btnReady.transform.FindChild("glow").gameObject.SetActiveRecursively(state: true);
		}
	}

	private bool HasValidFleet(LobbyPlayer player)
	{
		return HasValidFleetSize(player, player.m_fleetValue);
	}

	private bool Is2VS1Game()
	{
		if ((m_settings.m_gameType == GameType.Assassination || m_settings.m_gameType == GameType.Points) && m_settings.m_nrOfPlayers == 3)
		{
			return true;
		}
		return false;
	}

	private bool HasValidFleetSize(LobbyPlayer player, int size)
	{
		bool dubble = NeedDoubleFleet(player);
		if (!m_settings.m_fleetSizeLimits.ValidSize(size, dubble))
		{
			return false;
		}
		return true;
	}

	private void AddPlayer(LobbyPlayer player)
	{
		GameObject gameObject = null;
		if (m_settings.m_gameType == GameType.Campaign || m_settings.m_gameType == GameType.Challenge)
		{
			switch (player.m_id)
			{
			case 0:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam1PlayerItem", m_guiCamera);
				break;
			case 1:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam2PlayerItem", m_guiCamera);
				break;
			case 2:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam3PlayerItem", m_guiCamera);
				break;
			case 3:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam4PlayerItem", m_guiCamera);
				break;
			}
		}
		else
		{
			switch (player.m_team)
			{
			case 0:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam1PlayerItem", m_guiCamera);
				break;
			case 1:
				gameObject = GuiUtils.CreateGui("Lobby/LobbyTeam2PlayerItem", m_guiCamera);
				break;
			}
		}
		bool flag = player.m_id == m_settings.m_localPlayerID;
		GuiUtils.FindChildOf(gameObject, "lblPlayerName").GetComponent<SpriteText>().Text = player.m_name;
		SimpleSprite component = GuiUtils.FindChildOf(gameObject, "playerFlag").GetComponent<SimpleSprite>();
		Texture2D flagTexture = GuiUtils.GetFlagTexture(player.m_flag);
		GuiUtils.SetImage(component, flagTexture);
		if (!player.m_readyToStart)
		{
			GuiUtils.FindChildOf(gameObject, "PlayerReady").transform.position = new Vector3(0f, 10000f, 0f);
		}
		SpriteText component2 = GuiUtils.FindChildOf(gameObject, "SelectedFleetLabel").GetComponent<SpriteText>();
		SpriteText component3 = GuiUtils.FindChildOf(gameObject, "FleetValueLabel").GetComponent<SpriteText>();
		if (m_noFleet)
		{
			component2.Text = string.Empty;
			component3.Text = string.Empty;
		}
		else if (flag)
		{
			if (player.m_fleet == string.Empty)
			{
				component2.Text = Localize.instance.Translate("$lobby_nofleetselected");
				component3.Text = string.Empty;
			}
			else if (HasValidFleet(player))
			{
				component2.Text = player.m_fleet;
				component3.Text = player.m_fleetValue.ToString();
			}
			else
			{
				component2.Text = Localize.instance.Translate("$lobby_invalidsize");
				component3.Text = player.m_fleetValue.ToString();
				component2.SetColor(Color.red);
				component3.SetColor(Color.red);
			}
		}
		else if (player.m_fleet != string.Empty)
		{
			component2.Text = Localize.instance.Translate("$lobby_fleetselected");
			component3.Text = player.m_fleetValue.ToString();
		}
		else
		{
			component2.Text = string.Empty;
			component3.Text = string.Empty;
		}
		if (player.m_status != 0 || flag)
		{
			GuiUtils.FindChildOf(gameObject, "PlayerStatus_Offline").transform.position = new Vector3(0f, 10000f, 0f);
		}
		if (player.m_status != PlayerPresenceStatus.Online || flag)
		{
			GuiUtils.FindChildOf(gameObject, "PlayerStatus_Online").transform.position = new Vector3(0f, 10000f, 0f);
		}
		if (player.m_status != PlayerPresenceStatus.InGame || flag)
		{
			GuiUtils.FindChildOf(gameObject, "PlayerStatus_Present").transform.position = new Vector3(0f, 10000f, 0f);
		}
		SimpleSprite component4 = GuiUtils.FindChildOf(gameObject, "Player_Admin").GetComponent<SimpleSprite>();
		if (!player.m_admin)
		{
			component4.transform.position = new Vector3(0f, 10000f, 0f);
		}
		UIButton component5 = GuiUtils.FindChildOf(gameObject, "btnRemove").GetComponent<UIButton>();
		if (flag || !m_currentPlayer.m_admin)
		{
			component5.gameObject.transform.position = new Vector3(0f, 10000f, 0f);
		}
		else
		{
			component5.SetValueChangedDelegate(OnRemoveClicked);
		}
		if (player.m_team == 1)
		{
			m_team2List.AddItem(gameObject.GetComponent<UIListItemContainer>());
		}
		else
		{
			m_team1List.AddItem(gameObject.GetComponent<UIListItemContainer>());
		}
	}

	public void Update()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Update();
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Update();
		}
	}

	public void OnLevelWasLoaded()
	{
		if (m_fleetEditor != null)
		{
			m_fleetEditor.OnLevelWasLoaded();
		}
	}

	private void FillFleetList()
	{
		m_selectedOpenFleet = string.Empty;
		m_fleets.Clear();
		m_fleetsSize.Clear();
		m_fleetList.SelectedItem = null;
		m_fleetList.ClearList(destroy: true);
		List<FleetDef> fleetDefs = m_userManClient.GetFleetDefs(0);
		foreach (FleetDef item in fleetDefs)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_gameFleetItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Name").GetComponent<SpriteText>().Text = item.m_name;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Ships").GetComponent<SpriteText>().Text = item.m_ships.Count.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Points").GetComponent<SpriteText>().Text = item.m_value.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Size").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey(FleetSizes.GetSizeClassName(item.m_value));
			UIRadioBtn component = GuiUtils.FindChildOf(gameObject, "RadioButton").GetComponent<UIRadioBtn>();
			component.SetValueChangedDelegate(OnOpenFleetListSelection);
			UIListItemContainer component2 = gameObject.GetComponent<UIListItemContainer>();
			if (!item.m_available)
			{
				component.controlIsEnabled = false;
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "LblDLCNotAvailable").transform.Translate(new Vector3(0f, 0f, 20f));
			}
			m_fleetList.AddItem(component2);
			m_fleets.Add(item.m_name);
			m_fleetsSize.Add(item.m_value);
		}
		SetModifyFleetButtonsStatus(enable: false);
	}

	public void Hide()
	{
		DebugUtils.Assert(m_gui != null);
		m_gui.SetActiveRecursively(state: false);
		m_selectFleetGui.SetActiveRecursively(state: false);
		m_invitePanel.m_inviteGui.SetActiveRecursively(state: false);
		m_visible = false;
	}

	public void Show()
	{
		if (!m_visible)
		{
			m_visible = true;
			m_gui.SetActiveRecursively(state: true);
			Setup(m_noFleet, m_allowMatchmaking, m_settings, m_playerList);
		}
	}

	public void Close()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_rpc.Unregister("Friends");
		if (m_chooseFleetMenu != null)
		{
			m_chooseFleetMenu.Close();
			m_chooseFleetMenu = null;
		}
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Close();
			m_fleetEditor = null;
		}
		if (m_renameDialog != null)
		{
			UnityEngine.Object.Destroy(m_renameDialog);
			m_renameDialog = null;
		}
		if (m_gui != null)
		{
			UnityEngine.Object.Destroy(m_gui);
			m_gui = null;
		}
		UnityEngine.Object.Destroy(m_selectFleetGui);
		UnityEngine.Object.Destroy(m_invitePanel.m_inviteGui);
		UnityEngine.Object.Destroy(m_chat.m_listItem);
		ChatClient chatClient = m_chatClient;
		chatClient.m_onNewMessage = (Action<ChannelID, ChatClient.ChatMessage>)Delegate.Remove(chatClient.m_onNewMessage, new Action<ChannelID, ChatClient.ChatMessage>(OnNewChatMessage));
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
	}

	private void LoadGUI(GameObject guiCamera)
	{
		DebugUtils.Assert(guiCamera != null, "LobbyMenu ctor called with NULL camera !");
		m_gui = GuiUtils.CreateGui("LobbyMenu", guiCamera);
		DebugUtils.Assert(m_gui != null, "LobbyMenu failed to validate root object m_gui !");
	}

	public static string TranslatedMapName(string mapName)
	{
		return Localize.instance.TranslateKey("mapname_" + mapName);
	}

	private bool NeedDoubleFleet(LobbyPlayer player)
	{
		if (Is2VS1Game() && GetPlayersInTeam(player.m_team) == 1)
		{
			return true;
		}
		return false;
	}

	private string GetFleetSize(LobbyPlayer player)
	{
		string empty = string.Empty;
		if (m_settings.m_gameType == GameType.Campaign || m_settings.m_gameType == GameType.Challenge)
		{
			return m_settings.m_fleetSizeLimits.ToString();
		}
		int num = m_settings.m_fleetSizeLimits.max;
		if (NeedDoubleFleet(player))
		{
			num *= 2;
		}
		return Localize.instance.TranslateKey(FleetSizes.GetSizeClassName(m_settings.m_fleetSizeLimits)) + " (" + num + ")";
	}

	private void SetupMapIfno(MapInfo map)
	{
		DebugUtils.Assert(m_mapIcon != null);
		DebugUtils.Assert(m_lblMapName != null);
		if (map.m_thumbnail == string.Empty)
		{
			return;
		}
		Texture2D texture2D = Resources.Load("MapThumbs/" + map.m_thumbnail) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning("Map Thumbnail " + map.m_thumbnail + " is missing");
			return;
		}
		GuiUtils.SetImage(m_mapIcon, texture2D);
		switch (m_settings.m_gameType)
		{
		case GameType.Assassination:
			m_lblGameType.Text = Localize.instance.Translate("$lobby_assassination");
			break;
		case GameType.Points:
			m_lblGameType.Text = Localize.instance.Translate("$lobby_skirmish");
			break;
		case GameType.Campaign:
			m_lblGameType.Text = Localize.instance.Translate("$lobby_campaign");
			break;
		case GameType.Challenge:
			m_lblGameType.Text = Localize.instance.Translate("$lobby_challenge");
			break;
		default:
			DebugUtils.Assert(condition: false, "Invalid game mode");
			break;
		}
		m_lblMapName.Text = TranslatedMapName(map.m_name);
		m_lblMapSize.Text = Localize.instance.Translate(map.m_size + "x" + map.m_size);
		if (m_settings.m_gameType == GameType.Campaign || m_settings.m_gameType == GameType.Challenge)
		{
			string text = "$creategame_fleetsize: " + m_settings.m_fleetSizeLimits.ToString() + " $label_pointssmall";
			GuiUtils.FindChildOf(m_gui, "FleetSizeBigLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(text);
			m_lblFleetSize.Text = m_settings.m_fleetSizeLimits.ToString();
		}
		else
		{
			m_lblFleetSize.Text = Localize.instance.TranslateKey(FleetSizes.GetSizeClassName(m_settings.m_fleetSizeLimits)) + " (" + m_settings.m_fleetSizeLimits.ToString() + ")";
		}
		m_lblNrOfPlayers.Text = Localize.instance.Translate(m_settings.m_nrOfPlayers + Localize.instance.Translate(" $label_players"));
		if (m_settings.m_maxTurnTime > 0.0)
		{
			m_lblMaxTurnTime.Text = Localize.instance.Translate(Utils.FormatTimeLeftString(m_settings.m_maxTurnTime));
		}
		else
		{
			m_lblMaxTurnTime.Text = Localize.instance.Translate("$label_none");
		}
		if (m_settings.m_gameType == GameType.Campaign || m_settings.m_gameType == GameType.Challenge || m_settings.m_gameType == GameType.Custom)
		{
			m_lblMissionDescription.Text = Localize.instance.Translate(map.m_description);
		}
		else
		{
			switch (m_settings.m_gameType)
			{
			case GameType.Points:
				m_lblMissionDescription.Text = Localize.instance.Translate("$creategame_desc_skirmish");
				break;
			case GameType.Assassination:
				m_lblMissionDescription.Text = Localize.instance.Translate("$creategame_desc_ass");
				break;
			}
		}
		if (m_settings.m_gameType == GameType.Points)
		{
			int num = (int)((float)m_settings.m_fleetSizeLimits.max * m_settings.m_targetScore);
			if (m_settings.m_nrOfPlayers >= 3)
			{
				num *= 2;
			}
			m_lblPointGoalValue.Text = num.ToString();
			return;
		}
		if (m_lblPointGoalTitel != null)
		{
			m_lblPointGoalTitel.gameObject.SetActiveRecursively(state: false);
		}
		if (m_lblPointGoalValue != null)
		{
			m_lblPointGoalValue.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void Exit()
	{
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	private void SetModifyFleetButtonsStatus(bool enable)
	{
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Select_Button").GetComponent<UIButton>().controlIsEnabled = enable;
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Edit_Button").GetComponent<UIButton>().controlIsEnabled = enable;
	}

	private void OnSwitchTeam(IUIObject obj)
	{
		if (m_visible)
		{
			m_rpc.Invoke("SwitchTeam");
		}
	}

	private void OnReadyPressed(IUIObject obj)
	{
		if (m_visible)
		{
			m_rpc.Invoke("ReadyToStart");
		}
	}

	private void OnBackPressed(IUIObject obj)
	{
		if (m_visible)
		{
			Exit();
		}
	}

	private void OnLeaveGamePressed(IUIObject obj)
	{
		if (m_visible && !m_currentPlayer.m_admin)
		{
			m_msgBox = new MsgBox(m_guiCamera, MsgBox.Type.YesNo, "$Lobby_LeaveGame", null, null, OnLeaveGameYes, OnMsgBoxCancel);
		}
	}

	private void OnDisbandGamePressed(IUIObject obj)
	{
		if (m_visible && m_currentPlayer.m_admin)
		{
			m_msgBox = new MsgBox(m_guiCamera, MsgBox.Type.YesNo, "$Lobby_DisbandGame", null, null, OnDisbandGameYes, OnMsgBoxCancel);
		}
	}

	private void OnMsgBoxCancel()
	{
		m_msgBox = null;
	}

	private void OnDisbandGameYes()
	{
		m_msgBox = null;
		m_rpc.Invoke("DisbandGame");
	}

	private void OnLeaveGameYes()
	{
		m_msgBox = null;
		m_rpc.Invoke("KickSelf");
	}

	private void OnRemoveClicked(IUIObject button)
	{
		if (m_currentPlayer.m_admin)
		{
			string text = GuiUtils.FindChildOf(button.transform.parent, "lblPlayerName").GetComponent<SpriteText>().Text;
			if (m_playerRemovedDelegate != null)
			{
				m_playerRemovedDelegate(text);
			}
		}
	}

	private void OnChooseFleetClicked(LobbyMenu_Player sender)
	{
		PLog.Log("Lobby recived delegate that " + sender.GetPlayer().m_name + " wants to choose fleet.");
		SetupChooseFleet();
	}

	private void OnInviteClicked(IUIObject button)
	{
		DebugUtils.Assert(m_currentPlayer != null, "LobbyMenu::OnInviteClicked() no current player specified !");
		DebugUtils.Assert(m_currentPlayer.m_admin, "LobbyMenu::OnInviteClicked() current player is not admin !");
		m_invitePanel.m_inviteFriendDialog.gameObject.SetActiveRecursively(state: true);
		m_invitePanel.m_inviteFriendDialog.BringIn();
		m_invitePanel.m_selectedIndex = -1;
		m_rpc.Register("Friends", RPC_Friends);
		m_rpc.Invoke("RequestFriends");
	}

	private void OnInviteCancel(IUIObject obj)
	{
		m_invitePanel.m_friendList.ClearList(destroy: true);
		m_invitePanel.m_inviteFriendDialog.Dismiss();
	}

	private void OnInviteSelected(IUIObject obj)
	{
		if (m_invitePanel.m_selectedIndex != -1)
		{
			FriendData friendData = m_invitePanel.m_friends[m_invitePanel.m_selectedIndex];
			PLog.Log("invite friend " + friendData.m_name);
			m_rpc.Invoke("Invite", friendData.m_friendID);
			m_invitePanel.m_friendList.ClearList(destroy: true);
			m_invitePanel.m_inviteFriendDialog.Dismiss();
		}
	}

	private void OnFriendPressed(IUIObject obj)
	{
		m_invitePanel.m_selectedIndex = obj.transform.parent.GetComponent<UIListItemContainer>().Index;
	}

	private void RPC_Friends(PTech.RPC rpc, List<object> args)
	{
		rpc.Unregister("Friends");
		m_invitePanel.m_friends.Clear();
		foreach (object arg in args)
		{
			FriendData friendData = new FriendData();
			friendData.FromArray((byte[])arg);
			if (friendData.m_status == FriendData.FriendStatus.IsFriend)
			{
				m_invitePanel.m_friends.Add(friendData);
			}
		}
		m_invitePanel.m_friendList.ClearList(destroy: true);
		foreach (FriendData friend in m_invitePanel.m_friends)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_invitePanel.m_friendListItem) as GameObject;
			GuiUtils.FindChildOf(gameObject, "FriendNameLabel").GetComponent<SpriteText>().Text = friend.m_name;
			SimpleSprite component = GuiUtils.FindChildOf(gameObject, "FriendOnlineStatus").GetComponent<SimpleSprite>();
			SimpleSprite component2 = GuiUtils.FindChildOf(gameObject, "FriendOfflineStatus").GetComponent<SimpleSprite>();
			if (friend.m_online)
			{
				component2.transform.Translate(0f, 0f, 20f);
			}
			else
			{
				component.transform.Translate(0f, 0f, 20f);
			}
			GuiUtils.FindChildOf(gameObject, "InviteFriendRadioButton").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnFriendPressed);
			SimpleSprite component3 = GuiUtils.FindChildOf(gameObject, "FriendFlag").GetComponent<SimpleSprite>();
			Texture2D flagTexture = GuiUtils.GetFlagTexture(friend.m_flagID);
			GuiUtils.SetImage(component3, flagTexture);
			UIListItemContainer component4 = gameObject.GetComponent<UIListItemContainer>();
			GuiUtils.FixedItemContainerInstance(component4);
			m_invitePanel.m_friendList.AddItem(component4);
		}
	}

	private void SetupChooseFleet()
	{
		m_selectFleetGui.SetActiveRecursively(state: true);
		m_selectFleetGui.GetComponent<UIPanel>().BringIn();
	}

	private void OnOpenFleetListSelection(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		m_selectedOpenFleet = m_fleets[component.Index];
		SetModifyFleetButtonsStatus(enable: true);
		bool flag = HasValidFleetSize(m_currentPlayer, m_fleetsSize[component.Index]) || Is2VS1Game();
		PLog.Log("valid size " + flag + " size " + m_fleetsSize[component.Index]);
		GuiUtils.FindChildOf(m_selectFleetGui, "OpenFleetDialog_Select_Button").GetComponent<UIButton>().controlIsEnabled = flag;
	}

	private void OnCancelOpenFleet(IUIObject obj)
	{
		m_selectFleetGui.GetComponent<UIPanel>().Dismiss();
	}

	private void OnNewFleet(IUIObject obj)
	{
		m_selectedFleet = string.Empty;
		SetupFleetEditor(returnToFleetSelector: true);
	}

	private void OnEditOpenFleet(IUIObject obj)
	{
		m_selectedFleet = m_selectedOpenFleet;
		SetupFleetEditor(returnToFleetSelector: true);
	}

	private void OnSelectOpenFleet(IUIObject obj)
	{
		OnCancelOpenFleet(null);
		m_rpc.Invoke("SetFleet", m_selectedOpenFleet);
	}

	private void OnSelectFleet(IUIObject obj)
	{
		SetupChooseFleet();
	}

	private void OnEditFleet(IUIObject obj)
	{
		SetupFleetEditor(returnToFleetSelector: false);
	}

	private bool IsOneFleetOnly()
	{
		if (m_mapInfo.m_gameMode == GameType.Campaign)
		{
			return true;
		}
		if (m_mapInfo.m_gameMode == GameType.Challenge)
		{
			return true;
		}
		return false;
	}

	private void SetupFleetEditor(bool returnToFleetSelector)
	{
		Hide();
		m_fleetSelectorWasOpen = returnToFleetSelector;
		if (m_fleetEditor != null)
		{
			m_fleetEditor.Close();
		}
		string text = m_selectedFleet;
		FleetSize fleetSize = m_settings.m_fleetSizeLimits;
		if (NeedDoubleFleet(m_currentPlayer))
		{
			fleetSize = FleetSizes.sizes[1];
		}
		if (string.IsNullOrEmpty(text))
		{
			text = string.Empty;
		}
		m_fleetEditor = new FleetMenu(m_guiCamera, m_userManClient, text, m_campaignID, fleetSize, IsOneFleetOnly(), m_musMan);
		m_fleetEditor.m_onExit = OnFleetEditorExit;
	}

	private void OnFleetEditorExit()
	{
		FleetMenu fleetEditor = m_fleetEditor;
		fleetEditor.m_onExit = (FleetMenu.OnExitDelegate)Delegate.Remove(fleetEditor.m_onExit, new FleetMenu.OnExitDelegate(OnFleetEditorExit));
		m_fleetEditor.Close();
		m_fleetEditor = null;
		Show();
		if (m_fleetSelectorWasOpen)
		{
			SetupChooseFleet();
		}
		if (!string.IsNullOrEmpty(m_selectedFleet))
		{
			m_rpc.Invoke("FleetUpdated");
		}
	}

	private void SetupChat()
	{
		GameObject go = GuiUtils.FindChildOf(m_gui, "ChatContainer");
		m_chat.m_messageList = GuiUtils.FindChildOf(go, "GlobalFeedList").GetComponent<UIScrollList>();
		m_chat.m_textInput = GuiUtils.FindChildOf(go, "GlobalMessageBox").GetComponent<UITextField>();
		m_chat.m_listItem = GuiUtils.CreateGui("Lobby/GlobalChatListItem", m_guiCamera);
		m_chat.m_listItem.transform.Translate(new Vector3(1000000f, 0f, 0f));
		m_chat.m_textInput.SetCommitDelegate(OnSendChatMessage);
		ChatClient chatClient = m_chatClient;
		chatClient.m_onNewMessage = (Action<ChannelID, ChatClient.ChatMessage>)Delegate.Combine(chatClient.m_onNewMessage, new Action<ChannelID, ChatClient.ChatMessage>(OnNewChatMessage));
		List<ChatClient.ChatMessage> allMessages = m_chatClient.GetAllMessages(ChannelID.General);
		foreach (ChatClient.ChatMessage item in allMessages)
		{
			AddChatMessage(item);
		}
	}

	private void OnSendChatMessage(IKeyFocusable control)
	{
		if (control.Content != string.Empty)
		{
			m_chatClient.SendMessage(ChannelID.General, control.Content);
			m_chat.m_textInput.Text = string.Empty;
		}
		UIManager.instance.FocusObject = m_chat.m_textInput;
	}

	private void OnNewChatMessage(ChannelID channel, ChatClient.ChatMessage msg)
	{
		AddChatMessage(msg);
	}

	private void AddChatMessage(ChatClient.ChatMessage msg)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_chat.m_listItem) as GameObject;
		SpriteText component = gameObject.transform.Find("GlobalChatTimestampLabel").GetComponent<SpriteText>();
		SpriteText component2 = gameObject.transform.Find("GlobalChatNameLabel").GetComponent<SpriteText>();
		SpriteText component3 = gameObject.transform.Find("GlobalChatMessageLabel").GetComponent<SpriteText>();
		component.Text = msg.m_date.ToString("yyyy-MM-d HH:mm");
		component2.Text = msg.m_name;
		component3.Text = msg.m_message;
		m_chat.m_messageList.AddItem(gameObject);
		while (m_chat.m_messageList.Count > 40)
		{
			m_chat.m_messageList.RemoveItem(0, destroy: true);
		}
		m_chat.m_messageList.ScrollListTo(1f);
	}
}
