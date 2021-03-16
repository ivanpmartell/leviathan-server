using UnityEngine;

public class GenericCheckBox : MonoBehaviour
{
	public delegate void OnChecked(GameObject checkBox);

	public delegate void OnUnChecked(GameObject checkBox);

	private const int STATE_UnChecked = 0;

	private const int STATE_Checked = 1;

	private bool m_isChecked;

	public OnChecked m_onCheckedDelegate;

	public OnUnChecked m_onUnCheckedDelegate;

	private void Start()
	{
		SetState(0);
	}

	private void Update()
	{
	}

	public void OnPress()
	{
		ToggleCheck();
	}

	public void OnTap()
	{
		ToggleCheck();
	}

	private void ToggleCheck()
	{
		m_isChecked = !m_isChecked;
		if (m_isChecked)
		{
			if (m_onCheckedDelegate != null)
			{
				m_onCheckedDelegate(base.gameObject);
			}
			SetState(1);
		}
		else
		{
			if (m_onUnCheckedDelegate != null)
			{
				m_onUnCheckedDelegate(base.gameObject);
			}
			SetState(0);
		}
	}

	private void SetState(int stateNumber)
	{
		UIStateToggleBtn component = base.gameObject.GetComponent<UIStateToggleBtn>();
		if (component != null)
		{
			component.SetState(stateNumber);
		}
	}
}
