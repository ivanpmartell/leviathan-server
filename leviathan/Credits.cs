using UnityEngine;

internal class Credits
{
	private UIButton m_button;

	private UIPanel m_creditsPanel;

	private float m_creditsTimer;

	private float m_creditsDelay = 8f;

	private int m_creditsIndex;

	private int m_creditsMaxIndex = 6;

	private MusicManager m_musicMan;

	public Credits(GameObject creditsRoot, MusicManager musMan)
	{
		m_creditsPanel = creditsRoot.GetComponent<UIPanel>();
		m_creditsPanel.gameObject.SetActiveRecursively(state: false);
		m_musicMan = musMan;
		if ((bool)GuiUtils.FindChildOf(creditsRoot, "ExitCreditsButton"))
		{
			m_button = GuiUtils.FindChildOf(creditsRoot, "ExitCreditsButton").GetComponent<UIButton>();
			m_button.GetComponent<UIButton>().SetValueChangedDelegate(onExit);
		}
	}

	private void onExit(IUIObject button)
	{
		Close();
	}

	public void Start()
	{
		m_musicMan.SetMusic("music-credits");
		m_creditsPanel.gameObject.SetActiveRecursively(state: false);
		m_creditsPanel.gameObject.active = true;
		if ((bool)m_button)
		{
			m_button.gameObject.active = true;
		}
		m_creditsTimer = 0f;
		m_creditsIndex = 0;
		m_creditsPanel.GetComponent<UIPanelManager>().DismissImmediate();
		m_creditsPanel.GetComponent<UIPanelManager>().BringIn(0);
	}

	private void Close()
	{
		m_musicMan.SetMusic("menu");
		m_creditsPanel.gameObject.SetActiveRecursively(state: false);
	}

	public void Update(float dt)
	{
		if (!m_creditsPanel.gameObject.active)
		{
			return;
		}
		if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Space))
		{
			Close();
		}
		m_creditsTimer += dt;
		if (m_creditsTimer > m_creditsDelay)
		{
			m_creditsTimer = 0f;
			m_creditsIndex++;
			PLog.Log("Next credits screen " + m_creditsIndex);
			if (m_creditsIndex >= m_creditsMaxIndex)
			{
				PLog.Log("done with credits");
				Close();
			}
			else
			{
				UIPanelManager component = m_creditsPanel.GetComponent<UIPanelManager>();
				component.BringIn(m_creditsIndex);
			}
		}
	}
}
