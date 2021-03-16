#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

public class ChooseFleetMenu
{
	public delegate void OnExitDelegate();

	public OnExitDelegate m_onExit;

	public Action<string, int> m_onProceed;

	private GameObject m_gui;

	private bool isValid;

	private UserManClient m_userManClient;

	private FleetSizeClass m_fleetSizeClass;

	private FleetSize m_fleetSizeLimit;

	private int m_campaignID;

	private GameObject m_guiCamera;

	private MsgBox m_msgBox;

	private Transform listContainer;

	private UIScrollList lstFleetInfos;

	private UIButton btnProceed;

	private UIButton btnNewFleet;

	private UIButton btnCancel;

	private UIButton btnDelete;

	private UIPanel panelButtons;

	private SpriteText lblTitle;

	private ListBox_FleetInfo_Filler fleetInfoFiller;

	public Transform ListContainer
	{
		get
		{
			return listContainer;
		}
		private set
		{
			listContainer = value;
		}
	}

	public ListBox_FleetInfo_Filler InfoFiller
	{
		get
		{
			return fleetInfoFiller;
		}
		private set
		{
			fleetInfoFiller = value;
		}
	}

	public UIPanel Panel_Buttons
	{
		get
		{
			return panelButtons;
		}
		private set
		{
			panelButtons = value;
		}
	}

	public UIScrollList List_FleetInfos
	{
		get
		{
			return lstFleetInfos;
		}
		private set
		{
			lstFleetInfos = value;
		}
	}

	public UIButton Button_Proceed
	{
		get
		{
			return btnProceed;
		}
		private set
		{
			btnProceed = value;
		}
	}

	public UIButton Button_NewFleet
	{
		get
		{
			return btnNewFleet;
		}
		private set
		{
			btnNewFleet = value;
		}
	}

	public UIButton Button_Cancel
	{
		get
		{
			return btnCancel;
		}
		private set
		{
			btnCancel = value;
		}
	}

	public UIButton Button_Delete
	{
		get
		{
			return btnDelete;
		}
		private set
		{
			btnDelete = value;
		}
	}

	public SpriteText Label_Title
	{
		get
		{
			return lblTitle;
		}
		private set
		{
			lblTitle = value;
		}
	}

	public bool IsValid
	{
		get
		{
			return isValid;
		}
		private set
		{
			isValid = value;
		}
	}

	public ChooseFleetMenu(GameObject guiCamera, UserManClient userManClient, FleetSizeClass fleetClass, FleetSize fleetSize, string fleetType, int campaignID)
	{
		m_guiCamera = guiCamera;
		m_fleetSizeClass = fleetClass;
		m_fleetSizeLimit = fleetSize;
		m_campaignID = campaignID;
		DebugUtils.Assert(guiCamera != null, "ChooseFleetMenu ctor called with NULL camera !");
		m_gui = GuiUtils.CreateGui("ChooseFleetDialog", guiCamera);
		DebugUtils.Assert(m_gui != null, "ChooseFleetMenu failed to validate root object m_gui !");
		m_userManClient = userManClient;
		if (m_userManClient == null)
		{
			Debug.LogWarning("ChooseFleetMenu ctor called with NULL UserManClient, will not be able to fill data !");
		}
		InitializeAndValidate();
		InfoFiller.Initialize();
		FillData();
		RegisterDelegatesToComponents();
		btnProceed.controlIsEnabled = false;
		if (fleetClass != FleetSizeClass.None)
		{
			SetTitle(Localize.instance.Translate("$choosefleet_library_fleet " + fleetClass.ToString() + " (" + fleetSize.min + "-" + fleetSize.max + ")"), toUpper: true);
			btnProceed.GetComponent<UIButton>().Text = "OK";
		}
		else
		{
			SetTitle(Localize.instance.Translate("$choosefleet_library_fleet"), toUpper: true);
		}
		btnDelete.GetComponent<UIButton>().controlIsEnabled = false;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Combine(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
	}

	public void Close()
	{
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		UnRegisterDelegatesFromComponents();
		UnityEngine.Object.Destroy(m_gui);
	}

	private void Exit()
	{
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	public void SetTitle(string title, bool toUpper)
	{
		string text = title.Trim();
		if (!string.IsNullOrEmpty(text) && toUpper)
		{
			text = text.ToUpper();
		}
		Label_Title.GetComponent<SpriteText>().Text = text;
	}

	private void OnUserManUpdate()
	{
		FillData();
	}

	private void RegisterDelegatesToComponents()
	{
		btnProceed.GetComponent<UIButton>().AddValueChangedDelegate(OnProceedPressed);
		btnNewFleet.GetComponent<UIButton>().AddValueChangedDelegate(OnNewFleetPressed);
		btnCancel.GetComponent<UIButton>().AddValueChangedDelegate(OnCancelPressed);
		btnDelete.GetComponent<UIButton>().AddValueChangedDelegate(OnDeletePressed);
		ListBox_FleetInfo_Filler infoFiller = InfoFiller;
		infoFiller.m_onFleetChangedDelegate = (ListBox_FleetInfo_Filler.OnFleetChanged)Delegate.Combine(infoFiller.m_onFleetChangedDelegate, new ListBox_FleetInfo_Filler.OnFleetChanged(SelectedFleetChanged));
	}

	private void UnRegisterDelegatesFromComponents()
	{
		Button_Proceed.GetComponent<UIButton>().RemoveValueChangedDelegate(OnProceedPressed);
		Button_NewFleet.GetComponent<UIButton>().RemoveValueChangedDelegate(OnNewFleetPressed);
		Button_Cancel.GetComponent<UIButton>().RemoveValueChangedDelegate(OnCancelPressed);
		ListBox_FleetInfo_Filler infoFiller = InfoFiller;
		infoFiller.m_onFleetChangedDelegate = (ListBox_FleetInfo_Filler.OnFleetChanged)Delegate.Remove(infoFiller.m_onFleetChangedDelegate, new ListBox_FleetInfo_Filler.OnFleetChanged(SelectedFleetChanged));
	}

	private void FillData()
	{
		List<ShipDef> shipDefs = m_userManClient.GetShipDefs(m_campaignID);
		List<FleetDef> fleetDefs = m_userManClient.GetFleetDefs(m_campaignID);
		InfoFiller.Clear();
		int num = int.MaxValue;
		foreach (FleetDef item in fleetDefs)
		{
			int value = item.m_value;
			if (value < num)
			{
				num = value;
			}
			InfoFiller.AddItem(item.m_name, value.ToString(), "2011/01/12");
		}
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
	}

	private void ForceCreateNewFleet()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
		}
		if (m_onProceed == null)
		{
			PLog.LogWarning("~~~ NOONE is listening to ONPROCEED !");
		}
		if (m_onProceed != null)
		{
			m_onProceed(string.Empty, m_campaignID);
		}
	}

	private void DontForceCreateNewFleet()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
		}
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	public string GetSelectedItem()
	{
		return InfoFiller.GetSelectedItemsName();
	}

	public void Hide()
	{
		m_gui.SetActiveRecursively(state: false);
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
	}

	public void Show()
	{
		InfoFiller.enabled = true;
		InfoFiller.UnSelect();
		FillData();
		btnProceed.controlIsEnabled = false;
		btnDelete.controlIsEnabled = false;
		m_gui.SetActiveRecursively(state: true);
		UserManClient userManClient = m_userManClient;
		userManClient.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Remove(userManClient.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
		UserManClient userManClient2 = m_userManClient;
		userManClient2.m_onUpdated = (UserManClient.UpdatedHandler)Delegate.Combine(userManClient2.m_onUpdated, new UserManClient.UpdatedHandler(OnUserManUpdate));
	}

	private void OnProceedPressed(IUIObject obj)
	{
		if (m_onProceed != null)
		{
			string selectedItem = GetSelectedItem();
			if (!string.IsNullOrEmpty(selectedItem))
			{
				m_onProceed(selectedItem, m_campaignID);
			}
		}
	}

	private void OnNewFleetPressed(IUIObject obj)
	{
		if (m_onProceed != null)
		{
			m_onProceed(string.Empty, m_campaignID);
		}
	}

	private void OnCancelPressed(IUIObject obj)
	{
		Exit();
	}

	private void OnDeletePressed(IUIObject obj)
	{
		m_msgBox = MsgBox.CreateYesNoMsgBox(m_guiCamera, "Are you sure you want to delete the selected fleet", OnConfirmDelete, null);
	}

	private void OnConfirmDelete()
	{
		string selectedItem = GetSelectedItem();
		if (string.IsNullOrEmpty(selectedItem))
		{
			Debug.Log("No item selected to delete, this should never happen since the button should be disabled !");
			return;
		}
		InfoFiller.RemoveFleet(selectedItem);
		m_userManClient.RemoveFleet(selectedItem);
		SelectedFleetChanged(string.Empty);
	}

	private void SelectedFleetChanged(string fleetName)
	{
		if (string.IsNullOrEmpty(fleetName))
		{
			btnProceed.controlIsEnabled = false;
			btnDelete.controlIsEnabled = false;
		}
		else
		{
			btnProceed.controlIsEnabled = true;
			btnDelete.controlIsEnabled = true;
		}
	}

	private void OnDonePressed(IUIObject obj)
	{
		Exit();
	}

	public void InitializeAndValidate()
	{
		if (!IsValid)
		{
			DebugUtils.Assert(Validate_lblTitle(), "ChooseFleetMenu failed to validate label named lblTitle !");
			DebugUtils.Assert(Validate_ListContainer(), "ChooseFleetMenu failed to validate transform named ListContainer !");
			DebugUtils.Assert(Validate_lstFleetInfos(), "ChooseFleetMenu failed to validate list named lstFleetInfos !");
			DebugUtils.Assert(Validate_fleetInfoFiller(), "ChooseFleetMenu failed to validate ListBox_FleetInfo_Filler-script on the lstFleetInfo-object !");
			DebugUtils.Assert(Validate_buttonsPanel(), "ChooseFleetMenu failed to validate panel named ButtonsPanel !");
			DebugUtils.Assert(Validate_btnProceed(), "ChooseFleetMenu failed to validate button named btnEdit !");
			DebugUtils.Assert(Validate_btnNewFleet(), "ChooseFleetMenu failed to validate button named btnNewFleet !");
			DebugUtils.Assert(Validate_btnCancel(), "ChooseFleetMenu failed to validate button named btnCancel !");
			DebugUtils.Assert(Validate_btnDelete(), "ChooseFleetMenu failed to validate button named btnDelete !");
			IsValid = true;
		}
	}

	private bool Validate_buttonsPanel()
	{
		if (!ValidateTransform("dialog_bg/ButtonsPanel", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		panelButtons = go.GetComponent<UIPanel>();
		return panelButtons != null;
	}

	private bool Validate_lblTitle()
	{
		if (!ValidateTransform("dialog_bg/lblTitle", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		lblTitle = go.GetComponent<SpriteText>();
		return lblTitle != null;
	}

	private bool Validate_ListContainer()
	{
		ListContainer = m_gui.transform.FindChild("dialog_bg/ListContainer");
		return ListContainer != null;
	}

	private bool Validate_lstFleetInfos()
	{
		if (!ValidateTransform("dialog_bg/ListContainer/lstFleetInfos", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		lstFleetInfos = go.GetComponent<UIScrollList>();
		return lstFleetInfos != null;
	}

	private bool Validate_btnProceed()
	{
		if (!ValidateTransform("dialog_bg/ButtonsPanel/btnEdit", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnProceed = go.GetComponent<UIButton>();
		return btnProceed != null;
	}

	private bool Validate_btnNewFleet()
	{
		if (!ValidateTransform("dialog_bg/ButtonsPanel/btnNewFleet", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnNewFleet = go.GetComponent<UIButton>();
		return btnNewFleet != null;
	}

	private bool Validate_btnCancel()
	{
		if (!ValidateTransform("dialog_bg/ButtonsPanel/btnCancel", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnCancel = go.GetComponent<UIButton>();
		return btnCancel != null;
	}

	private bool Validate_btnDelete()
	{
		if (!ValidateTransform("dialog_bg/ButtonsPanel/btnDelete", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnDelete = go.GetComponent<UIButton>();
		return btnDelete != null;
	}

	private bool Validate_fleetInfoFiller()
	{
		InfoFiller = List_FleetInfos.GetComponent<ListBox_FleetInfo_Filler>();
		return InfoFiller != null;
	}

	private bool ValidateTransform(string name, out GameObject go)
	{
		go = null;
		Transform transform = m_gui.transform.FindChild(name);
		if (transform == null)
		{
			return false;
		}
		go = transform.gameObject;
		return true;
	}
}
