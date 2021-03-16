#define DEBUG
using System.Collections.Generic;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/Gui/ListBox_Ships_Filler")]
public class ListBox_Ships_Filler : MonoBehaviour
{
	private UIScrollList m_parent;

	private FleetMenu m_menu;

	public GameObject m_GUI_Prefab;

	public List<ShipDef> m_ships;

	private void Start()
	{
	}

	public void Initialize(List<ShipDef> ships, FleetMenu menu)
	{
		m_ships = ships;
		m_menu = menu;
		m_parent = base.gameObject.GetComponent<UIScrollList>();
		DebugUtils.Assert(m_parent != null, "ListBox_Ships_Filler script must be attached to an UIScrollList !");
		if (m_parent == null)
		{
			return;
		}
		DebugUtils.Assert(m_GUI_Prefab != null, "ListBox_Ships_Filler script has no GUI prefab !");
		if (m_GUI_Prefab == null)
		{
			return;
		}
		GUI_Blueprint_Ship component = m_GUI_Prefab.GetComponent<GUI_Blueprint_Ship>();
		DebugUtils.Assert(component != null, "ListBox_Ships_Filler script's prefab does not have a GUI_Blueprint_Ship-script !");
		if (component == null || m_ships == null || m_ships.Count <= 0)
		{
			return;
		}
		foreach (ShipDef ship in m_ships)
		{
			Add(ship);
		}
		m_parent.PositionItems();
	}

	public void Add(ShipDef def)
	{
		GameObject obj;
		GUI_Blueprint_Ship gUI_Blueprint_Ship = ShipDef_to_GUI(def, out obj);
		gUI_Blueprint_Ship.DisableTrashButton();
		m_parent.AddItem(obj);
	}

	public GUI_Blueprint_Ship ShipDef_to_GUI(ShipDef def, out GameObject obj)
	{
		obj = Object.Instantiate(m_GUI_Prefab) as GameObject;
		GUI_Blueprint_Ship component = obj.GetComponent<GUI_Blueprint_Ship>();
		component.Initialize(def, m_menu);
		return component;
	}

	private void Update()
	{
	}

	public List<GUI_Blueprint_Ship> CreateListItems_From(List<ShipDef> definitions)
	{
		if (definitions == null || definitions.Count == 0)
		{
			return null;
		}
		List<GUI_Blueprint_Ship> list = new List<GUI_Blueprint_Ship>();
		foreach (ShipDef definition in definitions)
		{
			GameObject obj;
			GUI_Blueprint_Ship item = ShipDef_to_GUI(definition, out obj);
			list.Add(item);
			Object.DestroyImmediate(obj);
		}
		return list;
	}
}
