#define DEBUG
using System.Collections.Generic;
using System.Xml;
using PTech;
using UnityEngine;

public class ShipMenu
{
	public class HpPosition
	{
		public int m_OrderNum;

		public Section.SectionType m_sectionType;

		public Vector2i m_gridPosition;
	}

	public enum PartType
	{
		Hull,
		Hadpoints
	}

	public delegate void OnExitDelegate();

	public delegate void OnSaveDelegate(ShipDef shipDef);

	private const float m_moveTreshold = 20f;

	public OnExitDelegate m_onExit;

	public OnSaveDelegate m_onSave;

	private int m_campaignID;

	private string m_shipSeries = string.Empty;

	private GameObject portShip;

	private GameObject markerBase;

	private ShipDef m_portShipDef;

	private string m_part = string.Empty;

	private Direction m_currentDir;

	private GameObject m_selectedHPModule;

	private GameObject m_lblTotalCost;

	private GameObject m_HPMenu;

	private GameObject m_HPDeleta;

	private GameObject m_HPRotate;

	private GameObject m_HPViewCone;

	private GameObject m_gui;

	private GameObject m_saveDialog;

	private GameObject m_guiCamera;

	private GameObject m_sceneCamera;

	private UserManClient m_userManClient;

	private Vector3 m_minEdge;

	private Vector3 m_maxEdge;

	private bool m_draging;

	private Vector3 m_dragStart;

	private Vector3 m_dragStartMousePos;

	private Vector3 m_dragLastPos;

	private GameObject m_dragObject;

	private GameObject m_dragObjectIcon;

	private int m_dragWidth = 1;

	private int m_dragHeight = 1;

	private HPModule.HPModuleType m_dragType = HPModule.HPModuleType.Any;

	private Vector3 m_dropPosition;

	private Battery m_dropBattery;

	private float m_mouseMoveSpeed = 0.001f;

	private float m_camsize = 30f;

	private float m_zoomMin = 20f;

	private float m_zoomMax = 60f;

	private float m_allowZoomTime;

	private Vector3 m_listPost = new Vector3(0f, 0f, 0f);

	private bool m_pinchZoom;

	private float m_pinchStartDistance = -1f;

	private bool m_showAllViewCones;

	private bool m_shipModified;

	private MsgBox m_msgBox;

	private bool m_onSaveExit;

	private string m_prevScene;

	private UIPanel m_panelFleetTop;

	private UIPanel m_panelShipBrowser;

	private UIPanel m_panelShipName;

	private UIPanel m_panelShipInfo;

	private UIPanel m_panelShipEquipment;

	private GameObject m_infoArmament;

	private GameObject m_infoBody;

	private GameObject m_infoBodyTop;

	private FleetShip m_editShip;

	private int m_fleetBaseCost;

	private int m_buttonGroup;

	private FleetMenu m_fleetMenu;

	private FleetSize m_fleetSize;

	private int m_maxHardpoints = 10;

	public ShipMenu(GameObject gui, GameObject guiCamera, UserManClient userManClient, FleetShip fleetship, int campaignID, int fleetPoints, FleetMenu fleetmenu, FleetSize fleetSize)
	{
		m_guiCamera = guiCamera;
		m_userManClient = userManClient;
		m_campaignID = campaignID;
		m_gui = gui;
		m_fleetMenu = fleetmenu;
		m_editShip = fleetship;
		m_fleetSize = fleetSize;
		RegisterDelegatesToComponents();
		byte[] data = m_editShip.m_definition.ToArray();
		m_portShipDef = new ShipDef();
		m_portShipDef.FromArray(data);
		m_shipSeries = m_portShipDef.m_prefab;
		int shipValue = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
		m_fleetBaseCost = fleetPoints - shipValue;
		RecalcCost();
		FillPartsList();
		fleetmenu.CleanUp();
		m_shipModified = false;
		OnLevelWasLoaded();
		m_panelFleetTop.Dismiss();
		m_panelShipBrowser.Dismiss();
		m_panelShipName.BringIn();
		m_panelShipInfo.BringIn();
		m_panelShipEquipment.BringIn();
		m_allowZoomTime = Time.time + 1f;
		ResetShipMenuGui(body: true);
		m_infoArmament.SetActiveRecursively(state: false);
		m_infoBody.SetActiveRecursively(state: false);
		m_infoBodyTop.SetActiveRecursively(state: false);
	}

	~ShipMenu()
	{
		PLog.Log("ShipMenu DESTROYED");
	}

	private GameObject ShowInfoPanel(GameObject go)
	{
		m_infoArmament.SetActiveRecursively(state: false);
		m_infoBody.SetActiveRecursively(state: false);
		m_infoBodyTop.SetActiveRecursively(state: false);
		go.SetActiveRecursively(state: true);
		return go;
	}

	private void ResetShipMenuGui(bool body)
	{
		if (body)
		{
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_CostLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_DescriptionLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_FlavorLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_OfficialNameLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = string.Empty;
			return;
		}
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = string.Empty;
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_OfficialNameLabel").GetComponent<SpriteText>().Text = string.Empty;
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_SizeLabel").GetComponent<SpriteText>().Text = string.Empty;
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_CostLabel").GetComponent<SpriteText>().Text = string.Empty;
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_DescriptionLabel").GetComponent<SpriteText>().Text = string.Empty;
		GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_FlavorLabel").GetComponent<SpriteText>().Text = string.Empty;
		SimpleSprite component = GuiUtils.FindChildOf(m_infoArmament.transform, "ShipInfoPanel_Image").GetComponent<SimpleSprite>();
		component.gameObject.SetActiveRecursively(state: false);
		for (int i = 1; i <= 8; i++)
		{
			string name = "ShipInfoPanel_Stats" + i + "Label";
			string name2 = "ShipInfoPanel_Stats" + i + "Value";
			GuiUtils.FindChildOf(m_infoArmament.transform, name).GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_infoArmament.transform, name2).GetComponent<SpriteText>().Text = string.Empty;
		}
	}

	public void SpawnMarkers(string type, List<Vector3> points, Quaternion rotation)
	{
		Texture2D texture = null;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		switch (type)
		{
		case "used":
			texture = Resources.Load("shipeditor/hardpoint_red_diffuse") as Texture2D;
			vector = new Vector3(0f, 0f, 0f);
			break;
		case "all":
			texture = Resources.Load("shipeditor/hardpoint_green_diffuse") as Texture2D;
			vector = new Vector3(0f, 0.01f, 0f);
			break;
		case "offensive":
			texture = Resources.Load("shipeditor/hardpoint_yellow_diffuse") as Texture2D;
			vector = new Vector3(0f, 0.01f, 0f);
			break;
		case "defensive":
			texture = Resources.Load("shipeditor/hardpoint_blue_diffuse") as Texture2D;
			vector = new Vector3(0f, 0.01f, 0f);
			break;
		}
		foreach (Vector3 point in points)
		{
			GameObject gameObject = ObjectFactory.instance.Create("HpMarkerShow", point + vector, rotation);
			gameObject.transform.parent = markerBase.transform;
			gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
		}
	}

	public void SpawnBatteryMarkers()
	{
		for (int i = 0; i < markerBase.transform.GetChildCount(); i++)
		{
			Object.Destroy(markerBase.transform.GetChild(i).gameObject);
		}
		Battery[] componentsInChildren = m_editShip.m_ship.GetComponentsInChildren<Battery>();
		Quaternion quaternion = default(Quaternion);
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.GetUnusedTiles(out var points);
			quaternion = battery.transform.rotation;
			SpawnMarkers("used", points, quaternion);
		}
		Battery[] array2 = componentsInChildren;
		foreach (Battery battery2 in array2)
		{
			battery2.GetFitTiles(out var points2, m_dragWidth, m_dragHeight, fillall: true);
			quaternion = battery2.transform.rotation;
			string type = "all";
			if (battery2.m_allowDefensive && !battery2.m_allowOffensive)
			{
				type = "defensive";
			}
			else if (!battery2.m_allowDefensive && battery2.m_allowOffensive)
			{
				type = "offensive";
			}
			if (m_dragType == HPModule.HPModuleType.Defensive && !battery2.m_allowDefensive)
			{
				type = "used";
			}
			if (m_dragType == HPModule.HPModuleType.Offensive && !battery2.m_allowOffensive)
			{
				type = "used";
			}
			SpawnMarkers(type, points2, quaternion);
		}
	}

	private void ResetBatteryMarkers()
	{
		m_dragWidth = 1;
		m_dragHeight = 1;
		m_dragType = HPModule.HPModuleType.Any;
		SpawnBatteryMarkers();
		GuiUtils.FindChildOf(m_gui.transform, "MaxArmsPanel").GetComponent<UIPanel>().Dismiss();
	}

	private void Zoom(float zoomDelta, float dt)
	{
		if (Time.time < m_allowZoomTime || zoomDelta == 0f)
		{
			return;
		}
		if (zoomDelta > 0f)
		{
			m_camsize -= 75f * zoomDelta * m_camsize * 0.0002f;
			if (m_camsize < m_zoomMin)
			{
				m_camsize = m_zoomMin;
			}
		}
		else if (zoomDelta < 0f)
		{
			m_camsize += 75f * (0f - zoomDelta) * m_camsize * 0.0002f;
			if (m_camsize > m_zoomMax)
			{
				m_camsize = m_zoomMax;
			}
		}
		Camera component = m_sceneCamera.GetComponent<Camera>();
		component.orthographicSize = m_camsize;
	}

	public void CreateInformationList(GameObject panel, Dictionary<string, string> dict)
	{
		foreach (KeyValuePair<string, string> item in dict)
		{
			SetLine(GenerateLine(panel), item.Key, item.Value);
		}
	}

	public GameObject GenerateLine(GameObject root)
	{
		GameObject original = Resources.Load("gui/ShipEditorInfoLine") as GameObject;
		Vector3 vector = new Vector3(0f, 0f, -4f);
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		gameObject.transform.parent = root.transform;
		gameObject.transform.localPosition = m_listPost + vector;
		m_listPost += new Vector3(0f, -20f, 0f);
		return gameObject;
	}

	public void SetLine(GameObject line, string text, string value)
	{
		GuiUtils.FindChildOf(line, "Text").GetComponent<SpriteText>().Text = Localize.instance.Translate(text);
		GuiUtils.FindChildOf(line, "Value").GetComponent<SpriteText>().Text = Localize.instance.Translate(value);
	}

	public void RemoveChildren(GameObject obj)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			Transform child = obj.transform.GetChild(i);
			Object.Destroy(child.gameObject);
		}
		m_listPost = new Vector3(0f, 0f, 0f);
	}

	private void SetInformationName(string prefabName)
	{
		GameObject prefab = ObjectFactory.instance.GetPrefab(prefabName);
		if ((bool)prefab)
		{
			m_part = prefabName;
			ShowInfoPanel(m_infoBody);
			SetInformation(prefab);
		}
	}

	private string ColorCodedString(float value)
	{
		if (value == 0f)
		{
			return value.ToString();
		}
		if (value > 0f)
		{
			return Constants.m_buffColor + value;
		}
		return Constants.m_nerfColor + value;
	}

	public void SetInformation(GameObject go)
	{
		if (go == null)
		{
			ShowInfoPanel(m_infoArmament);
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_OfficialNameLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_SizeLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_CostLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_DescriptionLabel").GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_FlavorLabel").GetComponent<SpriteText>().Text = string.Empty;
			for (int i = 1; i <= 8; i++)
			{
				string name = "ShipInfoPanel_Stats" + i + "Label";
				string name2 = "ShipInfoPanel_Stats" + i + "Value";
				GuiUtils.FindChildOf(m_panelShipInfo.transform, name).GetComponent<SpriteText>().Text = string.Empty;
				GuiUtils.FindChildOf(m_panelShipInfo.transform, name2).GetComponent<SpriteText>().Text = string.Empty;
			}
			SimpleSprite component = GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_Image").GetComponent<SimpleSprite>();
			component.gameObject.SetActiveRecursively(state: false);
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_Stats_bg").SetActiveRecursively(state: false);
			return;
		}
		HPModule component2 = go.GetComponent<HPModule>();
		if ((bool)component2)
		{
			ShowInfoPanel(m_infoArmament);
			HPModuleSettings module = ComponentDB.instance.GetModule(component2.name);
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_name");
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_OfficialNameLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_productname");
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_SizeLabel").GetComponent<SpriteText>().Text = component2.GetWidth() + "x" + component2.GetLength();
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_CostLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(module.m_value + " $label_pointssmall");
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_DescriptionLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_details");
			GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_FlavorLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_flavor");
			SimpleSprite component3 = GuiUtils.FindChildOf(m_panelShipInfo.transform, "ShipInfoPanel_Image").GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component3, "EquipmentImages/EquipmentImg_" + m_part);
			component3.gameObject.SetActiveRecursively(state: true);
			Dictionary<string, string> shipEditorInfo = go.GetComponent<HPModule>().GetShipEditorInfo();
			int num = 1;
			foreach (KeyValuePair<string, string> item in shipEditorInfo)
			{
				string name3 = "ShipInfoPanel_Stats" + num + "Label";
				string name4 = "ShipInfoPanel_Stats" + num + "Value";
				GuiUtils.FindChildOf(m_panelShipInfo.transform, name3).GetComponent<SpriteText>().Text = Localize.instance.Translate(item.Key);
				GuiUtils.FindChildOf(m_panelShipInfo.transform, name4).GetComponent<SpriteText>().Text = Localize.instance.Translate(item.Value);
				num++;
			}
		}
		Section component4 = go.GetComponent<Section>();
		if ((bool)component4)
		{
			GameObject gameObject = null;
			gameObject = ((component4.GetSectionType() != Section.SectionType.Top) ? ShowInfoPanel(m_infoBody) : ShowInfoPanel(m_infoBodyTop));
			SectionSettings section = ComponentDB.instance.GetSection(component4.name);
			GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_NameLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_name");
			GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_OfficialNameLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_productname");
			GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_CostLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(section.m_value + " $label_pointssmall");
			GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_DescriptionLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_details");
			GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_FlavorLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + m_part + "_flavor");
			if (component4.GetSectionType() == Section.SectionType.Top)
			{
				GuiUtils.FindChildOf(gameObject.transform, "ShipInfoPanel_SightStatValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_sightRange);
				SetDotsAllOn(m_infoBody, "SightDot");
				SetDots(m_infoBody, "SightDot", component4.m_rating.m_sight);
				return;
			}
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_ArmorStatValue").GetComponent<SpriteText>().Text = component4.m_armorClass.ToString();
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_HealthStatValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_maxHealth);
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_ForwardMaxSpeedValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_speed);
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_ForwardAccValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_acceleration);
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_ReverseMaxSpeedValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_reverseSpeed);
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_ReverseAccValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_reverseAcceleration);
			GuiUtils.FindChildOf(m_infoBody.transform, "ShipInfoPanel_TurnSpeedValue").GetComponent<SpriteText>().Text = ColorCodedString(component4.m_modifiers.m_turnSpeed);
			SetDotsAllOn(m_infoBody, "HealthDot");
			SetDotsAllOn(m_infoBody, "ArmorDot");
			SetDotsAllOn(m_infoBody, "SpeedDot");
			SetDots(m_infoBody, "HealthDot", component4.m_rating.m_health);
			SetDots(m_infoBody, "ArmorDot", component4.m_rating.m_armor);
			SetDots(m_infoBody, "SpeedDot", component4.m_rating.m_speed);
		}
	}

	private bool IsDeveloper()
	{
		return Application.isEditor;
	}

	public void SetDragObject(string partName)
	{
		if (m_dragObject != null)
		{
			Object.Destroy(m_dragObject);
			if (m_dragObjectIcon != null)
			{
				Object.Destroy(m_dragObjectIcon);
			}
		}
		m_dragObject = null;
		if (partName.Length != 0)
		{
			Quaternion rot = default(Quaternion);
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = 50f;
			m_dragObject = ObjectFactory.instance.Create(partName, mousePosition, rot);
			if (!(m_dragObject == null))
			{
				m_dragObject.GetComponent<BoxCollider>().enabled = false;
				PrepareDraging(Input.mousePosition, Input.mousePosition, m_dragObject);
				StartDraging(Input.mousePosition);
				HPModule component = m_dragObject.GetComponent<HPModule>();
				if (m_currentDir == Direction.Forward || m_currentDir == Direction.Backward)
				{
					m_dragWidth = component.GetWidth();
					m_dragHeight = component.GetLength();
				}
				else
				{
					m_dragWidth = component.GetLength();
					m_dragHeight = component.GetWidth();
				}
				m_dragType = component.m_type;
				SetInformation(m_dragObject);
				if (component.m_type == HPModule.HPModuleType.Defensive)
				{
					m_dragObjectIcon = GuiUtils.CreateGui("Shipyard/ArmsDefensivePickedUp", m_guiCamera);
				}
				else
				{
					m_dragObjectIcon = GuiUtils.CreateGui("Shipyard/ArmsOffensivePickedUp", m_guiCamera);
				}
				SimpleSprite component2 = GuiUtils.FindChildOf(m_dragObjectIcon, "ArmamentListItemThumbnail").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component2, GuiUtils.GetArmamentThumbnail(partName));
				GuiUtils.FindChildOf(m_dragObjectIcon, "ArmamentListItemSize").GetComponent<SpriteText>().Text = component.m_width + "X" + component.m_length;
				m_dragObjectIcon.SetActiveRecursively(state: false);
				if (!IsDeveloper() && m_portShipDef.NumberOfHardpoints() >= m_maxHardpoints)
				{
					m_dragWidth = 128;
					m_dragHeight = 128;
					GuiUtils.FindChildOf(m_gui.transform, "MaxArmsPanel").GetComponent<UIPanel>().BringIn();
				}
				SpawnBatteryMarkers();
			}
		}
		else
		{
			m_dragWidth = 1;
			m_dragHeight = 1;
		}
	}

	private void PrepareDraging(Vector3 hitPos, Vector3 mousePos, GameObject go)
	{
		SetSelectedHpModule(null);
		m_dragStart = hitPos;
		m_dragStartMousePos = mousePos;
		m_dragObject = go;
	}

	private void StartDraging(Vector3 mousePos)
	{
		m_draging = true;
		m_dragLastPos = mousePos;
		OnDragStarted(m_dragStart, m_dragStartMousePos, m_dragObject);
	}

	private void OnMouseReleased(Vector3 pos, GameObject go)
	{
	}

	private void OnDragStarted(Vector3 pos, Vector3 mousePos, GameObject go)
	{
	}

	private void ListPartsDelegate(ref POINTER_INFO ptr)
	{
	}

	private bool IsInsideHardPointList(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "ShipEquipmentPanel");
		if (go.transform.position.x > gameObject.transform.position.x + 280f)
		{
			return false;
		}
		return true;
	}

	private GameObject PlayEffect(string name, Vector3 position)
	{
		GameObject original = Resources.Load(name) as GameObject;
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		gameObject.transform.position = position;
		return gameObject;
	}

	private void OnDragUpdate(Vector3 startPos, Vector3 pos, Vector3 mouseDelta, GameObject go)
	{
		if (m_dragObject == null)
		{
			Vector3 vector = -mouseDelta * m_mouseMoveSpeed * 50f;
			m_sceneCamera.transform.localPosition += new Vector3(vector.x, 0f, vector.y);
			Vector3 vector2 = m_sceneCamera.transform.localPosition - portShip.transform.position;
			if (vector2.x < -10f)
			{
				vector2.x = -10f;
			}
			if (vector2.x > 10f)
			{
				vector2.x = 10f;
			}
			if (vector2.z < -10f)
			{
				vector2.z = -10f;
			}
			if (vector2.z > 10f)
			{
				vector2.z = 10f;
			}
			m_sceneCamera.transform.localPosition = portShip.transform.position + vector2;
			return;
		}
		pos.z = 50f;
		Vector3 position = m_sceneCamera.camera.ScreenToWorldPoint(pos);
		m_dragObject.transform.position = position;
		if ((bool)m_dragObjectIcon)
		{
			pos.z = -10f;
			m_dragObjectIcon.transform.position = m_guiCamera.camera.ScreenToWorldPoint(pos);
		}
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "ShipEquipmentPanel");
		if (IsInsideHardPointList(m_dragObjectIcon))
		{
			m_dragObjectIcon.SetActiveRecursively(state: true);
			m_dragObject.SetActiveRecursively(state: false);
		}
		else
		{
			if (!m_dragObject.active)
			{
				PlayEffect("GUI_effects/GUI_smoke", m_dragObject.transform.position);
			}
			m_dragObjectIcon.SetActiveRecursively(state: false);
			m_dragObject.SetActiveRecursively(state: true);
		}
		SnapDragObject2();
	}

	private void OnDragStoped(Vector3 pos, GameObject go)
	{
		SetDragObject(string.Empty);
		if (!IsInsideHardPointList(m_dragObjectIcon))
		{
			SetInformation(null);
		}
	}

	private float GetPinchDistance()
	{
		if (Input.touchCount == 2)
		{
			return Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
		}
		return 0f;
	}

	private void DragSelected()
	{
		if (!(m_selectedHPModule == null))
		{
			PlayEffect("GUI_effects/GUI_smoke", m_selectedHPModule.transform.position);
			string name = m_selectedHPModule.name;
			RemovePart(m_selectedHPModule);
			SetSelectedHpModule(null);
			SetPart(name, refreshship: false);
		}
	}

	private void UpdateMotion()
	{
		if (m_saveDialog != null)
		{
			return;
		}
		UIManager component = m_guiCamera.GetComponent<UIManager>();
		bool flag = component.DidAnyPointerHitUI();
		if (Input.touchCount <= 1)
		{
			if (!m_draging)
			{
				if (flag)
				{
					return;
				}
				if (Input.GetMouseButtonDown(0))
				{
					if ((bool)m_selectedHPModule && m_selectedHPModule == GetHardpointAtMouse())
					{
						DragSelected();
						return;
					}
					PrepareDraging(Input.mousePosition, Input.mousePosition, null);
				}
				if (Input.GetMouseButton(0))
				{
					float num = Vector3.Distance(m_dragStartMousePos, Input.mousePosition);
					if (num > 10f)
					{
						StartDraging(Input.mousePosition);
					}
				}
				if (Input.GetMouseButtonUp(0))
				{
					OnMouseReleased(Input.mousePosition, null);
				}
			}
			else
			{
				if (Input.GetMouseButton(0))
				{
					Vector3 mouseDelta = Input.mousePosition - m_dragLastPos;
					m_dragLastPos = Input.mousePosition;
					OnDragUpdate(m_dragStart, MousePosition(), mouseDelta, m_dragObject);
				}
				if (Input.GetMouseButtonUp(0) && m_draging)
				{
					m_draging = false;
					DropModule2();
					OnDragStoped(Input.mousePosition, m_dragObject);
				}
			}
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		Zoom(axis * 100f, Time.deltaTime);
		if (Input.touchCount == 2 && !m_draging)
		{
			float pinchDistance = GetPinchDistance();
			if (!m_pinchZoom)
			{
				m_pinchZoom = true;
				m_pinchStartDistance = pinchDistance;
			}
			else
			{
				float num2 = (0f - (m_pinchStartDistance - pinchDistance)) / 5f;
				m_pinchStartDistance = pinchDistance;
				Zoom(num2 * 10f, Time.deltaTime);
			}
		}
		if (m_pinchZoom && Input.touchCount < 2)
		{
			m_pinchZoom = false;
		}
	}

	public void SnapDragObject()
	{
		m_dragObject.transform.rotation = Battery.GetRotation(m_currentDir);
		int layerMask = (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("EDITOR ONLY"));
		GameObject gameObject = GameObject.Find("MainCamera");
		Ray ray = gameObject.camera.ScreenPointToRay(MousePosition());
		if (Physics.Raycast(ray, out var hitInfo, 10000f, layerMask) && !(hitInfo.collider.gameObject.name != "visual"))
		{
			Battery component = hitInfo.collider.gameObject.transform.parent.gameObject.GetComponent<Battery>();
			component.WorldToTile(hitInfo.point, out var x, out var y);
			if (component.CanPlaceAt(x, y, m_dragWidth, m_dragHeight, null))
			{
				m_dragObject.transform.position = component.transform.position + component.GetModulePosition(x, y, m_dragWidth, m_dragHeight);
				m_dragObject.transform.rotation = m_dragObject.transform.rotation * component.transform.rotation;
			}
		}
	}

	public void SnapDragObject2()
	{
		HPModule component = m_dragObject.GetComponent<HPModule>();
		m_dragObject.transform.rotation = Battery.GetRotation(m_currentDir);
		Vector3 position = MousePosition();
		position.z = 50f;
		Vector3 vector = m_sceneCamera.camera.ScreenToWorldPoint(position);
		m_dragObject.transform.position = vector;
		Battery[] componentsInChildren = portShip.GetComponentsInChildren<Battery>();
		float num = 500000f;
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			if (!battery.AllowedModule(component))
			{
				continue;
			}
			battery.GetFitTiles(out var points, m_dragWidth, m_dragHeight, fillall: false);
			foreach (Vector3 item in points)
			{
				vector.y = item.y;
				Vector3 vector2 = item - vector;
				if (vector2.magnitude < num)
				{
					num = vector2.magnitude;
					m_dropPosition = item;
					m_dropBattery = battery;
				}
			}
		}
		if (m_dropBattery == null)
		{
			return;
		}
		if (num > 5f)
		{
			m_dropBattery = null;
			return;
		}
		m_dropBattery.WorldToTile(m_dropPosition, out var x, out var y);
		if (!m_dropBattery.CanPlaceAt(x, y, m_dragWidth, m_dragHeight, null))
		{
			m_dropBattery = null;
			return;
		}
		m_dragObject.transform.position = m_dropBattery.GetWorldPlacePos(x, y, component);
		m_dragObject.transform.rotation = m_dragObject.transform.rotation * m_dropBattery.transform.rotation;
	}

	public void DropModule2()
	{
		if (m_portShipDef.NumberOfHardpoints() >= m_maxHardpoints)
		{
			if (!IsDeveloper())
			{
				ResetBatteryMarkers();
				return;
			}
			PLog.LogWarning("To many hardpoints on boat. This will not be a valid Playerboat.");
		}
		if (m_dropBattery == null)
		{
			if ((bool)m_dragObject)
			{
				PlayEffect("GUI_effects/GUI_Watersplash", m_dragObject.transform.position);
			}
			ResetBatteryMarkers();
			return;
		}
		m_dropBattery.WorldToTile(m_dropPosition, out var x, out var y);
		HpPosition hpPosition = new HpPosition();
		hpPosition.m_OrderNum = m_dropBattery.GetOrderNumber();
		hpPosition.m_sectionType = m_dropBattery.GetSectionType();
		hpPosition.m_gridPosition.x = x;
		hpPosition.m_gridPosition.y = y;
		m_dragWidth = 1;
		m_dragHeight = 1;
		m_dragType = HPModule.HPModuleType.Any;
		AddPart(m_part, m_dropBattery.gameObject, x, y, m_currentDir, verifyPosition: true);
		SetSelectedHpModule(GetHPModule(hpPosition));
		PlayEffect("GUI_effects/GUI_sparks", m_dropPosition);
	}

	public void DropModule()
	{
		PLog.Log("DropModule 1");
		int layerMask = (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("EDITOR ONLY"));
		GameObject gameObject = GameObject.Find("Camera");
		Ray ray = gameObject.camera.ScreenPointToRay(MousePosition());
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(ray, out hitInfo, 10000f, layerMask);
		Color color = new Color(1f, 0f, 0.5f, 1f);
		if (!flag)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10000f);
			return;
		}
		Debug.DrawLine(ray.origin, hitInfo.point);
		Debug.DrawLine(hitInfo.point, ray.origin + ray.direction * 10000f, color);
		PLog.Log("DropModule 2: " + hitInfo.collider.gameObject.name);
		if (!(hitInfo.collider.gameObject.name != "visual"))
		{
			PLog.Log("DropModule 3");
			Battery component = hitInfo.collider.gameObject.transform.parent.gameObject.GetComponent<Battery>();
			component.WorldToTile(hitInfo.point, out var x, out var y);
			HpPosition hpPosition = new HpPosition();
			hpPosition.m_OrderNum = component.GetOrderNumber();
			hpPosition.m_sectionType = component.GetSectionType();
			hpPosition.m_gridPosition.x = x;
			hpPosition.m_gridPosition.y = y;
			AddPart(m_part, hitInfo.collider.gameObject.transform.parent.gameObject, x, y, m_currentDir, verifyPosition: true);
			SetSelectedHpModule(GetHPModule(hpPosition));
		}
	}

	private GameObject GetHardpointAtMouse()
	{
		int layerMask = (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("EDITOR ONLY"));
		GameObject gameObject = GameObject.Find("MainCamera");
		Ray ray = gameObject.camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(ray, out hitInfo, 10000f, layerMask);
		Color color = new Color(1f, 0f, 0.5f, 1f);
		if (!flag)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10000f);
			return null;
		}
		Debug.DrawLine(ray.origin, hitInfo.point);
		Debug.DrawLine(hitInfo.point, ray.origin + ray.direction * 10000f, color);
		GameObject result = null;
		if (hitInfo.collider.gameObject.name != "visual")
		{
			result = hitInfo.collider.gameObject;
		}
		return result;
	}

	public void Update()
	{
		UpdateMotion();
		GameObject hardpointAtMouse = GetHardpointAtMouse();
		if ((bool)hardpointAtMouse && Input.GetMouseButtonUp(0))
		{
			if ((bool)m_selectedHPModule)
			{
				PLog.Log("Should dragn drop");
			}
			PLog.Log("Eggg dragn drop");
			SetSelectedHpModule(hardpointAtMouse);
		}
		if (m_msgBox != null)
		{
			m_msgBox.Update();
		}
	}

	private SectionDef GetSectionDef(Section.SectionType type, bool recreate)
	{
		switch (type)
		{
		case Section.SectionType.Front:
			if (recreate)
			{
				m_portShipDef.m_frontSection = new SectionDef();
			}
			return m_portShipDef.m_frontSection;
		case Section.SectionType.Mid:
			if (recreate)
			{
				m_portShipDef.m_midSection = new SectionDef();
			}
			return m_portShipDef.m_midSection;
		case Section.SectionType.Rear:
			if (recreate)
			{
				m_portShipDef.m_rearSection = new SectionDef();
			}
			return m_portShipDef.m_rearSection;
		default:
			if (recreate)
			{
				m_portShipDef.m_topSection = new SectionDef();
			}
			return m_portShipDef.m_topSection;
		}
	}

	private string RemovePart(GameObject go)
	{
		Battery component = go.transform.parent.GetComponent<Battery>();
		HPModule component2 = go.GetComponent<HPModule>();
		Vector2i gridPos = component2.GetGridPos();
		Section component3 = component.transform.parent.GetComponent<Section>();
		SectionDef sectionDef = GetSectionDef(component3.m_type, recreate: false);
		sectionDef.RemoveModule(component.GetOrderNumber(), gridPos);
		SpawnPortShip();
		return string.Empty;
	}

	private void AddPart(string name, GameObject go, int x, int y, Direction dir, bool verifyPosition)
	{
		Battery component = go.GetComponent<Battery>();
		GameObject prefab = ObjectFactory.instance.GetPrefab(name);
		if (prefab == null)
		{
			PLog.LogError("Missing prefab " + name);
			return;
		}
		HPModule component2 = prefab.GetComponent<HPModule>();
		if (!verifyPosition || component.CanPlaceAt(x, y, m_dragWidth, m_dragHeight, null))
		{
			Section component3 = component.transform.parent.GetComponent<Section>();
			SectionDef sectionDef = GetSectionDef(component3.m_type, recreate: false);
			sectionDef.m_modules.Add(new ModuleDef(name, component.GetOrderNumber(), new Vector2i(x, y), dir));
			SpawnPortShip();
		}
	}

	private List<string> GetAvailableShips()
	{
		List<string> availableShips = m_userManClient.GetAvailableShips(m_campaignID);
		List<string> list = new List<string>();
		foreach (string item in availableShips)
		{
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			if (!(prefab == null))
			{
				Ship component = prefab.GetComponent<Ship>();
				if (!(component == null) && (IsDeveloper() || component.m_editByPlayer) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public void Close()
	{
		CloseDialog();
		SetDragObject(string.Empty);
		UnRegisterDelegatesToComponents();
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "BodyBowScrollList");
		gameObject.GetComponent<UIScrollList>().ClearList(destroy: true);
		GameObject gameObject2 = GuiUtils.FindChildOf(m_gui.transform, "BodyMidScrollList");
		gameObject2.GetComponent<UIScrollList>().ClearList(destroy: true);
		GameObject gameObject3 = GuiUtils.FindChildOf(m_gui.transform, "BodySternScrollList");
		gameObject3.GetComponent<UIScrollList>().ClearList(destroy: true);
		GameObject gameObject4 = GuiUtils.FindChildOf(m_gui.transform, "BodyTopScrollList");
		gameObject4.GetComponent<UIScrollList>().ClearList(destroy: true);
		GameObject gameObject5 = GuiUtils.FindChildOf(m_gui.transform, "ArmsDefensiveScrollList");
		gameObject5.GetComponent<UIScrollList>().ClearList(destroy: true);
		GameObject gameObject6 = GuiUtils.FindChildOf(m_gui.transform, "ArmsOffensiveScrollList");
		gameObject6.GetComponent<UIScrollList>().ClearList(destroy: true);
		m_panelFleetTop.BringIn();
		m_panelShipBrowser.BringIn();
		m_panelShipName.Dismiss();
		m_panelShipInfo.Dismiss();
		m_panelShipEquipment.Dismiss();
		if (portShip != null)
		{
			Object.Destroy(portShip);
		}
		GameObject gameObject7 = GameObject.Find("Main");
		DebugUtils.Assert(m_sceneCamera != null, "Failed to find Main viewpoint");
	}

	private List<string> GetAvailableHPModules()
	{
		List<string> availableHPModules = m_userManClient.GetAvailableHPModules(m_campaignID);
		List<string> list = new List<string>();
		foreach (string item in availableHPModules)
		{
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			if (!(prefab == null))
			{
				HPModule component = prefab.GetComponent<HPModule>();
				if (!(component == null) && (IsDeveloper() || component.m_editByPlayer))
				{
					list.Add(item);
				}
			}
		}
		HashSet<string> collection = new HashSet<string>(list);
		return new List<string>(collection);
	}

	private List<string> GetFilteredHPModules(HPModule.HPModuleType filterType)
	{
		List<string> availableHPModules = GetAvailableHPModules();
		if (availableHPModules == null)
		{
			PLog.Log("No HPModules are listed by the userManClient.");
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (string item in availableHPModules)
		{
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			if (prefab != null)
			{
				HPModule component = prefab.GetComponent<HPModule>();
				if (component.m_type == filterType)
				{
					list.Add(item);
				}
			}
			else
			{
				PLog.LogWarning("HPModule prefab do not exist." + item);
				list.Add(item);
			}
		}
		return list;
	}

	private List<string> GetFilteredSections(Section.SectionType filterType)
	{
		List<string> availableSections = m_userManClient.GetAvailableSections(m_campaignID);
		List<string> list = new List<string>();
		if (availableSections == null)
		{
			PLog.Log("No Sections are listed by the userManClient.");
			return new List<string>();
		}
		foreach (string item in availableSections)
		{
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			if (prefab != null)
			{
				Section component = prefab.GetComponent<Section>();
				if (m_shipSeries == component.m_series && component.m_type == filterType)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private void SetDotsAllOn(GameObject bodyItem, string name)
	{
		for (int i = 1; i <= 5; i++)
		{
			string name2 = name + i;
			GameObject gameObject = GuiUtils.FindChildOf(bodyItem, name2);
			if (gameObject == null)
			{
				break;
			}
			SimpleSprite component = gameObject.GetComponent<SimpleSprite>();
			string name3 = string.Empty;
			if (name == "HealthDot")
			{
				name3 = "ShipyardBodyLamps/gui_shipyard_bodylamp_on_health";
			}
			if (name == "ArmorDot")
			{
				name3 = "ShipyardBodyLamps/gui_shipyard_bodylamp_on_armor";
			}
			if (name == "SpeedDot")
			{
				name3 = "ShipyardBodyLamps/gui_shipyard_bodylamp_on_speed";
			}
			GuiUtils.SetImage(component, name3);
		}
	}

	private void SetDots(GameObject bodyItem, string name, int level)
	{
		for (int i = level; i <= 5; i++)
		{
			string name2 = name + i;
			GameObject gameObject = GuiUtils.FindChildOf(bodyItem, name2);
			if (gameObject == null)
			{
				break;
			}
			SimpleSprite component = gameObject.GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component, "ShipyardBodyLamps/gui_shipyard_bodylamp_off");
		}
	}

	public void FillScrollList(UIScrollList list, List<string> content, string prefabName, bool hardpoints)
	{
		UIScrollList component = GuiUtils.FindChildOf(m_gui.transform, "ShipBrowserClassScrollList").GetComponent<UIScrollList>();
		GameObject original = Resources.Load("gui/Shipyard/" + prefabName) as GameObject;
		foreach (string item in content)
		{
			GameObject gameObject = Object.Instantiate(original) as GameObject;
			GuiUtils.LocalizeGui(gameObject);
			if (hardpoints)
			{
				GUI_Blueprint_Part component2 = GuiUtils.FindChildOf(gameObject, prefabName + "Button").GetComponent<GUI_Blueprint_Part>();
				component2.Initialize(item, this);
				GuiUtils.FindChildOf(gameObject, "InfoButton").GetComponent<GUI_Blueprint_Part>().Initialize(item, this);
				SpriteText component3 = GuiUtils.FindChildOf(gameObject, "ArmamentListItemHeader").GetComponent<SpriteText>();
				GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext1").GetComponent<SpriteText>().Text = string.Empty;
				GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext2").GetComponent<SpriteText>().Text = string.Empty;
				GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext3").GetComponent<SpriteText>().Text = string.Empty;
				GameObject prefab = ObjectFactory.instance.GetPrefab(item);
				HPModule component4 = prefab.GetComponent<HPModule>();
				List<string> hardpointInfo = component4.GetHardpointInfo();
				HPModuleSettings module = ComponentDB.instance.GetModule(item);
				GuiUtils.FindChildOf(gameObject, "ArmamentListItemSize").GetComponent<SpriteText>().Text = component4.m_width + "X" + component4.m_length;
				SimpleSprite component5 = GuiUtils.FindChildOf(gameObject, "ArmamentListItemThumbnail").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component5, "ArmamentThumbnails/ArmamentThumb_" + item);
				component3.Text = Localize.instance.TranslateRecursive("$" + item + "_name");
				GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext1").GetComponent<SpriteText>().Text = module.m_value + Localize.instance.Translate(" $label_pointssmall");
				if (hardpointInfo.Count >= 1)
				{
					GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext2").GetComponent<SpriteText>().Text = hardpointInfo[0];
				}
				if (hardpointInfo.Count >= 2)
				{
					GuiUtils.FindChildOf(gameObject, "ArmamentListItemSubtext3").GetComponent<SpriteText>().Text = hardpointInfo[1];
				}
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "InfoButton").GetComponent<GUI_Blueprint_Part>().Initialize(item, this);
				SpriteText component6 = GuiUtils.FindChildOf(gameObject, "BodyListItemHeader").GetComponent<SpriteText>();
				GameObject prefab2 = ObjectFactory.instance.GetPrefab(item);
				Section component7 = prefab2.GetComponent<Section>();
				component6.Text = Localize.instance.TranslateRecursive("$" + item + "_name");
				SectionSettings section = ComponentDB.instance.GetSection(item);
				GuiUtils.FindChildOf(gameObject, "BodyListItemCost").GetComponent<SpriteText>().Text = section.m_value + Localize.instance.Translate(" $label_pointssmall");
				SetDots(gameObject, "HealthDot", component7.m_rating.m_health);
				SetDots(gameObject, "ArmorDot", component7.m_rating.m_armor);
				SetDots(gameObject, "SpeedDot", component7.m_rating.m_speed);
				SetDots(gameObject, "SightDot", component7.m_rating.m_sight);
				GameObject gameObject2 = GuiUtils.FindChildOf(gameObject, "BodyListItemCheckmark");
				GUI_Blueprint_Part gUI_Blueprint_Part = gameObject2.AddComponent<GUI_Blueprint_Part>();
				gUI_Blueprint_Part.Initialize(item, this);
				UIRadioBtn component8 = gameObject2.GetComponent<UIRadioBtn>();
				component8.useParentForGrouping = false;
				component8.SetGroup(m_buttonGroup);
				component8.scriptWithMethodToInvoke = gUI_Blueprint_Part;
				component8.methodToInvoke = "OnPress";
				component8.whenToInvoke = POINTER_INFO.INPUT_EVENT.PRESS;
				if (item == m_portShipDef.m_frontSection.m_prefab)
				{
					component8.Value = true;
				}
				if (item == m_portShipDef.m_midSection.m_prefab)
				{
					component8.Value = true;
				}
				if (item == m_portShipDef.m_rearSection.m_prefab)
				{
					component8.Value = true;
				}
				if (item == m_portShipDef.m_topSection.m_prefab)
				{
					component8.Value = true;
				}
			}
			list.AddItem(gameObject);
		}
		m_buttonGroup++;
	}

	public void FillPartsList()
	{
		m_buttonGroup = 0;
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "BodyBowScrollList");
		gameObject.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject.GetComponent<UIScrollList>(), GetFilteredSections(Section.SectionType.Front), "BodyListItem", hardpoints: false);
		GameObject gameObject2 = GuiUtils.FindChildOf(m_gui.transform, "BodyMidScrollList");
		gameObject2.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject2.GetComponent<UIScrollList>(), GetFilteredSections(Section.SectionType.Mid), "BodyListItem", hardpoints: false);
		GameObject gameObject3 = GuiUtils.FindChildOf(m_gui.transform, "BodySternScrollList");
		gameObject3.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject3.GetComponent<UIScrollList>(), GetFilteredSections(Section.SectionType.Rear), "BodyListItem", hardpoints: false);
		GameObject gameObject4 = GuiUtils.FindChildOf(m_gui.transform, "BodyTopScrollList");
		gameObject4.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject4.GetComponent<UIScrollList>(), GetFilteredSections(Section.SectionType.Top), "BodyListItem_Top", hardpoints: false);
		GameObject gameObject5 = GuiUtils.FindChildOf(m_gui.transform, "ArmsDefensiveScrollList");
		gameObject5.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject5.GetComponent<UIScrollList>(), GetFilteredHPModules(HPModule.HPModuleType.Defensive), "ArmsDefensiveListItem", hardpoints: true);
		GameObject gameObject6 = GuiUtils.FindChildOf(m_gui.transform, "ArmsOffensiveScrollList");
		gameObject6.GetComponent<UIScrollList>().ClearList(destroy: true);
		FillScrollList(gameObject6.GetComponent<UIScrollList>(), GetFilteredHPModules(HPModule.HPModuleType.Offensive), "ArmsOffensiveListItem", hardpoints: true);
	}

	public void ShowInfo(string partName)
	{
		PLog.Log("ShowInfo: " + partName);
		m_part = partName;
		Quaternion rot = default(Quaternion);
		Vector3 mousePosition = Input.mousePosition;
		GameObject gameObject = ObjectFactory.instance.Create(partName, mousePosition, rot);
		SetInformation(gameObject);
		Object.Destroy(gameObject);
	}

	public void SetPart(string partName, bool refreshship)
	{
		GameObject prefab = ObjectFactory.instance.GetPrefab(partName);
		if (prefab.GetComponent<Section>() != null)
		{
			if (prefab == null)
			{
				PLog.LogError("Missing prefab " + partName);
				return;
			}
			Section.SectionType type = prefab.GetComponent<Section>().m_type;
			GetSectionDef(type, recreate: true).m_prefab = partName;
			if (refreshship)
			{
				SpawnPortShip();
			}
			GameObject prefab2 = ObjectFactory.instance.GetPrefab(partName);
			if ((bool)prefab2)
			{
				m_part = partName;
				ShowInfoPanel(m_infoBody);
				SetInformation(prefab2);
			}
		}
		else
		{
			m_part = partName;
			SetSelectedHpModule(null);
			SetDragObject(m_part);
		}
	}

	private void SetShowAllViewCones(bool show)
	{
		Gun[] componentsInChildren = portShip.GetComponentsInChildren<Gun>();
		Gun[] array = componentsInChildren;
		foreach (Gun gun in array)
		{
			if (show)
			{
				gun.ShowViewCone();
			}
			else
			{
				gun.HideViewCone();
			}
		}
	}

	private void SetSelectedHpModule(GameObject go)
	{
		HPModule hPModule = null;
		if (m_selectedHPModule != null)
		{
			hPModule = m_selectedHPModule.GetComponent<HPModule>();
			hPModule.SetHighlight(enabled: false);
			hPModule.EnableSelectionMarker(enabled: false);
			if (!m_showAllViewCones)
			{
				Gun component = m_selectedHPModule.GetComponent<Gun>();
				if (component != null)
				{
					component.HideViewCone();
				}
			}
		}
		m_selectedHPModule = go;
		if (go == null)
		{
			SetInformation(null);
			return;
		}
		m_part = go.name;
		SetInformation(go);
		hPModule = m_selectedHPModule.GetComponent<HPModule>();
		hPModule.SetHighlight(enabled: true);
		hPModule.EnableSelectionMarker(enabled: true);
		Gun component2 = m_selectedHPModule.GetComponent<Gun>();
		if (component2 != null)
		{
			component2.ShowViewCone();
		}
	}

	private void RecalcCost()
	{
		int shipValue = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
		m_lblTotalCost.GetComponent<SpriteText>().Text = shipValue + " " + Localize.instance.Translate(" $label_pointssmall");
		int num = m_fleetBaseCost + shipValue;
		m_fleetMenu.SetFleetCost(num);
		bool flag = m_fleetMenu.HasValidFleet(num);
		string text = "[#FFD700]";
		text = ((!flag) ? Constants.m_shipYardSize_Invalid.ToString() : Constants.m_shipYardSize_Valid.ToString());
		GuiUtils.FindChildOf(m_gui.transform, "FleetSizeValue").GetComponent<SpriteText>().Text = text + num + " / " + m_fleetSize.max + " " + Localize.instance.Translate(" $label_pointssmall");
	}

	private void SetShipClassName(string name)
	{
		GuiUtils.FindChildOf(m_gui, "ShipClassValueLabel").GetComponent<SpriteText>().Text = name;
	}

	private void SpawnPortShip()
	{
		if (m_editShip.m_ship != null)
		{
			Object.Destroy(m_editShip.m_ship);
		}
		SetShipClassName(Localize.instance.TranslateRecursive("$" + m_portShipDef.m_prefab + "_name"));
		SetShipTitle(m_portShipDef.m_name);
		m_editShip.m_ship = ShipFactory.CreateShip(m_portShipDef, m_editShip.m_shipPosition, Quaternion.identity, -1);
		portShip = m_editShip.m_ship;
		NetObj[] componentsInChildren = m_editShip.m_ship.GetComponentsInChildren<NetObj>();
		NetObj[] array = componentsInChildren;
		foreach (NetObj netObj in array)
		{
			netObj.SetVisible(visible: true);
		}
		ParticleSystem[] componentsInChildren2 = portShip.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.SetActiveRecursively(state: false);
		}
		m_maxHardpoints = portShip.GetComponent<Ship>().m_maxHardpoints;
		RecalcCost();
		markerBase = new GameObject();
		markerBase.transform.parent = m_editShip.m_ship.transform;
		SpawnBatteryMarkers();
		m_shipModified = true;
		string text = m_portShipDef.NumberOfHardpoints() + "/" + m_maxHardpoints + " " + Localize.instance.Translate("$arms");
		GuiUtils.FindChildOf(m_gui.transform, "ArmsLabel").GetComponent<SpriteText>().Text = text;
	}

	public void OnLevelWasLoaded()
	{
		SpawnPortShip();
		m_sceneCamera = GameObject.Find("MainCamera");
		DebugUtils.Assert(m_sceneCamera != null, "Failed to find camera");
		m_shipModified = false;
	}

	private void Exit(bool save)
	{
		if (save && m_onSave != null)
		{
			m_onSave(m_portShipDef);
		}
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	private void OpenSaveDialog(string title, string text)
	{
		m_saveDialog = GuiUtils.CreateGui("GenericInputDialog", m_guiCamera);
		GenericTextInput component = m_saveDialog.GetComponent<GenericTextInput>();
		DebugUtils.Assert(component != null, "Failed to create GenericTextInput, prefab does not have a GenericTextInput-script on it!");
		component.Initialize(title, "Cancel", "Ok", text, OnSaveDialogCancelPressed, OnSaveDialogOkPressed);
		component.AllowEmptyInput = false;
	}

	private void UnRegisterDelegatesToComponents()
	{
		GuiUtils.FindChildOf(m_gui.transform, "DoneButton").GetComponent<UIButton>().RemoveValueChangedDelegate(OnBackPressed);
		GuiUtils.FindChildOf(m_gui.transform, "BodyBowButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodyMidButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodySternButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodyTopButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ArmsDefensiveButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ArmsOffensiveButton").GetComponent<UIPanelTab>().RemoveValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ShipNameInputBox").GetComponent<UITextField>().RemoveValueChangedDelegate(OnRenameShip);
		GuiUtils.FindChildOf(m_gui.transform, "SaveBlueprintButton").GetComponent<UIButton>().RemoveValueChangedDelegate(OnSaveBluePrint);
	}

	private void RegisterDelegatesToComponents()
	{
		GuiUtils.FindChildOf(m_gui, "HelpButton2").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		m_panelFleetTop = GuiUtils.FindChildOf(m_gui.transform, "FleetTopPanel").GetComponent<UIPanel>();
		m_panelShipBrowser = GuiUtils.FindChildOf(m_gui.transform, "FleetShipBrowserPanel").GetComponent<UIPanel>();
		m_panelShipName = GuiUtils.FindChildOf(m_gui.transform, "ShipNamePanel").GetComponent<UIPanel>();
		m_panelShipInfo = GuiUtils.FindChildOf(m_gui.transform, "ShipInfoPanel").GetComponent<UIPanel>();
		m_panelShipEquipment = GuiUtils.FindChildOf(m_gui.transform, "ShipEquipmentPanel").GetComponent<UIPanel>();
		m_infoArmament = GuiUtils.FindChildOf(m_panelShipInfo.transform, "Armament");
		m_infoBody = GuiUtils.FindChildOf(m_panelShipInfo.transform, "Body");
		m_infoBodyTop = GuiUtils.FindChildOf(m_panelShipInfo.transform, "Body_Top");
		m_HPMenu = GuiUtils.FindChildOf(m_gui.transform, "ShipManipulators");
		m_HPDeleta = GuiUtils.FindChildOf(m_gui.transform, "DeleteArmButton");
		m_HPRotate = GuiUtils.FindChildOf(m_gui.transform, "RotateArmButton");
		m_HPViewCone = GuiUtils.FindChildOf(m_gui.transform, "HPViewCone");
		GuiUtils.FindChildOf(m_gui.transform, "DoneButton").GetComponent<UIButton>().AddValueChangedDelegate(OnBackPressed);
		GuiUtils.FindChildOf(m_gui.transform, "BodyBowButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodyMidButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodySternButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "BodyTopButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ArmsDefensiveButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ArmsOffensiveButton").GetComponent<UIPanelTab>().AddValueChangedDelegate(RefreshList);
		GuiUtils.FindChildOf(m_gui.transform, "ShipNameInputBox").GetComponent<UITextField>().AddValueChangedDelegate(OnRenameShip);
		GuiUtils.FindChildOf(m_gui.transform, "SaveBlueprintButton").GetComponent<UIButton>().AddValueChangedDelegate(OnSaveBluePrint);
		m_lblTotalCost = GuiUtils.FindChildOf(m_gui.transform, "PointsLabel");
		GuiUtils.FindChildOf(m_gui.transform, "btnSaveXml").GetComponent<UIButton>().AddValueChangedDelegate(OnSaveXml);
		if (!IsDeveloper())
		{
			GuiUtils.FindChildOf(m_gui.transform, "btnSaveXml").transform.position = new Vector3(5000f, 0f, 0f);
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

	private void OnRenameShip(IUIObject control)
	{
		string text = GuiUtils.FindChildOf(m_gui.transform, "ShipNameInputBox").GetComponent<UITextField>().Text;
		text = text.Trim();
		if (!(m_portShipDef.m_name == text))
		{
			m_portShipDef.m_name = text;
			SetShipTitle(m_portShipDef.m_name);
			m_shipModified = true;
			PLog.Log("*************************************************");
		}
	}

	private void OnSaveBluePrint(IUIObject obj)
	{
		m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$label_saveblueprintas"), m_portShipDef.m_name, DialogCancelPressed, SaveBluePrintDialogOkPressed);
	}

	private void DialogCancelPressed2(IUIObject obj)
	{
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void DialogCancelPressed()
	{
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void RenameDialogOkPressed(string text)
	{
		string name = text.Trim();
		m_portShipDef.m_name = name;
		SetShipTitle(m_portShipDef.m_name);
		m_shipModified = true;
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void SaveBluePrintDialogOkPressed(string text)
	{
		string name = text.Trim();
		m_portShipDef.m_name = name;
		m_portShipDef.m_value = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
		m_portShipDef.m_campaignID = m_campaignID;
		m_userManClient.AddShip(m_portShipDef);
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		m_saveDialog = GuiUtils.OpenAlertDialog(m_guiCamera, Localize.instance.Translate("$shipedit_blueprint_created"), string.Empty);
		GuiUtils.FindChildOf(m_saveDialog.transform, "DismissButton").GetComponent<UIButton>().Text = Localize.instance.Translate("$button_dismiss");
		GuiUtils.FindChildOf(m_saveDialog.transform, "DismissButton").GetComponent<UIButton>().SetValueChangedDelegate(DialogCancelPressed2);
		m_userManClient.UnlockAchievement(0);
	}

	private void ShowInfoSection(Section.SectionType type)
	{
		SectionDef sectionDef = GetSectionDef(type, recreate: false);
		SetInformationName(sectionDef.m_prefab);
	}

	private void RefreshList(IUIObject obj)
	{
	}

	private bool IsDialogVisible()
	{
		if (m_saveDialog != null)
		{
			return true;
		}
		if (m_msgBox != null)
		{
			return true;
		}
		return false;
	}

	private void OnBackPressed(IUIObject obj)
	{
		if (!IsDialogVisible())
		{
			m_portShipDef.m_value = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
			if (m_shipModified)
			{
				m_saveDialog = GuiUtils.OpenMultiChoiceDialog(m_guiCamera, Localize.instance.Translate("$shipedit_savechange"), OnDoneCancel, OnDoneSaveNo, OnDoneSaveYes);
			}
			else
			{
				Exit(save: false);
			}
		}
	}

	private void CloseDialog()
	{
		if (!(m_saveDialog == null))
		{
			Object.Destroy(m_saveDialog);
			m_saveDialog = null;
		}
	}

	private void OnDoneCancel(IUIObject obj)
	{
		CloseDialog();
	}

	private void OnDoneSaveNo(IUIObject obj)
	{
		CloseDialog();
		Exit(save: false);
	}

	private void OnDoneSaveYes(IUIObject obj)
	{
		CloseDialog();
		if (string.IsNullOrEmpty(m_portShipDef.m_name))
		{
			m_onSaveExit = true;
			OpenSaveDialog("Save As", string.Empty);
		}
		else
		{
			SetShipTitle(m_portShipDef.m_name);
			Exit(save: true);
		}
	}

	private void OnSaveDialogCancelPressed()
	{
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private bool ShipExist(string shipname)
	{
		List<ShipDef> shipDefs = m_userManClient.GetShipDefs(m_campaignID);
		foreach (ShipDef item in shipDefs)
		{
			if (item.m_name == shipname)
			{
				return true;
			}
		}
		return false;
	}

	private void OnSaveDialogOkPressed(string text)
	{
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		string text2 = text.Trim();
		if (ShipExist(text2))
		{
			m_saveDialog.SetActiveRecursively(state: false);
			m_msgBox = new MsgBox(m_guiCamera, MsgBox.Type.YesNo, string.Format(Localize.instance.Translate("$shipedit_overwrite")), null, null, OverwriteSave, DontOverwriteSave);
			return;
		}
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		m_portShipDef.m_name = text2;
		SetShipTitle(m_portShipDef.m_name);
		m_shipModified = false;
		m_onSave(m_portShipDef);
		if (m_onSaveExit)
		{
			Exit(save: false);
		}
	}

	private void OverwriteSave()
	{
		m_msgBox.Close();
		m_msgBox = null;
		string name = m_saveDialog.GetComponent<GenericTextInput>().Text.Trim();
		Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		m_portShipDef.m_name = name;
		SetShipTitle(m_portShipDef.m_name);
		m_onSave(m_portShipDef);
		if (m_onSaveExit)
		{
			Exit(save: false);
		}
	}

	private void DontOverwriteSave()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_saveDialog.SetActiveRecursively(state: true);
		m_saveDialog.GetComponent<GenericTextInput>().Text = string.Empty;
	}

	private void OnSavePressed(IUIObject obj)
	{
		m_portShipDef.m_value = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
		OpenSaveDialog("Enter the name of this ship", m_portShipDef.m_name);
	}

	private void OnSaveXml(IUIObject obj)
	{
		m_portShipDef.m_value = ShipDefUtils.GetShipValue(m_portShipDef, ComponentDB.instance);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.IndentChars = "\t";
		XmlWriter xmlWriter = XmlWriter.Create("editorship.xml", xmlWriterSettings);
		xmlWriter.WriteStartElement("root");
		m_portShipDef.Save(xmlWriter);
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
	}

	public Direction GetNextDirection(Direction dir)
	{
		dir++;
		if (dir > Direction.Left)
		{
			dir = Direction.Forward;
		}
		return dir;
	}

	private void OnRotate(IUIObject obj)
	{
		if (m_selectedHPModule == null)
		{
			return;
		}
		HpPosition hpPosition = new HpPosition();
		GameObject gameObject = m_selectedHPModule.transform.parent.gameObject;
		Battery component = gameObject.GetComponent<Battery>();
		hpPosition.m_OrderNum = component.GetOrderNumber();
		hpPosition.m_sectionType = component.GetSectionType();
		string name = m_selectedHPModule.name;
		PLog.LogWarning("partName: " + name);
		HPModule component2 = m_selectedHPModule.GetComponent<HPModule>();
		hpPosition.m_gridPosition = component2.GetGridPos();
		Direction dir = component2.GetDir();
		Direction nextDirection = GetNextDirection(dir);
		PLog.Log("Trying Position: " + nextDirection);
		component2.SetDir(nextDirection);
		if (!component.CanPlaceAt(hpPosition.m_gridPosition.x, hpPosition.m_gridPosition.y, component2.GetWidth(), component2.GetLength(), component2))
		{
			nextDirection = GetNextDirection(nextDirection);
			PLog.Log("Trying Position: " + nextDirection);
			component2.SetDir(nextDirection);
			if (!component.CanPlaceAt(hpPosition.m_gridPosition.x, hpPosition.m_gridPosition.y, component2.GetWidth(), component2.GetLength(), component2))
			{
				return;
			}
			component2.SetDir(dir);
		}
		RemovePart(m_selectedHPModule);
		PLog.LogWarning("batteryGo: " + gameObject.name);
		AddPart(name, gameObject, hpPosition.m_gridPosition.x, hpPosition.m_gridPosition.y, nextDirection, verifyPosition: false);
		m_currentDir = nextDirection;
		SetSelectedHpModule(GetHPModule(hpPosition));
	}

	private void OnViewCone(IUIObject obj)
	{
		m_showAllViewCones = !m_showAllViewCones;
		SetShowAllViewCones(m_showAllViewCones);
		if (!m_showAllViewCones && m_selectedHPModule != null)
		{
			Gun component = m_selectedHPModule.GetComponent<Gun>();
			if (component != null)
			{
				component.ShowViewCone();
			}
		}
	}

	private GameObject GetHPModule(HpPosition hpPos)
	{
		Ship component = portShip.GetComponent<Ship>();
		Section section = component.GetSection(hpPos.m_sectionType);
		Battery battery = section.GetBattery(hpPos.m_OrderNum);
		HPModule moduleAt = battery.GetModuleAt(hpPos.m_gridPosition.x, hpPos.m_gridPosition.y);
		if (moduleAt == null)
		{
			return null;
		}
		return moduleAt.gameObject;
	}

	private void SetShipTitle(string name)
	{
		GuiUtils.FindChildOf(m_gui.transform, "ShipNameInputBox").GetComponent<UITextField>().Text = name;
	}

	private Vector3 MousePosition()
	{
		return Input.mousePosition;
	}
}
