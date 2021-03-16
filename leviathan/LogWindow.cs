#define DEBUG
using System.Collections.Generic;
using UnityEngine;

public class LogWindow
{
	public enum LogWindow_ScreenAlignment
	{
		PrefabDefined,
		Left,
		Right
	}

	public GameObject m_gui;

	public bool m_autoSlideToFocusLastItem = true;

	public float m_fillPercentNeededBeforeScrollEnabled = 0.95f;

	protected bool m_isValid;

	protected GameObject m_guiCam;

	protected bool m_isRevealed;

	protected bool m_isHidden;

	protected float m_totalHeightOfItemsInList;

	protected UIListItem m_latestEntry;

	private UIButton btnExpand;

	private UIInteractivePanel m_interactivePanel;

	protected UIScrollList m_UIList;

	protected UISlider m_slider;

	private Vector3 m_definedRevealedPos;

	private Vector3 m_definedHiddenPos;

	private StatusLight_Basic m_newMessageIcon;

	private Transform m_anchor;

	public bool IsValid
	{
		get
		{
			return m_isValid;
		}
		private set
		{
			m_isValid = value;
		}
	}

	public float Width
	{
		get
		{
			if (m_UIList == null)
			{
				return 0f;
			}
			float x = m_UIList.viewableArea.x;
			if (m_slider == null)
			{
				return x;
			}
			return x + m_slider.height;
		}
		set
		{
			if (!(m_UIList == null))
			{
				float num = 0f;
				if (m_slider == null)
				{
					num = m_slider.height;
				}
				m_UIList.viewableArea = new Vector2(value - num, m_UIList.viewableArea.y);
			}
		}
	}

	public float Height
	{
		get
		{
			if (m_UIList == null)
			{
				return 0f;
			}
			return m_UIList.viewableArea.y;
		}
		set
		{
			if (!(m_UIList == null))
			{
				m_UIList.viewableArea = new Vector2(m_UIList.viewableArea.x, value);
			}
		}
	}

	public void Close()
	{
		if (m_gui != null)
		{
			SpriteText[] componentsInChildren = m_gui.GetComponentsInChildren<SpriteText>();
			foreach (SpriteText spriteText in componentsInChildren)
			{
				spriteText.Delete();
			}
			m_gui.SetActiveRecursively(state: false);
			Object.DestroyImmediate(m_gui);
		}
	}

	public virtual void Initialize(GameObject guiCam, bool startVisible, List<string> messages, LogWindow_ScreenAlignment alignment)
	{
		m_guiCam = guiCam;
		LoadGUI();
		ValidateComponents();
		m_newMessageIcon.SetOnOff(onOff: false);
		EZTransition transition = m_interactivePanel.GetTransition(0);
		EZTransition transition2 = m_interactivePanel.GetTransition(2);
		switch (alignment)
		{
		case LogWindow_ScreenAlignment.Left:
			SetToLeftOfScreen();
			break;
		case LogWindow_ScreenAlignment.Right:
			SetToRightOfScreen();
			break;
		default:
			m_definedRevealedPos = transition.animParams[0].vec;
			m_definedHiddenPos = transition2.animParams[0].vec;
			break;
		}
		transition.AddTransitionEndDelegate(RevealDone);
		transition2.AddTransitionEndDelegate(DismissDone);
		m_interactivePanel.SetDragDropDelegate(null);
		m_interactivePanel.SetDragDropInternalDelegate(null);
		m_interactivePanel.SetValueChangedDelegate(null);
		m_interactivePanel.SetInputDelegate(null);
		m_interactivePanel.controlIsEnabled = false;
		btnExpand.AddInputDelegate(OnExpand);
		m_fillPercentNeededBeforeScrollEnabled = Mathf.Clamp01(m_fillPercentNeededBeforeScrollEnabled);
		m_totalHeightOfItemsInList = ((!m_UIList.spacingAtEnds) ? 0f : m_UIList.itemSpacing);
		m_latestEntry = null;
		if (messages != null && messages.Count > 0)
		{
			messages.ForEach(delegate(string m)
			{
				AddText(m);
			});
		}
		else
		{
			UpdateSlider();
		}
		if (startVisible)
		{
			QuickSetRevealed();
		}
		else
		{
			QuickSetHidden();
		}
	}

	public void SetToRightOfScreen()
	{
		SetToRightOfScreen(0f, -1f);
	}

	public void SetToRightOfScreen(float yFromCenter, float z)
	{
		if (m_guiCam.camera == null)
		{
			PLog.LogError("LogWindow could not be set to the right of screen, invalid camera !");
			return;
		}
		float pixelWidth = m_guiCam.camera.pixelWidth;
		float num = m_guiCam.camera.pixelHeight / 2f;
		Vector3 position = new Vector3(pixelWidth + Width / 2f, num + yFromCenter, z);
		position = m_guiCam.camera.ScreenToWorldPoint(position);
		Vector3 visiblePos = position - new Vector3(Width, 0f, 0f);
		SetVisiblePos(visiblePos);
		SetHiddenPos(position);
	}

	public void SetToLeftOfScreen()
	{
		SetToLeftOfScreen(0f, -1f);
	}

	public void SetToLeftOfScreen(float yFromCenter, float z)
	{
		if (m_guiCam.camera == null)
		{
			PLog.LogError("LogWindow could not be set to the left of screen, invalid camera !");
			return;
		}
		float num = 0f;
		float num2 = m_guiCam.camera.pixelHeight / 2f;
		Vector3 position = new Vector3(num - Width / 2f, num2 + yFromCenter, z);
		position = m_guiCam.camera.ScreenToWorldPoint(position);
		Vector3 visiblePos = position + new Vector3(Width, 0f, 0f);
		SetVisiblePos(visiblePos);
		SetHiddenPos(position);
	}

	public void SetVisiblePos(Vector3 screenspacePos)
	{
		DebugUtils.Assert(m_isValid, "Can't call SetVisiblePos, need to call Initialize() first !");
		EZTransition transition = m_interactivePanel.GetTransition(0);
		DebugUtils.Assert(transition != null, "Failed to find transition[0] !");
		m_definedRevealedPos = (transition.animParams[0].vec = screenspacePos);
	}

	public void SetHiddenPos(Vector3 screenspacePos)
	{
		DebugUtils.Assert(m_isValid, "Can't call SetHiddenPos, need to call Initialize() first !");
		EZTransition transition = m_interactivePanel.GetTransition(2);
		DebugUtils.Assert(transition != null, "Failed to find transition[2] !");
		m_definedHiddenPos = (transition.animParams[0].vec = screenspacePos);
	}

	public virtual void AddText(string text)
	{
		if (m_isHidden)
		{
			m_newMessageIcon.SetOnOff(onOff: true);
		}
		CreateTextItem(text);
	}

	protected virtual void CreateTextItem(string text)
	{
		LogMsg logMsg = new LogMsg_Text(m_guiCam, text);
		m_totalHeightOfItemsInList += logMsg.Height();
		m_totalHeightOfItemsInList += m_UIList.itemSpacing;
		m_latestEntry = logMsg.m_listItemComponent;
		m_UIList.AddItem(m_latestEntry);
		UpdateSlider();
	}

	protected virtual void UpdateSlider()
	{
		bool flag = m_totalHeightOfItemsInList >= m_fillPercentNeededBeforeScrollEnabled * (m_UIList.viewableArea.y - m_UIList.extraEndSpacing);
		m_slider.enabled = flag;
		UIScrollKnob knob = m_slider.GetKnob();
		if (knob != null)
		{
			knob.controlIsEnabled = flag;
			knob.enabled = true;
		}
		if (m_autoSlideToFocusLastItem && flag && m_latestEntry != null)
		{
			m_UIList.ScrollToItem(m_latestEntry, 0f);
		}
	}

	protected virtual void LoadGUI()
	{
		if (m_gui == null)
		{
			m_gui = GuiUtils.CreateGui("LogDisplay/LogWindow", m_guiCam);
		}
	}

	public void TurnOffRenderers()
	{
		if (m_gui != null)
		{
			m_gui.SetActiveRecursively(state: false);
		}
	}

	public void TurnOnRenderers()
	{
		if (m_gui != null)
		{
			m_gui.SetActiveRecursively(state: true);
		}
	}

	public void SetActiveRecursively(bool p)
	{
		if (p || !(m_gui == null))
		{
			if (p && m_gui == null)
			{
				LoadGUI();
			}
			m_gui.SetActiveRecursively(p);
		}
	}

	public void Hide()
	{
		btnExpand.controlIsEnabled = false;
		m_isRevealed = false;
		m_interactivePanel.Hide();
	}

	public void Reveal()
	{
		btnExpand.controlIsEnabled = false;
		m_isHidden = false;
		m_interactivePanel.Reveal();
	}

	private void OnExpand(ref POINTER_INFO ptr)
	{
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP || ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
			if (m_isHidden)
			{
				Reveal();
			}
			else if (m_isRevealed)
			{
				Hide();
			}
		}
	}

	private string DEBUG_CurrentState()
	{
		return "We are now " + (m_isHidden ? "Hidden" : ((!m_isRevealed) ? "In transition" : "Revealed"));
	}

	private void RevealDone(EZTransition transition)
	{
		btnExpand.controlIsEnabled = true;
		m_isRevealed = true;
		m_newMessageIcon.SetOnOff(onOff: false);
	}

	private void DismissDone(EZTransition transition)
	{
		btnExpand.controlIsEnabled = true;
		m_isHidden = true;
	}

	public void QuickSetHidden()
	{
		m_interactivePanel.StopAllCoroutines();
		m_gui.transform.position = m_definedHiddenPos;
		m_isRevealed = false;
		m_isHidden = true;
		btnExpand.controlIsEnabled = true;
	}

	public void QuickSetRevealed()
	{
		m_interactivePanel.StopAllCoroutines();
		m_gui.transform.position = m_definedRevealedPos;
		m_isRevealed = true;
		m_isHidden = false;
		btnExpand.controlIsEnabled = true;
		m_newMessageIcon.SetOnOff(onOff: false);
		UpdateSlider();
	}

	private void DEBUG_DefinedTransitionPositions()
	{
	}

	private void ValidateComponents()
	{
		if (!m_isValid)
		{
			DebugUtils.Assert(Validate_InteractivePanel(), "LogWindow base does not have a UIInteractivePanel-component, or m_gui is null !");
			DebugUtils.Assert(Validate_ExpandButton(), "LogWindow failed to validate button named btnExpand !");
			DebugUtils.Assert(Validate_List(), "LogWindow failed to validate label named UIScrollList named list !");
			DebugUtils.Assert(Validate_NewMessageIcon(), "LogWindow failed to validate StatusLight_Basic named iconNewMessage !");
			DebugUtils.Assert(Validate_Slider(), "LogWindow failed to validate UISlider named slider !");
			DoAdditionalValidation();
			m_isValid = true;
		}
	}

	protected virtual void DoAdditionalValidation()
	{
	}

	private bool Validate_InteractivePanel()
	{
		if (m_gui == null)
		{
			return false;
		}
		m_interactivePanel = m_gui.GetComponent<UIInteractivePanel>();
		return m_interactivePanel != null;
	}

	private bool Validate_Slider()
	{
		if (!ValidateTransform("slider", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_slider = go.GetComponent<UISlider>();
		return m_slider != null;
	}

	private bool Validate_ExpandButton()
	{
		if (!ValidateTransform("btnExpand", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnExpand = go.GetComponent<UIButton>();
		return btnExpand != null;
	}

	private bool Validate_NewMessageIcon()
	{
		if (!ValidateTransform("iconNewMessage", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_newMessageIcon = go.GetComponent<StatusLight_Basic>();
		m_newMessageIcon.Initialize();
		m_newMessageIcon.SetOnOff(onOff: false);
		return m_newMessageIcon != null;
	}

	private bool Validate_List()
	{
		if (!ValidateTransform("list", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_UIList = go.GetComponent<UIScrollList>();
		return m_UIList != null;
	}

	protected virtual bool ValidateTransform(string name, out GameObject go)
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
