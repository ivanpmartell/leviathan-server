using System.Collections.Generic;
using UnityEngine;

public class BtnClickOnEscape : MonoBehaviour
{
	private static List<BtnClickOnEscape> m_wsObjectList = new List<BtnClickOnEscape>();

	private UIButton button;

	public static bool IsAnyButtonsActive()
	{
		if (m_wsObjectList.Count == 0)
		{
			return false;
		}
		foreach (BtnClickOnEscape wsObject in m_wsObjectList)
		{
			if (wsObject.gameObject.active)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		m_wsObjectList.Add(this);
		button = GetComponent<UIButton>();
	}

	private void OnDestroy()
	{
		int num = m_wsObjectList.IndexOf(this);
		if (num >= 0)
		{
			int index = m_wsObjectList.Count - 1;
			m_wsObjectList[num] = m_wsObjectList[index];
			m_wsObjectList.RemoveAt(index);
		}
	}

	private bool IsFront()
	{
		foreach (BtnClickOnEscape wsObject in m_wsObjectList)
		{
			if (wsObject != this && wsObject.gameObject.active && base.transform.position.z > wsObject.transform.position.z)
			{
				return false;
			}
		}
		return true;
	}

	private void Update()
	{
		if (IsFront() && Utils.AndroidBack())
		{
			UIManager component = button.RenderCamera.GetComponent<UIManager>();
			component.FocusObject = null;
			POINTER_INFO ptr = default(POINTER_INFO);
			ptr.evt = button.whenToInvoke;
			button.OnInput(ptr);
		}
	}
}
