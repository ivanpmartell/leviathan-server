#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Scripts/Gui/GUI_Blueprint_Part")]
public sealed class GUI_Blueprint_Part : MonoBehaviour
{
	public delegate void OnTrashPressedDelegate(GameObject target);

	public OnTrashPressedDelegate m_trashPressedDelegate;

	private ShipMenu m_menu;

	private string m_name = "Undefined";

	private UIListItem m_UIListItem_Component;

	private GameObject m_lblName;

	private GameObject m_lblInfo;

	private GameObject m_lblSize;

	private GameObject lblCost;

	private GameObject m_sprIcon;

	private bool m_hasInitialized;

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

	private void Start()
	{
	}

	public void Initialize(string name, ShipMenu menu)
	{
		m_name = name;
		m_menu = menu;
	}

	public void Initialize()
	{
		if (!m_hasInitialized)
		{
			base.gameObject.GetComponent<UIListItem>().AddInputDelegate(PartsDelegate);
			GuiUtils.ValidateGuLabel(base.gameObject, "lblName", out m_lblName);
			GuiUtils.ValidateGuLabel(base.gameObject, "lblInfo", out m_lblInfo);
			GuiUtils.ValidateGuLabel(base.gameObject, "lblSize", out m_lblSize);
			GuiUtils.ValidateGuLabel(base.gameObject, "lblCost", out lblCost);
			GuiUtils.ValidateSimpelSprite(base.gameObject, "Icon", out m_sprIcon);
			m_hasInitialized = true;
		}
	}

	private void PartsDelegate(ref POINTER_INFO ptr)
	{
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.DRAG && Input.mousePosition.x > 300f)
		{
			ptr.evt = POINTER_INFO.INPUT_EVENT.RELEASE_OFF;
		}
		else if (ptr.evt != POINTER_INFO.INPUT_EVENT.MOVE && ptr.evt != 0)
		{
		}
	}

	private void FillData()
	{
		GameObject prefab = ObjectFactory.instance.GetPrefab(m_name);
		if (prefab == null)
		{
			m_lblName.GetComponent<SpriteText>().Text = "DEV: " + m_name;
			m_lblInfo.GetComponent<SpriteText>().Text = "DO NOT USE";
			m_lblSize.GetComponent<SpriteText>().Text = "DO NOT USE";
			return;
		}
		Section component = prefab.GetComponent<Section>();
		if (component != null)
		{
			SectionSettings section = ComponentDB.instance.GetSection(m_name);
			DebugUtils.Assert(section != null);
			m_lblName.GetComponent<SpriteText>().Text = Localize.instance.Translate(component.GetName());
			lblCost.GetComponent<SpriteText>().Text = section.m_value.ToString();
			if (component.m_GUITexture != null)
			{
				SimpleSprite component2 = m_sprIcon.GetComponent<SimpleSprite>();
				float width = component2.width;
				float height = component2.height;
				component2.SetTexture(component.m_GUITexture);
				float x = 64f;
				float y = 64f;
				if (component.m_GUITexture != null)
				{
					x = component.m_GUITexture.width;
					y = component.m_GUITexture.height;
				}
				SimpleSprite component3 = m_sprIcon.GetComponent<SimpleSprite>();
				component3.Setup(width, height, new Vector2(0f, y), new Vector2(x, y));
				component3.SetTexture(component.m_GUITexture);
				component3.UpdateUVs();
			}
		}
		HPModule component4 = prefab.GetComponent<HPModule>();
		if (!(component4 != null))
		{
			return;
		}
		HPModuleSettings module = ComponentDB.instance.GetModule(m_name);
		DebugUtils.Assert(module != null);
		List<string> hardpointInfo = component4.GetHardpointInfo();
		string text = component4.m_width + "x" + component4.m_length;
		m_lblName.GetComponent<SpriteText>().Text = component4.GetName();
		lblCost.GetComponent<SpriteText>().Text = module.m_value.ToString();
		m_lblSize.GetComponent<SpriteText>().Text = text;
		if (hardpointInfo.Count >= 1)
		{
			m_lblInfo.GetComponent<SpriteText>().Text = hardpointInfo[0];
		}
		if (component4.m_GUITexture != null)
		{
			SimpleSprite component5 = m_sprIcon.GetComponent<SimpleSprite>();
			component5.SetTexture(component4.m_GUITexture);
			float width2 = component5.width;
			float height2 = component5.height;
			float x2 = 64f;
			float y2 = 64f;
			if (component4.m_GUITexture != null)
			{
				x2 = component4.m_GUITexture.width;
				y2 = component4.m_GUITexture.height;
			}
			SimpleSprite component6 = m_sprIcon.GetComponent<SimpleSprite>();
			component6.Setup(width2, height2, new Vector2(0f, y2), new Vector2(x2, y2));
			component6.SetTexture(component4.m_GUITexture);
			component6.UpdateUVs();
		}
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

	private bool HandleDrop(GameObject target)
	{
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

	public void OnTap()
	{
	}

	public void OnPress()
	{
		m_menu.SetPart(m_name, refreshship: true);
	}

	public void OnPressInfo()
	{
		m_menu.ShowInfo(m_name);
	}

	public void OnRelease()
	{
	}

	public void OnMove()
	{
	}

	internal void Hide()
	{
		base.gameObject.SetActiveRecursively(state: false);
	}

	internal void Show()
	{
		base.gameObject.SetActiveRecursively(state: true);
	}
}
