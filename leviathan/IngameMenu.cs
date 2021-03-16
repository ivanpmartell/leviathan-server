using System;
using UnityEngine;

public class IngameMenu
{
	public Action m_OnSurrender;

	public Action m_OnLeave;

	public Action m_OnOptions;

	public Action m_OnSwitchGame;

	public Action m_OnBackToMenu;

	public Action m_OnQuitGame;

	private GameObject m_gui;

	private UIButton m_btnSurrender;

	private UIButton m_btnLeave;

	private UIButton m_btnOptions;

	private UIButton m_btnBackToMenu;

	private UIButton m_btnExit;

	public IngameMenu(GameObject menuRoot)
	{
		m_gui = menuRoot;
		m_btnBackToMenu = GuiUtils.FindChildOf(m_gui, "btnBackToMenu").GetComponent<UIButton>();
		m_btnExit = GuiUtils.FindChildOf(m_gui, "btnExit").GetComponent<UIButton>();
		m_btnOptions = GuiUtils.FindChildOf(m_gui, "btnOptions").GetComponent<UIButton>();
		m_btnSurrender = GuiUtils.FindChildOf(m_gui, "btnSurrender").GetComponent<UIButton>();
		m_btnLeave = GuiUtils.FindChildOf(m_gui, "btnLeave").GetComponent<UIButton>();
		m_btnSurrender.SetValueChangedDelegate(SurrenderClicked);
		m_btnOptions.SetValueChangedDelegate(OptionsClicked);
		m_btnBackToMenu.SetValueChangedDelegate(BackToMenuClicked);
		m_btnExit.SetValueChangedDelegate(ExitClicked);
		m_btnLeave.SetValueChangedDelegate(LeaveClicked);
		m_btnSurrender.controlIsEnabled = false;
		m_btnLeave.gameObject.SetActiveRecursively(state: false);
	}

	public void SetSurrenderStatus(bool surrendered, bool inPlanning, bool inReplayMode, bool isAdmin)
	{
		if (inReplayMode || !m_gui.active)
		{
			m_btnSurrender.gameObject.SetActiveRecursively(state: false);
			m_btnLeave.gameObject.SetActiveRecursively(state: false);
		}
		else if (surrendered)
		{
			m_btnSurrender.gameObject.SetActiveRecursively(state: false);
			if (!isAdmin)
			{
				m_btnLeave.gameObject.SetActiveRecursively(state: true);
			}
		}
		else
		{
			m_btnSurrender.gameObject.SetActiveRecursively(state: true);
			m_btnLeave.gameObject.SetActiveRecursively(state: false);
			m_btnSurrender.controlIsEnabled = inPlanning;
		}
	}

	public void Close()
	{
	}

	private void SurrenderClicked(IUIObject ignore)
	{
		if (m_OnSurrender != null)
		{
			m_OnSurrender();
		}
	}

	private void OptionsClicked(IUIObject ignore)
	{
		if (m_OnOptions != null)
		{
			m_OnOptions();
		}
	}

	private void SwitchGameClicked(IUIObject ignore)
	{
		if (m_OnSwitchGame != null)
		{
			m_OnSwitchGame();
		}
	}

	private void BackToMenuClicked(IUIObject ignore)
	{
		if (m_OnBackToMenu != null)
		{
			m_OnBackToMenu();
		}
	}

	private void ExitClicked(IUIObject ignore)
	{
		if (m_OnQuitGame != null)
		{
			m_OnQuitGame();
		}
	}

	private void LeaveClicked(IUIObject ignore)
	{
		if (m_OnLeave != null)
		{
			m_OnLeave();
		}
	}
}
