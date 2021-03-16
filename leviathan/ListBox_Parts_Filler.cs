#define DEBUG
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Scripts/Gui/ListBox_Parts_Filler")]
public class ListBox_Parts_Filler : MonoBehaviour
{
	private UIScrollList m_parent;

	private ShipMenu m_menu;

	public GameObject m_GUI_Prefab;

	private List<string> m_parts;

	private void Start()
	{
	}

	public void Initialize(List<string> ships, ShipMenu menu)
	{
		m_parts = ships;
		m_menu = menu;
		m_parent = base.gameObject.GetComponent<UIScrollList>();
		DebugUtils.Assert(m_parent != null, "ListBox_Parts_Filler script must be attached to an UIScrollList !");
		if (m_parent == null)
		{
			return;
		}
		DebugUtils.Assert(m_GUI_Prefab != null, "ListBox_Parts_Filler script has no GUI prefab !");
		if (m_GUI_Prefab == null)
		{
			return;
		}
		GUI_Blueprint_Part component = m_GUI_Prefab.GetComponent<GUI_Blueprint_Part>();
		DebugUtils.Assert(component != null, "ListBox_Parts_Filler script's prefab does not have a GUI_Blueprint_Part-script !");
		if (component == null || m_parts == null || m_parts.Count <= 0)
		{
			return;
		}
		foreach (string part in m_parts)
		{
			Add(part);
		}
		m_parent.PositionItems();
	}

	public void Add(string def)
	{
		Part_to_GUI(def, out var obj);
		m_parent.AddItem(obj);
	}

	public GUI_Blueprint_Part Part_to_GUI(string def, out GameObject obj)
	{
		obj = Object.Instantiate(m_GUI_Prefab) as GameObject;
		GUI_Blueprint_Part component = obj.GetComponent<GUI_Blueprint_Part>();
		component.Initialize(def, m_menu);
		return component;
	}

	private void Update()
	{
	}

	public List<GUI_Blueprint_Part> CreateListItems_From(List<string> definitions)
	{
		if (definitions == null || definitions.Count == 0)
		{
			return null;
		}
		List<GUI_Blueprint_Part> list = new List<GUI_Blueprint_Part>();
		foreach (string definition in definitions)
		{
			GameObject obj;
			GUI_Blueprint_Part item = Part_to_GUI(definition, out obj);
			list.Add(item);
			Object.DestroyImmediate(obj);
		}
		return list;
	}
}
