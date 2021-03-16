using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/Gui/FleetEditor_PreviewArea")]
public class FleetEditor_PreviewArea : MonoBehaviour
{
	public delegate void CollectionChanged(ShipDef def, int numItemsInCollection);

	private const float CloneSpacing = 3f;

	private const float PanelYSpacing = 6f;

	private float CloneWidth = 290f;

	private float CloneHeight = 140f;

	public int m_shipsPerRow = 2;

	public int m_maxTotalShips = 9;

	public GameObject m_GUIShip_prefab;

	public CollectionChanged m_onItemAdded;

	public CollectionChanged m_onItemRemoved;

	private Transform FirstItemCenterPos
	{
		get
		{
			Transform transform = base.gameObject.transform.Find("FirstItemCenterPos");
			if (transform != null)
			{
				return transform;
			}
			return base.transform;
		}
	}

	public int NumItems
	{
		get
		{
			UIPanel component = base.gameObject.GetComponent<UIPanel>();
			return (!(component == null)) ? (component.transform.childCount - 1) : 0;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Clear()
	{
		GetAllClones().ForEach(delegate(GameObject child)
		{
			UnityEngine.Object.Destroy(child);
		});
	}

	public void AddClone(GUI_Blueprint_Ship blueprint)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_GUIShip_prefab) as GameObject;
		GUI_Blueprint_Ship component = gameObject.GetComponent<GUI_Blueprint_Ship>();
		string arg = blueprint.Name;
		int num = 0;
		foreach (GameObject allClone in GetAllClones())
		{
			GUI_Blueprint_Ship component2 = allClone.GetComponent<GUI_Blueprint_Ship>();
			if (!(component2 == null))
			{
				string text = component2.Name;
				string value = blueprint.Name;
				bool flag = text.StartsWith(value);
				bool flag2 = string.Compare(component2.Type, blueprint.Type) == 0;
				bool flag3 = component2.Cost == blueprint.Cost;
				if (flag && flag2 && flag3)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			arg = $"{arg}({num + 1})";
		}
		component.Initialize(blueprint.ShipDefinition, null);
		component.Name = arg;
		component.AllowDragDrop = false;
		component.DisableEdit();
		component.EnableTrashButton(OnItemDelete);
		AddCloneAndUpdate(gameObject);
	}

	private void AddCloneAndUpdate(GameObject clone)
	{
		UIPanel component = base.gameObject.GetComponent<UIPanel>();
		int num = component.transform.childCount - 2;
		if (num < 0)
		{
			num = 0;
		}
		int num2 = (int)Math.Floor((double)num / (double)m_shipsPerRow);
		int num3 = num - num2 * m_shipsPerRow;
		if (num3 < 0)
		{
			num3 = 0;
		}
		clone.transform.position = Vector3.zero;
		clone.transform.localPosition = Vector3.zero;
		component.MakeChild(clone);
		clone.transform.position = new Vector3(CalcXPosOfPanelItemNr(num3), CalcYPosOfPanelNr(num2), -0.1f);
		if (m_onItemAdded != null)
		{
			m_onItemAdded(clone.GetComponent<GUI_Blueprint_Ship>().ShipDefinition, NumItems);
		}
	}

	private void OnItemDelete(GameObject target)
	{
		List<GameObject> allClones = GetAllClones();
		int num = allClones.IndexOf(target);
		if (num == -1)
		{
			return;
		}
		string fromString = target.GetComponent<GUI_Blueprint_Ship>().Name;
		StringUtils.TryRemoveCopyText(ref fromString);
		ShipDef shipDefinition = target.GetComponent<GUI_Blueprint_Ship>().ShipDefinition;
		UnityEngine.Object.Destroy(target);
		allClones.Remove(allClones[num]);
		UIPanel component = base.gameObject.GetComponent<UIPanel>();
		if (num < component.transform.childCount - 1)
		{
			int num2 = 0;
			foreach (GameObject item in allClones)
			{
				if (num2 >= num)
				{
					int num3 = (int)Math.Floor((double)num2 / (double)m_shipsPerRow);
					int nr = num2 - num3 * m_shipsPerRow;
					GUI_Blueprint_Ship component2 = item.GetComponent<GUI_Blueprint_Ship>();
					string fromString2 = component2.Name;
					if (StringUtils.ContainsParanthesesAndNumber(fromString2) && fromString2.Contains(fromString))
					{
						string text = StringUtils.ExtractCopyNumber(fromString2, includeParantheses: false);
						if (text == "2")
						{
							StringUtils.TryRemoveCopyText(ref fromString2);
						}
						else
						{
							string text2 = (int.Parse(text) - 1).ToString();
							fromString2 = fromString2.Replace("(" + text + ")", "(" + text2 + ")");
						}
						component2.Name = fromString2;
					}
					item.transform.position = new Vector3(CalcXPosOfPanelItemNr(nr), CalcYPosOfPanelNr(num3), -0.1f);
				}
				num2++;
			}
		}
		if (m_onItemRemoved != null)
		{
			m_onItemRemoved(shipDefinition, component.transform.childCount - 3);
		}
	}

	private List<GameObject> GetAllClones()
	{
		UIPanel component = base.gameObject.GetComponent<UIPanel>();
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in component.transform)
		{
			GameObject gameObject = item.gameObject;
			GUI_Blueprint_Ship component2 = gameObject.GetComponent<GUI_Blueprint_Ship>();
			if (!(component2 == null))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<ShipDef> GetAllShips()
	{
		List<ShipDef> list = new List<ShipDef>();
		foreach (GameObject allClone in GetAllClones())
		{
			GUI_Blueprint_Ship component = allClone.GetComponent<GUI_Blueprint_Ship>();
			if (component.ShipDefinition != null)
			{
				list.Add(component.ShipDefinition);
			}
		}
		return list;
	}

	public List<ShipInstanceDef> GetAllShipsAsInstanceDefs()
	{
		List<ShipInstanceDef> list = new List<ShipInstanceDef>();
		foreach (GameObject allClone in GetAllClones())
		{
			list.Add(new ShipInstanceDef(allClone.GetComponent<GUI_Blueprint_Ship>().Name));
		}
		return list;
	}

	internal void Hide()
	{
		foreach (GameObject allClone in GetAllClones())
		{
			allClone.GetComponent<GUI_Blueprint_Ship>().Hide();
		}
		base.gameObject.SetActiveRecursively(state: false);
	}

	internal void Show()
	{
		base.gameObject.SetActiveRecursively(state: true);
		foreach (GameObject allClone in GetAllClones())
		{
			allClone.GetComponent<GUI_Blueprint_Ship>().Show();
		}
	}

	private float CalcRowLength()
	{
		float num = 0f;
		int num2 = m_shipsPerRow - 1;
		if (num2 > 0)
		{
			num = (float)num2 * 3f;
		}
		return (float)m_shipsPerRow * CloneWidth + num;
	}

	private float CalcXPosOfPanelItemNr(int nr)
	{
		float num = ((nr <= 0) ? 0f : (3f * (float)nr));
		return FirstItemCenterPos.position.x + num + (float)nr * CloneWidth;
	}

	private float CalcYPosOfPanelNr(int nr)
	{
		float num = ((nr <= 0) ? 0f : (6f * (float)nr));
		return FirstItemCenterPos.position.y - (num + (float)nr * CloneHeight);
	}
}
