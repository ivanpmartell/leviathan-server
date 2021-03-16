#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowerMenu
{
	public static float m_maxGuiScale = 2.5f;

	public Action<HPModule> m_onModuleSelected;

	public Action<HPModule> m_onModuleDragged;

	public Action<Ship> m_onMoveForward;

	public Action<Ship> m_onMoveReverse;

	public Action<Ship> m_onMoveRotate;

	public Action<Ship> m_onToggleSupply;

	private List<ModuleButton> m_buttons = new List<ModuleButton>();

	private MoveButton m_forwardButton;

	private MoveButton m_reverseButton;

	private MoveButton m_rotateButton;

	private AnchorButton m_anchorButton;

	private GameObject m_supplyButton;

	private GameObject m_bkg;

	private GameObject m_guiCamera;

	private Camera m_gameCamera;

	private LineDrawer m_lineDrawer;

	private Ship m_ship;

	private int m_lineType;

	private bool m_canOrder;

	private bool m_localOwner;

	private ModuleButton m_dragging;

	private bool m_moveForward;

	private bool m_moveReverse;

	private bool m_moveRotate;

	private float m_lowestScreenPos;

	public FlowerMenu(Camera gameCamera, GameObject guiCamera, Ship ship, bool canOrder, bool localOwner)
	{
		m_guiCamera = guiCamera;
		m_gameCamera = gameCamera;
		m_ship = ship;
		m_canOrder = canOrder;
		m_localOwner = localOwner;
		m_lineDrawer = m_gameCamera.GetComponent<LineDrawer>();
		m_lineType = m_lineDrawer.GetTypeID("flowerLine");
		List<HPModule> modules = new List<HPModule>();
		m_ship.GetAllHPModules(ref modules);
		m_bkg = GuiUtils.CreateGui("IngameGui/CommandRose/CommandRose", m_guiCamera);
		DebugUtils.Assert(m_bkg);
		bool currentMaintenanceMode = ship.GetCurrentMaintenanceMode();
		foreach (HPModule item in modules)
		{
			m_buttons.Add(new ModuleButton(item, m_guiCamera, m_localOwner, OnButtonPressed, OnButtonDragged, canOrder && !currentMaintenanceMode));
		}
		SortButtons();
		if (m_canOrder && !currentMaintenanceMode)
		{
			m_forwardButton = new MoveButton(MoveButton.MoveType.Forward, m_guiCamera, OnButtonForward);
			m_reverseButton = new MoveButton(MoveButton.MoveType.Reverse, m_guiCamera, OnButtonReverse);
			m_rotateButton = new MoveButton(MoveButton.MoveType.Rotate, m_guiCamera, OnButtonRotate);
			SupportShip supportShip = ship as SupportShip;
			if (supportShip != null)
			{
				m_supplyButton = CreateSupplyButton(supportShip);
			}
		}
		UpdateGuiPos();
	}

	private GameObject CreateSupplyButton(SupportShip supportShip)
	{
		GameObject gameObject = GuiUtils.CreateGui("IngameGui/FlowerButtonSupply", m_guiCamera);
		GameObject gameObject2 = GuiUtils.FindChildOf(gameObject, "Enable");
		GameObject gameObject3 = GuiUtils.FindChildOf(gameObject, "Disable");
		gameObject2.SetActiveRecursively(!supportShip.IsSupplyEnabled());
		gameObject3.SetActiveRecursively(supportShip.IsSupplyEnabled());
		gameObject2.GetComponent<UIButton>().SetValueChangedDelegate(OnSupplyToggle);
		gameObject3.GetComponent<UIButton>().SetValueChangedDelegate(OnSupplyToggle);
		return gameObject;
	}

	private void PlaceSupplyButton()
	{
		if (m_supplyButton != null)
		{
			Camera camera = m_guiCamera.camera;
			Vector3 pos = m_ship.transform.position + new Vector3(0f, m_ship.m_deckHeight, 0f);
			Vector3 position = GuiUtils.WorldToGuiPos(m_gameCamera, camera, pos);
			m_supplyButton.transform.position = position;
		}
	}

	private void OnSupplyToggle(IUIObject obj)
	{
		m_onToggleSupply(m_ship);
		UnityEngine.Object.Destroy(m_supplyButton);
		m_supplyButton = CreateSupplyButton(m_ship as SupportShip);
	}

	private void SortButtons()
	{
		Vector3 forward = m_ship.transform.forward;
		Vector3 position = m_ship.transform.position;
		foreach (ModuleButton button in m_buttons)
		{
			Vector3 position2 = button.m_module.transform.position;
			Vector3 rhs = position2 - position;
			button.m_sortPos = 0f - Vector3.Dot(forward, rhs);
		}
		m_buttons.Sort((ModuleButton a, ModuleButton b) => a.m_sortPos.CompareTo(b.m_sortPos));
	}

	public Ship GetShip()
	{
		return m_ship;
	}

	public void Close()
	{
		if (m_dragging != null)
		{
			m_dragging.m_button.GetComponent<UIButton>().CancelDrag();
		}
		foreach (ModuleButton button in m_buttons)
		{
			UnityEngine.Object.Destroy(button.m_button);
		}
		if (m_forwardButton != null)
		{
			UnityEngine.Object.Destroy(m_forwardButton.m_button);
		}
		if (m_reverseButton != null)
		{
			UnityEngine.Object.Destroy(m_reverseButton.m_button);
		}
		if (m_rotateButton != null)
		{
			UnityEngine.Object.Destroy(m_rotateButton.m_button);
		}
		if (m_anchorButton != null)
		{
			m_anchorButton.Close();
		}
		UnityEngine.Object.Destroy(m_bkg);
		if (m_supplyButton != null)
		{
			UnityEngine.Object.Destroy(m_supplyButton);
		}
	}

	public void LateUpdate(float dt)
	{
		UpdateGuiPos();
	}

	private void UpdateGuiPos()
	{
		m_lowestScreenPos = 10000f;
		float num = Mathf.Tan((float)Math.PI / 180f * m_gameCamera.fieldOfView * 0.5f);
		float guiScale = Vector3.Distance(m_gameCamera.transform.position, m_ship.transform.position) * num * 0.02f;
		Camera camera = m_guiCamera.camera;
		PlaceBkg(guiScale);
		PlaceModuleButtons(guiScale);
		if (m_supplyButton != null)
		{
			PlaceSupplyButton();
		}
		if (m_anchorButton != null)
		{
			m_anchorButton.UpdatePosition(guiScale, camera, m_gameCamera, ref m_lowestScreenPos);
		}
		if (m_forwardButton != null)
		{
			m_forwardButton.UpdatePosition(guiScale, m_ship, camera, m_gameCamera, ref m_lowestScreenPos);
		}
		if (m_reverseButton != null)
		{
			m_reverseButton.UpdatePosition(guiScale, m_ship, camera, m_gameCamera, ref m_lowestScreenPos);
		}
		if (m_rotateButton != null)
		{
			m_rotateButton.UpdatePosition(guiScale, m_ship, camera, m_gameCamera, ref m_lowestScreenPos);
		}
	}

	private void PlaceBkg(float guiScale)
	{
		Camera camera = m_guiCamera.camera;
		float num = Mathf.Clamp(guiScale, 1f, m_maxGuiScale);
		float num2 = (m_ship.GetLength() / 2f + 6f) * num;
		float num3 = (m_ship.GetWidth() / 2f + 6f) * num;
		m_bkg.transform.position = m_ship.transform.position + new Vector3(0f, m_ship.m_deckHeight, 0f);
		m_bkg.transform.rotation = m_ship.GetRealRot();
		SimpleSprite component = m_bkg.GetComponent<SimpleSprite>();
		float num4 = 1.5f;
		component.SetSize(num3 * 2f * num4, num2 * 2f * num4);
	}

	private void PlaceModuleButtons(float guiScale)
	{
		Camera camera = m_guiCamera.camera;
		float num = Mathf.Clamp(guiScale, 1f, m_maxGuiScale);
		float num2 = m_ship.m_Width / 2f;
		Vector3 right = m_ship.transform.right;
		Vector3 forward = m_ship.transform.forward;
		Vector3 position = m_ship.transform.position;
		foreach (ModuleButton button in m_buttons)
		{
			button.m_point1 = Vector3.zero;
		}
		List<ModuleButton> list = new List<ModuleButton>();
		List<ModuleButton> list2 = new List<ModuleButton>();
		foreach (ModuleButton button2 in m_buttons)
		{
			Vector3 position2 = button2.m_module.transform.position;
			Vector3 rhs = position2 - position;
			Vector3 normalized = rhs.normalized;
			float f = Vector3.Dot(right, rhs);
			if (Mathf.Abs(f) < 0.1f)
			{
				f = ((list.Count >= list2.Count) ? 1f : (-1f));
			}
			float num3 = Mathf.Sign(f);
			if ((double)num3 < 0.1)
			{
				list.Add(button2);
			}
			else
			{
				list2.Add(button2);
			}
		}
		float length = (m_ship.GetLength() / 2f + 6f) * num;
		float width = (m_ship.GetWidth() / 2f + 6f) * num;
		LayoutSide(list, left: true, length, width, camera);
		LayoutSide(list2, left: false, length, width, camera);
	}

	private void LayoutSide(List<ModuleButton> buttons, bool left, float length, float width, Camera guiCamera)
	{
		float num = 160f / (float)(buttons.Count + 1);
		for (int i = 0; i < buttons.Count; i++)
		{
			float num2 = 10f + (float)(i + 1) * num;
			float f = (float)Math.PI / 180f * num2;
			Vector3 vector = new Vector3(Mathf.Sin(f) * width, 0f, Mathf.Cos(f) * length);
			if (left)
			{
				vector.x *= -1f;
			}
			vector = m_ship.transform.TransformDirection(vector);
			ModuleButton moduleButton = buttons[i];
			moduleButton.m_point0 = moduleButton.m_module.transform.position;
			moduleButton.m_point1 = m_ship.transform.position + vector;
			Vector3 position = GuiUtils.WorldToGuiPos(m_gameCamera, guiCamera, moduleButton.m_point1);
			moduleButton.m_button.transform.position = position;
			if (moduleButton.m_button.transform.position.y - 19f < m_lowestScreenPos)
			{
				m_lowestScreenPos = moduleButton.m_button.transform.position.y - 19f;
			}
		}
	}

	public void Update(float dt)
	{
		if (m_dragging != null)
		{
			m_dragging.m_button.GetComponent<UIButton>().CancelDrag();
			m_onModuleDragged(m_dragging.m_module);
			m_dragging = null;
			return;
		}
		if (m_moveForward && m_onMoveForward != null)
		{
			m_forwardButton.m_button.GetComponent<UIButton>().CancelDrag();
			m_onMoveForward(m_ship);
			return;
		}
		if (m_moveReverse && m_onMoveReverse != null)
		{
			m_reverseButton.m_button.GetComponent<UIButton>().CancelDrag();
			m_onMoveReverse(m_ship);
			return;
		}
		if (m_moveRotate && m_onMoveRotate != null)
		{
			m_rotateButton.m_button.GetComponent<UIButton>().CancelDrag();
			m_onMoveRotate(m_ship);
			return;
		}
		foreach (ModuleButton button in m_buttons)
		{
			m_lineDrawer.DrawLine(button.m_point0, button.m_point1, m_lineType, 0.2f);
			Gun gun = button.m_module as Gun;
			if ((bool)gun && gun.GetMaxAmmo() > 0 && m_localOwner)
			{
				float num = gun.GetLoadedSalvo() + (float)gun.GetAmmo();
				int num2 = (int)(num / (float)gun.GetSalvoSize());
				button.m_ammoText.Text = num2.ToString();
				if (num <= 0f && !gun.IsDisabled())
				{
					button.m_noammoIcon.SetActiveRecursively(state: true);
				}
				else
				{
					button.m_noammoIcon.SetActiveRecursively(state: false);
				}
			}
			if (button.m_module.IsDisabled())
			{
				button.m_disabledButton.SetActiveRecursively(state: true);
				button.m_button.GetComponent<UIButton>().SetControlState(UIButton.CONTROL_STATE.DISABLED);
			}
			else
			{
				button.m_disabledButton.SetActiveRecursively(state: false);
				UIButton component = button.m_button.GetComponent<UIButton>();
				if (component.controlState == UIButton.CONTROL_STATE.DISABLED)
				{
					component.SetControlState(UIButton.CONTROL_STATE.NORMAL);
				}
			}
			button.m_module.GetChargeLevel(out var i, out var time);
			button.SetCharge(i, time);
		}
	}

	private void OnButtonPressed(IUIObject obj)
	{
		GameObject gameObject = obj.gameObject;
		foreach (ModuleButton button in m_buttons)
		{
			if (button.m_button == gameObject || button.m_disabledButton == gameObject)
			{
				m_onModuleSelected(button.m_module);
				break;
			}
		}
	}

	private void OnButtonDragged(EZDragDropParams obj)
	{
		if (m_dragging != null)
		{
			return;
		}
		GameObject gameObject = obj.dragObj.gameObject;
		foreach (ModuleButton button in m_buttons)
		{
			if (button.m_button == gameObject)
			{
				ToolTipDisplay toolTip = ToolTipDisplay.GetToolTip(button.m_button);
				if ((bool)toolTip)
				{
					toolTip.StopToolTip();
				}
				m_dragging = button;
				break;
			}
		}
	}

	private void OnButtonForward(EZDragDropParams obj)
	{
		m_moveForward = true;
	}

	private void OnButtonReverse(EZDragDropParams obj)
	{
		m_moveReverse = true;
	}

	private void OnButtonRotate(EZDragDropParams obj)
	{
		m_moveRotate = true;
	}

	public float GetLowestScreenPos()
	{
		return m_lowestScreenPos;
	}

	public bool IsMouseOver()
	{
		UIManager instance = UIManager.instance;
		foreach (ModuleButton button in m_buttons)
		{
			if (button.MouseOver())
			{
				return true;
			}
		}
		if (m_forwardButton != null && m_forwardButton.MouseOver())
		{
			return true;
		}
		if (m_reverseButton != null && m_reverseButton.MouseOver())
		{
			return true;
		}
		if (m_rotateButton != null && m_rotateButton.MouseOver())
		{
			return true;
		}
		if (m_anchorButton != null && m_anchorButton.MouseOver())
		{
			return true;
		}
		if (m_supplyButton != null && GuiUtils.HasPointerRecursive(UIManager.instance, m_supplyButton))
		{
			return true;
		}
		return false;
	}
}
