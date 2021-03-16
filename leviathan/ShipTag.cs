using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class ShipTag
{
	public enum StatusIconType
	{
		HPDisabled,
		MoveDisabled,
		OOC,
		Repair,
		Sink,
		ViewDisabled,
		Grounded,
		MAX_STATUSICONS
	}

	private class StatusIcon
	{
		public GameObject m_root;

		public SpriteText m_statusText;
	}

	private GameObject m_gui;

	private UIProgressBar m_healthBar;

	private UIProgressBar m_supplyBar;

	private SpriteText m_healthText;

	private SpriteText m_supplyText;

	private SpriteText m_shipName;

	private SimpleSprite m_flag;

	private SimpleSprite m_sinkIcon;

	private SimpleSprite m_supplyIcon;

	private UIScrollList m_iconList;

	private Camera m_guiCamera;

	private Unit m_unit;

	private int m_owner = -1;

	private bool m_visible = true;

	private GameType m_gameType;

	private float m_cullDistance = 500f;

	private List<GameObject> m_iconPrefabs;

	private List<StatusIcon> m_statusIcons = new List<StatusIcon>();

	public ShipTag(Unit ship, GameObject guiCamera, GameType gameType, List<GameObject> iconPrefabs)
	{
		m_unit = ship;
		m_guiCamera = guiCamera.camera;
		m_gameType = gameType;
		m_iconPrefabs = iconPrefabs;
		if (m_unit is Ship && m_unit.m_shipTag == Unit.ShipTagType.Normal)
		{
			m_cullDistance = 800f;
			if (m_unit is SupportShip)
			{
				m_gui = GuiUtils.CreateGui("ShipTag_Supply", guiCamera);
				m_supplyBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Supply").GetComponent<UIProgressBar>();
				m_supplyText = GuiUtils.FindChildOf(m_gui, "lblCurrentSupply").GetComponent<SpriteText>();
			}
			else
			{
				m_gui = GuiUtils.CreateGui("ShipTag", guiCamera);
			}
			m_flag = GuiUtils.FindChildOf(m_gui, "Flag").GetComponent<SimpleSprite>();
			m_sinkIcon = GuiUtils.FindChildOf(m_gui, "SinkIcon").GetComponent<SimpleSprite>();
			m_supplyIcon = GuiUtils.FindChildOf(m_gui, "SupplyIcon").GetComponent<SimpleSprite>();
		}
		else
		{
			m_cullDistance = 800f;
			Platform platform = m_unit as Platform;
			if ((bool)platform && platform.m_supplyEnabled)
			{
				m_gui = GuiUtils.CreateGui("ShipTag_Supply", guiCamera);
				m_supplyBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Supply").GetComponent<UIProgressBar>();
				m_supplyText = GuiUtils.FindChildOf(m_gui, "lblCurrentSupply").GetComponent<SpriteText>();
				m_flag = GuiUtils.FindChildOf(m_gui, "Flag").GetComponent<SimpleSprite>();
				m_sinkIcon = GuiUtils.FindChildOf(m_gui, "SinkIcon").GetComponent<SimpleSprite>();
				m_supplyIcon = GuiUtils.FindChildOf(m_gui, "SupplyIcon").GetComponent<SimpleSprite>();
			}
			else
			{
				m_gui = GuiUtils.CreateGui("ShipTag_mini", guiCamera);
			}
		}
		m_shipName = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "ShipNameLabel");
		m_healthBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Health").GetComponent<UIProgressBar>();
		m_iconList = GuiUtils.FindChildOfComponent<UIScrollList>(m_gui, "StatusScrollist");
		m_healthText = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "lblCurrentHealth");
		for (int i = 0; i < 7; i++)
		{
			m_statusIcons.Add(null);
		}
		m_gui.SetActiveRecursively(state: false);
	}

	public void Close()
	{
		UnityEngine.Object.Destroy(m_gui);
	}

	public void SetVisible(bool visible)
	{
		if (m_gui.active)
		{
			m_gui.SetActiveRecursively(state: false);
		}
		m_visible = visible;
	}

	public void Update(Camera camera, int localPlayerID, int localTeamID)
	{
		if (m_visible && m_unit.IsVisible() && !m_unit.IsDead() && camera.transform.position.y < m_cullDistance)
		{
			if (!m_gui.active)
			{
				m_gui.SetActiveRecursively(state: true);
			}
			UpdateTagPosition(camera);
			m_healthBar.Value = (float)m_unit.GetHealth() / (float)m_unit.GetMaxHealth();
			if (m_healthText != null)
			{
				m_healthText.Text = m_unit.GetHealth().ToString();
			}
			if (m_shipName != null)
			{
				m_shipName.Text = m_unit.GetName();
			}
			UpdateSupplyBar();
			UpdateTeamColor(localPlayerID, localTeamID);
			UpdateFlag();
			UpdateIcons();
		}
		else if (m_gui.active)
		{
			m_gui.SetActiveRecursively(state: false);
		}
	}

	private void UpdateIcons()
	{
		if (m_iconList == null)
		{
			return;
		}
		Ship ship = m_unit as Ship;
		if (!(ship == null))
		{
			if (ship.IsTakingWater())
			{
				EnableStatusIcon(StatusIconType.Sink).m_statusText.Text = ship.GetTimeToSink().ToString("F0");
			}
			else
			{
				DisableStatusIcon(StatusIconType.Sink);
			}
			if (ship.IsSupplied() || ship.IsAutoRepairing())
			{
				EnableStatusIcon(StatusIconType.Repair);
			}
			else
			{
				DisableStatusIcon(StatusIconType.Repair);
			}
			if (ship.IsEngineDamaged())
			{
				EnableStatusIcon(StatusIconType.MoveDisabled).m_statusText.Text = ship.GetEngineRepairTime().ToString("F0");
			}
			else
			{
				DisableStatusIcon(StatusIconType.MoveDisabled);
			}
			if (ship.IsBridgeDamaged())
			{
				EnableStatusIcon(StatusIconType.ViewDisabled).m_statusText.Text = ship.GetBridgeRepairTime().ToString("F0");
			}
			else
			{
				DisableStatusIcon(StatusIconType.ViewDisabled);
			}
			if (ship.IsOutOfControl())
			{
				EnableStatusIcon(StatusIconType.OOC).m_statusText.Text = ship.GetControlRepairTime().ToString("F1");
			}
			else
			{
				DisableStatusIcon(StatusIconType.OOC);
			}
			if (ship.IsGrounded())
			{
				EnableStatusIcon(StatusIconType.Grounded).m_statusText.Text = ship.GetGroundedTime().ToString("F1");
			}
			else
			{
				DisableStatusIcon(StatusIconType.Grounded);
			}
		}
	}

	private StatusIcon EnableStatusIcon(StatusIconType type)
	{
		StatusIcon statusIcon = m_statusIcons[(int)type];
		if (statusIcon != null)
		{
			return statusIcon;
		}
		statusIcon = new StatusIcon();
		m_statusIcons[(int)type] = statusIcon;
		statusIcon.m_root = UnityEngine.Object.Instantiate(m_iconPrefabs[(int)type]) as GameObject;
		statusIcon.m_statusText = GuiUtils.FindChildOfComponent<SpriteText>(statusIcon.m_root, "StatusLabel");
		m_iconList.AddItem(statusIcon.m_root);
		return statusIcon;
	}

	private void DisableStatusIcon(StatusIconType type)
	{
		StatusIcon statusIcon = m_statusIcons[(int)type];
		if (statusIcon != null)
		{
			int index = statusIcon.m_root.GetComponent<UIListItemContainer>().Index;
			m_iconList.RemoveItem(index, destroy: true);
			m_statusIcons[(int)type] = null;
		}
	}

	private void UpdateTagPosition(Camera camera)
	{
		GameCamera component = camera.GetComponent<GameCamera>();
		Vector3 vector = m_unit.transform.position;
		if (!m_unit.m_centerShipTag)
		{
			float num = 0f - Math.Abs((m_unit.transform.forward * m_unit.GetLength()).z * 0.5f) - m_unit.GetWidth() * 0.5f;
			vector.z += num;
		}
		vector = GuiUtils.WorldToGuiPos(camera, m_guiCamera, vector);
		vector.z = 8f;
		FlowerMenu flowerMenu = component.GetFlowerMenu();
		if (flowerMenu != null && flowerMenu.GetShip() == m_unit)
		{
			float lowestScreenPos = component.GetFlowerMenu().GetLowestScreenPos();
			if (vector.y > lowestScreenPos)
			{
				vector.y = lowestScreenPos;
			}
		}
		m_gui.transform.position = vector;
	}

	private void UpdateSupplyBar()
	{
		SupportShip supportShip = m_unit as SupportShip;
		if (supportShip != null)
		{
			m_supplyBar.Value = (float)supportShip.GetResources() / (float)supportShip.GetMaxResources();
			m_supplyText.Text = supportShip.GetResources() + "/" + supportShip.GetMaxResources();
		}
		Platform platform = m_unit as Platform;
		if ((bool)platform && platform.m_supplyEnabled)
		{
			m_supplyBar.Value = (float)platform.GetResources() / (float)platform.GetMaxResources();
			m_supplyText.Text = platform.GetResources() + "/" + platform.GetMaxResources();
			m_sinkIcon.renderer.enabled = false;
			m_supplyIcon.renderer.enabled = false;
		}
	}

	private void UpdateTeamColor(int localPlayerID, int localTeamID)
	{
		if (m_gameType == GameType.Campaign || m_gameType == GameType.Challenge)
		{
			if (m_unit.GetOwner() == localPlayerID)
			{
				m_healthBar.SetColor(Color.green);
			}
			else if (m_unit.GetOwnerTeam() == localTeamID)
			{
				m_healthBar.SetColor(Color.yellow);
			}
			else
			{
				m_healthBar.SetColor(Color.red);
			}
		}
		else
		{
			TurnMan.instance.GetPlayerColors(m_unit.GetOwner(), out var primaryColor);
			m_healthBar.Color = primaryColor;
		}
	}

	private void UpdateFlag()
	{
		if (m_unit.GetOwner() != m_owner && m_flag != null)
		{
			m_owner = m_unit.GetOwner();
			int playerFlag = TurnMan.instance.GetPlayerFlag(m_owner);
			Texture2D flagTexture = GuiUtils.GetFlagTexture(playerFlag);
			if (flagTexture != null)
			{
				GuiUtils.SetImage(m_flag, flagTexture);
				m_flag.renderer.enabled = true;
			}
			else
			{
				m_flag.renderer.enabled = false;
			}
		}
	}

	public Unit GetUnit()
	{
		return m_unit;
	}
}
