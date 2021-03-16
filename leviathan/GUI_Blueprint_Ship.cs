#define DEBUG
using System;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/Gui/GUI_Blueprint_Ship")]
public sealed class GUI_Blueprint_Ship : MonoBehaviour
{
	public delegate void OnTrashPressedDelegate(GameObject target);

	private const bool DEBUG = true;

	public OnTrashPressedDelegate m_trashPressedDelegate;

	internal Texture2D m_iconTexture;

	public float m_sectionsScale_X = 1.8f;

	public float m_sectionsScale_Y = 2.2f;

	private FleetMenu m_menu;

	private int m_cost;

	private ShipDef m_definition;

	private string m_name = "Undefined";

	private string m_type = "Undefined";

	private GameObject m_sprIcon;

	private GameObject btnEdit;

	private GameObject btnDelete;

	private UIListItem m_UIListItem_Component;

	private GameObject m_lblName;

	private GameObject m_lblType;

	private GameObject m_lblCost;

	private GameObject m_sectionsContainer;

	private GameObject m_btnTrash;

	private bool m_hasInitialized;

	public ShipDef ShipDefinition
	{
		get
		{
			return m_definition;
		}
		private set
		{
			if (m_definition == value)
			{
				return;
			}
			m_definition = value;
			if (m_definition != null)
			{
				Cost = m_definition.m_value;
				Name = m_definition.m_name.Trim();
				Type = string.Empty;
				GameObject prefab = ObjectFactory.instance.GetPrefab(m_definition.m_prefab);
				Ship component = prefab.GetComponent<Ship>();
				DebugUtils.Assert(component != null, "Failed to get icon from prefab in ShipDef !");
				Texture2D icon = component.m_icon;
				if (icon == null)
				{
					Debug.LogWarning("Failed to get icon from prefab in ShipDef !");
				}
				IconTexture = icon;
			}
		}
	}

	public int Cost
	{
		get
		{
			return m_cost;
		}
		private set
		{
			if (m_cost != value)
			{
				m_cost = value;
				if (m_lblCost != null)
				{
					m_lblCost.GetComponent<SpriteText>().Text = Cost.ToString();
				}
			}
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			if (string.IsNullOrEmpty(m_name))
			{
				m_name = "Undefined";
			}
			m_lblName.GetComponent<SpriteText>().Text = m_name;
		}
	}

	public string Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
			m_lblType.GetComponent<SpriteText>().Text = m_type;
		}
	}

	public bool AllowDragDrop
	{
		get
		{
			if (m_UIListItem_Component != null)
			{
				return m_UIListItem_Component.IsDraggable;
			}
			return false;
		}
		set
		{
			if (m_UIListItem_Component != null)
			{
				m_UIListItem_Component.IsDraggable = value;
			}
		}
	}

	private float ListItemWidth => (!(m_UIListItem_Component == null)) ? m_UIListItem_Component.width : 0f;

	private float ListItemHeight => (!(m_UIListItem_Component == null)) ? m_UIListItem_Component.height : 0f;

	private Vector3 ListItemPosition => (!(m_UIListItem_Component == null)) ? m_UIListItem_Component.transform.position : Vector3.zero;

	public UIListItem UIListItemComponent => m_UIListItem_Component;

	public Texture2D IconTexture
	{
		get
		{
			return m_iconTexture;
		}
		set
		{
			m_iconTexture = value;
			UpdateIconSprite();
		}
	}

	private void Start()
	{
	}

	public void Initialize(ShipDef def, FleetMenu menu)
	{
		if (m_sectionsScale_X < 1f)
		{
			m_sectionsScale_X = 1f;
		}
		if (m_sectionsScale_Y < 1f)
		{
			m_sectionsScale_Y = 1f;
		}
		Initialize();
		ShipDefinition = def;
		m_menu = menu;
		DisableTrashButton();
	}

	public void Initialize()
	{
		if (!m_hasInitialized)
		{
			DebugUtils.Assert(Validate_Parent(), "GUI_Blueprint_Ship failed to validate gameObject !");
			DebugUtils.Assert(Validate_NameLabel(), "GUI_Blueprint_Ship failed to validate label named NameLabel !");
			DebugUtils.Assert(Validate_TypeLabel(), "GUI_Blueprint_Ship failed to validate label named TypeLabel !");
			DebugUtils.Assert(Validate_CostLabel(), "GUI_Blueprint_Ship failed to validate label named CostLabel !");
			DebugUtils.Assert(Validate_ShipSections(), "GUI_Blueprint_Ship failed to validate transform named ShipParts !");
			DebugUtils.Assert(Validate_TrashButton(), "GUI_Blueprint_Ship failed to validate button named btnTrash !");
			btnEdit = GuiUtils.FindChildOf(base.transform, "btnEdit");
			btnEdit.GetComponent<UIButton>().AddValueChangedDelegate(OnEditShipPressed);
			btnDelete = GuiUtils.FindChildOf(base.transform, "btnTrash");
			btnDelete.GetComponent<UIButton>().Text = Localize.instance.Translate(btnDelete.GetComponent<UIButton>().Text);
			btnEdit.GetComponent<UIButton>().Text = Localize.instance.Translate(btnEdit.GetComponent<UIButton>().Text);
			GuiUtils.ValidateSimpelSprite(base.gameObject, "Icon", out m_sprIcon);
			m_UIListItem_Component.AddDragDropDelegate(OnDragDropped);
			m_hasInitialized = true;
		}
	}

	public void DisableEdit()
	{
		btnEdit.SetActiveRecursively(state: false);
	}

	private void OnEditShipPressed(IUIObject obj)
	{
		PLog.LogWarning("EDIT SHIP " + m_definition.m_name);
		if (m_menu != null)
		{
			m_menu.OnShipSelectet(m_definition);
		}
	}

	public void EnableTrashButton(OnTrashPressedDelegate del)
	{
		m_btnTrash.SetActiveRecursively(state: true);
		m_btnTrash.active = true;
		Renderer[] componentsInChildren = m_btnTrash.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = true;
		}
		m_btnTrash.renderer.enabled = true;
		UIButton component = m_btnTrash.GetComponent<UIButton>();
		component.controlIsEnabled = true;
		component.Hide(tf: false);
		component.SetSize(50f, 50f);
		if (del != null)
		{
			m_trashPressedDelegate = del;
		}
		component.AddValueChangedDelegate(OnTrashButtonPressed);
	}

	public void DisableTrashButton()
	{
		m_btnTrash.SetActiveRecursively(state: false);
		m_btnTrash.active = false;
		m_btnTrash.renderer.enabled = false;
		Renderer[] componentsInChildren = m_btnTrash.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
		UIButton component = m_btnTrash.GetComponent<UIButton>();
		component.controlIsEnabled = false;
		component.Hide(tf: true);
		component.enabled = false;
		component.SetSize(0f, 0f);
		m_trashPressedDelegate = null;
		component.RemoveValueChangedDelegate(OnTrashButtonPressed);
	}

	private void OnDragDropped(EZDragDropParams dropParams)
	{
		if (dropParams.dragObj == null)
		{
			return;
		}
		if (string.Compare(dropParams.evt.ToString().Trim().ToLower(), "dropped") == 0)
		{
			if (dropParams.dragObj.DropTarget == null)
			{
				dropParams.dragObj.DropHandled = false;
				dropParams.dragObj.CancelDrag();
				return;
			}
			dropParams.dragObj.DropHandled = HandleDrop(dropParams.dragObj.DropTarget);
			dropParams.dragObj.CancelDrag();
		}
		dropParams.dragObj.DropHandled = true;
	}

	private void OnTrashButtonPressed(IUIObject ignore)
	{
		if (m_trashPressedDelegate != null)
		{
			m_trashPressedDelegate(base.gameObject);
		}
	}

	private bool HandleDrop(GameObject target)
	{
		if (string.Compare(target.name.ToLower().Trim(), "fleeteditor_previewarea") == 0)
		{
			FleetEditor_PreviewArea component = target.GetComponent<FleetEditor_PreviewArea>();
			if (component == null)
			{
				Debug.LogWarning("Trying to interact with object named \"FleetEditor_PreviewArea\" that does not have the correct script !");
				return false;
			}
			component.AddClone(this);
			return true;
		}
		return false;
	}

	[Obsolete("This does not scale all the textures of the children properly. Don't use it untill solved.")]
	private void SetSizes(float width, float height)
	{
		if (m_UIListItem_Component != null)
		{
			m_UIListItem_Component.SetSize(width, height);
		}
	}

	private void UpdateIconSprite()
	{
		if (m_sectionsContainer != null)
		{
			SimpleSprite component = m_sprIcon.GetComponent<SimpleSprite>();
			float width = component.width;
			float height = component.height;
			component.SetTexture(m_iconTexture);
			float x = 64f;
			float y = 64f;
			if (m_iconTexture != null)
			{
				x = m_iconTexture.width;
				y = m_iconTexture.height;
			}
			component.Setup(width, height, new Vector2(0f, y), new Vector2(x, y));
		}
	}

	public void OnTap()
	{
	}

	public void OnPress()
	{
	}

	public void OnRelease()
	{
	}

	public void OnMove()
	{
	}

	private bool Validate_NameLabel()
	{
		bool flag = false;
		Transform transform = base.transform.Find("NameLabel");
		m_lblName = ((!(transform == null)) ? transform.gameObject : null);
		DebugUtils.Assert(m_lblName != null, "GUI_Blueprint_Ship script must be attached to a GameObject that has an GameObject named \"NameLabel\"");
		if (m_lblName == null)
		{
			return false;
		}
		flag = m_lblName.GetComponent<SpriteText>() != null;
		DebugUtils.Assert(flag, "GUI_Blueprint_Ship LabelName is not a SpriteText !");
		return flag;
	}

	private bool Validate_TypeLabel()
	{
		bool flag = false;
		Transform transform = base.transform.Find("TypeLabel");
		m_lblType = ((!(transform == null)) ? transform.gameObject : null);
		DebugUtils.Assert(m_lblType != null, "GUI_Blueprint_Ship script must be attached to a GameObject that has an GameObject named \"TypeLabel\"");
		if (m_lblType == null)
		{
			return false;
		}
		flag = m_lblType.GetComponent<SpriteText>() != null;
		DebugUtils.Assert(flag, "GUI_Blueprint_Ship TypeLabel is not a SpriteText !");
		return flag;
	}

	private bool Validate_CostLabel()
	{
		bool flag = false;
		Transform transform = base.transform.Find("CostLabel");
		m_lblCost = ((!(transform == null)) ? transform.gameObject : null);
		DebugUtils.Assert(m_lblCost != null, "GUI_Blueprint_Ship script must be attached to a GameObject that has an GameObject named \"CostLabel\"");
		if (m_lblCost == null)
		{
			return false;
		}
		flag = m_lblCost.GetComponent<SpriteText>() != null;
		DebugUtils.Assert(flag, "GUI_Blueprint_Ship CostLabel is not a SpriteText !");
		return flag;
	}

	private bool Validate_Parent()
	{
		m_UIListItem_Component = base.gameObject.GetComponent<UIListItem>();
		DebugUtils.Assert(m_UIListItem_Component != null, "GUI_Blueprint_Ship script must be attached to a GameObject that has an IUIListItem");
		return m_UIListItem_Component != null;
	}

	private bool Validate_ShipSections()
	{
		Transform transform = base.transform.Find("ShipParts");
		m_sectionsContainer = ((!(transform == null)) ? transform.gameObject : null);
		DebugUtils.Assert(m_sectionsContainer != null, "GUI_Blueprint_Ship script must be attached to a GameObject that has an GameObject named \"ShipParts\"");
		if (m_sectionsContainer == null)
		{
			return false;
		}
		GameObject gameObject = m_sectionsContainer.transform.GetChild(0).gameObject;
		bool flag = gameObject.GetComponent<SimpleSprite>() != null;
		DebugUtils.Assert(flag, "GUI_Blueprint_Ship detects that ShipPart 1 isn't a SimpleSprite !");
		if (!flag)
		{
			return false;
		}
		return true;
	}

	private bool Validate_TrashButton()
	{
		if (!ValidateTransform("btnTrash", out m_btnTrash))
		{
			return false;
		}
		if (m_btnTrash == null)
		{
			return false;
		}
		return m_btnTrash.GetComponent<UIButton>() != null;
	}

	private bool ValidateTransform(string name, out GameObject go)
	{
		go = null;
		Transform transform = base.gameObject.transform.FindChild(name);
		if (transform == null)
		{
			return false;
		}
		go = transform.gameObject;
		return true;
	}

	internal void Hide()
	{
		base.gameObject.SetActiveRecursively(state: false);
	}

	internal void Show()
	{
		base.gameObject.SetActiveRecursively(state: true);
		DisableEdit();
	}
}
