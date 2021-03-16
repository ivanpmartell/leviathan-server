#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class CreateGame
{
	public delegate void CreateDelegate(GameType gametype, string campaign, string map, int players, int fleetSize, float targetScore, double turnTime, bool autoJoin);

	public Action m_onClose;

	public CreateDelegate m_onCreateGame;

	private GameObject m_createGameView;

	private MapMan m_mapMan;

	private UserManClient m_userMan;

	private GameObject m_guiCamera;

	private GameObject m_createSkirmishTab;

	private GameObject m_createCampaignTab;

	private GameObject m_createChallengeTab;

	private GameObject m_createAssTab;

	private UIButton m_skirmishTabButton;

	private UIButton m_campaignTabButton;

	private UIButton m_challengeTabButton;

	private UIButton m_assTabButton;

	private SpriteText m_mapSizeLabel;

	private SpriteText m_mapDescriptionLabel;

	private SpriteText m_mapNameLabel;

	private SimpleSprite m_mapImage;

	private bool m_hasCreated;

	private int m_selectedCampaignID = -1;

	private int m_targetScore = 60;

	private List<CampaignInfo> m_availableCampaigns = new List<CampaignInfo>();

	private List<string> m_mapList = new List<string>();

	private List<KeyValuePair<UIRadioBtn, double>> m_turnTimeButtons = new List<KeyValuePair<UIRadioBtn, double>>();

	private MsgBox m_msgBox;

	public CreateGame(GameObject guiCamera, GameObject createGameView, MapMan mapman, UserManClient userMan)
	{
		m_guiCamera = guiCamera;
		m_mapMan = mapman;
		m_userMan = userMan;
		m_createGameView = createGameView;
		GuiUtils.FindChildOf(m_createGameView, "CancelButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCreateGameCancel);
		m_mapDescriptionLabel = GuiUtils.FindChildOf(m_createGameView, "MapDescLbl").GetComponent<SpriteText>();
		m_mapSizeLabel = GuiUtils.FindChildOf(m_createGameView, "MapSizeLbl").GetComponent<SpriteText>();
		m_mapNameLabel = GuiUtils.FindChildOf(m_createGameView, "MapNameLbl").GetComponent<SpriteText>();
		m_mapImage = GuiUtils.FindChildOf(m_createGameView, "MapImg").GetComponent<SimpleSprite>();
		m_skirmishTabButton = GuiUtils.FindChildOf(m_createGameView, "SkirmishTabButton").GetComponent<UIButton>();
		m_campaignTabButton = GuiUtils.FindChildOf(m_createGameView, "CampaignTabButton").GetComponent<UIButton>();
		m_challengeTabButton = GuiUtils.FindChildOf(m_createGameView, "ChallengeTabButton").GetComponent<UIButton>();
		m_assTabButton = GuiUtils.FindChildOf(m_createGameView, "AssTabButton").GetComponent<UIButton>();
		m_skirmishTabButton.SetValueChangedDelegate(OnCreateSkirmishTab);
		m_campaignTabButton.SetValueChangedDelegate(OnCreateCampaignTab);
		m_challengeTabButton.SetValueChangedDelegate(OnCreateChallengeTab);
		m_assTabButton.SetValueChangedDelegate(OnCreateAssTab);
		m_createSkirmishTab = GuiUtils.FindChildOf(m_createGameView, "Settings_Points");
		GuiUtils.FindChildOf(m_createSkirmishTab, "CreateButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCreateSkirmishGame);
		GuiUtils.FindChildOf(m_createSkirmishTab, "MapmenuButton").GetComponent<UIButton>().SetValueChangedDelegate(OnShowSkirmishMapMenu);
		GuiUtils.FindChildOf(m_createSkirmishTab, "ScrollList").GetComponent<UIScrollList>().SetValueChangedDelegate(OnSkirmishMapSelected);
		GuiUtils.FindChildOf(m_createSkirmishTab, "PlusButton").GetComponent<UIButton>().SetValueChangedDelegate(OnPlusTargetScore);
		GuiUtils.FindChildOf(m_createSkirmishTab, "MinusButton").GetComponent<UIButton>().SetValueChangedDelegate(OnMinusTargetScore);
		m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishFleetSizeChange);
		m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishFleetSizeChange);
		m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishFleetSizeChange);
		m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishPlayersChange);
		m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishPlayersChange);
		m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnSkirmishPlayersChange);
		m_createCampaignTab = GuiUtils.FindChildOf(m_createGameView, "Settings_Campaign");
		GuiUtils.FindChildOf(m_createCampaignTab, "CreateButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCreateCampaignGame);
		GuiUtils.FindChildOf(m_createCampaignTab, "MapmenuButton").GetComponent<UIButton>().SetValueChangedDelegate(OnShowCampaignMapMenu);
		GuiUtils.FindChildOf(m_createCampaignTab, "ArrowLeftButton").GetComponent<UIButton>().SetValueChangedDelegate(OnPrevCampaign);
		GuiUtils.FindChildOf(m_createCampaignTab, "ArrowRightButton").GetComponent<UIButton>().SetValueChangedDelegate(OnNextCampaign);
		GuiUtils.FindChildOf(m_createCampaignTab, "ScrollList").GetComponent<UIScrollList>().SetValueChangedDelegate(OnCampaignMapSelected);
		m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/One").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnCampaignPlayersChange);
		m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnCampaignPlayersChange);
		m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnCampaignPlayersChange);
		m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnCampaignPlayersChange);
		m_createChallengeTab = GuiUtils.FindChildOf(m_createGameView, "Settings_Challenge");
		GuiUtils.FindChildOf(m_createChallengeTab, "CreateButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCreateChallengeGame);
		GuiUtils.FindChildOf(m_createChallengeTab, "MapmenuButton").GetComponent<UIButton>().SetValueChangedDelegate(OnShowChallengeMapMenu);
		GuiUtils.FindChildOf(m_createChallengeTab, "ScrollList").GetComponent<UIScrollList>().SetValueChangedDelegate(OnChallengeMapSelected);
		m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/One").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnChallengePlayersChange);
		m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnChallengePlayersChange);
		m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnChallengePlayersChange);
		m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnChallengePlayersChange);
		m_createAssTab = GuiUtils.FindChildOf(m_createGameView, "Settings_Ass");
		GuiUtils.FindChildOf(m_createAssTab, "CreateButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCreateAssassinateGame);
		GuiUtils.FindChildOf(m_createAssTab, "MapmenuButton").GetComponent<UIButton>().SetValueChangedDelegate(OnShowAssassinationMapMenu);
		GuiUtils.FindChildOf(m_createAssTab, "ScrollList").GetComponent<UIScrollList>().SetValueChangedDelegate(OnAssassinationMapSelected);
		m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnAssassinatePlayersChange);
		m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnAssassinatePlayersChange);
		m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().SetValueChangedDelegate(OnAssassinatePlayersChange);
		m_createGameView.SetActiveRecursively(state: false);
	}

	public void Show()
	{
		m_hasCreated = false;
		m_createGameView.SetActiveRecursively(state: true);
		ShowCreateCampaignGame();
	}

	public void Hide()
	{
		m_createGameView.SetActiveRecursively(state: false);
	}

	private void OnShowSkirmishMapMenu(IUIObject obj)
	{
		PLog.Log("show mapmenu");
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createSkirmishTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (component.IsShowing)
		{
			component.Dismiss();
		}
		else
		{
			component.BringIn();
		}
	}

	private void OnShowCampaignMapMenu(IUIObject obj)
	{
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createCampaignTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (component.IsShowing)
		{
			component.Dismiss();
		}
		else
		{
			component.BringIn();
		}
	}

	private void OnShowChallengeMapMenu(IUIObject obj)
	{
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createChallengeTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (component.IsShowing)
		{
			component.Dismiss();
		}
		else
		{
			component.BringIn();
		}
	}

	private void OnShowAssassinationMapMenu(IUIObject obj)
	{
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createAssTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (component.IsShowing)
		{
			component.Dismiss();
		}
		else
		{
			component.BringIn();
		}
	}

	private void OnPlusTargetScore(IUIObject obj)
	{
		m_targetScore += 10;
		if (m_targetScore > 100)
		{
			m_targetScore = 100;
		}
		UpdateTargetScoreWidgets();
	}

	private void OnSkirmishFleetSizeChange(IUIObject obj)
	{
		UpdateTargetScoreWidgets();
	}

	private void OnSkirmishPlayersChange(IUIObject obj)
	{
		int num = 1;
		if (obj.name == "Two")
		{
			num = 2;
		}
		if (obj.name == "Three")
		{
			num = 3;
		}
		if (obj.name == "Four")
		{
			num = 4;
		}
		if (num == 3)
		{
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().controlIsEnabled = false;
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().controlIsEnabled = false;
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().Value = true;
			UpdateTargetScoreWidgets();
		}
		else
		{
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().controlIsEnabled = true;
		}
		UpdateTargetScoreWidgets();
		SetupTurnTimers(m_createSkirmishTab, num);
	}

	private void OnCampaignPlayersChange(IUIObject obj)
	{
		SetupTurnTimers(m_createCampaignTab, GetCampaignPlayers());
	}

	private void OnChallengePlayersChange(IUIObject obj)
	{
		SetupTurnTimers(m_createChallengeTab, GetChallengePlayers());
	}

	private void OnAssassinatePlayersChange(IUIObject obj)
	{
		int num = 1;
		if (obj.name == "Two")
		{
			num = 2;
		}
		if (obj.name == "Three")
		{
			num = 3;
		}
		if (obj.name == "Four")
		{
			num = 4;
		}
		if (num == 3)
		{
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().controlIsEnabled = false;
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().controlIsEnabled = false;
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().Value = true;
		}
		else
		{
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().controlIsEnabled = true;
			m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().controlIsEnabled = true;
		}
		SetupTurnTimers(m_createAssTab, num);
	}

	private void OnMinusTargetScore(IUIObject obj)
	{
		int num = 10;
		m_targetScore -= 10;
		if (m_targetScore < num)
		{
			m_targetScore = num;
		}
		UpdateTargetScoreWidgets();
	}

	private void UpdateTargetScoreWidgets()
	{
		int num = 10;
		GuiUtils.FindChildOf(m_createSkirmishTab, "MinusButton").GetComponent<UIButton>().controlIsEnabled = m_targetScore > num;
		GuiUtils.FindChildOf(m_createSkirmishTab, "PlusButton").GetComponent<UIButton>().controlIsEnabled = m_targetScore < 100;
		int num2 = (int)((float)FleetSizes.sizes[(int)GetSkirmishFleetSize()].max * ((float)m_targetScore / 100f));
		int skirmishGamePlayers = GetSkirmishGamePlayers();
		if (skirmishGamePlayers >= 3)
		{
			num2 *= 2;
		}
		GuiUtils.FindChildOf(m_createSkirmishTab.gameObject, "Points").GetComponent<SpriteText>().Text = m_targetScore + "%";
		GuiUtils.FindChildOf(m_createSkirmishTab.gameObject, "PointsConversion").GetComponent<SpriteText>().Text = num2.ToString();
	}

	private void OnSkirmishMapSelected(IUIObject obj)
	{
		UIScrollList uIScrollList = obj as UIScrollList;
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createSkirmishTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (uIScrollList.SelectedItem != null)
		{
			GuiUtils.FindChildOf(m_createSkirmishTab, "MapmenuButton").GetComponent<UIButton>().Text = uIScrollList.SelectedItem.Text;
			MapInfo mapByName = m_mapMan.GetMapByName(GameType.Points, string.Empty, m_mapList[uIScrollList.SelectedItem.Index]);
			SetMap(mapByName);
			m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 2;
			m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 3;
			m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 4;
			m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value = true;
		}
		component.Dismiss();
	}

	private void OnCampaignMapSelected(IUIObject obj)
	{
		UIScrollList uIScrollList = obj as UIScrollList;
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createCampaignTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (uIScrollList.SelectedItem != null)
		{
			GuiUtils.FindChildOf(m_createCampaignTab, "MapmenuButton").GetComponent<UIButton>().Text = uIScrollList.SelectedItem.Text;
			MapInfo mapByName = m_mapMan.GetMapByName(GameType.Campaign, m_availableCampaigns[m_selectedCampaignID].m_name, m_mapList[uIScrollList.SelectedItem.Index]);
			DebugUtils.Assert(mapByName != null);
			SetMap(mapByName);
			m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 2;
			m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 3;
			m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 4;
			m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/One").GetComponent<UIRadioBtn>().Value = true;
		}
		component.Dismiss();
	}

	private void OnChallengeMapSelected(IUIObject obj)
	{
		UIScrollList uIScrollList = obj as UIScrollList;
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createChallengeTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (uIScrollList.SelectedItem != null)
		{
			GuiUtils.FindChildOf(m_createChallengeTab, "MapmenuButton").GetComponent<UIButton>().Text = uIScrollList.SelectedItem.Text;
			MapInfo mapByName = m_mapMan.GetMapByName(GameType.Challenge, string.Empty, m_mapList[uIScrollList.SelectedItem.Index]);
			SetMap(mapByName);
			m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 2;
			m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 3;
			m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 4;
			m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/One").GetComponent<UIRadioBtn>().Value = true;
		}
		component.Dismiss();
	}

	private void OnAssassinationMapSelected(IUIObject obj)
	{
		UIScrollList uIScrollList = obj as UIScrollList;
		UIBistateInteractivePanel component = GuiUtils.FindChildOf(m_createAssTab.gameObject, "MapMenuDropdown").GetComponent<UIBistateInteractivePanel>();
		if (uIScrollList.SelectedItem != null)
		{
			GuiUtils.FindChildOf(m_createAssTab, "MapmenuButton").GetComponent<UIButton>().Text = uIScrollList.SelectedItem.Text;
			MapInfo mapByName = m_mapMan.GetMapByName(GameType.Assassination, string.Empty, m_mapList[uIScrollList.SelectedItem.Index]);
			SetMap(mapByName);
			m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 2;
			m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 3;
			m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().controlIsEnabled = mapByName.m_player >= 4;
			m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value = true;
		}
		component.Dismiss();
	}

	private FleetSizeClass GetSkirmishFleetSize()
	{
		FleetSizeClass result = FleetSizeClass.Medium;
		if (m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Small;
		}
		if (m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Medium;
		}
		if (m_createSkirmishTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Heavy;
		}
		return result;
	}

	private int GetSkirmishGamePlayers()
	{
		int result = 1;
		if (m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value)
		{
			result = 2;
		}
		if (m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().Value)
		{
			result = 3;
		}
		if (m_createSkirmishTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().Value)
		{
			result = 4;
		}
		return result;
	}

	private void OnCreateSkirmishGame(IUIObject obj)
	{
		if (!m_hasCreated)
		{
			int skirmishGamePlayers = GetSkirmishGamePlayers();
			IUIListObject selectedItem = GuiUtils.FindChildOf(m_createSkirmishTab, "ScrollList").GetComponent<UIScrollList>().SelectedItem;
			if (selectedItem == null)
			{
				m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "No map selected", null);
				return;
			}
			string map = m_mapList[selectedItem.Index];
			bool autoJoin = true;
			GameType gametype = GameType.Points;
			string empty = string.Empty;
			float targetScore = (float)m_targetScore / 100f;
			FleetSizeClass skirmishFleetSize = GetSkirmishFleetSize();
			m_onCreateGame(gametype, empty, map, skirmishGamePlayers, (int)skirmishFleetSize, targetScore, GetSelectedTurnTime(skirmishGamePlayers), autoJoin);
			m_hasCreated = true;
		}
	}

	private int GetCampaignPlayers()
	{
		int result = 1;
		if (m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value)
		{
			result = 2;
		}
		if (m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().Value)
		{
			result = 3;
		}
		if (m_createCampaignTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().Value)
		{
			result = 4;
		}
		return result;
	}

	private void OnCreateCampaignGame(IUIObject obj)
	{
		int campaignPlayers = GetCampaignPlayers();
		IUIListObject selectedItem = GuiUtils.FindChildOf(m_createCampaignTab, "ScrollList").GetComponent<UIScrollList>().SelectedItem;
		if (selectedItem == null)
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "No map selected", null);
			return;
		}
		string name = m_availableCampaigns[m_selectedCampaignID].m_name;
		CreateCampaignGame(name, selectedItem.Index, campaignPlayers);
	}

	public void CreateCampaignGame(string campaignName, int mapNr, int players)
	{
		if (!m_hasCreated)
		{
			List<MapInfo> campaignMaps = m_mapMan.GetCampaignMaps(campaignName);
			MapInfo mapInfo = campaignMaps[mapNr];
			bool autoJoin = true;
			FleetSizeClass fleetSize = FleetSizeClass.None;
			float targetScore = 0f;
			m_onCreateGame(GameType.Campaign, campaignName, mapInfo.m_name, players, (int)fleetSize, targetScore, GetSelectedTurnTime(players), autoJoin);
			m_hasCreated = true;
		}
	}

	private void OnNextCampaign(IUIObject obj)
	{
		SetSelectedCampaign(m_selectedCampaignID + 1);
	}

	private void OnPrevCampaign(IUIObject obj)
	{
		SetSelectedCampaign(m_selectedCampaignID - 1);
	}

	private int GetChallengePlayers()
	{
		int result = 1;
		if (m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value)
		{
			result = 2;
		}
		if (m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().Value)
		{
			result = 3;
		}
		if (m_createChallengeTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().Value)
		{
			result = 4;
		}
		return result;
	}

	private void OnCreateChallengeGame(IUIObject obj)
	{
		if (!m_hasCreated)
		{
			int challengePlayers = GetChallengePlayers();
			IUIListObject selectedItem = GuiUtils.FindChildOf(m_createChallengeTab, "ScrollList").GetComponent<UIScrollList>().SelectedItem;
			if (selectedItem == null)
			{
				m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "No map selected", null);
				return;
			}
			string map = m_mapList[selectedItem.Index];
			bool autoJoin = true;
			FleetSizeClass fleetSize = FleetSizeClass.None;
			float targetScore = 0f;
			string empty = string.Empty;
			m_onCreateGame(GameType.Challenge, empty, map, challengePlayers, (int)fleetSize, targetScore, GetSelectedTurnTime(challengePlayers), autoJoin);
			m_hasCreated = true;
		}
	}

	private FleetSizeClass GetAssassinationFleetSize()
	{
		FleetSizeClass result = FleetSizeClass.Medium;
		if (m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Small").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Small;
		}
		if (m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Medium").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Medium;
		}
		if (m_createAssTab.transform.FindChild("FleetSize/FleetSizeRow/interior/Large").GetComponent<UIRadioBtn>().Value)
		{
			result = FleetSizeClass.Heavy;
		}
		return result;
	}

	private int GetAssassinationPlayers()
	{
		int result = 1;
		if (m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Two").GetComponent<UIRadioBtn>().Value)
		{
			result = 2;
		}
		if (m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Three").GetComponent<UIRadioBtn>().Value)
		{
			result = 3;
		}
		if (m_createAssTab.transform.FindChild("Players/PlayerRow/interior/Four").GetComponent<UIRadioBtn>().Value)
		{
			result = 4;
		}
		return result;
	}

	private void OnCreateAssassinateGame(IUIObject obj)
	{
		if (!m_hasCreated)
		{
			int assassinationPlayers = GetAssassinationPlayers();
			IUIListObject selectedItem = GuiUtils.FindChildOf(m_createAssTab, "ScrollList").GetComponent<UIScrollList>().SelectedItem;
			if (selectedItem == null)
			{
				m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "No map selected", null);
				return;
			}
			string map = m_mapList[selectedItem.Index];
			bool autoJoin = true;
			float targetScore = 1f;
			string empty = string.Empty;
			FleetSizeClass assassinationFleetSize = GetAssassinationFleetSize();
			m_onCreateGame(GameType.Assassination, empty, map, assassinationPlayers, (int)assassinationFleetSize, targetScore, GetSelectedTurnTime(assassinationPlayers), autoJoin);
			m_hasCreated = true;
		}
	}

	private void OnCreateGameCancel(IUIObject obj)
	{
		m_createGameView.SetActiveRecursively(state: false);
		if (m_onClose != null)
		{
			m_onClose();
		}
	}

	private void OnOpenCreateGame(IUIObject obj)
	{
		m_createGameView.SetActiveRecursively(state: true);
		ShowCreateSkirmishGame();
	}

	private void OnCreateSkirmishTab(IUIObject obj)
	{
		ShowCreateSkirmishGame();
	}

	private void ShowCreateSkirmishGame()
	{
		m_createSkirmishTab.SetActiveRecursively(state: true);
		m_createCampaignTab.SetActiveRecursively(state: false);
		m_createChallengeTab.SetActiveRecursively(state: false);
		m_createAssTab.SetActiveRecursively(state: false);
		m_skirmishTabButton.controlIsEnabled = false;
		m_campaignTabButton.controlIsEnabled = true;
		m_challengeTabButton.controlIsEnabled = true;
		m_assTabButton.controlIsEnabled = true;
		UIScrollList component = GuiUtils.FindChildOf(m_createSkirmishTab, "ScrollList").GetComponent<UIScrollList>();
		GameObject gameObject = GuiUtils.FindChildOf(m_createSkirmishTab, "LevelListItem").gameObject;
		List<string> availableMaps = m_userMan.GetAvailableMaps();
		List<MapInfo> skirmishMaps = m_mapMan.GetSkirmishMaps();
		component.ClearList(destroy: true);
		m_mapList.Clear();
		foreach (MapInfo item in skirmishMaps)
		{
			if (availableMaps.Contains(item.m_name))
			{
				component.CreateItem(gameObject, TranslatedMapName(item.m_name));
				m_mapList.Add(item.m_name);
			}
		}
		if (component.Count > 0)
		{
			component.SetSelectedItem(0);
		}
		UpdateTargetScoreWidgets();
		SetupTurnTimers(m_createSkirmishTab, GetSkirmishGamePlayers());
	}

	private void OnCreateCampaignTab(IUIObject obj)
	{
		ShowCreateCampaignGame();
	}

	private void SetupTurnTimers(GameObject panel, int players)
	{
		m_turnTimeButtons.Clear();
		for (int i = 0; i < 10; i++)
		{
			UIRadioBtn uIRadioBtn = GuiUtils.FindChildOfComponent<UIRadioBtn>(panel, "TurnTime" + i);
			if (uIRadioBtn != null)
			{
				uIRadioBtn.controlIsEnabled = players != 1;
				uIRadioBtn.Text = Localize.instance.Translate(Utils.FormatTimeLeftString(Constants.m_turnTimeLimits[i]));
				m_turnTimeButtons.Add(new KeyValuePair<UIRadioBtn, double>(uIRadioBtn, Constants.m_turnTimeLimits[i]));
			}
		}
	}

	private double GetSelectedTurnTime(int players)
	{
		if (players == 1)
		{
			return -1.0;
		}
		foreach (KeyValuePair<UIRadioBtn, double> turnTimeButton in m_turnTimeButtons)
		{
			if (turnTimeButton.Key.Value)
			{
				return turnTimeButton.Value;
			}
		}
		return -1.0;
	}

	private void ShowCreateCampaignGame()
	{
		m_createSkirmishTab.SetActiveRecursively(state: false);
		m_createCampaignTab.SetActiveRecursively(state: true);
		m_createChallengeTab.SetActiveRecursively(state: false);
		m_createAssTab.SetActiveRecursively(state: false);
		m_skirmishTabButton.controlIsEnabled = true;
		m_campaignTabButton.controlIsEnabled = false;
		m_challengeTabButton.controlIsEnabled = true;
		m_assTabButton.controlIsEnabled = true;
		List<CampaignInfo> campaigns = m_mapMan.GetCampaigns();
		List<string> availableCampaigns = m_userMan.GetAvailableCampaigns();
		m_availableCampaigns.Clear();
		foreach (CampaignInfo item in campaigns)
		{
			if (availableCampaigns.Contains(item.m_name))
			{
				m_availableCampaigns.Add(item);
			}
		}
		SetSelectedCampaign(0);
		SetupTurnTimers(m_createCampaignTab, GetCampaignPlayers());
	}

	private void SetSelectedCampaign(int id)
	{
		if (id >= 0 && id < m_availableCampaigns.Count)
		{
			m_selectedCampaignID = id;
			GuiUtils.FindChildOf(m_createCampaignTab, "CampaignNameLbl").GetComponent<SpriteText>().Text = Localize.instance.Translate("$" + m_availableCampaigns[m_selectedCampaignID].m_name);
			GuiUtils.FindChildOf(m_createCampaignTab, "CampaignDescLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(m_availableCampaigns[m_selectedCampaignID].m_description);
			GuiUtils.FindChildOf(m_createCampaignTab, "ArrowLeftButton").GetComponent<UIButton>().controlIsEnabled = m_selectedCampaignID != 0;
			GuiUtils.FindChildOf(m_createCampaignTab, "ArrowRightButton").GetComponent<UIButton>().controlIsEnabled = m_selectedCampaignID != m_availableCampaigns.Count - 1;
			UpdateCampaignMapList();
		}
	}

	private void OnCreateChallengeTab(IUIObject obj)
	{
		ShowCreateChallengeGame();
	}

	private void OnCreateAssTab(IUIObject obj)
	{
		ShowCreateAssassinateGame();
	}

	private void ShowCreateChallengeGame()
	{
		m_createSkirmishTab.SetActiveRecursively(state: false);
		m_createCampaignTab.SetActiveRecursively(state: false);
		m_createAssTab.SetActiveRecursively(state: false);
		m_createChallengeTab.SetActiveRecursively(state: true);
		m_skirmishTabButton.controlIsEnabled = true;
		m_campaignTabButton.controlIsEnabled = true;
		m_challengeTabButton.controlIsEnabled = false;
		m_assTabButton.controlIsEnabled = true;
		UIScrollList component = GuiUtils.FindChildOf(m_createChallengeTab, "ScrollList").GetComponent<UIScrollList>();
		GameObject gameObject = GuiUtils.FindChildOf(m_createChallengeTab, "LevelListItem").gameObject;
		List<string> availableMaps = m_userMan.GetAvailableMaps();
		List<MapInfo> challengeMaps = m_mapMan.GetChallengeMaps();
		component.ClearList(destroy: true);
		m_mapList.Clear();
		foreach (MapInfo item in challengeMaps)
		{
			if (availableMaps.Contains(item.m_name))
			{
				component.CreateItem(gameObject, TranslatedMapName(item.m_name));
				m_mapList.Add(item.m_name);
			}
		}
		if (component.Count > 0)
		{
			component.SetSelectedItem(0);
		}
		SetupTurnTimers(m_createChallengeTab, GetChallengePlayers());
	}

	private void ShowCreateAssassinateGame()
	{
		m_createSkirmishTab.SetActiveRecursively(state: false);
		m_createCampaignTab.SetActiveRecursively(state: false);
		m_createChallengeTab.SetActiveRecursively(state: false);
		m_createAssTab.SetActiveRecursively(state: true);
		m_skirmishTabButton.controlIsEnabled = true;
		m_campaignTabButton.controlIsEnabled = true;
		m_challengeTabButton.controlIsEnabled = true;
		m_assTabButton.controlIsEnabled = false;
		UIScrollList component = GuiUtils.FindChildOf(m_createAssTab, "ScrollList").GetComponent<UIScrollList>();
		GameObject gameObject = GuiUtils.FindChildOf(m_createAssTab, "LevelListItem").gameObject;
		List<string> availableMaps = m_userMan.GetAvailableMaps();
		List<MapInfo> assassinationMaps = m_mapMan.GetAssassinationMaps();
		component.ClearList(destroy: true);
		m_mapList.Clear();
		foreach (MapInfo item in assassinationMaps)
		{
			if (availableMaps.Contains(item.m_name))
			{
				component.CreateItem(gameObject, TranslatedMapName(item.m_name));
				m_mapList.Add(item.m_name);
			}
		}
		if (component.Count > 0)
		{
			component.SetSelectedItem(0);
		}
		SetupTurnTimers(m_createAssTab, GetAssassinationPlayers());
	}

	private void UpdateCampaignMapList()
	{
		UIScrollList component = GuiUtils.FindChildOf(m_createCampaignTab, "ScrollList").GetComponent<UIScrollList>();
		GameObject gameObject = GuiUtils.FindChildOf(m_createCampaignTab, "LevelListItem").gameObject;
		string name = m_availableCampaigns[m_selectedCampaignID].m_name;
		List<string> unlockedCampaignMaps = m_userMan.GetUnlockedCampaignMaps(name);
		component.ClearList(destroy: true);
		List<MapInfo> campaignMaps = m_mapMan.GetCampaignMaps(name);
		m_mapList.Clear();
		foreach (MapInfo item in campaignMaps)
		{
			UIListItem uIListItem = component.CreateItem(gameObject, TranslatedMapName(item.m_name)) as UIListItem;
			m_mapList.Add(item.m_name);
			if (!unlockedCampaignMaps.Contains(item.m_name))
			{
				uIListItem.SetControlState(UIButton.CONTROL_STATE.DISABLED);
			}
		}
		component.SetSelectedItem(0);
	}

	public static string TranslatedMapName(string mapName)
	{
		return Localize.instance.TranslateKey("mapname_" + mapName);
	}

	private void SetMap(MapInfo map)
	{
		m_mapNameLabel.Text = TranslatedMapName(map.m_name);
		m_mapDescriptionLabel.Text = Localize.instance.Translate(map.m_description);
		m_mapSizeLabel.Text = map.m_size + "x" + map.m_size;
		Texture2D texture2D = Resources.Load("MapThumbs/" + map.m_thumbnail) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning("Map Thumbnail " + map.m_thumbnail + " is missing");
			return;
		}
		m_mapImage.SetTexture(texture2D);
		m_mapImage.UpdateUVs();
	}
}
