using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class Hud
{
	public enum Mode
	{
		Outcome,
		Planning,
		Waiting,
		Replay,
		ReplayOutcome
	}

	private enum ControlType
	{
		Planning,
		Outcome,
		Replay,
		None
	}

	private class Chat
	{
		public UIPanel m_globalChatPanel;

		public UIPanel m_teamChatPanel;

		public UITextField m_globalTextInput;

		public UITextField m_teamTextInput;

		public UIScrollList m_globalMessageList;

		public UIScrollList m_teamMessageList;

		public GameObject m_listItem;

		public UIPanelManager m_panMan;
	}

	public Action m_onCommit;

	public Action<ExitState, int> m_onExit;

	public Action m_onPlayPause;

	public Action m_onStartTest;

	public Action m_onStopTest;

	public Action m_onStopOutcome;

	public Action m_onNextTurn;

	public Action m_onPrevTurn;

	public Action m_onQuitGame;

	public Action m_onSurrender;

	public Action m_onFadeComplete;

	public bool m_showObjectives;

	private GameObject m_gui;

	private PTech.RPC m_rpc;

	private TurnMan m_turnMan;

	private ChatClient m_chatClient;

	private Mode m_mode;

	private bool m_visible = true;

	private GameType m_gameType;

	private GameObject m_battlebarCampaign;

	private GameObject m_battlebarAssassinate;

	private GameObject m_battlebarPoints;

	private GameObject m_battlebarCurrent;

	private Dictionary<Mode, UIPanel> m_statusTexts = new Dictionary<Mode, UIPanel>();

	private GameObject m_controlPanelPlanning;

	private GameObject m_controlPanelOutcome;

	private GameObject m_timeline;

	private GameObject m_newTurnSoundPrefab;

	private UIButton m_nextTurnButton;

	private UIButton m_prevTurnButton;

	private UIButton m_startOutcomeButton;

	private UIButton m_restartOutcomeButton;

	private UIButton m_stopOutcomeButton;

	private UIButton m_pauseOutcomeButton;

	private UIButton m_commitButton;

	private UIStateToggleBtn m_hideHudButton;

	private SpriteText m_currentTurn;

	private SpriteText m_FPS_label;

	private SpriteText m_gameNameLabel;

	private SpriteText m_turnTimeRemainingLabel;

	private UIButton m_disbandGameButton;

	private bool m_teamChatWasVisible;

	private bool m_globalChatWasVisible;

	private bool m_objectivesWasVisible;

	private bool m_switchGameWasVisible;

	public UIPanelManager m_panManMainMenu;

	private Chat m_chat = new Chat();

	private bool m_onlineGame = true;

	private bool m_replayMode;

	private bool m_showFps;

	private GameObject m_guiCamera;

	private int m_turn;

	private int m_localPlayerID = -1;

	private int m_gameID;

	private List<GamePost> m_switchGameList = new List<GamePost>();

	private IngameMenu m_ingameMenu;

	private MissionLog m_missionLog;

	private MessageLog m_messageLog;

	private ShipTagMan m_shipTagMan;

	private Battlebar m_battlebar;

	private MsgBox m_msgBox;

	private string m_tempKickUserName;

	public Hud(PTech.RPC rpc, GameObject guiCamera, TurnMan turnMan, ChatClient chatClient, bool replayMode, bool onlineGame)
	{
		m_rpc = rpc;
		m_guiCamera = guiCamera;
		m_turnMan = turnMan;
		m_chatClient = chatClient;
		m_onlineGame = onlineGame;
		m_replayMode = replayMode;
		m_shipTagMan = new ShipTagMan(guiCamera);
		SetupGui();
		SetupChat();
		SetupIngameMenu();
		m_messageLog = new MessageLog(m_rpc, m_guiCamera);
		SetupBattlebar(GameType.None);
		SetControlPanel(Mode.Waiting);
		SetVisible(visible: false, hideAll: true, affectBattleBar: true);
		m_rpc.Register("GameList", RPC_GameList);
	}

	private void SetupGui()
	{
		m_gui = GuiUtils.CreateGui("IngameGui/Hud NEW", m_guiCamera);
		m_missionLog = new MissionLog(m_gui, m_guiCamera);
		m_gameNameLabel = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "GamenameLabel");
		m_hideHudButton = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_gui, "HideUIButton");
		m_hideHudButton.SetValueChangedDelegate(OnHideGui);
		GuiUtils.FindChildOf(m_gui, "HelpButton1").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		GuiUtils.FindChildOf(m_gui, "HelpButton2").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		GuiUtils.FindChildOf(m_gui, "HelpButton3").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		m_currentTurn = GuiUtils.FindChildOf(m_gui, "TurnLabel").GetComponent<SpriteText>();
		m_battlebarAssassinate = GuiUtils.FindChildOf(m_gui, "Battlebar_Versus_Assassinate");
		m_battlebarPoints = GuiUtils.FindChildOf(m_gui, "Battlebar_Versus_Points");
		m_battlebarCampaign = GuiUtils.FindChildOf(m_gui, "Battlebar_Campaign_Challenge");
		m_controlPanelPlanning = GuiUtils.FindChildOf(m_gui, "ControlPanel_Planning");
		m_restartOutcomeButton = GuiUtils.FindChildOf(m_controlPanelPlanning, "Replay_Button").GetComponent<UIButton>();
		m_restartOutcomeButton.SetValueChangedDelegate(OnRestartOutcomePressed);
		m_commitButton = GuiUtils.FindChildOf(m_controlPanelPlanning, "Commit_Button").GetComponent<UIButton>();
		m_commitButton.SetValueChangedDelegate(OnCommitPressed);
		m_turnTimeRemainingLabel = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "LblPlanningTimer");
		m_turnTimeRemainingLabel.Text = string.Empty;
		m_controlPanelOutcome = GuiUtils.FindChildOf(m_gui, "ControlPanel_Outcome_Replay_Record");
		m_nextTurnButton = GuiUtils.FindChildOf(m_controlPanelOutcome, "Next_Button").GetComponent<UIButton>();
		m_nextTurnButton.SetValueChangedDelegate(OnNextTurnPressed);
		m_prevTurnButton = GuiUtils.FindChildOf(m_controlPanelOutcome, "Prev_Button").GetComponent<UIButton>();
		m_prevTurnButton.SetValueChangedDelegate(OnPrevTurnPressed);
		m_startOutcomeButton = GuiUtils.FindChildOf(m_controlPanelOutcome, "Play_Button").GetComponent<UIButton>();
		m_startOutcomeButton.SetValueChangedDelegate(OnStartOutcomePressed);
		m_stopOutcomeButton = GuiUtils.FindChildOf(m_controlPanelOutcome, "Stop_Button").GetComponent<UIButton>();
		m_stopOutcomeButton.SetValueChangedDelegate(OnStopOutcomePressed);
		m_pauseOutcomeButton = GuiUtils.FindChildOf(m_controlPanelOutcome, "Pause_Button").GetComponent<UIButton>();
		m_pauseOutcomeButton.SetValueChangedDelegate(OnPauseOutcomePressed);
		m_disbandGameButton = GuiUtils.FindChildOfComponent<UIButton>(m_gui, "BtnDisband");
		if (m_disbandGameButton != null)
		{
			m_disbandGameButton.SetValueChangedDelegate(OnDisbandGame);
		}
		UIPanelTab uIPanelTab = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarAssassinate, "Button_SwitchGame");
		UIPanelTab uIPanelTab2 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarPoints, "Button_SwitchGame");
		UIPanelTab uIPanelTab3 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarCampaign, "Button_SwitchGame");
		uIPanelTab.SetValueChangedDelegate(OnOpenSwitchGame);
		uIPanelTab2.SetValueChangedDelegate(OnOpenSwitchGame);
		uIPanelTab3.SetValueChangedDelegate(OnOpenSwitchGame);
		if (!m_onlineGame)
		{
			uIPanelTab.Hide(tf: true);
			uIPanelTab2.Hide(tf: true);
			uIPanelTab3.Hide(tf: true);
		}
		m_panManMainMenu = GuiUtils.FindChildOfComponent<UIPanelManager>(m_gui, "MainMenuPanelMan");
		m_chat.m_panMan = GuiUtils.FindChildOfComponent<UIPanelManager>(m_gui, "ChatPanelMan");
		UIPanelTab uIPanelTab4 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarAssassinate, "Button_TeamChat");
		UIPanelTab uIPanelTab5 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarPoints, "Button_TeamChat");
		uIPanelTab4.SetValueChangedDelegate(OnTeamChatOpen);
		uIPanelTab5.SetValueChangedDelegate(OnTeamChatOpen);
		if (!m_onlineGame)
		{
			uIPanelTab4.Hide(tf: true);
			uIPanelTab5.Hide(tf: true);
		}
		UIPanelTab uIPanelTab6 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarAssassinate, "Button_GlobalChat");
		UIPanelTab uIPanelTab7 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarPoints, "Button_GlobalChat");
		UIPanelTab uIPanelTab8 = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarCampaign, "Button_GlobalChat");
		uIPanelTab6.SetValueChangedDelegate(OnGlobalChatOpen);
		uIPanelTab7.SetValueChangedDelegate(OnGlobalChatOpen);
		uIPanelTab8.SetValueChangedDelegate(OnGlobalChatOpen);
		if (!m_onlineGame)
		{
			uIPanelTab6.Hide(tf: true);
			uIPanelTab7.Hide(tf: true);
			uIPanelTab8.Hide(tf: true);
		}
		m_statusTexts.Add(Mode.Waiting, GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Status_Commit"));
		m_statusTexts.Add(Mode.Planning, GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Status_Planning"));
		m_statusTexts.Add(Mode.Outcome, GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Status_Outcome"));
		m_statusTexts.Add(Mode.Replay, GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Status_Record"));
		m_statusTexts.Add(Mode.ReplayOutcome, GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Status_Replay"));
		m_newTurnSoundPrefab = Resources.Load("NewTurnSound") as GameObject;
		m_FPS_label = GuiUtils.FindChildOf(m_gui, "lblFPS").GetComponent<SpriteText>();
		m_FPS_label.Text = string.Empty;
		m_FPS_label.gameObject.SetActiveRecursively(state: false);
	}

	public void SetBattleBarButtonState(string name, int state)
	{
		UIPanelTab uIPanelTab = null;
		uIPanelTab = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarAssassinate, name);
		if (uIPanelTab != null && uIPanelTab.controlIsEnabled)
		{
			uIPanelTab.SetState(state);
		}
		uIPanelTab = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarPoints, name);
		if (uIPanelTab != null && uIPanelTab.controlIsEnabled)
		{
			uIPanelTab.SetState(state);
		}
		uIPanelTab = GuiUtils.FindChildOfComponent<UIPanelTab>(m_battlebarCampaign, name);
		if (uIPanelTab != null && uIPanelTab.controlIsEnabled)
		{
			uIPanelTab.SetState(state);
		}
	}

	public bool DismissAnyPopup()
	{
		if ((bool)m_panManMainMenu.CurrentPanel)
		{
			SetBattleBarButtonState("Button_SwitchGame", 1);
			SetBattleBarButtonState("Button_MainMenu", 1);
			m_panManMainMenu.Dismiss();
			return true;
		}
		if ((bool)m_chat.m_panMan.CurrentPanel)
		{
			SetBattleBarButtonState("Button_TeamChat", 1);
			SetBattleBarButtonState("Button_GlobalChat", 1);
			SetBattleBarButtonState("Button_Objectives", 1);
			m_chat.m_panMan.Dismiss();
			return true;
		}
		return false;
	}

	private void OnTeamChatOpen(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIManager.instance.FocusObject = m_chat.m_teamTextInput;
		}
	}

	private void OnGlobalChatOpen(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			UIManager.instance.FocusObject = m_chat.m_globalTextInput;
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

	private void OnOpenSwitchGame(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			m_rpc.Invoke("RequestGameList", false);
		}
	}

	private void RPC_GameList(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("Gto games ");
		m_switchGameList.Clear();
		foreach (object arg in args)
		{
			GamePost gamePost = new GamePost();
			gamePost.FromArray((byte[])arg);
			if (gamePost.m_gameID != m_gameID)
			{
				m_switchGameList.Add(gamePost);
			}
		}
		m_switchGameList.Sort();
		UIScrollList uIScrollList = GuiUtils.FindChildOfComponent<UIScrollList>(m_gui, "SwitchGame_Scrollist");
		uIScrollList.ClearList(destroy: true);
		foreach (GamePost switchGame in m_switchGameList)
		{
			string text = ((switchGame.m_turn < 0) ? "-" : (switchGame.m_turn + 1).ToString());
			GameObject gameObject = GuiUtils.CreateGui("gamelist/Gamelist_listitem", m_guiCamera);
			GuiUtils.FindChildOf(gameObject, "NameValueLabel").GetComponent<SpriteText>().Text = switchGame.m_gameName;
			GuiUtils.FindChildOf(gameObject, "TypeValueLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey("gametype_" + switchGame.m_gameType.ToString().ToLower());
			GuiUtils.FindChildOf(gameObject, "MapValueLabel").GetComponent<SpriteText>().Text = CreateGame.TranslatedMapName(switchGame.m_level);
			GuiUtils.FindChildOf(gameObject, "PlayersValueLabel").GetComponent<SpriteText>().Text = switchGame.m_connectedPlayers + "/" + switchGame.m_maxPlayers;
			GuiUtils.FindChildOf(gameObject, "TurnValueLabel").GetComponent<SpriteText>().Text = text;
			GuiUtils.FindChildOf(gameObject, "CreatedValueLabel").GetComponent<SpriteText>().Text = switchGame.m_createDate.ToString("yyyy-MM-d HH:mm");
			if (switchGame.m_needAttention)
			{
				GuiUtils.FindChildOf(gameObject, "StateIcon_Default").transform.Translate(0f, 0f, 20f);
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "StateIcon_NewTurn").transform.Translate(0f, 0f, 20f);
			}
			GuiUtils.FindChildOfComponent<UIButton>(gameObject, "Button").SetValueChangedDelegate(OnSwitchGamePressed);
			uIScrollList.AddItem(gameObject);
		}
	}

	private void OnSwitchGamePressed(IUIObject obj)
	{
		int index = obj.transform.parent.GetComponent<UIListItemContainer>().Index;
		GamePost gamePost = m_switchGameList[index];
		m_onExit(ExitState.JoinGame, gamePost.m_gameID);
	}

	private void SetControlPanel(Mode type)
	{
		switch (type)
		{
		case Mode.Waiting:
			m_controlPanelPlanning.GetComponent<UIPanel>().Dismiss();
			m_controlPanelOutcome.GetComponent<UIPanel>().Dismiss();
			break;
		case Mode.Planning:
			m_controlPanelPlanning.GetComponent<UIPanel>().BringIn();
			m_controlPanelOutcome.GetComponent<UIPanel>().Dismiss();
			m_timeline = GuiUtils.FindChildOf(m_controlPanelPlanning, "Timeline SuperSprite");
			break;
		case Mode.Outcome:
			m_controlPanelPlanning.GetComponent<UIPanel>().Dismiss();
			m_controlPanelOutcome.GetComponent<UIPanel>().BringIn();
			m_timeline = GuiUtils.FindChildOf(m_controlPanelOutcome, "Timeline SuperSprite");
			m_stopOutcomeButton.gameObject.SetActiveRecursively(state: false);
			m_nextTurnButton.gameObject.SetActiveRecursively(state: false);
			m_prevTurnButton.gameObject.SetActiveRecursively(state: false);
			break;
		case Mode.Replay:
			m_controlPanelPlanning.GetComponent<UIPanel>().Dismiss();
			m_controlPanelOutcome.GetComponent<UIPanel>().BringIn();
			m_timeline = GuiUtils.FindChildOf(m_controlPanelOutcome, "Timeline SuperSprite");
			m_stopOutcomeButton.gameObject.SetActiveRecursively(state: true);
			m_nextTurnButton.gameObject.SetActiveRecursively(state: true);
			m_prevTurnButton.gameObject.SetActiveRecursively(state: true);
			break;
		case Mode.ReplayOutcome:
			m_controlPanelPlanning.GetComponent<UIPanel>().Dismiss();
			m_controlPanelOutcome.GetComponent<UIPanel>().BringIn();
			m_timeline = GuiUtils.FindChildOf(m_controlPanelOutcome, "Timeline SuperSprite");
			m_stopOutcomeButton.gameObject.SetActiveRecursively(state: true);
			m_nextTurnButton.gameObject.SetActiveRecursively(state: false);
			m_prevTurnButton.gameObject.SetActiveRecursively(state: false);
			break;
		}
		if (m_replayMode)
		{
			m_stopOutcomeButton.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void SetupBattlebar(GameType gameType)
	{
		m_battlebarAssassinate.SetActiveRecursively(state: false);
		m_battlebarCampaign.SetActiveRecursively(state: false);
		m_battlebarPoints.SetActiveRecursively(state: false);
		m_battlebarCurrent = null;
		if (m_battlebar != null)
		{
			m_battlebar.Close();
			m_battlebar = null;
		}
		switch (gameType)
		{
		case GameType.Assassination:
			m_battlebarCurrent = m_battlebarAssassinate;
			break;
		case GameType.Points:
			m_battlebarCurrent = m_battlebarPoints;
			break;
		case GameType.Challenge:
		case GameType.Campaign:
			m_battlebarCurrent = m_battlebarCampaign;
			break;
		}
		if (m_battlebarCurrent != null)
		{
			m_battlebar = new Battlebar(m_gameType, m_battlebarCurrent, m_guiCamera, m_turnMan);
		}
	}

	private void SetupIngameMenu()
	{
		m_ingameMenu = new IngameMenu(GuiUtils.FindChildOf(m_gui, "MainMenuContainer"));
		m_ingameMenu.m_OnSurrender = IngameMenu_Surrender;
		m_ingameMenu.m_OnOptions = IngameMenu_Options;
		m_ingameMenu.m_OnSwitchGame = IngameMenu_SwitchGame;
		m_ingameMenu.m_OnBackToMenu = IngameMenu_BackToMenu;
		m_ingameMenu.m_OnQuitGame = IngameMenu_QuitGame;
		m_ingameMenu.m_OnLeave = IngameMenu_LeaveGame;
	}

	private void SetupChat()
	{
		m_chat.m_listItem = GuiUtils.CreateGui("Lobby/GlobalChatListItem", m_guiCamera);
		m_chat.m_listItem.transform.Translate(new Vector3(1000000f, 0f, 0f));
		GameObject gameObject = GuiUtils.FindChildOf(m_gui, "GlobalChatContainer");
		m_chat.m_globalChatPanel = gameObject.GetComponent<UIPanel>();
		m_chat.m_globalMessageList = GuiUtils.FindChildOf(gameObject, "GlobalFeedList").GetComponent<UIScrollList>();
		m_chat.m_globalTextInput = GuiUtils.FindChildOf(gameObject, "GlobalMessageBox").GetComponent<UITextField>();
		m_chat.m_globalTextInput.SetCommitDelegate(OnSendGlobalChatMessage);
		GameObject gameObject2 = GuiUtils.FindChildOf(m_gui, "TeamChatContainer");
		m_chat.m_teamChatPanel = gameObject2.GetComponent<UIPanel>();
		m_chat.m_teamMessageList = GuiUtils.FindChildOf(gameObject2, "TeamFeedList").GetComponent<UIScrollList>();
		m_chat.m_teamTextInput = GuiUtils.FindChildOf(gameObject2, "TeamMessageBox").GetComponent<UITextField>();
		m_chat.m_teamTextInput.SetCommitDelegate(OnSendTeamChatMessage);
		List<ChatClient.ChatMessage> allMessages = m_chatClient.GetAllMessages(ChannelID.General);
		foreach (ChatClient.ChatMessage item in allMessages)
		{
			AddChatMessage(ChannelID.General, item);
		}
		List<ChatClient.ChatMessage> allMessages2 = m_chatClient.GetAllMessages(ChannelID.Team0);
		foreach (ChatClient.ChatMessage item2 in allMessages2)
		{
			AddChatMessage(ChannelID.Team0, item2);
		}
		List<ChatClient.ChatMessage> allMessages3 = m_chatClient.GetAllMessages(ChannelID.Team1);
		foreach (ChatClient.ChatMessage item3 in allMessages3)
		{
			AddChatMessage(ChannelID.Team1, item3);
		}
		ChatClient chatClient = m_chatClient;
		chatClient.m_onNewMessage = (Action<ChannelID, ChatClient.ChatMessage>)Delegate.Combine(chatClient.m_onNewMessage, new Action<ChannelID, ChatClient.ChatMessage>(OnNewChatMessage));
	}

	private void OnSendGlobalChatMessage(IKeyFocusable control)
	{
		string content = control.Content;
		if (content.IndexOf("/") == 0)
		{
			content = content.Replace("/", string.Empty);
			CheatMan.instance.ActivateCheat(content, this);
			m_chat.m_globalTextInput.Text = string.Empty;
			return;
		}
		if (control.Content != string.Empty)
		{
			m_chatClient.SendMessage(ChannelID.General, control.Content);
			m_chat.m_globalTextInput.Text = string.Empty;
		}
		UIManager.instance.FocusObject = m_chat.m_globalTextInput;
	}

	private void OnSendTeamChatMessage(IKeyFocusable control)
	{
		if (control.Content != string.Empty)
		{
			int num;
			switch (m_turnMan.GetPlayerTeam(m_localPlayerID))
			{
			case -1:
				return;
			case 0:
				num = 1;
				break;
			default:
				num = 2;
				break;
			}
			ChannelID channel = (ChannelID)num;
			m_chatClient.SendMessage(channel, control.Content);
			m_chat.m_teamTextInput.Text = string.Empty;
		}
		UIManager.instance.FocusObject = m_chat.m_teamTextInput;
	}

	private void OnNewChatMessage(ChannelID channel, ChatClient.ChatMessage msg)
	{
		AddChatMessage(channel, msg);
		if (m_battlebar != null)
		{
			if (channel == ChannelID.General)
			{
				m_battlebar.SetGlobalChatGlow(enabled: true);
			}
			else
			{
				m_battlebar.SetTeamChatGlow(enabled: true);
			}
		}
	}

	public void AddChatMessage(ChannelID channel, ChatClient.ChatMessage msg)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_chat.m_listItem) as GameObject;
		SpriteText component = gameObject.transform.Find("GlobalChatTimestampLabel").GetComponent<SpriteText>();
		SpriteText component2 = gameObject.transform.Find("GlobalChatNameLabel").GetComponent<SpriteText>();
		SpriteText component3 = gameObject.transform.Find("GlobalChatMessageLabel").GetComponent<SpriteText>();
		component.Text = msg.m_date.ToString("yyyy-MM-d HH:mm");
		component2.Text = msg.m_name;
		component3.Text = msg.m_message;
		UIScrollList uIScrollList = ((channel != 0) ? m_chat.m_teamMessageList : m_chat.m_globalMessageList);
		uIScrollList.AddItem(gameObject);
		while (uIScrollList.Count > 40)
		{
			uIScrollList.RemoveItem(0, destroy: true);
		}
		uIScrollList.ScrollListTo(1f);
	}

	public void ShowFps(bool enabled)
	{
		if (enabled != m_showFps)
		{
			m_FPS_label.gameObject.SetActiveRecursively(enabled);
			m_showFps = enabled;
		}
	}

	public void Close()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		if (m_missionLog != null)
		{
			m_missionLog.Close();
		}
		if (m_messageLog != null)
		{
			m_messageLog.Close();
		}
		UnityEngine.Object.Destroy(m_gui);
		m_shipTagMan.Close();
		UnityEngine.Object.Destroy(m_chat.m_listItem);
		ChatClient chatClient = m_chatClient;
		chatClient.m_onNewMessage = (Action<ChannelID, ChatClient.ChatMessage>)Delegate.Remove(chatClient.m_onNewMessage, new Action<ChannelID, ChatClient.ChatMessage>(OnNewChatMessage));
	}

	private void SetGameType(GameType type)
	{
		if (type != m_gameType)
		{
			m_gameType = type;
			m_shipTagMan.SetGameType(type);
			SetupBattlebar(type);
		}
	}

	public void SetVisible(bool visible, bool hideAll, bool affectBattleBar)
	{
		m_visible = visible;
		HitText.instance.SetVisible(visible);
		m_shipTagMan.SetVisible(visible);
		m_messageLog.SetVisible(visible);
		Route.m_drawGui = visible;
		if (affectBattleBar && m_battlebar != null)
		{
			m_battlebar.SetVisible(visible);
		}
		if (hideAll)
		{
			m_hideHudButton.Hide(!m_visible);
		}
		if (m_visible)
		{
			if (m_battlebarCurrent != null)
			{
				GuiUtils.FindChildOf(m_battlebarCurrent, "Button_GlobalChat").SetActiveRecursively(state: true);
			}
			if (m_globalChatWasVisible)
			{
				GuiUtils.FindChildOf(m_gui, "GlobalChatContainer").GetComponent<UIPanel>().BringIn();
			}
			if (m_switchGameWasVisible)
			{
				GuiUtils.FindChildOf(m_gui, "SwitchGameContainer").GetComponent<UIPanel>().BringIn();
			}
			if (m_gameType == GameType.Campaign || m_gameType == GameType.Challenge)
			{
				if (m_battlebarCurrent != null)
				{
					GuiUtils.FindChildOf(m_battlebarCurrent, "Button_Objectives").SetActiveRecursively(state: true);
				}
				if (m_objectivesWasVisible)
				{
					GuiUtils.FindChildOf(m_gui, "ObjectivesContainer").GetComponent<UIPanel>().BringIn();
				}
			}
			else
			{
				if (m_battlebarCurrent != null)
				{
					GuiUtils.FindChildOf(m_battlebarCurrent, "Button_TeamChat").SetActiveRecursively(state: true);
				}
				if (m_teamChatWasVisible)
				{
					GuiUtils.FindChildOf(m_gui, "TeamChatContainer").GetComponent<UIPanel>().BringIn();
				}
			}
			m_currentTurn.gameObject.SetActiveRecursively(state: true);
			m_gameNameLabel.gameObject.SetActiveRecursively(state: true);
			m_turnTimeRemainingLabel.gameObject.SetActiveRecursively(state: true);
			SetStatusText(m_mode);
			SetControlPanel(m_mode);
			m_FPS_label.gameObject.SetActiveRecursively(m_showFps);
			return;
		}
		if (m_battlebarCurrent != null)
		{
			GuiUtils.FindChildOf(m_battlebarCurrent, "Button_GlobalChat").SetActiveRecursively(state: false);
		}
		m_globalChatWasVisible = GuiUtils.FindChildOf(m_gui, "GlobalChatContainer").active;
		GuiUtils.FindChildOf(m_gui, "GlobalChatContainer").GetComponent<UIPanel>().Dismiss();
		m_switchGameWasVisible = GuiUtils.FindChildOf(m_gui, "SwitchGameContainer").active;
		GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "SwitchGameContainer").Dismiss();
		if (m_gameType == GameType.Campaign || m_gameType == GameType.Challenge)
		{
			if (m_battlebarCurrent != null)
			{
				GuiUtils.FindChildOf(m_battlebarCurrent, "Button_Objectives").SetActiveRecursively(state: false);
			}
			m_objectivesWasVisible = GuiUtils.FindChildOf(m_gui, "ObjectivesContainer").active;
			GuiUtils.FindChildOf(m_gui, "ObjectivesContainer").GetComponent<UIPanel>().Dismiss();
		}
		else
		{
			if (m_battlebarCurrent != null)
			{
				m_teamChatWasVisible = GuiUtils.FindChildOf(m_gui, "TeamChatContainer").active;
				GuiUtils.FindChildOf(m_battlebarCurrent, "Button_TeamChat").SetActiveRecursively(state: false);
			}
			GuiUtils.FindChildOf(m_gui, "TeamChatContainer").GetComponent<UIPanel>().Dismiss();
		}
		m_controlPanelOutcome.SetActiveRecursively(state: false);
		m_controlPanelPlanning.SetActiveRecursively(state: false);
		m_FPS_label.gameObject.SetActiveRecursively(state: false);
		m_currentTurn.gameObject.SetActiveRecursively(state: false);
		m_gameNameLabel.gameObject.SetActiveRecursively(state: false);
		m_turnTimeRemainingLabel.gameObject.SetActiveRecursively(state: false);
		HideAllStatusText();
		GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "MainMenuContainer").Dismiss();
	}

	public void SetMode(Mode mode)
	{
		m_mode = mode;
		if (m_visible)
		{
			SetStatusText(mode);
			SetControlPanel(mode);
			switch (m_mode)
			{
			case Mode.Replay:
				m_restartOutcomeButton.Hide(tf: true);
				m_startOutcomeButton.Hide(tf: true);
				m_pauseOutcomeButton.Hide(tf: false);
				m_startOutcomeButton.controlIsEnabled = true;
				break;
			case Mode.Outcome:
				m_messageLog.ShowMessage(MessageLog.TextPosition.Middle, "$hud_announcement_outcome", "$hud_announcement_turnx " + m_turn, "TurnMessage", 0.6f);
				UnityEngine.Object.Instantiate(m_newTurnSoundPrefab);
				m_restartOutcomeButton.Hide(tf: true);
				m_startOutcomeButton.Hide(tf: true);
				m_pauseOutcomeButton.Hide(tf: false);
				m_startOutcomeButton.controlIsEnabled = true;
				break;
			case Mode.Planning:
				m_messageLog.ShowMessage(MessageLog.TextPosition.Middle, "$hud_announcement_planning", "$hud_announcement_turnx " + m_turn, "TurnMessage", 0.6f);
				m_restartOutcomeButton.Hide(tf: false);
				m_restartOutcomeButton.controlIsEnabled = true;
				m_startOutcomeButton.Hide(tf: true);
				m_pauseOutcomeButton.Hide(tf: true);
				break;
			case Mode.Waiting:
				m_restartOutcomeButton.Hide(tf: false);
				m_restartOutcomeButton.controlIsEnabled = false;
				m_startOutcomeButton.Hide(tf: true);
				m_pauseOutcomeButton.Hide(tf: true);
				break;
			}
		}
	}

	public void SetGameInfo(string name, GameType gameType, int gameID, bool localAdmin)
	{
		m_gameNameLabel.Text = name;
		m_gameID = gameID;
		if (m_disbandGameButton != null)
		{
			m_disbandGameButton.controlIsEnabled = localAdmin;
		}
		SetGameType(gameType);
	}

	public void SetPlaybackData(int frame, int totalFrames, int turn, double turnTimeLeft)
	{
		float num = Time.fixedDeltaTime * (float)frame;
		float i = ((totalFrames <= 0) ? 0f : ((float)frame / (float)totalFrames));
		if (m_timeline != null)
		{
			GuiUtils.SetAnimationSetProgress(m_timeline, i);
		}
		string text = ((!(turnTimeLeft > 0.0)) ? string.Empty : Localize.instance.Translate(Utils.FormatTimeLeftString(turnTimeLeft)));
		if (m_currentTurn != null)
		{
			if (text.Length > 0)
			{
				m_currentTurn.Text = Localize.instance.Translate("$hud_turn") + " " + turn + "  (" + text + ")";
			}
			else
			{
				m_currentTurn.Text = Localize.instance.Translate("$hud_turn") + " " + turn;
			}
		}
		m_turnTimeRemainingLabel.Text = text;
		m_turn = turn;
	}

	private void SetStatusText(Mode mode)
	{
		for (int i = 0; i < m_statusTexts.Count; i++)
		{
			if (i == (int)mode)
			{
				m_statusTexts[(Mode)i].BringIn();
			}
			else
			{
				m_statusTexts[(Mode)i].Dismiss();
			}
		}
	}

	private void HideAllStatusText()
	{
		for (int i = 0; i < m_statusTexts.Count; i++)
		{
			m_statusTexts[(Mode)i].Dismiss();
		}
	}

	public void SetTargetScore(int targetScore)
	{
		if (m_battlebar != null)
		{
			m_battlebar.SetTargetScore(targetScore);
		}
	}

	private void IngameMenu_Surrender()
	{
		PLog.Log("::: Hud::IngameMenu_Surrender() Called via delegate :::");
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$label_dialog_surrender_message", OnSurrenderConfirm, OnMsgBoxCancel);
	}

	private void OnSurrenderConfirm()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_onSurrender();
	}

	private void IngameMenu_Options()
	{
		PLog.Log("::: Hud::IngameMenu_Options() Called via delegate :::");
		OptionsWindow optionsWindow = new OptionsWindow(m_guiCamera, inGame: true);
	}

	private void IngameMenu_SwitchGame()
	{
		PLog.Log("::: Hud::IngameMenu_SwitchGame() Called via delegate :::");
	}

	private void IngameMenu_BackToMenu()
	{
		PLog.Log("::: Hud::IngameMenu_BackToMenu() Called via delegate :::");
		if (m_onExit != null)
		{
			m_onExit(ExitState.Normal, 0);
		}
	}

	private void IngameMenu_QuitGame()
	{
		if (m_onQuitGame != null)
		{
			m_onQuitGame();
		}
	}

	private void IngameMenu_LeaveGame()
	{
		m_rpc.Invoke("KickSelf");
		if (m_onExit != null)
		{
			m_onExit(ExitState.Normal, 0);
		}
	}

	public void Update(Camera camera)
	{
		if (m_showFps)
		{
			string text = "FPS: " + (int)(1f / Time.smoothDeltaTime) + "\n";
			text = text + "FrameTime: " + (Time.smoothDeltaTime * 1000f).ToString("F1") + " \n";
			string text2 = text;
			text = text2 + "Total sent: " + m_rpc.GetTotalSentData() / 1000 + " kB\n";
			text2 = text;
			text = text2 + "Total recv: " + m_rpc.GetTotalRecvData() / 1000 + " kB\n";
			text2 = text;
			text = text2 + "Sent/s: " + m_rpc.GetSentDataPerSec() + " B/s\n";
			text2 = text;
			text = text2 + "Recv/s: " + m_rpc.GetRecvDataPerSec() + " B/s\n";
			m_FPS_label.Text = text;
		}
		UIManager component = m_guiCamera.GetComponent<UIManager>();
		bool flag = component.FocusObject != null;
		if (m_visible)
		{
			if (m_mode == Mode.Planning && Input.GetKeyDown(KeyCode.Space) && !flag)
			{
				m_onCommit();
			}
			if (m_mode == Mode.Outcome && Input.GetKeyDown(KeyCode.Space) && !flag)
			{
				TogglePaus();
			}
			ShowFps(CheatMan.instance.m_showFps);
			if (Input.GetKeyDown(KeyCode.Return) && m_onlineGame && !m_chat.m_globalChatPanel.gameObject.active && !m_chat.m_teamChatPanel.gameObject.active)
			{
				m_chat.m_panMan.BringIn(m_chat.m_globalChatPanel);
				SetBattleBarButtonState("Button_GlobalChat", 0);
				UIManager.instance.FocusObject = m_chat.m_globalTextInput;
			}
			if (m_chat.m_globalChatPanel.gameObject.active && m_battlebar != null)
			{
				m_battlebar.SetGlobalChatGlow(enabled: false);
			}
			if (m_chat.m_teamChatPanel.gameObject.active && m_battlebar != null)
			{
				m_battlebar.SetTeamChatGlow(enabled: false);
			}
		}
		m_shipTagMan.Update(camera, Time.deltaTime);
		m_missionLog.Update(camera, Time.deltaTime);
		m_messageLog.Update();
	}

	public void FixedUpdate()
	{
	}

	public void UpdatePlayerStates(List<ClientPlayer> players, int localPlayerID, bool inPlanning, bool inReplayMode, bool isAdmin)
	{
		m_localPlayerID = localPlayerID;
		if (m_battlebar != null)
		{
			m_battlebar.Update(players, localPlayerID);
		}
		if (m_ingameMenu == null)
		{
			return;
		}
		foreach (ClientPlayer player in players)
		{
			if (player.m_id == localPlayerID)
			{
				m_ingameMenu.SetSurrenderStatus(player.m_surrender, inPlanning, inReplayMode, isAdmin);
			}
		}
	}

	private void OnCommitPressed(IUIObject obj)
	{
		if (m_onCommit != null)
		{
			m_onCommit();
		}
	}

	private void OnStartOutcomePressed(IUIObject obj)
	{
		m_restartOutcomeButton.Hide(tf: true);
		m_pauseOutcomeButton.Hide(tf: false);
		m_startOutcomeButton.Hide(tf: true);
		m_onPlayPause();
	}

	private void OnRestartOutcomePressed(IUIObject obj)
	{
		m_restartOutcomeButton.Hide(tf: true);
		m_pauseOutcomeButton.Hide(tf: false);
		m_startOutcomeButton.Hide(tf: true);
		m_onPlayPause();
	}

	private void OnNextTurnPressed(IUIObject obj)
	{
		m_onNextTurn();
	}

	private void OnPrevTurnPressed(IUIObject obj)
	{
		m_onPrevTurn();
	}

	private void OnStopOutcomePressed(IUIObject obj)
	{
		if (m_onStopOutcome != null)
		{
			m_onStopOutcome();
		}
	}

	private void OnPauseOutcomePressed(IUIObject obj)
	{
		m_pauseOutcomeButton.Hide(tf: true);
		m_startOutcomeButton.Hide(tf: false);
		m_restartOutcomeButton.Hide(tf: true);
		m_onPlayPause();
	}

	private void OnHideGui(IUIObject obj)
	{
		SetVisible(!m_visible, hideAll: false, affectBattleBar: true);
	}

	private void TogglePaus()
	{
		bool flag = m_pauseOutcomeButton.IsHidden();
		m_pauseOutcomeButton.Hide(!flag);
		m_startOutcomeButton.Hide(flag);
		m_restartOutcomeButton.Hide(!flag);
		m_onPlayPause();
	}

	private void OnDisbandGame(IUIObject button)
	{
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$hud_confirmdisband", OnDisbandYes, OnMsgBoxCancel);
	}

	private void OnKickPlayer(IUIObject button)
	{
		m_tempKickUserName = "qwe";
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$hud_confirmkick " + m_tempKickUserName, OnKickPlayerYes, OnMsgBoxCancel);
	}

	private void OnKickPlayerYes()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_rpc.Invoke("KickPlayer", m_tempKickUserName);
	}

	private void OnDisbandYes()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_rpc.Invoke("DisbandGame");
	}

	private void OnMsgBoxCancel()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
	}
}
