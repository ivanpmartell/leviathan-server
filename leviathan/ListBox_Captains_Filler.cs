#define DEBUG
using UnityEngine;

[AddComponentMenu("Scripts/Gui/ListBox_Captains_Filler")]
public class ListBox_Captains_Filler : MonoBehaviour
{
	private UIScrollList m_parent;

	private void Start()
	{
		m_parent = base.gameObject.GetComponent<UIScrollList>();
		DebugUtils.Assert(m_parent != null, "ListBox_Captains_Filler script must be attached to an UIScrollList");
		if (!(m_parent == null))
		{
		}
	}

	private void Update()
	{
	}
}
