using UnityEngine;

public class guitest : MonoBehaviour
{
	public GameObject m_gui;

	private void Awake()
	{
		UIActionBtn component = m_gui.GetComponent<UIActionBtn>();
		component.SetValueChangedDelegate(Button);
	}

	public void Button(IUIObject info)
	{
		PLog.Log("Pressed button");
	}
}
