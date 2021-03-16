using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class EndGameMenu
{
	private class Chat
	{
		public UITextField m_textInput;

		public UIScrollList m_messageList;

		public GameObject m_listItem;
	}

	public Action<int> m_onLeavePressed;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private PTech.RPC m_rpc;

	private ChatClient m_chatClient;

	private EndGameData m_endgameData;

	private GameSettings m_gameSettings;

	private MsgBox m_msgBox;

	private GameObject m_saveReplayProgress;

	private GameObject m_saveDialog;

	private UIButton m_saveReplayButton;

	private string m_addFriendTempName = string.Empty;

	private Chat m_chat = new Chat();

	public EndGameMenu(GameObject guiCamera, EndGameData endgameData, GameSettings gameSettings, PTech.RPC rpc, ChatClient chatClient, MusicManager musMan)
	{
		m_guiCamera = guiCamera;
		m_endgameData = endgameData;
		m_gameSettings = gameSettings;
		m_rpc = rpc;
		m_chatClient = chatClient;
		switch (gameSettings.m_gameType)
		{
		case GameType.Challenge:
		case GameType.Campaign:
			m_gui = GuiUtils.CreateGui("EndGame/EndGameWindow_CampaignChallenge", guiCamera);
			SetupCampaignPlayerList();
			break;
		case GameType.Points:
			m_gui = GuiUtils.CreateGui("EndGame/EndGameWindow_Points", guiCamera);
			SetupPointsPlayerList();
			break;
		case GameType.Assassination:
			m_gui = GuiUtils.CreateGui("EndGame/EndGameWindow_Assassin", guiCamera);
			SetupAssasinatePlayerList();
			break;
		}
		SetupAccolades();
		GuiUtils.FindChildOf(m_gui, "ExitButton").GetComponent<UIButton>().SetValueChangedDelegate(OnLeavePressed);
		m_saveReplayButton = GuiUtils.FindChildOf(m_gui, "SaveReplayButton").GetComponent<UIButton>();
		m_saveReplayButton.SetValueChangedDelegate(OnSaveReplayPressed);
		SpriteText component = GuiUtils.FindChildOf(m_gui, "EndGameTitleLabel").GetComponent<SpriteText>();
		bool flag = false;
		bool flag2 = false;
		if (gameSettings.m_gameType == GameType.Campaign || gameSettings.m_gameType == GameType.Challenge)
		{
			flag = ((endgameData.m_outcome == GameOutcome.Victory) ? true : false);
		}
		else
		{
			int team = endgameData.m_players[endgameData.m_localPlayerID].m_team;
			flag = endgameData.m_winnerTeam == team;
			flag2 = endgameData.m_winnerTeam == -1;
		}
		if (flag)
		{
			component.Text = Localize.instance.Translate("$Victory");
			musMan.SetMusic("victory");
		}
		else if (flag2)
		{
			component.Text = Localize.instance.Translate("$Draw");
			musMan.SetMusic("victory");
		}
		else
		{
			component.Text = Localize.instance.Translate("$Defeat");
			musMan.SetMusic("defeat");
		}
		SetupChat();
		m_rpc.Register("FriendRequestReply", RPC_FriendRequestReply);
	}

	private void SetupAccolades()
	{
		GuiUtils.FindChildOf(m_gui, "DestroyerValueLabel").GetComponent<SpriteText>().Text = m_endgameData.m_AccoladeDestroy;
		GuiUtils.FindChildOf(m_gui, "MostlyHarmlessValueLabel").GetComponent<SpriteText>().Text = m_endgameData.m_AccoladeHarmless;
		GuiUtils.FindChildOf(m_gui, "BestUseOfShieldsValueLabel").GetComponent<SpriteText>().Text = m_endgameData.m_AccoladeShields;
		GuiUtils.FindChildOf(m_gui, "LongDistanceMarinerValueLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate("$label_notapplicable");
		GuiUtils.FindChildOf(m_gui, "SlowpokeValueLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate("$label_notapplicable");
	}

	private void SetupCampaignPlayerList()
	{
		UIScrollList component = GuiUtils.FindChildOf(m_gui, "TeamScrollList").GetComponent<UIScrollList>();
		foreach (EndGame_PlayerStatistics player in m_endgameData.m_players)
		{
			GameObject gameObject = GuiUtils.CreateGui("EndGame/TeamListItem_CampaignChallenge", m_guiCamera);
			GuiUtils.FindChildOf(gameObject, "PlayerNameLabel").GetComponent<SpriteText>().Text = player.m_name;
			GuiUtils.FindChildOf(gameObject, "PlayerShipsSunkLabel").GetComponent<SpriteText>().Text = player.m_shipsSunk.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsLostLabel").GetComponent<SpriteText>().Text = player.m_shipsLost.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsDamagedLabel").GetComponent<SpriteText>().Text = player.m_shipsDamaged.ToString();
			UIButton component2 = GuiUtils.FindChildOf(gameObject, "PlayerAddFriendButton").GetComponent<UIButton>();
			if (player.m_playerID == m_endgameData.m_localPlayerID)
			{
				component2.transform.Translate(new Vector3(10000f, 0f, 0f));
			}
			else
			{
				component2.SetValueChangedDelegate(OnAddFriend);
			}
			Texture2D flagTexture = GuiUtils.GetFlagTexture(player.m_flag);
			SimpleSprite component3 = GuiUtils.FindChildOf(gameObject, "PlayerFlag").GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component3, flagTexture);
			component.AddItem(gameObject.GetComponent<UIListItemContainer>());
		}
		component.ScrollToItem(0, 0f);
	}

	private void SetupPointsPlayerList()
	{
		bool flag = m_endgameData.m_winnerTeam == -1;
		SpriteText component = GuiUtils.FindChildOf(m_gui, "LosingTeamTotalScoreLabel").GetComponent<SpriteText>();
		SpriteText component2 = GuiUtils.FindChildOf(m_gui, "WinningTeamTotalScoreLabel").GetComponent<SpriteText>();
		if (flag)
		{
			SpriteText component3 = GuiUtils.FindChildOf(m_gui, "WinningTeamTitleLabel").GetComponent<SpriteText>();
			SpriteText component4 = GuiUtils.FindChildOf(m_gui, "LosingTeamTitleLabel").GetComponent<SpriteText>();
			component3.Text = string.Empty;
			component4.Text = string.Empty;
		}
		UIScrollList component5 = GuiUtils.FindChildOf(m_gui, "LosingTeamScrollList").GetComponent<UIScrollList>();
		UIScrollList component6 = GuiUtils.FindChildOf(m_gui, "WinningTeamScrollList").GetComponent<UIScrollList>();
		foreach (EndGame_PlayerStatistics player in m_endgameData.m_players)
		{
			GameObject gameObject = GuiUtils.CreateGui("EndGame/TeamListItem_Points", m_guiCamera);
			GuiUtils.FindChildOf(gameObject, "PlayerNameLabel").GetComponent<SpriteText>().Text = player.m_name;
			GuiUtils.FindChildOf(gameObject, "PlayerPointsLabel").GetComponent<SpriteText>().Text = player.m_score.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsSunkLabel").GetComponent<SpriteText>().Text = player.m_shipsSunk.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsLostLabel").GetComponent<SpriteText>().Text = player.m_shipsLost.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsDamagedLabel").GetComponent<SpriteText>().Text = player.m_shipsDamaged.ToString();
			UIButton component7 = GuiUtils.FindChildOf(gameObject, "PlayerAddFriendButton").GetComponent<UIButton>();
			if (player.m_playerID == m_endgameData.m_localPlayerID)
			{
				component7.transform.Translate(new Vector3(10000f, 0f, 0f));
			}
			else
			{
				component7.SetValueChangedDelegate(OnAddFriend);
			}
			Texture2D flagTexture = GuiUtils.GetFlagTexture(player.m_flag);
			SimpleSprite component8 = GuiUtils.FindChildOf(gameObject, "PlayerFlag").GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component8, flagTexture);
			if (player.m_team == m_endgameData.m_winnerTeam || (flag && player.m_team == 0))
			{
				component6.AddItem(gameObject.GetComponent<UIListItemContainer>());
				component2.Text = player.m_teamScore + " " + Localize.instance.TranslateKey("label_pointssmall");
			}
			else
			{
				component5.AddItem(gameObject.GetComponent<UIListItemContainer>());
				component.Text = player.m_teamScore + " " + Localize.instance.TranslateKey("label_pointssmall");
			}
		}
		component5.ScrollToItem(0, 0f);
		component6.ScrollToItem(0, 0f);
	}

	private void SetupAssasinatePlayerList()
	{
		bool flag = m_endgameData.m_winnerTeam == -1;
		UIScrollList component = GuiUtils.FindChildOf(m_gui, "LosingTeamScrollList").GetComponent<UIScrollList>();
		UIScrollList component2 = GuiUtils.FindChildOf(m_gui, "WinningTeamScrollList").GetComponent<UIScrollList>();
		if (flag)
		{
			SpriteText component3 = GuiUtils.FindChildOf(m_gui, "WinningTeamTitleLabel").GetComponent<SpriteText>();
			SpriteText component4 = GuiUtils.FindChildOf(m_gui, "LosingTeamTitleLabel").GetComponent<SpriteText>();
			component3.Text = string.Empty;
			component4.Text = string.Empty;
		}
		foreach (EndGame_PlayerStatistics player in m_endgameData.m_players)
		{
			GameObject gameObject = GuiUtils.CreateGui("EndGame/TeamListItem_Assassin", m_guiCamera);
			GuiUtils.FindChildOf(gameObject, "PlayerNameLabel").GetComponent<SpriteText>().Text = player.m_name;
			GuiUtils.FindChildOf(gameObject, "PlayerShipsSunkLabel").GetComponent<SpriteText>().Text = player.m_shipsSunk.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsLostLabel").GetComponent<SpriteText>().Text = player.m_shipsLost.ToString();
			GuiUtils.FindChildOf(gameObject, "PlayerShipsDamagedLabel").GetComponent<SpriteText>().Text = player.m_shipsDamaged.ToString();
			string text = CreateAssasinatedList(player.m_playerID);
			GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "PlayerFlagshipsSunkLabel").Text = text;
			UIButton component5 = GuiUtils.FindChildOf(gameObject, "PlayerAddFriendButton").GetComponent<UIButton>();
			if (player.m_playerID == m_endgameData.m_localPlayerID)
			{
				component5.transform.Translate(new Vector3(10000f, 0f, 0f));
			}
			else
			{
				component5.SetValueChangedDelegate(OnAddFriend);
			}
			Texture2D flagTexture = GuiUtils.GetFlagTexture(player.m_flag);
			SimpleSprite component6 = GuiUtils.FindChildOf(gameObject, "PlayerFlag").GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component6, flagTexture);
			if (player.m_team == m_endgameData.m_winnerTeam || (flag && player.m_team == 0))
			{
				component2.AddItem(gameObject.GetComponent<UIListItemContainer>());
			}
			else
			{
				component.AddItem(gameObject.GetComponent<UIListItemContainer>());
			}
		}
		component.ScrollToItem(0, 0f);
		component2.ScrollToItem(0, 0f);
	}

	private string CreateAssasinatedList(int killer)
	{
		string text = string.Empty;
		foreach (EndGame_PlayerStatistics player in m_endgameData.m_players)
		{
			if (player.m_flagshipKiller0 == killer)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += player.m_name;
			}
			if (player.m_flagshipKiller1 == killer)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text = text + player.m_name + "#2";
			}
		}
		return text;
	}

	public void Close()
	{
		if (m_gui != null)
		{
			UnityEngine.Object.Destroy(m_gui);
			m_gui = null;
		}
		if (m_saveReplayProgress != null)
		{
			UnityEngine.Object.Destroy(m_saveReplayProgress);
		}
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_rpc.Unregister("FriendRequestReply");
	}

	private void OnAddFriend(IUIObject button)
	{
		SpriteText component = GuiUtils.FindChildOf(button.transform.parent, "PlayerNameLabel").GetComponent<SpriteText>();
		m_addFriendTempName = component.Text;
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "$endgame_addfriend " + m_addFriendTempName, OnAddFriendYes, OnAddFriendNo);
	}

	public void Update()
	{
		if (Utils.AndroidBack())
		{
			OnLeavePressed(null);
		}
	}

	private void OnAddFriendYes()
	{
		m_rpc.Invoke("FriendRequest", m_addFriendTempName);
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void OnAddFriendNo()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void RPC_FriendRequestReply(PTech.RPC rpc, List<object> args)
	{
		ErrorCode errorCode = (ErrorCode)(int)args[0];
		string text = ((errorCode != ErrorCode.FriendUserDoesNotExist) ? "$already_friends" : "$user_does_not_exist");
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, text, null);
	}

	private void OnLeavePressed(IUIObject obj)
	{
		PLog.Log("EndGameMenu:OnLeavePressed()");
		m_rpc.Invoke("SeenEndGame");
		if (m_onLeavePressed != null)
		{
			m_onLeavePressed(m_endgameData.m_autoJoinGameID);
		}
	}

	private void OnSaveReplayPressed(IUIObject obj)
	{
		if (m_saveDialog == null)
		{
			m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$label_savereplayas"), m_gameSettings.m_gameName, OnSaveReplayCancel, OnSaveReplayOk);
		}
	}

	private void OnSaveReplayCancel()
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void OnSaveReplayOk(string text)
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		m_saveReplayProgress = GuiUtils.CreateGui("dialogs/Dialog_Progress", m_guiCamera);
		m_rpc.Register("SaveReplayReply", RPC_SaveReplayReply);
		m_rpc.Invoke("SaveReplay", text);
	}

	private void RPC_SaveReplayReply(PTech.RPC rpc, List<object> args)
	{
		rpc.Unregister("SaveReplayReply");
		UnityEngine.Object.Destroy(m_saveReplayProgress);
		m_saveReplayProgress = null;
		if (!(bool)args[0])
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_replayexists", OnMsgBoxOK);
		}
		else
		{
			m_saveReplayButton.controlIsEnabled = false;
		}
	}

	private void SetupChat()
	{
		GameObject go = GuiUtils.FindChildOf(m_gui, "GlobalChatContainer");
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

	private void OnMsgBoxOK()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}
}
