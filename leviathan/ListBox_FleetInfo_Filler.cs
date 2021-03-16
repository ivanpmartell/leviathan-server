#define DEBUG
using UnityEngine;

[AddComponentMenu("Scripts/Gui/ListBox_FleetInfo_Filler")]
public class ListBox_FleetInfo_Filler : MonoBehaviour
{
	public delegate void OnFleetChanged(string fleetName);

	private UIScrollList lstThis;

	private GameObject prefab;

	public OnFleetChanged m_onFleetChangedDelegate;

	public UIScrollList ScrollList
	{
		get
		{
			return lstThis;
		}
		private set
		{
			lstThis = value;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Initialize()
	{
		DebugUtils.Assert(Validate_ScrollList(), "ListBox_FleetInfo_Filler must be put on a GameObject with a UIScrollList-script !");
		ValidatePrefab();
		ScrollList.AddValueChangedDelegate(ScrollListValueChanged);
	}

	private void ScrollListValueChanged(IUIObject obj)
	{
		string selectedItemsName = GetSelectedItemsName();
		if (m_onFleetChangedDelegate != null)
		{
			m_onFleetChangedDelegate(selectedItemsName);
		}
	}

	public void Hide()
	{
		ScrollList.SetSelectedItem(-1);
		ScrollList.ClearList(destroy: true);
	}

	public void Clear()
	{
		ScrollList.ClearList(destroy: true);
	}

	public GameObject AddItem(string name, string size, string date)
	{
		name = name.Trim();
		size = size.Trim();
		date = date.Trim();
		if (prefab != null)
		{
			GameObject gameObject = Object.Instantiate(prefab) as GameObject;
			SetDate(SetSize(SetName(gameObject, name), size), date);
			ScrollList.AddItem(gameObject);
			ScrollList.PositionItems();
			ScrollList.LateUpdate();
			return gameObject;
		}
		return null;
	}

	public string GetSelectedItemsName()
	{
		IUIObject lastClickedControl = ScrollList.LastClickedControl;
		if (lastClickedControl == null || lastClickedControl.gameObject == null)
		{
			return string.Empty;
		}
		if (!lastClickedControl.controlIsEnabled)
		{
			return string.Empty;
		}
		return lastClickedControl.transform.FindChild("lblName").gameObject.GetComponent<SpriteText>().Text;
	}

	public void UnSelect()
	{
		ScrollList.SetSelectedItem(-1);
	}

	internal void RemoveFleet(string fleetNameToRemove)
	{
		int num = -1;
		for (int i = 0; i < ScrollList.Count; i++)
		{
			GameObject gameObject = ScrollList.GetItem(i).gameObject;
			if (gameObject.GetComponent<UIListItemContainer>() == null)
			{
				continue;
			}
			Transform transform = gameObject.transform.FindChild("bkg/lblName");
			if (transform == null)
			{
				continue;
			}
			GameObject gameObject2 = transform.gameObject;
			if (!(gameObject2 == null))
			{
				SpriteText component = gameObject2.GetComponent<SpriteText>();
				if (!(component == null) && string.Compare(component.Text, fleetNameToRemove) == 0)
				{
					num = i;
					break;
				}
			}
		}
		if (num < 0)
		{
			Debug.LogWarning($"ListBox_FleetInfo_Filler::RemoveFleet( {fleetNameToRemove} ) Failed to find object to remove");
			return;
		}
		ScrollList.RemoveItem(num, destroy: true);
		ScrollList.PositionItems();
		ScrollList.LateUpdate();
	}

	private GameObject SetName(GameObject O, string name)
	{
		O.transform.FindChild("bkg/lblName").gameObject.GetComponent<SpriteText>().Text = name;
		return O;
	}

	private GameObject SetSize(GameObject O, string size)
	{
		O.transform.FindChild("bkg/lblSize").gameObject.GetComponent<SpriteText>().Text = size;
		return O;
	}

	private GameObject SetDate(GameObject O, string date)
	{
		O.transform.FindChild("bkg/lblDate").gameObject.GetComponent<SpriteText>().Text = date;
		return O;
	}

	private void ValidatePrefab()
	{
		prefab = Resources.Load("gui/FleetInfoListItem", typeof(GameObject)) as GameObject;
		DebugUtils.Assert(prefab != null, "ListBox_FleetInfo_Filler failed to locate the prefab \"gui/FleetInfoListItem\" in Resources !");
		DebugUtils.Assert(Validate_lblName(), "ListBox_FleetInfo_Filler failed to validate label named lblName !");
		DebugUtils.Assert(Validate_lblSize(), "ListBox_FleetInfo_Filler failed to validate label named lblSize !");
		DebugUtils.Assert(Validate_lblDate(), "ListBox_FleetInfo_Filler failed to validate label named lblDate !");
	}

	private bool Validate_ScrollList()
	{
		ScrollList = base.gameObject.GetComponent<UIScrollList>();
		return ScrollList != null;
	}

	private bool Validate_lblName()
	{
		GameObject gameObject = prefab.transform.FindChild("bkg/lblName").gameObject;
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.GetComponent<SpriteText>() != null;
	}

	private bool Validate_lblSize()
	{
		GameObject gameObject = prefab.transform.FindChild("bkg/lblSize").gameObject;
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.GetComponent<SpriteText>() != null;
	}

	private bool Validate_lblDate()
	{
		GameObject gameObject = prefab.transform.FindChild("bkg/lblDate").gameObject;
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.GetComponent<SpriteText>() != null;
	}
}
