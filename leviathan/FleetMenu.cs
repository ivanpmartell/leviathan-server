#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class FleetMenu
{
	public delegate void OnExitDelegate();

	private static MsgBox m_okToOverideOnSaveDialog;

	public string m_selectedFleet = string.Empty;

	public List<string> m_fleets = new List<string>();

	private FleetShip m_deleteShipInfo;

	public OnExitDelegate m_onExit;

	private Vector3 m_cameraCurrentPosition = default(Vector3);

	private float m_cameraCurrentSize = 70f;

	private Vector3 m_cameraGoalPosition = default(Vector3);

	private float m_cameraGoalSize = 70f;

	private float m_cameraPositionSpeed = 1f;

	private float m_cameraSizeSpeed = 1f;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private UserManClient m_userManClient;

	private int m_campaignID;

	private float m_shipyardTime;

	private MsgBox m_msgBox;

	private GameObject m_saveDialog;

	private ShipMenu m_shipMenu;

	private ShipDef m_shipToEdit;

	private GameObject lblFleetName;

	private bool m_fleetModified;

	private bool m_onSaveExit;

	private string m_fleetName;

	private string m_originalFleetName;

	private bool m_removeOrginalFleet;

	private int m_fleetCost;

	private int m_setFleetCost = -1;

	private GameObject m_sceneCamera;

	private UIPanel m_ShipBrowserPanel;

	private UIPanel m_FleetTopPanel;

	private UIPanel m_ShipNamePanel;

	private UIPanel m_InfoBlueprint;

	private UIPanel m_InfoClass;

	private UIPanel m_openFleetDlg;

	private GameObject lblPoints;

	private List<FleetShip> m_fleetShips = new List<FleetShip>();

	private FleetShip m_editShip;

	private float m_xOffset;

	private List<ShipDef> m_bluePrints = new List<ShipDef>();

	private string m_selectedBlueprint;

	private FleetSize m_fleetSize;

	private bool m_oneFleetOnly;

	private MusicManager m_musicMan;

	private bool m_clearFleet;

	private bool m_afterSaveOpenFleet;

	public FleetMenu(GameObject guiCamera, UserManClient userManClient, string fleetName, int campaignID, FleetSize fleetSize, bool oneFleetOnly, MusicManager musicMan)
	{
		m_musicMan = musicMan;
		m_oneFleetOnly = oneFleetOnly;
		NetObj.SetSimulating(enabled: false);
		m_guiCamera = guiCamera;
		m_userManClient = userManClient;
		m_campaignID = campaignID;
		m_gui = GuiUtils.CreateGui("Shipyard/Shipyard", m_guiCamera);
		m_fleetSize = fleetSize;
		m_sceneCamera = GameObject.Find("MainCamera");
		DebugUtils.Assert(m_sceneCamera != null, "Failed to find camera");
		RegisterDelegatesToComponents();
		m_originalFleetName = fleetName;
		m_removeOrginalFleet = false;
		m_fleetName = fleetName;
		SetFleetName(m_fleetName);
		m_ShipBrowserPanel.BringIn();
		m_FleetTopPanel.BringIn();
		m_InfoBlueprint.Dismiss();
		m_InfoClass.Dismiss();
		m_openFleetDlg.Dismiss();
		List<ShipDef> shipDefs = userManClient.GetShipDefs(campaignID);
		List<ShipDef> shipListFromFleet = userManClient.GetShipListFromFleet(fleetName, campaignID);
		SetupCamera();
		m_musicMan.SetMusic("music-credits");
		SetSizeIndicators(fleetSize);
		SetupClassList();
		SetupBluePrint();
		SetAddShipButtonStatus(enable: true);
		FleetShipsFill();
		RecalcFleetCost();
		SetFleetModified(isModified: false);
		CleanUp();
		UserManClient userManClient2 = m_userManClient;
		userManClient2.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Combine(userManClient2.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
	}

	~FleetMenu()
	{
		PLog.Log("FleetMenu DESTROYED");
	}

	public void CleanUp()
	{
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private void OnUserManUpdate()
	{
		SetupBluePrint();
		RefreshShipButtonStatus();
	}

	private void SetSizeIndicators(FleetSize fleetSize)
	{
		PLog.Log("SetSizeIndicators: " + fleetSize.min + "  " + fleetSize.max);
		string text = "$fleetsize_freesize_shipyard";
		if (FleetSizes.GetSizeClass(fleetSize) == FleetSizeClass.Small)
		{
			text = "$fleetsize_small_shipyard";
		}
		if (FleetSizes.GetSizeClass(fleetSize) == FleetSizeClass.Medium)
		{
			text = "$fleetsize_medium_shipyard";
		}
		if (FleetSizes.GetSizeClass(fleetSize) == FleetSizeClass.Heavy)
		{
			text = "$fleetsize_large_shipyard";
		}
		if (FleetSizes.GetSizeClass(fleetSize) == FleetSizeClass.Custom)
		{
			text = fleetSize.max + " $label_pointssmall";
		}
		SetFleetCost(4242);
		GuiUtils.FindChildOf(m_gui.transform, "SizeRestrictionLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(text);
	}

	private void SetupClassList()
	{
		GameObject original = Resources.Load("gui/Shipyard/ShipBrowserClassListItem") as GameObject;
		List<string> availableShips = GetAvailableShips();
		UIScrollList component = GuiUtils.FindChildOf(m_gui.transform, "ShipBrowserClassScrollList").GetComponent<UIScrollList>();
		component.ClearList(destroy: true);
		foreach (string item in availableShips)
		{
			ShipDef shipDef = GetShipDef(item, string.Empty);
			if (shipDef != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
				GuiUtils.LocalizeGui(gameObject);
				GuiUtils.FindChildOf(gameObject, "ClassMainButton").GetComponent<UIButton>().AddValueChangedDelegate(OnAddShip);
				GuiUtils.FindChildOf(gameObject, "ClassInfoButton").GetComponent<UIButton>().AddValueChangedDelegate(OnShowClassInfo);
				SpriteText component2 = GuiUtils.FindChildOf(gameObject, "ClassButtonLabel").GetComponent<SpriteText>();
				component2.Text = Localize.instance.TranslateRecursive("$" + item + "_name");
				SimpleTag simpleTag = GuiUtils.FindChildOf(gameObject, "ClassMainButton").transform.parent.gameObject.AddComponent<SimpleTag>();
				simpleTag.m_tag = item;
				SimpleSprite component3 = GuiUtils.FindChildOf(gameObject, "ClassImage").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component3, "ClassSilouettes/Silouette" + shipDef.m_prefab);
				SpriteText component4 = GuiUtils.FindChildOf(gameObject, "ClassCostValue").GetComponent<SpriteText>();
				component4.Text = ShipDefUtils.GetShipValue(shipDef, ComponentDB.instance) + Localize.instance.Translate(" $label_pointssmall");
				component.AddItem(gameObject);
			}
		}
	}

	private void SetupBluePrint()
	{
		GameObject original = Resources.Load("gui/Shipyard/ShipBrowserBlueprintListItem") as GameObject;
		m_bluePrints = m_userManClient.GetShipDefs(m_campaignID);
		UIScrollList component = GuiUtils.FindChildOf(m_gui.transform, "ShipBrowserBlueprintScrollList").GetComponent<UIScrollList>();
		component.ClearList(destroy: true);
		foreach (ShipDef bluePrint in m_bluePrints)
		{
			if (!(m_selectedBlueprint == bluePrint.m_name))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
				GuiUtils.LocalizeGui(gameObject);
				GuiUtils.FindChildOf(gameObject, "BlueprintMainButton").GetComponent<UIButton>().AddValueChangedDelegate(OnAddBlueprint);
				GuiUtils.FindChildOf(gameObject, "BlueprintInfoButton").GetComponent<UIButton>().AddValueChangedDelegate(OnShowBluePrintInfo);
				SpriteText component2 = GuiUtils.FindChildOf(gameObject, "BlueprintButtonLabel").GetComponent<SpriteText>();
				component2.Text = bluePrint.m_name;
				SimpleSprite component3 = GuiUtils.FindChildOf(gameObject, "ClassImage").GetComponent<SimpleSprite>();
				GuiUtils.SetImage(component3, "ClassSilouettes/Silouette" + bluePrint.m_prefab);
				SpriteText component4 = GuiUtils.FindChildOf(gameObject, "BlueprintCostValue").GetComponent<SpriteText>();
				component4.Text = ShipDefUtils.GetShipValue(bluePrint, ComponentDB.instance) + Localize.instance.Translate(" $label_pointssmall");
				component.AddItem(gameObject);
			}
		}
	}

	private void SetZ(GameObject go, float z)
	{
		Vector3 localPosition = go.transform.localPosition;
		localPosition.z = z;
		go.transform.localPosition = localPosition;
	}

	private void SetAddShipButtonStatus(bool enable)
	{
		UIScrollList component = GuiUtils.FindChildOf(m_gui.transform, "ShipBrowserClassScrollList").GetComponent<UIScrollList>();
		for (int i = 0; i < component.Count; i++)
		{
			IUIListObject item = component.GetItem(i);
			GuiUtils.FindChildOf(item.gameObject, "ClassMainButton").GetComponent<UIButton>().controlIsEnabled = enable;
		}
		UIScrollList component2 = GuiUtils.FindChildOf(m_gui.transform, "ShipBrowserBlueprintScrollList").GetComponent<UIScrollList>();
		for (int j = 0; j < component2.Count; j++)
		{
			IUIListObject item2 = component2.GetItem(j);
			GameObject gameObject = GuiUtils.FindChildOf(item2.transform, "BlueprintButtonLabel");
			SpriteText component3 = gameObject.GetComponent<SpriteText>();
			ShipDef bluePrintSipDef = GetBluePrintSipDef(component3.Text);
			if (bluePrintSipDef.m_available)
			{
				SetZ(GuiUtils.FindChildOf(item2.gameObject, "LblDLCNotAvailable"), 5f);
				GuiUtils.FindChildOf(item2.gameObject, "BlueprintMainButton").GetComponent<UIButton>().controlIsEnabled = enable;
				GuiUtils.FindChildOf(item2.gameObject, "BlueprintInfoButton").GetComponent<UIButton>().controlIsEnabled = enable;
			}
			else
			{
				SetZ(GuiUtils.FindChildOf(item2.gameObject, "LblDLCNotAvailable"), -5f);
				GuiUtils.FindChildOf(item2.gameObject, "BlueprintMainButton").GetComponent<UIButton>().controlIsEnabled = false;
				GuiUtils.FindChildOf(item2.gameObject, "BlueprintInfoButton").GetComponent<UIButton>().controlIsEnabled = false;
			}
		}
	}

	private void FleetShipsClear()
	{
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			fleetShip.Destroy();
		}
		m_fleetShips.Clear();
		m_editShip = null;
	}

	private void FleetShipsHide()
	{
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			fleetShip.Destroy();
		}
	}

	private void FleetShipsShow()
	{
		string name = "Fleet";
		GameObject gameObject = GameObject.Find(name);
		m_xOffset = 0f;
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			fleetShip.Destroy();
			GameObject gameObject2 = SpawnShip(fleetShip.m_definition);
			Ship component = gameObject2.GetComponent<Ship>();
			fleetShip.m_maxHardpoints = component.m_maxHardpoints;
			fleetShip.m_ship = gameObject2;
			fleetShip.m_width = component.m_Width;
			fleetShip.m_length = component.m_length;
			fleetShip.m_basePosition = gameObject.transform.position + new Vector3(m_xOffset, 0f, 5f);
			fleetShip.m_shipPosition = fleetShip.m_basePosition + new Vector3(0f, 0f, 0f - (fleetShip.m_length / 2f + 18f));
			gameObject2.transform.position = fleetShip.m_shipPosition;
			m_xOffset += 25f;
			CreateFloater(fleetShip);
			fleetShip.m_cost = ShipDefUtils.GetShipValue(fleetShip.m_definition, ComponentDB.instance);
		}
		RecalcFleetCost();
	}

	private void FleetShipsFill()
	{
		FleetShipsClear();
		List<ShipDef> shipListFromFleet = m_userManClient.GetShipListFromFleet(m_fleetName, m_campaignID);
		if (shipListFromFleet == null || shipListFromFleet.Count == 0)
		{
			return;
		}
		string name = "Fleet";
		GameObject gameObject = GameObject.Find(name);
		foreach (ShipDef item in shipListFromFleet)
		{
			FleetShip fleetShip = new FleetShip();
			fleetShip.m_definition = item;
			m_fleetShips.Add(fleetShip);
		}
		FleetShipsShow();
		RefreshShipButtonStatus();
	}

	private void RefreshShipButtonStatus()
	{
		if (m_fleetShips.Count == 8)
		{
			SetAddShipButtonStatus(enable: false);
		}
		else
		{
			SetAddShipButtonStatus(enable: true);
		}
	}

	private GameObject SpawnShip(ShipDef def)
	{
		string name = "Fleet";
		GameObject gameObject = GameObject.Find(name);
		GameObject gameObject2 = ShipFactory.CreateShip(def, gameObject.transform.position, gameObject.transform.rotation, -1);
		NetObj[] componentsInChildren = gameObject2.GetComponentsInChildren<NetObj>();
		NetObj[] array = componentsInChildren;
		foreach (NetObj netObj in array)
		{
			netObj.SetVisible(visible: true);
		}
		ParticleSystem[] componentsInChildren2 = gameObject2.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.SetActiveRecursively(state: false);
		}
		return gameObject2;
	}

	public void CreateFloater(FleetShip fleetInfo)
	{
		GameObject original = GuiUtils.FindChildOf(m_gui.transform, "FloatingInfo");
		fleetInfo.m_floatingInfo = UnityEngine.Object.Instantiate(original) as GameObject;
		fleetInfo.m_floatingInfo.transform.parent = m_gui.transform;
		GuiUtils.FindChildOf(fleetInfo.m_floatingInfo, "FloatingInfoRemoveButton").GetComponent<UIButton>().AddValueChangedDelegate(OnDeleteShip);
		GuiUtils.FindChildOf(fleetInfo.m_floatingInfo, "FloatingInfoCloneButton").GetComponent<UIButton>().AddValueChangedDelegate(OnCloneShip);
		GuiUtils.FindChildOf(fleetInfo.m_floatingInfo, "BtnMoveLeft").GetComponent<UIButton>().AddValueChangedDelegate(OnMoveShipLeft);
		GuiUtils.FindChildOf(fleetInfo.m_floatingInfo, "BtnMoveRight").GetComponent<UIButton>().AddValueChangedDelegate(OnMoveShipRight);
		if (m_fleetShips.Count == 8)
		{
			GuiUtils.FindChildOf(fleetInfo.m_floatingInfo, "FloatingInfoCloneButton").GetComponent<UIButton>().controlIsEnabled = false;
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
				if (!(component == null) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public Unit GetUnit(GameObject go)
	{
		if (go.name == "boxcollider")
		{
			go = go.transform.parent.gameObject;
		}
		Section component = go.GetComponent<Section>();
		if ((bool)component)
		{
			return component.GetUnit();
		}
		HPModule component2 = go.GetComponent<HPModule>();
		if ((bool)component2)
		{
			return component2.GetUnit();
		}
		return null;
	}

	public FleetShip SelectShip()
	{
		ToolTipDisplay toolTip = ToolTipDisplay.GetToolTip(m_gui);
		if ((bool)toolTip && toolTip.GetHelpMode())
		{
			return null;
		}
		int layerMask = (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("units"));
		GameObject gameObject = GameObject.Find("MainCamera");
		Ray ray = gameObject.camera.ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out var hitInfo, 10000f, layerMask))
		{
			return null;
		}
		Unit unit = GetUnit(hitInfo.collider.gameObject);
		if (!unit)
		{
			return null;
		}
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			if (fleetShip.m_ship.GetComponent<Unit>() == unit)
			{
				return fleetShip;
			}
		}
		return null;
	}

	private void UpdateFocus()
	{
		Vector3 vector = m_cameraGoalPosition - m_cameraCurrentPosition;
		if (vector.magnitude < 5f)
		{
			m_cameraCurrentPosition = m_cameraGoalPosition;
		}
		else
		{
			vector.Normalize();
			Vector3 cameraCurrentPosition = m_cameraCurrentPosition;
			cameraCurrentPosition += vector * Time.deltaTime * m_cameraPositionSpeed;
			m_sceneCamera.transform.position = cameraCurrentPosition + new Vector3(0f, 100f, 0f);
			m_cameraCurrentPosition = cameraCurrentPosition;
		}
		float f = m_cameraGoalSize - m_cameraCurrentSize;
		if (Mathf.Abs(f) < 2f)
		{
			m_cameraCurrentSize = m_cameraGoalSize;
			return;
		}
		float cameraCurrentSize = m_cameraCurrentSize;
		cameraCurrentSize += Time.deltaTime * m_cameraSizeSpeed;
		m_sceneCamera.GetComponent<Camera>().orthographicSize = cameraCurrentSize;
		m_cameraCurrentSize = cameraCurrentSize;
	}

	public void Update()
	{
		m_shipyardTime += Time.deltaTime;
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			fleetShip.Update(m_sceneCamera, m_guiCamera);
		}
		UpdateFocus();
		if (m_setFleetCost != -1)
		{
			SetFleetCost(m_setFleetCost);
		}
		if (m_shipMenu != null)
		{
			m_shipMenu.Update();
			return;
		}
		if (!IsDialogVisible() && Input.GetMouseButtonDown(0))
		{
			m_editShip = SelectShip();
			if (m_editShip != null && (Application.isEditor || m_editShip.m_ship.GetComponent<Ship>().m_editByPlayer))
			{
				FleetShipsHide();
				m_shipMenu = null;
				m_shipMenu = new ShipMenu(m_gui, m_guiCamera, m_userManClient, m_editShip, m_campaignID, m_fleetCost, this, m_fleetSize);
				m_shipMenu.m_onExit = OnEditShipExit;
				m_shipMenu.m_onSave = OnSaveShip;
				SetView(m_editShip.m_shipPosition, m_editShip.m_length / 2f + 10f);
			}
		}
		if (m_msgBox != null)
		{
			m_msgBox.Update();
		}
	}

	public void Close()
	{
		CloseDialog();
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_musicMan.SetMusic("menu");
		PLog.Log("Closing FleetEditor !");
		m_userManClient.AddShipyardTime(m_shipyardTime);
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		FleetShipsClear();
		if (m_shipMenu != null)
		{
			m_shipMenu.Close();
			m_shipMenu = null;
		}
		UnRegisterDelegatesFromComponents();
		UnityEngine.Object.Destroy(m_gui);
		m_gui = null;
		GameObject gameObject = GameObject.Find("Main");
		DebugUtils.Assert(m_sceneCamera != null, "Failed to find Main viewpoint");
		m_sceneCamera.GetComponent<Camera>().orthographic = false;
		m_sceneCamera.transform.position = gameObject.transform.position;
		m_sceneCamera.transform.rotation = gameObject.transform.rotation;
	}

	public void OnLevelWasLoaded()
	{
		if (m_shipMenu != null)
		{
			m_shipMenu.OnLevelWasLoaded();
		}
	}

	public void OnShipSelectet(ShipDef def)
	{
		m_shipToEdit = def;
	}

	private void Exit()
	{
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	private void OpenQuestionDialog(string title, string text, EZValueChangedDelegate cancel, EZValueChangedDelegate ok)
	{
		m_saveDialog = GuiUtils.CreateGui("dialogs/Dialog_Question", m_guiCamera);
		GuiUtils.FindChildOf(m_saveDialog.transform, "Header").GetComponent<SpriteText>().Text = title;
		GuiUtils.FindChildOf(m_saveDialog.transform, "Message").GetComponent<SpriteText>().Text = text;
		GuiUtils.FindChildOf(m_saveDialog.transform, "CancelButton").GetComponent<UIButton>().AddValueChangedDelegate(cancel);
		GuiUtils.FindChildOf(m_saveDialog.transform, "OkButton").GetComponent<UIButton>().AddValueChangedDelegate(ok);
	}

	private void OpenMultiChoiceDialog(string text, EZValueChangedDelegate cancel, EZValueChangedDelegate nosave, EZValueChangedDelegate save)
	{
		m_saveDialog = GuiUtils.CreateGui("MsgBoxMultichoice", m_guiCamera);
		GuiUtils.FindChildOf(m_saveDialog.transform, "TextLabel").GetComponent<SpriteText>().Text = text;
		GuiUtils.FindChildOf(m_saveDialog.transform, "BtnCancel").GetComponent<UIButton>().AddValueChangedDelegate(cancel);
		GuiUtils.FindChildOf(m_saveDialog.transform, "BtnDontSave").GetComponent<UIButton>().AddValueChangedDelegate(nosave);
		GuiUtils.FindChildOf(m_saveDialog.transform, "BtnSave").GetComponent<UIButton>().AddValueChangedDelegate(save);
	}

	private void CloseDialog()
	{
		if (!(m_saveDialog == null))
		{
			UnityEngine.Object.Destroy(m_saveDialog);
			m_saveDialog = null;
		}
	}

	private void DialogCancelPressed()
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void DialogDeleteCancelPressed(IUIObject obj)
	{
		m_clearFleet = false;
		m_afterSaveOpenFleet = false;
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void DialogDeletePressed(IUIObject obj)
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		if (m_fleetName.Length != 0)
		{
			m_userManClient.RemoveFleet(m_fleetName);
		}
		DialogNewFleet(null);
	}

	private List<ShipInstanceDef> GetAllShipsAsInstanceDefs()
	{
		List<ShipInstanceDef> list = new List<ShipInstanceDef>();
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			list.Add(new ShipInstanceDef(fleetShip.m_definition.m_name));
			PLog.Log("Adding Ship: " + fleetShip.m_name);
		}
		return list;
	}

	private void SaveFleet(string newName)
	{
		FleetDef fleetDef = new FleetDef();
		fleetDef.m_name = newName;
		fleetDef.m_campaignID = m_campaignID;
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			PLog.Log("Adding Ship: " + fleetShip.m_name);
			fleetDef.m_ships.Add(fleetShip.m_definition);
		}
		fleetDef.UpdateValue();
		m_userManClient.AddFleet(fleetDef);
		m_userManClient.UnlockAchievement(1);
		m_fleetName = newName;
		SetFleetName(m_fleetName);
		SetFleetModified(isModified: false);
		if (m_removeOrginalFleet)
		{
			m_removeOrginalFleet = false;
			if (m_originalFleetName.Length != 0)
			{
				m_userManClient.RemoveFleet(m_originalFleetName);
			}
			m_originalFleetName = m_fleetName;
		}
		if (m_clearFleet)
		{
			DialogNewFleet(null);
		}
		if (m_afterSaveOpenFleet)
		{
			DialogOpenFleet(null);
		}
		PLog.Log("SAVE DONE");
	}

	private void RecalcFleetCost()
	{
		int num = 0;
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			num += fleetShip.m_cost;
		}
		string text = m_fleetShips.Count + Localize.instance.Translate("$num_of_ships");
		GuiUtils.FindChildOf(m_gui.transform, "ShipCounterLabel").GetComponent<SpriteText>().Text = text;
		SetFleetCost(num);
	}

	public bool HasValidFleet(int cost)
	{
		if (cost == 0)
		{
			return true;
		}
		if (cost >= m_fleetSize.min && cost <= m_fleetSize.max)
		{
			return true;
		}
		return false;
	}

	public void SetFleetCost(int cost)
	{
		m_setFleetCost = cost;
		GuiUtils.FindChildOf(m_gui.transform, "FleetSizeLabel").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "FleetSizeValue").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "FleetSizeIconNotOk").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "FleetSizeIconOk").SetActiveRecursively(state: false);
		string text = "[#FFD700]";
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "FleetSizeSlider_SetIndicator");
		if (gameObject == null)
		{
			PLog.Log("SLIDER NOT ALIVE ");
			return;
		}
		GameObject gameObject2 = GuiUtils.FindChildOf(m_gui.transform, "FleetSizeSlider_SetSize");
		if (gameObject2 == null)
		{
			return;
		}
		gameObject2.GetComponent<UISlider>().Value = (float)m_fleetSize.max / 8000f;
		if (!(gameObject2.GetComponent<UISlider>().GetKnob() == null))
		{
			m_setFleetCost = -1;
			UISlider component = gameObject.GetComponent<UISlider>();
			component.controlIsEnabled = false;
			float num = (float)cost / 8000f;
			if (num > 1f)
			{
				num = 1f;
			}
			component.Value = num;
			bool flag = HasValidFleet(cost);
			m_fleetCost = cost;
			string text2 = m_fleetCost.ToString();
			while (text2.Length < 4)
			{
				text2 = "0" + text2;
			}
			if (flag)
			{
				text = Constants.m_shipYardSize_Valid.ToString();
				GuiUtils.FindChildOf(m_gui.transform, "FleetSizeIconOk").SetActiveRecursively(state: true);
				GuiUtils.FindChildOf(m_gui.transform, "FleetSizeValue").SetActiveRecursively(state: true);
			}
			else
			{
				text = Constants.m_shipYardSize_Invalid.ToString();
				GuiUtils.FindChildOf(m_gui.transform, "FleetSizeIconNotOk").SetActiveRecursively(state: true);
				GuiUtils.FindChildOf(m_gui.transform, "FleetSizeLabel").SetActiveRecursively(state: true);
				GuiUtils.FindChildOf(m_gui.transform, "FleetSizeLabel").GetComponent<SpriteText>().Text = text + Localize.instance.Translate(" $fleetsize_exceeded") + " " + text2 + "/" + m_fleetSize.max + " " + Localize.instance.Translate(" $label_pointssmall");
			}
			SetFleetCostLabel(text + text2);
		}
	}

	private void OnSaveShip(ShipDef shipDef)
	{
		if (shipDef != null)
		{
			m_editShip.m_definition = shipDef;
			SetFleetModified(isModified: true);
		}
	}

	private void OnEditShipExit()
	{
		ShipMenu shipMenu = m_shipMenu;
		shipMenu.m_onExit = (ShipMenu.OnExitDelegate)Delegate.Remove(shipMenu.m_onExit, new ShipMenu.OnExitDelegate(OnEditShipExit));
		m_shipMenu.Close();
		m_shipMenu = null;
		GameObject gameObject = GameObject.Find("Center");
		Vector3 pos = gameObject.transform.position + new Vector3(0f, 100f, -50f);
		SetView(pos, 80f);
		m_editShip = null;
		FleetShipsShow();
	}

	private void OkToOverridePressed()
	{
		SaveFleet(m_fleetName);
	}

	private void NotOkToOverridePressed()
	{
		m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$shipedit_saveas"), m_fleetName + " (copy)", SaveDialogCancelPressed, SaveDialogOkPressed);
	}

	private void CancelOverridePressed()
	{
		if (m_okToOverideOnSaveDialog != null)
		{
			m_okToOverideOnSaveDialog.Close();
			m_okToOverideOnSaveDialog = null;
		}
	}

	private void RenameDialogOkPressed(string fleetName)
	{
		m_removeOrginalFleet = true;
		SetFleetModified(isModified: true);
		m_fleetName = fleetName;
		SetFleetName(m_fleetName);
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void SaveDialogOkPressed(string fleetName)
	{
		if (m_userManClient.GetFleet(fleetName, m_campaignID) != null)
		{
			m_saveDialog.SetActiveRecursively(state: false);
			m_msgBox = new MsgBox(m_guiCamera, MsgBox.Type.YesNo, string.Format(Localize.instance.Translate("$fleetedit_overwrite")), null, null, OverwriteSave, DontOverwriteSave);
			return;
		}
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		SaveFleet(fleetName);
		SetFleetModified(isModified: false);
		if (m_onSaveExit)
		{
			Exit();
		}
	}

	private void OverwriteSave()
	{
		m_msgBox.Close();
		m_msgBox = null;
		string newName = m_saveDialog.GetComponent<GenericTextInput>().Text.Trim();
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		SaveFleet(newName);
		SetFleetModified(isModified: false);
		if (m_clearFleet)
		{
			m_clearFleet = false;
			DialogNewFleet(null);
		}
		if (m_onSaveExit)
		{
			Exit();
		}
	}

	private void DontOverwriteSave()
	{
		m_msgBox.Close();
		m_msgBox = null;
		m_saveDialog.SetActiveRecursively(state: true);
		m_saveDialog.GetComponent<GenericTextInput>().Text = string.Empty;
	}

	private void SaveDialogCancelPressed()
	{
		Debug.Log("SaveDialog Cancel pressed !");
		m_clearFleet = false;
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
	}

	private void DoneSaveNo(IUIObject obj)
	{
		CloseDialog();
		Exit();
	}

	private void DoneSaveYes(IUIObject obj)
	{
		CloseDialog();
		if (string.IsNullOrEmpty(m_fleetName))
		{
			m_onSaveExit = true;
			m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$shipedit_saveas"), string.Empty, SaveDialogCancelPressed, SaveDialogOkPressed);
		}
		else
		{
			SaveFleet(m_fleetName);
			Exit();
		}
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
		if (GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class").active)
		{
			return true;
		}
		if (GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").active)
		{
			return true;
		}
		UIManager component = m_guiCamera.GetComponent<UIManager>();
		if (component.DidAnyPointerHitUI())
		{
			return true;
		}
		return false;
	}

	private void RegisterDelegatesToComponents()
	{
		m_ShipBrowserPanel = GuiUtils.FindChildOf(m_gui.transform, "FleetShipBrowserPanel").GetComponent<UIPanel>();
		m_FleetTopPanel = GuiUtils.FindChildOf(m_gui.transform, "FleetTopPanel").GetComponent<UIPanel>();
		m_ShipNamePanel = GuiUtils.FindChildOf(m_gui.transform, "ShipNamePanel").GetComponent<UIPanel>();
		m_InfoBlueprint = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").GetComponent<UIPanel>();
		m_InfoClass = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class").GetComponent<UIPanel>();
		m_openFleetDlg = GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog").GetComponent<UIPanel>();
		lblFleetName = GuiUtils.FindChildOf(m_gui.transform, "FleetNameInputBox");
		lblPoints = GuiUtils.FindChildOf(m_gui.transform, "FleetSizeValue");
		GuiUtils.FindChildOf(m_gui.transform, "NewFleetButton").GetComponent<UIButton>().AddValueChangedDelegate(OnNewFleet);
		GuiUtils.FindChildOf(m_gui.transform, "OpenFleetButton").GetComponent<UIButton>().AddValueChangedDelegate(OnOpenFleet);
		GuiUtils.FindChildOf(m_gui.transform, "SaveFleetButton").GetComponent<UIButton>().AddValueChangedDelegate(OnSaveFleet);
		GuiUtils.FindChildOf(m_gui.transform, "SaveFleetAsButton").GetComponent<UIButton>().AddValueChangedDelegate(OnSaveFleetAs);
		GuiUtils.FindChildOf(m_gui.transform, "FleetNameInputBox").GetComponent<UITextField>().AddValueChangedDelegate(OnRenameFleet);
		GuiUtils.FindChildOf(m_gui.transform, "FleetInfoButton").GetComponent<UIButton>().AddValueChangedDelegate(OnFleetInfo);
		GuiUtils.FindChildOf(m_gui.transform, "ExitButton").GetComponent<UIButton>().AddValueChangedDelegate(OnDonePressed);
		GuiUtils.FindChildOf(GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class"), "OkButton").GetComponent<UIButton>().AddValueChangedDelegate(OnHideClassInfo);
		GuiUtils.FindChildOf(GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint"), "OkButton").GetComponent<UIButton>().AddValueChangedDelegate(OnHideluePrintInfo);
		GuiUtils.FindChildOf(GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint"), "DeleteBlueprintButton").GetComponent<UIButton>().AddValueChangedDelegate(OnDeleteBluePrint);
		GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog_Cancel_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnOpenFleetCancel);
		GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog_Open_Button").GetComponent<UIButton>().AddValueChangedDelegate(OnOpenFleetOk);
		GuiUtils.FindChildOf(m_gui, "HelpButton1").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		GuiUtils.FindChildOfComponent<UIPanelTab>(m_gui, "ShipBlueprintButton").SetValueChangedDelegate(OnBlueprintTabPressed);
		GuiUtils.FindChildOfComponent<UIPanelTab>(m_gui, "ShipClassButton").SetValueChangedDelegate(OnShipClassTabPressed);
		if (m_oneFleetOnly)
		{
			GuiUtils.FindChildOf(m_gui.transform, "NewFleetButton").GetComponent<UIButton>().controlIsEnabled = false;
			GuiUtils.FindChildOf(m_gui.transform, "OpenFleetButton").GetComponent<UIButton>().controlIsEnabled = false;
			GuiUtils.FindChildOf(m_gui.transform, "SaveFleetAsButton").GetComponent<UIButton>().controlIsEnabled = false;
			GuiUtils.FindChildOf(m_gui.transform, "FleetNameInputBox").GetComponent<UITextField>().controlIsEnabled = false;
		}
		m_ShipNamePanel.gameObject.SetActiveRecursively(state: false);
	}

	private void OnBlueprintTabPressed(IUIObject obj)
	{
		GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "ShipBrowserBlueprintPanel").AddTempTransitionDelegate(OnBluprintTabTransitionComplete);
	}

	private void OnBluprintTabTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
	}

	private void OnShipClassTabPressed(IUIObject obj)
	{
		GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "ShipBrowserClassPanel").AddTempTransitionDelegate(OnShipClassTabTransitionComplete);
	}

	private void OnShipClassTabTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
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

	private void UnRegisterDelegatesFromComponents()
	{
	}

	private void OnNewFleet(IUIObject obj)
	{
		if (m_fleetModified)
		{
			m_clearFleet = true;
			OpenMultiChoiceDialog(Localize.instance.Translate("$fleetedit_savechange"), DialogDeleteCancelPressed, DialogNewFleet, OnSaveFleet);
		}
		else
		{
			DialogNewFleet(null);
		}
	}

	private void DialogNewFleet(IUIObject obj)
	{
		CloseDialog();
		m_clearFleet = false;
		FleetShipsClear();
		m_fleetName = string.Empty;
		SetFleetName(m_fleetName);
		SetFleetModified(isModified: false);
		RecalcFleetCost();
		SetAddShipButtonStatus(enable: true);
	}

	private void OnOpenFleet(IUIObject obj)
	{
		if (m_fleetModified)
		{
			m_afterSaveOpenFleet = true;
			OpenMultiChoiceDialog(Localize.instance.Translate("$fleetedit_savechange"), DialogDeleteCancelPressed, DialogOpenFleet, OnSaveFleet);
		}
		else
		{
			DialogOpenFleet(null);
		}
	}

	private void DialogOpenFleet(IUIObject obj)
	{
		m_afterSaveOpenFleet = false;
		CloseDialog();
		FillFleetList();
		UIPanel component = GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog").GetComponent<UIPanel>();
		component.BringIn();
	}

	private void OnOpenFleetCancel(IUIObject obj)
	{
		UIPanel component = GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog").GetComponent<UIPanel>();
		component.Dismiss();
	}

	private void OnOpenFleetOk(IUIObject obj)
	{
		UIPanel component = GuiUtils.FindChildOf(m_gui.transform, "OpenFleetDialog").GetComponent<UIPanel>();
		component.Dismiss();
		PLog.Log("Open Fleet: " + m_selectedFleet);
		m_originalFleetName = m_selectedFleet;
		m_fleetName = m_selectedFleet;
		FleetShipsFill();
		RecalcFleetCost();
		SetFleetModified(isModified: false);
		SetFleetName(m_fleetName);
	}

	private void FillFleetList()
	{
		GameObject original = Resources.Load("gui/Shipyard/FleetListItem") as GameObject;
		UIScrollList component = GuiUtils.FindChildOf(m_gui, "OpenFleetDialogList").GetComponent<UIScrollList>();
		m_fleets.Clear();
		component.ClearList(destroy: true);
		List<FleetDef> fleetDefs = m_userManClient.GetFleetDefs(0);
		foreach (FleetDef item in fleetDefs)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Name").GetComponent<SpriteText>().Text = item.m_name;
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Ships").GetComponent<SpriteText>().Text = item.m_ships.Count.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Points").GetComponent<SpriteText>().Text = item.m_value.ToString();
			GuiUtils.FindChildOf(gameObject, "FleetListItem_Size").GetComponent<SpriteText>().Text = Localize.instance.TranslateKey(FleetSizes.GetSizeClassName(item.m_value));
			UIRadioBtn component2 = GuiUtils.FindChildOf(gameObject, "RadioButton").GetComponent<UIRadioBtn>();
			component2.SetValueChangedDelegate(OnFleetSelected);
			if (!item.m_available)
			{
				component2.controlIsEnabled = false;
			}
			else
			{
				GuiUtils.FindChildOf(gameObject, "LblDLCNotAvailable").transform.Translate(new Vector3(0f, 0f, 20f));
			}
			UIListItemContainer component3 = gameObject.GetComponent<UIListItemContainer>();
			component.AddItem(component3);
			m_fleets.Add(item.m_name);
		}
	}

	private void OnFleetSelected(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		m_selectedFleet = m_fleets[component.Index];
	}

	private void OnSaveFleet(IUIObject obj)
	{
		if ((bool)m_saveDialog)
		{
			UnityEngine.Object.Destroy(m_saveDialog);
			m_saveDialog = null;
		}
		if (string.IsNullOrEmpty(m_fleetName))
		{
			m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$shipedit_saveas"), string.Empty, SaveDialogCancelPressed, SaveDialogOkPressed);
		}
		else
		{
			SaveFleet(m_fleetName);
		}
	}

	private void OnSaveFleetAs(IUIObject obj)
	{
		m_saveDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$shipedit_saveas"), m_fleetName, SaveDialogCancelPressed, SaveDialogOkPressed);
	}

	private void OnRenameFleet(IUIObject control)
	{
		string text = GuiUtils.FindChildOf(m_gui.transform, "FleetNameInputBox").GetComponent<UITextField>().Text;
		if (!(text == m_fleetName))
		{
			m_removeOrginalFleet = true;
			SetFleetModified(isModified: true);
			m_fleetName = text;
			SetFleetName(m_fleetName);
			UnityEngine.Object.Destroy(m_saveDialog);
			m_saveDialog = null;
		}
	}

	private void OnDeleteFleet(IUIObject obj)
	{
		OpenQuestionDialog(Localize.instance.Translate("$shipedit_delete_fleet_title"), Localize.instance.Translate("$shipedit_delete_fleet_text " + m_fleetName), DialogDeleteCancelPressed, DialogDeletePressed);
	}

	private void OnFleetInfo(IUIObject obj)
	{
		PLog.Log("OnFleetInfo");
	}

	private void OnDonePressed(IUIObject obj)
	{
		if (!IsDialogVisible())
		{
			if (!m_fleetModified)
			{
				Exit();
			}
			else
			{
				OpenMultiChoiceDialog(Localize.instance.Translate("$fleetedit_savechange"), DialogDeleteCancelPressed, DoneSaveNo, DoneSaveYes);
			}
		}
	}

	private void OnShowBluePrintInfo(IUIObject obj)
	{
		GameObject gameObject = GuiUtils.FindChildOf(obj.transform.parent, "BlueprintButtonLabel");
		SpriteText component = gameObject.GetComponent<SpriteText>();
		m_selectedBlueprint = component.Text;
		GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").GetComponent<UIPanel>().BringIn();
		SetBlueprintInformation(component.Text);
	}

	private void OnShowClassInfo(IUIObject obj)
	{
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class");
		gameObject.GetComponent<UIPanel>().BringIn();
		GameObject gameObject2 = obj.transform.parent.gameObject;
		SimpleTag component = gameObject2.GetComponent<SimpleTag>();
		SetClassInformation(component.m_tag);
	}

	private string DONEVALUE(string value)
	{
		return Localize.instance.Translate(value);
	}

	private void SetClassInformation(string className)
	{
		ShipDef shipDef = GetShipDef(className, string.Empty);
		if (shipDef != null)
		{
			GameObject gameObject = SpawnShip(shipDef);
			Ship component = gameObject.GetComponent<Ship>();
			GameObject go = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class");
			GuiUtils.FindChildOf(go, "ClassHeader").GetComponent<SpriteText>().Text = DONEVALUE(component.m_displayClassName);
			SimpleSprite component2 = GuiUtils.FindChildOf(go, "ClassImage").GetComponent<SimpleSprite>();
			GuiUtils.SetImage(component2, GuiUtils.GetShipThumbnail(className));
			GuiUtils.FindChildOf(go, "ClassDescriptionText").GetComponent<SpriteText>().Text = DONEVALUE(Localize.instance.TranslateRecursive("$" + className + "_flavor"));
			if (component.m_deepKeel)
			{
				GuiUtils.FindChildOf(go, "ClassStandardClassLabel1").GetComponent<SpriteText>().Text = DONEVALUE("$label_deepkeel: $button_yes");
			}
			else
			{
				GuiUtils.FindChildOf(go, "ClassStandardClassLabel1").GetComponent<SpriteText>().Text = DONEVALUE("$label_deepkeel: $button_no");
			}
			GuiUtils.FindChildOf(go, "ClassStandardClassLabel2").GetComponent<SpriteText>().Text = DONEVALUE("$Health " + component.GetMaxHealth());
			int armorClass = component.GetSectionFront().m_armorClass;
			int armorClass2 = component.GetSectionMid().m_armorClass;
			int armorClass3 = component.GetSectionRear().m_armorClass;
			GuiUtils.FindChildOf(go, "ClassStandardClassLabel3").GetComponent<SpriteText>().Text = DONEVALUE("$label_armor: $shipedit_shortsection_bow " + armorClass + ", $shipedit_shortsection_middle " + armorClass2 + ", $shipedit_shortsection_stern " + armorClass3);
			GuiUtils.FindChildOf(go, "ClassStandardClassLabel4").GetComponent<SpriteText>().Text = DONEVALUE("$Speed " + component.GetMaxSpeed() + " $shipedit_ahead, " + component.GetMaxReverseSpeed() + " $shipedit_astern");
			float num = component.GetLength() * 10f;
			GuiUtils.FindChildOf(go, "ClassStandardClassLabel5").GetComponent<SpriteText>().Text = DONEVALUE("$shipedit_length: " + num + " $shipedit_meters");
			GuiUtils.FindChildOf(go, "ClassStandardClassLabel6").GetComponent<SpriteText>().Text = string.Empty;
			int shipValue = ShipDefUtils.GetShipValue(shipDef, ComponentDB.instance);
			GuiUtils.FindChildOf(go, "ClassStandardClassTotalCost").GetComponent<SpriteText>().Text = DONEVALUE("$shipedit_totalcost: " + shipValue + " $label_pointssmall");
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private int SetBlueprintSectionInfo(Section section, string type)
	{
		SectionSettings section2 = ComponentDB.instance.GetSection(section.name);
		GameObject go = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint");
		GuiUtils.FindChildOf(go, "ModifiedClass_" + type + "NameLabel").GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + section.name + "_name");
		GuiUtils.FindChildOf(go, "ModifiedClass_" + type + "HealthLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate("$HEALTH " + section.m_maxHealth);
		GuiUtils.FindChildOf(go, "ModifiedClass_" + type + "ArmorLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate("$ARMOR " + section.m_armorClass);
		GuiUtils.FindChildOf(go, "ModifiedClass_" + type + "CostLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(string.Empty + section2.m_value + " $label_pointssmall");
		return section2.m_value;
	}

	private void SetBlueprintInformation(string blueprintName)
	{
		ShipDef bluePrintSipDef = GetBluePrintSipDef(blueprintName);
		if (bluePrintSipDef == null)
		{
			return;
		}
		GameObject gameObject = SpawnShip(bluePrintSipDef);
		Ship component = gameObject.GetComponent<Ship>();
		GameObject go = GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint");
		GuiUtils.FindChildOf(go, "BlueprintHeader").GetComponent<SpriteText>().Text = blueprintName;
		GuiUtils.FindChildOf(go, "BlueprintDescriptionText").GetComponent<SpriteText>().Text = Localize.instance.Translate("$" + bluePrintSipDef.m_prefab + "_flavor");
		SimpleSprite component2 = GuiUtils.FindChildOf(m_gui.transform, "ModifiedClass_ClassImage").GetComponent<SimpleSprite>();
		GuiUtils.SetImage(component2, GuiUtils.GetShipSilhouette(bluePrintSipDef.m_prefab));
		int num = 0;
		num += SetBlueprintSectionInfo(component.GetSectionFront(), "Bow");
		num += SetBlueprintSectionInfo(component.GetSectionMid(), "Mid");
		num += SetBlueprintSectionInfo(component.GetSectionRear(), "Stern");
		num += SetBlueprintSectionInfo(component.GetSectionTop(), "Top");
		GuiUtils.FindChildOf(go, "ModifiedClass_TotalCostLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate("$shipedit_baseoperationalcost: " + num + " $label_pointssmall");
		string text = bluePrintSipDef.NumberOfHardpoints() + "/12 ";
		GuiUtils.FindChildOf(go, "BlueprintArmsTotalArms").GetComponent<SpriteText>().Text = text;
		int num2 = 0;
		int num3 = 1;
		List<string> hardpointNames = bluePrintSipDef.GetHardpointNames();
		foreach (string item in hardpointNames)
		{
			HPModuleSettings module = ComponentDB.instance.GetModule(item);
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			HPModule component3 = prefab.GetComponent<HPModule>();
			GuiUtils.FindChildOf(go, "BluePrintArmsLabel" + num3).GetComponent<SpriteText>().Text = Localize.instance.TranslateRecursive("$" + component3.name + "_name");
			GuiUtils.FindChildOf(go, "BluePrintArmsCost" + num3).GetComponent<SpriteText>().Text = module.m_value + Localize.instance.Translate(" $label_pointssmall");
			num2 += module.m_value;
			num3++;
		}
		for (int i = num3; i <= 12; i++)
		{
			GuiUtils.FindChildOf(go, "BluePrintArmsLabel" + i).GetComponent<SpriteText>().Text = string.Empty;
			GuiUtils.FindChildOf(go, "BluePrintArmsCost" + i).GetComponent<SpriteText>().Text = string.Empty;
		}
		GuiUtils.FindChildOf(go, "BlueprintArmsCost").GetComponent<SpriteText>().Text = Localize.instance.Translate("$shipedit_totalarmscost: " + num2 + " $label_pointssmall");
		int num4 = num + num2;
		GuiUtils.FindChildOf(go, "BlueprintTotalCostValue").GetComponent<SpriteText>().Text = Localize.instance.Translate(num4 + " $label_pointssmall");
		UnityEngine.Object.Destroy(gameObject);
	}

	private void OnHideluePrintInfo(IUIObject obj)
	{
		GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").GetComponent<UIPanel>().Dismiss();
	}

	private void OnDeleteBluePrint(IUIObject obj)
	{
		GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").GetComponent<UIPanel>().Dismiss();
		OpenQuestionDialog(string.Empty, Localize.instance.Translate("$shipedit_delete_blueprint") + " " + m_selectedBlueprint, DialogDeleteCancelPressed, DialogDeleteBlueprint);
	}

	private void DialogDeleteBlueprint(IUIObject obj)
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		m_userManClient.RemoveShip(m_selectedBlueprint);
		SetupBluePrint();
	}

	private void OnHideClassInfo(IUIObject obj)
	{
		GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Class").GetComponent<UIPanel>().Dismiss();
	}

	private void OnAddBlueprint(IUIObject obj)
	{
		GameObject gameObject = GuiUtils.FindChildOf(obj.transform, "BlueprintButtonLabel");
		SpriteText component = gameObject.GetComponent<SpriteText>();
		ShipDef bluePrintSipDef = GetBluePrintSipDef(component.Text);
		if (bluePrintSipDef != null)
		{
			FleetShip fleetShip = new FleetShip();
			fleetShip.m_definition = bluePrintSipDef;
			m_fleetShips.Add(fleetShip);
			FleetShipsShow();
			SetFleetModified(isModified: true);
			RecalcFleetCost();
			if (m_fleetShips.Count == 8)
			{
				SetAddShipButtonStatus(enable: false);
			}
		}
	}

	private void OnAddShip(IUIObject obj)
	{
		GameObject gameObject = obj.transform.parent.gameObject;
		GameObject gameObject2 = GuiUtils.FindChildOf(obj.transform, "ClassButtonLabel");
		SimpleTag component = gameObject.GetComponent<SimpleTag>();
		SpriteText component2 = gameObject2.GetComponent<SpriteText>();
		ShipDef shipDef = GetShipDef(component.m_tag, component2.Text);
		if (shipDef != null)
		{
			FleetShip fleetShip = new FleetShip();
			fleetShip.m_definition = shipDef;
			m_fleetShips.Add(fleetShip);
			FleetShipsShow();
			SetFleetModified(isModified: true);
			RecalcFleetCost();
			if (m_fleetShips.Count == 8)
			{
				SetAddShipButtonStatus(enable: false);
			}
		}
	}

	private FleetShip GetFleetShip(IUIObject obj)
	{
		foreach (FleetShip fleetShip in m_fleetShips)
		{
			if (fleetShip.m_floatingInfo == obj.transform.parent.gameObject)
			{
				return fleetShip;
			}
		}
		return null;
	}

	private void OnDeleteShip(IUIObject obj)
	{
		PLog.Log("OnDeleteShip");
		m_deleteShipInfo = GetFleetShip(obj);
		if (m_deleteShipInfo != null)
		{
			GuiUtils.FindChildOf(m_gui.transform, "InfoDialog_Blueprint").GetComponent<UIPanel>().Dismiss();
			OpenQuestionDialog(string.Empty, Localize.instance.Translate("$FloatingInfoRemoveButton_tooltip") + " " + m_deleteShipInfo.m_definition.m_name + "?", DialogDeleteCancelPressed, DialogDeleteShip);
		}
	}

	private void DialogDeleteShip(IUIObject obj)
	{
		UnityEngine.Object.Destroy(m_saveDialog);
		m_saveDialog = null;
		if (m_deleteShipInfo != null)
		{
			m_deleteShipInfo.Destroy();
			m_fleetShips.Remove(m_deleteShipInfo);
			FleetShipsShow();
			SetFleetModified(isModified: true);
			RecalcFleetCost();
			SetAddShipButtonStatus(enable: true);
			if (m_fleetShips.Count == 0)
			{
				SetFleetModified(isModified: false);
			}
			m_deleteShipInfo = null;
		}
	}

	private void OnCloneShip(IUIObject obj)
	{
		if (m_fleetShips.Count == 8)
		{
			return;
		}
		foreach (FleetShip fleetShip2 in m_fleetShips)
		{
			if (fleetShip2.m_floatingInfo == obj.transform.parent.gameObject)
			{
				FleetShip fleetShip = new FleetShip();
				fleetShip.m_definition = fleetShip2.m_definition.Clone();
				m_fleetShips.Add(fleetShip);
				FleetShipsShow();
				SetFleetModified(isModified: true);
				RecalcFleetCost();
				if (m_fleetShips.Count == 8)
				{
					SetAddShipButtonStatus(enable: false);
				}
				break;
			}
		}
	}

	private void MoveShip(int shipIndex, bool left)
	{
		int num = shipIndex;
		if (left)
		{
			num = shipIndex - 1;
			if (num < 0)
			{
				num = m_fleetShips.Count - 1;
			}
		}
		else
		{
			num = shipIndex + 1;
			if (num > m_fleetShips.Count - 1)
			{
				num = 0;
			}
		}
		FleetShip value = m_fleetShips[num];
		m_fleetShips[num] = m_fleetShips[shipIndex];
		m_fleetShips[shipIndex] = value;
		FleetShipsShow();
		SetFleetModified(isModified: true);
	}

	private void OnMoveShipLeft(IUIObject obj)
	{
		int shipIndex = m_fleetShips.FindIndex((FleetShip s) => s.m_floatingInfo == obj.transform.parent.gameObject);
		MoveShip(shipIndex, left: true);
	}

	private void OnMoveShipRight(IUIObject obj)
	{
		int shipIndex = m_fleetShips.FindIndex((FleetShip s) => s.m_floatingInfo == obj.transform.parent.gameObject);
		MoveShip(shipIndex, left: false);
	}

	public DefaultSections GetDefaultSections(string shipSeries)
	{
		List<string> availableSections = m_userManClient.GetAvailableSections(m_campaignID);
		DefaultSections defaultSections = new DefaultSections();
		if (availableSections == null)
		{
			PLog.LogError("Ship of series " + shipSeries + " has no sections listed.");
			return defaultSections;
		}
		foreach (string item in availableSections)
		{
			GameObject prefab = ObjectFactory.instance.GetPrefab(item);
			if (!(prefab != null))
			{
				continue;
			}
			Section component = prefab.GetComponent<Section>();
			if (shipSeries == component.m_series && component.m_defaultSection)
			{
				if (component.m_type == Section.SectionType.Front)
				{
					defaultSections.m_front = item;
				}
				if (component.m_type == Section.SectionType.Mid)
				{
					defaultSections.m_mid = item;
				}
				if (component.m_type == Section.SectionType.Rear)
				{
					defaultSections.m_rear = item;
				}
				if (component.m_type == Section.SectionType.Top)
				{
					defaultSections.m_top = item;
				}
			}
		}
		return defaultSections;
	}

	private ShipDef GetShipDef(string shipSeries, string name)
	{
		DefaultSections defaultSections = GetDefaultSections(shipSeries);
		if (!defaultSections.IsValid())
		{
			PLog.LogError(shipSeries + " is missing default sections: " + defaultSections.ErrorMessage());
			return null;
		}
		ShipDef shipDef = new ShipDef();
		shipDef.m_name = name;
		shipDef.m_prefab = shipSeries;
		shipDef.m_frontSection = new SectionDef();
		shipDef.m_frontSection.m_prefab = defaultSections.m_front;
		shipDef.m_midSection = new SectionDef();
		shipDef.m_midSection.m_prefab = defaultSections.m_mid;
		shipDef.m_rearSection = new SectionDef();
		shipDef.m_rearSection.m_prefab = defaultSections.m_rear;
		shipDef.m_topSection = new SectionDef();
		shipDef.m_topSection.m_prefab = defaultSections.m_top;
		shipDef.m_value = ShipDefUtils.GetShipValue(shipDef, ComponentDB.instance);
		return shipDef;
	}

	private ShipDef GetBluePrintSipDef(string name)
	{
		foreach (ShipDef bluePrint in m_bluePrints)
		{
			if (bluePrint.m_name == name)
			{
				return bluePrint.Clone();
			}
		}
		return null;
	}

	private void SetFleetName(string name)
	{
		lblFleetName.GetComponent<UITextField>().Text = name;
	}

	private void SetFleetCostLabel(string value)
	{
		lblPoints.GetComponent<SpriteText>().Text = value + "/" + m_fleetSize.max + " " + Localize.instance.Translate(" $label_pointssmall");
	}

	private void SetFleetModified(bool isModified)
	{
		m_fleetModified = isModified;
		GuiUtils.FindChildOf(m_gui.transform, "SaveFleetButton").GetComponent<UIButton>().controlIsEnabled = m_fleetModified;
		if (!m_oneFleetOnly)
		{
			if (m_fleetShips.Count == 0)
			{
				GuiUtils.FindChildOf(m_gui.transform, "SaveFleetAsButton").GetComponent<UIButton>().controlIsEnabled = false;
			}
			else
			{
				GuiUtils.FindChildOf(m_gui.transform, "SaveFleetAsButton").GetComponent<UIButton>().controlIsEnabled = true;
			}
		}
	}

	private void SetupCamera()
	{
		GameObject gameObject = GameObject.Find("Center");
		gameObject.transform.localPosition = new Vector3(88f, 0f, 0f);
		Quaternion rotation = default(Quaternion);
		rotation.eulerAngles = new Vector3(90f, 0f, 0f);
		m_sceneCamera.transform.position = gameObject.transform.position + new Vector3(0f, 100f, -50f);
		m_sceneCamera.transform.rotation = rotation;
		m_cameraCurrentPosition = m_sceneCamera.transform.position;
		m_sceneCamera.GetComponent<Camera>().orthographic = true;
		m_sceneCamera.GetComponent<Camera>().orthographicSize = 80f;
		m_cameraGoalPosition = (m_cameraCurrentPosition = m_sceneCamera.transform.position);
	}

	private void SetView(Vector3 pos, float size)
	{
		m_cameraGoalPosition = pos;
		m_cameraGoalSize = size;
		m_cameraPositionSpeed = (m_cameraGoalPosition - m_cameraCurrentPosition).magnitude / 0.5f;
		float num = m_cameraGoalSize - m_cameraCurrentSize;
		m_cameraSizeSpeed = num / 0.5f;
	}
}
