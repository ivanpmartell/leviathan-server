#define DEBUG
using UnityEngine;

public class StatusWnd_Ship
{
	private const float m_iconSpacingX = 3f;

	private bool m_currentlyViewedAsFriend = true;

	private GameObject m_guiCam;

	private Ship m_ship;

	private GameObject m_gui;

	private SpriteText m_lblShipName;

	private SpriteText m_lblClassName;

	private SimpleSprite m_background;

	private UIProgressBar m_healthBar;

	private SpriteText m_healthText;

	private GameObject m_supplyWnd;

	private UIProgressBar m_supplyBar;

	private SpriteText m_supplyText;

	public bool CurrentlyViewedAsFriend
	{
		get
		{
			return m_currentlyViewedAsFriend;
		}
		private set
		{
			m_currentlyViewedAsFriend = value;
		}
	}

	public StatusWnd_Ship(Ship ship, GameObject GUICam, bool friendOrFoe)
	{
		m_guiCam = GUICam;
		m_ship = ship;
		SetupGui();
		Update();
	}

	public void Update()
	{
		if (m_ship.IsDead())
		{
			m_gui.transform.gameObject.SetActiveRecursively(state: false);
			return;
		}
		m_lblClassName.Text = Localize.instance.Translate(m_ship.GetClassName());
		m_lblShipName.Text = m_ship.GetName();
		m_healthBar.Value = (float)m_ship.GetHealth() / (float)m_ship.GetMaxHealth();
		m_healthText.Text = m_ship.GetHealth() + "/" + m_ship.GetMaxHealth();
		SupportShip supportShip = m_ship as SupportShip;
		if ((bool)supportShip)
		{
			m_supplyBar.Value = (float)supportShip.GetResources() / (float)supportShip.GetMaxResources();
			m_supplyText.Text = supportShip.GetResources() + "/" + supportShip.GetMaxResources();
		}
	}

	public void Close()
	{
		Object.DestroyImmediate(m_gui);
	}

	private void SetupGui()
	{
		if (m_ship is SupportShip)
		{
			m_gui = GuiUtils.CreateGui("IngameGui/StatusWnd_Ship_Supply", m_guiCam);
			m_supplyWnd = GuiUtils.FindChildOf(m_gui, "Supplybar");
			m_supplyBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Supply").GetComponent<UIProgressBar>();
			m_supplyText = GuiUtils.FindChildOf(m_gui, "lblCurrentSupply").GetComponent<SpriteText>();
		}
		else
		{
			m_gui = GuiUtils.CreateGui("IngameGui/StatusWnd_Ship", m_guiCam);
		}
		m_background = m_gui.transform.GetComponent<SimpleSprite>();
		DebugUtils.Assert(m_background != null, "StatusWnd_Ship has no SimpleSprite-component to be used as background !");
		m_lblShipName = GuiUtils.FindChildOf(m_gui, "lblShipName").GetComponent<SpriteText>();
		m_healthBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Health").GetComponent<UIProgressBar>();
		m_lblClassName = GuiUtils.FindChildOf(m_gui, "lblClassName").GetComponent<SpriteText>();
		m_healthText = GuiUtils.FindChildOf(m_gui, "lblCurrentHealth").GetComponent<SpriteText>();
	}
}
