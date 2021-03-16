#define DEBUG
using System;
using UnityEngine;

public class Splash
{
	public Action m_onDone;

	public Action m_onFadeoutComplete;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private MusicManager m_musicMan;

	private float m_delay = 2f;

	private float m_time;

	private int m_screen;

	private int m_maxScreens = 2;

	private bool m_done;

	private bool m_hiding;

	private bool m_finished;

	private UIPanel[] m_panels;

	private UIPanel m_currentPanel;

	public Splash(GameObject guiCamera, MusicManager musMan)
	{
		m_guiCamera = guiCamera;
		m_musicMan = musMan;
		m_musicMan.SetMusic("menu");
		m_gui = GuiUtils.CreateGui("Splash", m_guiCamera);
		m_panels = new UIPanel[m_maxScreens];
		for (int i = 0; i < m_maxScreens; i++)
		{
			string text = "Splash" + i;
			m_panels[i] = GuiUtils.FindChildOfComponent<UIPanel>(m_gui, text);
			m_panels[i].Dismiss();
			DebugUtils.Assert(m_panels[i] != null, "Missing panel " + text);
		}
		m_time = m_delay;
		ShowPanel(0);
	}

	public void Close()
	{
		PLog.Log("Derp close ");
		UnityEngine.Object.Destroy(m_gui);
	}

	public void Update()
	{
		if (m_finished)
		{
			PLog.Log("FADE OUT");
			m_onFadeoutComplete();
			return;
		}
		if (m_hiding)
		{
			PLog.Log("Dismissing");
			GuiUtils.FindChildOfComponent<UIPanel>(m_gui, "Bkg").Dismiss();
			m_currentPanel.Dismiss();
			m_currentPanel.AddTempTransitionDelegate(OnFadedOut);
			m_hiding = false;
		}
		if (!m_done)
		{
			m_time -= Time.deltaTime;
			if (m_time <= 0f)
			{
				NextScreen();
			}
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0))
			{
				m_screen = m_maxScreens;
				m_time = 0f;
			}
		}
	}

	private void NextScreen()
	{
		m_screen++;
		if (m_screen >= m_maxScreens)
		{
			m_done = true;
			m_hiding = true;
			PLog.Log("Splash done");
			m_onDone();
		}
		else
		{
			ShowPanel(m_screen);
			m_time = m_delay;
		}
	}

	private void OnFadedOut(UIPanelBase panel, EZTransition transition)
	{
		PLog.Log("On faded out");
		m_finished = true;
	}

	private void ShowPanel(int id)
	{
		if (m_currentPanel != null)
		{
			m_currentPanel.Dismiss();
			m_currentPanel = null;
		}
		m_currentPanel = m_panels[id];
		m_currentPanel.BringIn();
	}
}
