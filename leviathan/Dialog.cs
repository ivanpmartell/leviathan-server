using System;
using System.Collections.Generic;
using System.Xml;
using PTech;
using UnityEngine;

public class Dialog
{
	private class DialogCommand
	{
		public string m_command;

		public string m_param;

		public string m_attribute1;

		public string m_attribute2;

		public DialogCommand(string command, string param, string attribute1, string attribute2)
		{
			m_command = command;
			m_param = param;
			m_attribute1 = attribute1;
			m_attribute2 = attribute2;
		}
	}

	private List<DialogCommand> m_commands = new List<DialogCommand>();

	public int m_index;

	private bool m_showSkipButton = true;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private TurnMan m_turnMan;

	private SpriteText m_text;

	private SpriteText m_name;

	private SimpleSprite m_portrait;

	private SimpleSprite m_wallpaper;

	private SimpleSprite m_image;

	private SimpleSprite m_box;

	private PackedSprite m_portraitOverlay;

	private string m_currentName;

	private string m_currentText;

	private string m_currentPortrait;

	public Action<bool> m_onPlayDialog;

	public Action m_onEndDialog;

	private bool m_waitInput;

	private bool m_skipCutscenes;

	private MNAction.MNActionElement[] m_dialog;

	private int m_currentDialog;

	private GameCamera.Mode m_lastMode;

	private List<ClientPlayer> m_players;

	private Dictionary<string, string> m_macros = new Dictionary<string, string>();

	private List<DialogAdvice> m_advice = new List<DialogAdvice>();

	private int m_currentAdvice = -1;

	private string m_advicePortrait = string.Empty;

	private static bool m_dialogActive;

	private MorsePlayer m_morsePlayer = new MorsePlayer();

	private bool m_isBriefing;

	public Dialog(PTech.RPC rpc, GameObject guiCamera, Hud hud, TurnMan turnMan)
	{
		Clear();
		if (!(guiCamera == null))
		{
			m_guiCamera = guiCamera;
			m_turnMan = turnMan;
			Hide();
		}
	}

	private void OnLaunch(IUIObject obj)
	{
		EndDialog();
	}

	private void OnNextAdvise(IUIObject obj)
	{
		if (m_advice.Count != 0)
		{
			m_currentAdvice++;
			if (m_currentAdvice >= m_advice.Count)
			{
				m_currentAdvice = 0;
			}
			SimpleSprite component = GuiUtils.FindChildOf(m_gui.transform, "portrait").GetComponent<SimpleSprite>();
			SetPortrait(component, m_advice[m_currentAdvice].m_portrait);
			GuiUtils.FindChildOf(m_gui.transform, "text").GetComponent<SpriteText>().Text = Localize.instance.Translate(m_advice[m_currentAdvice].m_text);
		}
	}

	private void OnSkip(IUIObject obj)
	{
		if (!PlayNextScene())
		{
			EndDialog();
		}
	}

	private void OnNext(IUIObject obj)
	{
		if (m_waitInput)
		{
			m_waitInput = false;
		}
	}

	public void ForceEndDialog()
	{
		m_onEndDialog = null;
		EndDialog();
	}

	private void EndDialog()
	{
		m_dialogActive = false;
		m_index = 0;
		m_commands.Clear();
		GameCamera component = GameObject.Find("GameCamera").GetComponent<GameCamera>();
		component.SetMode(m_lastMode);
		m_advice = new List<DialogAdvice>();
		m_currentAdvice = -1;
		Close();
		if (m_onEndDialog != null)
		{
			m_onEndDialog();
		}
	}

	public void Close()
	{
		m_dialogActive = false;
		UnityEngine.Object.Destroy(m_gui);
		m_gui = null;
		m_advice = new List<DialogAdvice>();
		m_currentAdvice = -1;
		m_wallpaper = null;
		m_image = null;
		m_box = null;
	}

	public void Clear()
	{
		m_currentName = string.Empty;
		m_currentText = string.Empty;
		m_currentPortrait = string.Empty;
	}

	public void Hide()
	{
		m_commands.Clear();
		if ((bool)m_gui)
		{
			m_gui.SetActiveRecursively(state: false);
		}
	}

	public void Show()
	{
		if ((bool)m_gui)
		{
			m_gui.SetActiveRecursively(state: true);
		}
		if ((bool)m_wallpaper)
		{
			m_wallpaper.gameObject.SetActiveRecursively(state: false);
		}
		if ((bool)m_image)
		{
			m_image.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void CreateGui(string gui)
	{
		m_currentName = string.Empty;
		m_currentText = string.Empty;
		m_currentPortrait = string.Empty;
		if (gui == "dlg")
		{
			m_gui = GuiUtils.CreateGui("Dialog", m_guiCamera);
			m_text = GuiUtils.FindChildOf(m_gui.transform, "text").GetComponent<SpriteText>();
			m_name = GuiUtils.FindChildOf(m_gui.transform, "name").GetComponent<SpriteText>();
			m_text.Text = string.Empty;
			m_name.Text = string.Empty;
			m_portrait = GuiUtils.FindChildOf(m_gui.transform, "portrait").GetComponent<SimpleSprite>();
			m_portraitOverlay = GuiUtils.FindChildOf(m_gui.transform, "portrait_overlay").GetComponent<PackedSprite>();
			m_wallpaper = GuiUtils.FindChildOf(m_gui.transform, "wallpaper").GetComponent<SimpleSprite>();
			m_image = GuiUtils.FindChildOf(m_gui.transform, "image").GetComponent<SimpleSprite>();
			m_box = GuiUtils.FindChildOf(m_gui.transform, "box").GetComponent<SimpleSprite>();
			GuiUtils.FindChildOf(m_gui.transform, "btnSkip").GetComponent<UIButton>().AddValueChangedDelegate(OnSkip);
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").GetComponent<UIButton>().AddValueChangedDelegate(OnNext);
		}
		if (gui == "brf")
		{
			m_gui = GuiUtils.CreateGui("Dialog_Briefing", m_guiCamera);
			GuiUtils.FindChildOf(m_gui.transform, "btnSkip").GetComponent<UIButton>().AddValueChangedDelegate(OnLaunch);
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").GetComponent<UIButton>().AddValueChangedDelegate(OnNextAdvise);
		}
		if (gui == "debrf")
		{
			m_gui = GuiUtils.CreateGui("Dialog_Debriefing", m_guiCamera);
			GuiUtils.FindChildOf(m_gui.transform, "btnSkip").GetComponent<UIButton>().AddValueChangedDelegate(OnLaunch);
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").GetComponent<UIButton>().AddValueChangedDelegate(OnNextAdvise);
		}
		if (gui == "tut")
		{
			m_gui = GuiUtils.CreateGui("Dialog_Tutorial", m_guiCamera);
			m_text = GuiUtils.FindChildOf(m_gui.transform, "text").GetComponent<SpriteText>();
			m_name = GuiUtils.FindChildOf(m_gui.transform, "name").GetComponent<SpriteText>();
			m_text.Text = string.Empty;
			m_name.Text = string.Empty;
			m_portrait = GuiUtils.FindChildOf(m_gui.transform, "portrait").GetComponent<SimpleSprite>();
			m_portraitOverlay = GuiUtils.FindChildOf(m_gui.transform, "portrait_overlay").GetComponent<PackedSprite>();
			m_wallpaper = GuiUtils.FindChildOf(m_gui.transform, "wallpaper").GetComponent<SimpleSprite>();
			m_image = GuiUtils.FindChildOf(m_gui.transform, "image").GetComponent<SimpleSprite>();
			m_box = GuiUtils.FindChildOf(m_gui.transform, "box").GetComponent<SimpleSprite>();
			GuiUtils.FindChildOf(m_gui.transform, "btnSkip").GetComponent<UIButton>().AddValueChangedDelegate(OnSkip);
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").GetComponent<UIButton>().AddValueChangedDelegate(OnNext);
		}
	}

	public void LoadScene(string scenename)
	{
		m_showSkipButton = true;
		PLog.Log("LoadScene: " + scenename);
		m_commands.Clear();
		m_index = 0;
		XmlDocument xmlDocument = Utils.LoadXml(scenename);
		if (xmlDocument == null)
		{
			PLog.LogError("Missing dialog " + scenename);
			return;
		}
		XmlNode firstChild = xmlDocument.FirstChild;
		for (XmlNode xmlNode = firstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			string attribute = "250";
			string attribute2 = "250";
			if (xmlNode.Attributes["width"] != null)
			{
				attribute = xmlNode.Attributes["width"].Value;
			}
			if (xmlNode.Attributes["height"] != null)
			{
				attribute2 = xmlNode.Attributes["height"].Value;
			}
			if (xmlNode.Attributes["focus"] != null)
			{
				attribute = xmlNode.Attributes["focus"].Value;
			}
			m_commands.Add(new DialogCommand(xmlNode.Name, xmlNode.InnerText, attribute, attribute2));
		}
	}

	public void LoadDialog(string scenename)
	{
		m_isBriefing = false;
		CreateGui("dlg");
		LoadScene(scenename);
	}

	public void LoadBriefing(string scenename)
	{
		m_isBriefing = true;
		CreateGui("brf");
		LoadScene(scenename);
		if (scenename.Contains("debriefing"))
		{
			GuiUtils.FindChildOf(m_gui.transform, "btnSkip").GetComponent<UIButton>().Text = Localize.instance.Translate("$label_continue");
		}
	}

	public void LoadDebriefing(string scenename)
	{
		m_isBriefing = true;
		CreateGui("debrf");
		LoadScene(scenename);
	}

	public void LoadTutorial(string scenename)
	{
		m_isBriefing = false;
		CreateGui("tut");
		LoadScene(scenename);
	}

	public void SetImage(SimpleSprite sprite, string name, string attr1, string attr2)
	{
		int num = int.Parse(attr1);
		int num2 = int.Parse(attr2);
		if (name.Length == 0)
		{
			sprite.gameObject.SetActiveRecursively(state: false);
			return;
		}
		sprite.gameObject.SetActiveRecursively(state: true);
		Texture2D texture2D = Resources.Load(name) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogError("Failed to load texture");
		}
		float x = texture2D.width;
		float y = texture2D.height;
		sprite.Setup(num, num2, new Vector2(0f, y), new Vector2(x, y));
		sprite.SetTexture(texture2D);
		sprite.Setup(num, num2, new Vector2(0f, y), new Vector2(x, y));
		sprite.UpdateUVs();
	}

	public void SetPortrait(SimpleSprite sprite, string name)
	{
		if (name.Length == 0)
		{
			sprite.gameObject.SetActiveRecursively(state: false);
			return;
		}
		sprite.gameObject.SetActiveRecursively(state: true);
		float width = sprite.width;
		float height = sprite.height;
		Texture2D texture2D = Resources.Load(name) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogError("Failed to load texture");
		}
		float x = texture2D.width;
		float y = texture2D.height;
		sprite.Setup(width, height, new Vector2(0f, y), new Vector2(x, y));
		sprite.SetTexture(texture2D);
		sprite.Setup(width, height, new Vector2(0f, y), new Vector2(x, y));
		sprite.UpdateUVs();
	}

	private bool IsPlayback()
	{
		Camera component = GameObject.Find("GameCamera").GetComponent<Camera>();
		if (component != null && component.enabled)
		{
			return true;
		}
		return false;
	}

	public bool PlayNextScene()
	{
		MNAction.MNActionElement mNActionElement;
		while (true)
		{
			m_currentDialog++;
			if (m_currentDialog >= m_dialog.Length)
			{
				return false;
			}
			mNActionElement = m_dialog[m_currentDialog];
			if (mNActionElement.m_type == MNAction.ActionType.PlayScene)
			{
				if (!m_skipCutscenes)
				{
					string parameter = mNActionElement.m_parameter;
					LoadDialog(parameter);
					m_waitInput = false;
				}
				return true;
			}
			if (mNActionElement.m_type == MNAction.ActionType.ShowBriefing)
			{
				if (!m_skipCutscenes)
				{
					string parameter2 = mNActionElement.m_parameter;
					LoadBriefing(parameter2);
					m_waitInput = false;
				}
				return true;
			}
			if (mNActionElement.m_type == MNAction.ActionType.ShowDebriefing)
			{
				if (!m_skipCutscenes)
				{
					string parameter3 = mNActionElement.m_parameter;
					LoadDebriefing(parameter3);
					m_waitInput = false;
				}
				return true;
			}
			if (mNActionElement.m_type == MNAction.ActionType.ShowTutorial)
			{
				break;
			}
			if (mNActionElement.m_type == MNAction.ActionType.UpdateObjective)
			{
				TurnMan.instance.SetMissionObjective(mNActionElement.m_parameter, mNActionElement.m_objectiveStatus);
			}
			if (mNActionElement.m_type == MNAction.ActionType.MissionVictory)
			{
				TurnMan.instance.m_endGame = GameOutcome.Victory;
			}
			if (mNActionElement.m_type == MNAction.ActionType.MissionDefeat)
			{
				TurnMan.instance.m_endGame = GameOutcome.Defeat;
			}
			if (mNActionElement.m_type == MNAction.ActionType.MissionDefeat)
			{
				TurnMan.instance.m_endGame = GameOutcome.Defeat;
			}
			if (mNActionElement.m_type == MNAction.ActionType.Marker)
			{
				SetMarker(mNActionElement);
			}
			if (mNActionElement.m_type == MNAction.ActionType.Message)
			{
				MessageLog.instance.ShowMessage(MessageLog.TextPosition.Middle, "$" + mNActionElement.m_parameter, string.Empty, "NewsflashMessage", 2f);
			}
			if (mNActionElement.m_type == MNAction.ActionType.PlayerChange)
			{
				ActionPlayerChange(mNActionElement);
			}
			if (mNActionElement.m_type == MNAction.ActionType.Event)
			{
				ActionEvent(mNActionElement);
			}
			if (mNActionElement.m_type == MNAction.ActionType.MissionAchievement)
			{
				TurnMan.instance.m_missionAchievement = int.Parse(mNActionElement.m_parameter);
				Constants.AchivementId missionAchievement = (Constants.AchivementId)TurnMan.instance.m_missionAchievement;
				PLog.Log("Mission Achievement Set To: " + missionAchievement);
			}
		}
		if (!m_skipCutscenes)
		{
			string parameter4 = mNActionElement.m_parameter;
			LoadTutorial(parameter4);
			m_waitInput = false;
		}
		return true;
	}

	private void ActionEvent(MNAction.MNActionElement ae)
	{
		GameObject target = ae.GetTarget();
		if (!(target == null))
		{
			MNode component = target.GetComponent<MNode>();
			if ((bool)component)
			{
				component.OnEvent(ae.m_parameter);
			}
			Platform component2 = target.GetComponent<Platform>();
			if ((bool)component2)
			{
				component2.OnEvent(ae.m_parameter);
			}
		}
	}

	private void ActionPlayerChange(MNAction.MNActionElement ae)
	{
		GameObject target = ae.GetTarget();
		if (target == null)
		{
			return;
		}
		List<GameObject> targets = MNode.GetTargets(target);
		int owner = int.Parse(ae.m_parameter);
		for (int i = 0; i < targets.Count; i++)
		{
			GameObject gameObject = targets[i];
			Platform component = gameObject.GetComponent<Platform>();
			if ((bool)component)
			{
				component.SetOwner(owner);
				continue;
			}
			MNSpawn component2 = gameObject.GetComponent<MNSpawn>();
			if (!component2)
			{
				continue;
			}
			GameObject spawnedShip = component2.GetSpawnedShip();
			if (!(spawnedShip == null))
			{
				Ship component3 = spawnedShip.GetComponent<Ship>();
				if (!(component3 == null))
				{
					component3.SetOwner(owner);
					component3.ResetAiState();
				}
			}
		}
	}

	public void SetMarker(MNAction.MNActionElement ae)
	{
		GameObject target = ae.GetTarget();
		if (target == null)
		{
			return;
		}
		GameObject gameObject = target;
		Marker component = gameObject.GetComponent<Marker>();
		if (component != null)
		{
			if (ae.m_objectiveType == Unit.ObjectiveTypes.None)
			{
				component.SetVisibleState(visible: false);
			}
			else
			{
				component.SetVisibleState(visible: true);
			}
		}
		Unit component2 = gameObject.GetComponent<Unit>();
		if (component2 != null)
		{
			component2.SetObjective(ae.m_objectiveType);
		}
		MNSpawn component3 = gameObject.GetComponent<MNSpawn>();
		if (component3 != null)
		{
			component3.SetObjective(ae.m_objectiveType);
		}
	}

	public void SetCommands(MNAction.MNActionElement[] commands)
	{
		m_commands.Clear();
		m_index = 0;
		m_dialog = commands;
		m_currentDialog = -1;
		m_skipCutscenes = !IsPlayback();
		m_dialogActive = true;
	}

	public void PlayAll()
	{
		while (PlayNextScene())
		{
		}
	}

	public void Update(List<ClientPlayer> players)
	{
		if ((bool)m_gui)
		{
			m_morsePlayer.Update();
		}
		m_players = players;
		SetupMacros();
		if (TurnMan.instance != null && TurnMan.instance.m_dialog != null)
		{
			SetCommands(TurnMan.instance.m_dialog);
			TurnMan.instance.m_dialog = null;
			if (!PlayNextScene())
			{
				m_dialogActive = false;
				return;
			}
			m_onPlayDialog(m_isBriefing);
			GameCamera component = GameObject.Find("GameCamera").GetComponent<GameCamera>();
			m_lastMode = component.GetMode();
			component.SetMode(GameCamera.Mode.Disabled);
		}
		if (m_commands.Count == 0 || m_waitInput)
		{
			return;
		}
		while (m_index < m_commands.Count)
		{
			RunCommand(m_commands[m_index]);
			m_index++;
			if (m_waitInput)
			{
				OnNextAdvise(null);
				return;
			}
		}
		if (m_index >= m_commands.Count && !PlayNextScene())
		{
			EndDialog();
		}
	}

	public void FixedUpdate()
	{
	}

	private void SetupMacros()
	{
		m_macros.Clear();
		m_macros = new Dictionary<string, string>();
		foreach (ClientPlayer player in m_players)
		{
			string playerName = m_turnMan.GetPlayerName(player.m_id);
			m_macros["playername" + player.m_id] = playerName;
		}
	}

	private void RunCommand(DialogCommand cmd)
	{
		GameCamera component = GameObject.Find("GameCamera").GetComponent<GameCamera>();
		if (cmd.m_command == "NoSkip")
		{
			m_showSkipButton = false;
		}
		if (cmd.m_command == "PlaySound")
		{
			AudioClip clip = (AudioClip)Resources.Load(cmd.m_param, typeof(AudioClip));
			AudioSource.PlayClipAtPoint(clip, component.transform.position);
		}
		if (cmd.m_command == "SetName")
		{
			string text = (m_currentName = Localize.instance.Translate(cmd.m_param));
			m_name.Text = text;
		}
		if (cmd.m_command == "MissionAdviceName")
		{
			string text2 = Localize.instance.Translate(cmd.m_param);
			SpriteText component2 = GuiUtils.FindChildOf(m_gui.transform, "name").GetComponent<SpriteText>();
			component2.Text = text2;
		}
		if (cmd.m_command == "SetText")
		{
			string text3 = (m_currentText = Localize.instance.Translate(cmd.m_param));
			m_text.Text = text3;
		}
		if (cmd.m_command == "WaitInput")
		{
			m_waitInput = true;
		}
		if (cmd.m_command == "SetPortrait")
		{
			m_currentPortrait = cmd.m_param;
			SetPortrait(m_portrait, cmd.m_param);
		}
		if (cmd.m_command == "SetWallpaper")
		{
			if (cmd.m_param == string.Empty)
			{
				m_wallpaper.gameObject.SetActiveRecursively(state: false);
			}
			else
			{
				SetPortrait(m_wallpaper, cmd.m_param);
				m_wallpaper.gameObject.SetActiveRecursively(state: true);
			}
		}
		if (cmd.m_command == "SetImage")
		{
			if (cmd.m_param == string.Empty)
			{
				m_image.gameObject.SetActiveRecursively(state: false);
			}
			else
			{
				SetImage(m_image, cmd.m_param, cmd.m_attribute1, cmd.m_attribute2);
				m_image.gameObject.SetActiveRecursively(state: true);
			}
		}
		if ((bool)m_gui)
		{
			if (cmd.m_command == "MissionTitle")
			{
				string text4 = Localize.instance.Translate(cmd.m_param);
				GuiUtils.FindChildOf(m_gui.transform, "MissionTitleLabel").GetComponent<SpriteText>().Text = text4;
			}
			if (cmd.m_command == "MissionLabel")
			{
				string text5 = Localize.instance.Translate(cmd.m_param);
				GuiUtils.FindChildOf(m_gui.transform, "MissionNumberLabel").GetComponent<SpriteText>().Text = text5;
			}
			if (cmd.m_command == "MissionIcon")
			{
				SimpleSprite component3 = GuiUtils.FindChildOf(m_gui.transform, "MissionIcon").GetComponent<SimpleSprite>();
				SetPortrait(component3, cmd.m_param);
			}
			if (cmd.m_command == "MissionText")
			{
				GameObject original = Resources.Load("gui/Briefing/MissionBriefingListItem") as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
				GuiUtils.FindChildOf(gameObject.transform, "MissionBriefingText").GetComponent<SpriteText>().Text = Localize.instance.Translate(cmd.m_param).Replace("\\n", "\n");
				UIScrollList component4 = GuiUtils.FindChildOf(m_gui.transform, "MissionBriefingScrollist").GetComponent<UIScrollList>();
				UIListItemContainer component5 = gameObject.GetComponent<UIListItemContainer>();
				component4.AddItem(component5);
				m_morsePlayer.SetText("SOS SOS SOS SOS SOS SOS SOS SOS SOS SOS PARIS WE HAVE COLLISION WITH ICEBERG. SINKING. CAN HEAR NOTHING FOR NOISE OF STEAM");
			}
			if (cmd.m_command == "MissionImage1")
			{
				SimpleSprite component6 = GuiUtils.FindChildOf(m_gui.transform, "Image1").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component6, cmd.m_param);
			}
			if (cmd.m_command == "MissionImage2")
			{
				SimpleSprite component7 = GuiUtils.FindChildOf(m_gui.transform, "Image2").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component7, cmd.m_param);
			}
			if (cmd.m_command == "MissionImage3")
			{
				SimpleSprite component8 = GuiUtils.FindChildOf(m_gui.transform, "Image3").GetComponent<SimpleSprite>();
				SetPortrait(component8, cmd.m_param);
			}
			if (cmd.m_command == "MissionImage4")
			{
				SimpleSprite component9 = GuiUtils.FindChildOf(m_gui.transform, "Image4").GetComponent<SimpleSprite>();
				SetPortrait(component9, cmd.m_param);
			}
			if (cmd.m_command == "MissionPrimObj")
			{
				GameObject original2 = Resources.Load("gui/Briefing/PrimaryObjectivesListItem") as GameObject;
				GameObject gameObject2 = UnityEngine.Object.Instantiate(original2) as GameObject;
				UIScrollList component10 = GuiUtils.FindChildOf(m_gui.transform, "PrimaryObjectivesScrollist").GetComponent<UIScrollList>();
				UIListItemContainer component11 = gameObject2.GetComponent<UIListItemContainer>();
				component10.AddItem(component11);
				GuiUtils.FindChildOf(gameObject2.transform, "PrimaryObjectivesText").GetComponent<SpriteText>().Text = Localize.instance.Translate(cmd.m_param);
			}
			if (cmd.m_command == "MissionSecObj")
			{
				GameObject original3 = Resources.Load("gui/Briefing/SecondaryObjectivesListItem") as GameObject;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(original3) as GameObject;
				UIScrollList component12 = GuiUtils.FindChildOf(m_gui.transform, "SecondaryObjectivesScrollist").GetComponent<UIScrollList>();
				UIListItemContainer component13 = gameObject3.GetComponent<UIListItemContainer>();
				component12.AddItem(component13);
				GuiUtils.FindChildOf(gameObject3.transform, "SecondaryObjectivesText").GetComponent<SpriteText>().Text = Localize.instance.Translate(cmd.m_param);
			}
			if (cmd.m_command == "MissionAdvice")
			{
				string text6 = Localize.instance.Translate(cmd.m_param);
				DialogAdvice dialogAdvice = new DialogAdvice();
				dialogAdvice.m_text = text6;
				dialogAdvice.m_portrait = m_advicePortrait;
				m_advice.Add(dialogAdvice);
			}
			if (cmd.m_command == "MissionAdvicePort")
			{
				m_advicePortrait = cmd.m_param;
			}
			if (cmd.m_command == "MissionImagePosition")
			{
				string text7 = Localize.instance.Translate(cmd.m_param);
				GuiUtils.FindChildOf(m_gui.transform, "PositionLabel").GetComponent<SpriteText>().Text = text7;
			}
			if (cmd.m_command == "MissionImageTime")
			{
				string text8 = Localize.instance.Translate(cmd.m_param);
				GuiUtils.FindChildOf(m_gui.transform, "TimeLabel").GetComponent<SpriteText>().Text = text8;
			}
		}
		if (cmd.m_command == "TUT")
		{
			GameObject gameObject4 = GameObject.Find("tutorial");
			if ((bool)gameObject4)
			{
				MNTutorial component14 = gameObject4.GetComponent<MNTutorial>();
				component14.OnCommand(cmd.m_param, cmd.m_attribute1, cmd.m_attribute2);
			}
		}
		GuiUtils.FindChildOf(m_gui.transform, "btnSkip").SetActiveRecursively(m_showSkipButton);
		if (m_isBriefing)
		{
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").SetActiveRecursively(m_advice.Count >= 2);
		}
		else
		{
			GuiUtils.FindChildOf(m_gui.transform, "btnNext").SetActiveRecursively(state: true);
		}
		if (m_isBriefing && (bool)m_box)
		{
			m_box.gameObject.SetActiveRecursively(state: false);
		}
		if (m_currentName.Length != 0 || m_currentText.Length != 0 || m_currentPortrait.Length != 0)
		{
			m_box.gameObject.active = true;
		}
		if (m_currentName.Length != 0)
		{
			m_name.gameObject.active = true;
		}
		if (m_currentText.Length != 0)
		{
			m_text.gameObject.active = true;
		}
		if (m_currentPortrait.Length != 0)
		{
			m_portraitOverlay.gameObject.active = true;
			m_portrait.gameObject.active = true;
		}
	}

	public static bool IsDialogActive()
	{
		return m_dialogActive;
	}
}
