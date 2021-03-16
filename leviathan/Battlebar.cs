#define DEBUG
using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class Battlebar
{
	private class PlayerItem
	{
		public GameObject m_gui;

		public ClientPlayer m_clientPlayer;

		public SimpleSprite m_flag;

		public SpriteText m_name;

		public SpriteText m_phase;

		public SimpleSprite m_statusOnline;

		public SimpleSprite m_statusOffline;

		public SimpleSprite m_statusPresent;

		public UIStateToggleBtn[] m_flagshipStatus = new UIStateToggleBtn[2];

		public void SetName(string name)
		{
			m_name.Text = name;
		}

		public void SetFlag(int flag)
		{
			Texture2D flagTexture = GuiUtils.GetFlagTexture(flag);
			GuiUtils.SetImage(m_flag, flagTexture);
		}

		public void SetStatus(PlayerPresenceStatus status, int localPlayerID)
		{
			m_statusOnline.Hide(status != PlayerPresenceStatus.Online || m_clientPlayer.m_id == localPlayerID);
			m_statusPresent.Hide(status != PlayerPresenceStatus.InGame || m_clientPlayer.m_id == localPlayerID);
			m_statusOffline.Hide(status != 0 || m_clientPlayer.m_id == localPlayerID);
		}

		public void SetPhase(PlayerTurnStatus status)
		{
			if (ClientGame.instance.IsReplayMode())
			{
				m_phase.Text = string.Empty;
				return;
			}
			switch (status)
			{
			case PlayerTurnStatus.Dead:
				m_phase.Text = Localize.instance.TranslateKey("hud_battlebar_dead");
				break;
			case PlayerTurnStatus.Done:
				m_phase.Text = Localize.instance.TranslateKey("hud_battlebar_commited");
				break;
			case PlayerTurnStatus.Planning:
				m_phase.Text = Localize.instance.TranslateKey("hud_battlebar_planning");
				break;
			}
		}
	}

	private TurnMan m_turnMan;

	private GameType m_gameType;

	private GameObject m_guiRoot;

	private GameObject m_guiCamera;

	private bool m_visible;

	private UIScrollList m_team1List;

	private UIScrollList m_team2List;

	private SpriteText m_targetScore;

	private SpriteText m_team1Score;

	private SpriteText m_team2Score;

	private UIStateToggleBtn[] m_team1Flagship = new UIStateToggleBtn[2];

	private UIStateToggleBtn[] m_team2Flagship = new UIStateToggleBtn[2];

	private List<PlayerItem> m_players = new List<PlayerItem>();

	private PackedSprite m_globalChatGlow;

	private PackedSprite m_teamChatGlow;

	public Battlebar(GameType gameType, GameObject guiRoot, GameObject guiCamera, TurnMan turnMan)
	{
		m_gameType = gameType;
		m_turnMan = turnMan;
		m_guiRoot = guiRoot;
		m_guiCamera = guiCamera;
		m_targetScore = GuiUtils.FindChildOfComponent<SpriteText>(guiRoot, "TargetScore");
		m_team1Score = GuiUtils.FindChildOfComponent<SpriteText>(guiRoot, "ScoreLabel_Team1");
		m_team2Score = GuiUtils.FindChildOfComponent<SpriteText>(guiRoot, "ScoreLabel_Team2");
		m_team1List = GuiUtils.FindChildOfComponent<UIScrollList>(guiRoot, "Team1_List");
		m_team2List = GuiUtils.FindChildOfComponent<UIScrollList>(guiRoot, "Team2_List");
		m_team1Flagship[0] = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(guiRoot, "Team1_Flagship1");
		m_team1Flagship[1] = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(guiRoot, "Team1_Flagship2");
		m_team2Flagship[0] = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(guiRoot, "Team2_Flagship1");
		m_team2Flagship[1] = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(guiRoot, "Team2_Flagship2");
		GameObject gameObject = GuiUtils.FindChildOf(m_guiRoot, "Button_GlobalChat");
		if (gameObject != null)
		{
			m_globalChatGlow = GuiUtils.FindChildOfComponent<PackedSprite>(gameObject, "Button_Glow");
		}
		GameObject gameObject2 = GuiUtils.FindChildOf(m_guiRoot, "Button_TeamChat");
		if (gameObject2 != null)
		{
			m_teamChatGlow = GuiUtils.FindChildOfComponent<PackedSprite>(gameObject2, "Button_Glow");
		}
		if ((bool)m_team1Flagship[0])
		{
			m_team1Flagship[0].Hide(tf: true);
		}
		if ((bool)m_team1Flagship[1])
		{
			m_team1Flagship[1].Hide(tf: true);
		}
		if ((bool)m_team2Flagship[0])
		{
			m_team2Flagship[0].Hide(tf: true);
		}
		if ((bool)m_team2Flagship[1])
		{
			m_team2Flagship[1].Hide(tf: true);
		}
		m_players.Add(null);
		m_players.Add(null);
		m_players.Add(null);
		m_players.Add(null);
		SetGlobalChatGlow(enabled: false);
		SetTeamChatGlow(enabled: false);
		SetVisible(visible: true);
	}

	public void Close()
	{
	}

	public void SetVisible(bool visible)
	{
		m_guiRoot.SetActiveRecursively(visible);
		m_visible = visible;
	}

	public void Update(List<ClientPlayer> players, int localPlayerID)
	{
		if (m_gameType == GameType.Points)
		{
			m_team1Score.Text = m_turnMan.GetTeamScore(0).ToString();
			m_team2Score.Text = m_turnMan.GetTeamScore(1).ToString();
		}
		foreach (ClientPlayer player in players)
		{
			PlayerItem createPlayerItem = GetCreatePlayerItem(player);
			createPlayerItem.SetStatus(player.m_status, localPlayerID);
			createPlayerItem.SetPhase(player.m_turnStatus);
			if (m_gameType == GameType.Assassination)
			{
				if (createPlayerItem.m_flagshipStatus[0] != null)
				{
					createPlayerItem.m_flagshipStatus[0].SetToggleState((m_turnMan.GetFlagshipKiller(player.m_id, 0) >= 0) ? 1 : 0);
				}
				if (createPlayerItem.m_flagshipStatus[1] != null)
				{
					createPlayerItem.m_flagshipStatus[1].SetToggleState((m_turnMan.GetFlagshipKiller(player.m_id, 1) >= 0) ? 1 : 0);
				}
			}
		}
	}

	public void SetTargetScore(int targetScore)
	{
		if (m_targetScore != null)
		{
			m_targetScore.Text = targetScore.ToString();
		}
	}

	public void SetGlobalChatGlow(bool enabled)
	{
		if (m_globalChatGlow != null)
		{
			if (enabled)
			{
				m_globalChatGlow.PlayAnim(0);
			}
			else
			{
				m_globalChatGlow.StopAnim();
			}
		}
	}

	public void SetTeamChatGlow(bool enabled)
	{
		if (m_teamChatGlow != null)
		{
			if (enabled)
			{
				m_teamChatGlow.PlayAnim(0);
			}
			else
			{
				m_teamChatGlow.StopAnim();
			}
		}
	}

	private PlayerItem GetCreatePlayerItem(ClientPlayer player)
	{
		if (m_players[player.m_id] != null)
		{
			return m_players[player.m_id];
		}
		PlayerItem playerItem = new PlayerItem();
		playerItem.m_clientPlayer = player;
		if (m_gameType == GameType.Assassination || m_gameType == GameType.Points)
		{
			int playerTeam = m_turnMan.GetPlayerTeam(player.m_id);
			bool flag = m_gameType == GameType.Assassination && m_turnMan.GetNrOfPlayers() == 3 && m_turnMan.GetTeamSize(playerTeam) == 1;
			if (playerTeam == 0)
			{
				playerItem.m_gui = GuiUtils.CreateGui("IngameGui/Battlebar/Battlebar_Team1_Listitem", m_guiCamera);
				DebugUtils.Assert(playerItem.m_gui != null);
				if (m_gameType == GameType.Assassination)
				{
					playerItem.m_flagshipStatus[0] = m_team1Flagship[m_team1List.Count];
					playerItem.m_flagshipStatus[0].Hide(tf: false);
					if (flag)
					{
						playerItem.m_flagshipStatus[1] = m_team1Flagship[m_team1List.Count + 1];
						playerItem.m_flagshipStatus[1].Hide(tf: false);
					}
				}
				m_team1List.AddItem(playerItem.m_gui.GetComponent<UIListItemContainer>());
			}
			else
			{
				playerItem.m_gui = GuiUtils.CreateGui("IngameGui/Battlebar/Battlebar_Team2_Listitem", m_guiCamera);
				DebugUtils.Assert(playerItem.m_gui != null);
				if (m_gameType == GameType.Assassination)
				{
					playerItem.m_flagshipStatus[0] = m_team2Flagship[m_team2List.Count];
					playerItem.m_flagshipStatus[0].Hide(tf: false);
					if (flag)
					{
						playerItem.m_flagshipStatus[1] = m_team2Flagship[m_team2List.Count + 1];
						playerItem.m_flagshipStatus[1].Hide(tf: false);
					}
				}
				m_team2List.AddItem(playerItem.m_gui.GetComponent<UIListItemContainer>());
			}
		}
		else
		{
			playerItem.m_gui = GuiUtils.CreateGui("IngameGui/Battlebar/Battlebar_Player" + (player.m_id + 1) + "_Listitem", m_guiCamera);
			DebugUtils.Assert(playerItem.m_gui != null);
			m_team1List.AddItem(playerItem.m_gui.GetComponent<UIListItemContainer>());
		}
		playerItem.m_flag = GuiUtils.FindChildOfComponent<SimpleSprite>(playerItem.m_gui, "PlayerFlag");
		playerItem.m_name = GuiUtils.FindChildOfComponent<SpriteText>(playerItem.m_gui, "PlayerNameLabel");
		playerItem.m_phase = GuiUtils.FindChildOfComponent<SpriteText>(playerItem.m_gui, "PlayerPhaseLabel");
		playerItem.m_statusOnline = GuiUtils.FindChildOfComponent<SimpleSprite>(playerItem.m_gui, "PlayerStatus_Online");
		playerItem.m_statusOffline = GuiUtils.FindChildOfComponent<SimpleSprite>(playerItem.m_gui, "PlayerStatus_Offline");
		playerItem.m_statusPresent = GuiUtils.FindChildOfComponent<SimpleSprite>(playerItem.m_gui, "PlayerStatus_Present");
		m_players[player.m_id] = playerItem;
		playerItem.SetName(m_turnMan.GetPlayerName(player.m_id));
		playerItem.SetFlag(m_turnMan.GetPlayerFlag(player.m_id));
		playerItem.m_gui.SetActiveRecursively(m_visible);
		return playerItem;
	}
}
