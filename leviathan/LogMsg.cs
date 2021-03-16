using UnityEngine;

internal abstract class LogMsg
{
	public UIListItem m_listItemComponent;

	protected GameObject m_gui;

	public virtual void Remove()
	{
		if (m_gui != null)
		{
			Object.DestroyImmediate(m_gui);
		}
	}

	public virtual void Hide()
	{
		if (m_gui != null)
		{
			m_gui.SetActiveRecursively(state: false);
		}
	}

	public virtual void Show()
	{
		if (m_gui != null)
		{
			m_gui.SetActiveRecursively(state: true);
		}
	}

	public float Height()
	{
		if (m_listItemComponent == null)
		{
			return 0f;
		}
		return m_listItemComponent.height;
	}

	public float Width()
	{
		if (m_listItemComponent == null)
		{
			return 0f;
		}
		return m_listItemComponent.width;
	}
}
