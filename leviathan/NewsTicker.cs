using System.Collections.Generic;
using UnityEngine;

internal class NewsTicker
{
	private PdxNews m_pdxNews;

	private GDPBackend m_gdpBackend;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private GameObject m_itemPrefab;

	private UIScrollList m_scrollList;

	private GameObject m_item;

	private List<PdxNews.Entry> m_entries;

	private int m_itemID;

	private string m_currentUrl;

	public NewsTicker(PdxNews pdxNews, GDPBackend gdp, GameObject guiCamera)
	{
		m_guiCamera = guiCamera;
		m_pdxNews = pdxNews;
		m_gdpBackend = gdp;
		m_gui = GuiUtils.CreateGui("Ticker", m_guiCamera);
		m_itemPrefab = GuiUtils.FindChildOf(m_gui, "TickerListItem");
		m_scrollList = GuiUtils.FindChildOfComponent<UIScrollList>(m_gui, "TickerScrollList");
		m_item = Object.Instantiate(m_itemPrefab) as GameObject;
		m_scrollList.AddItem(m_item.GetComponent<UIListItem>());
		m_scrollList.SetValueChangedDelegate(OnItemSelected);
	}

	public void Close()
	{
		Object.Destroy(m_gui);
	}

	public void Update(float dt)
	{
		if (GetItems())
		{
			UpdateGui(dt);
		}
	}

	private bool GetItems()
	{
		if (m_entries != null)
		{
			return true;
		}
		m_entries = m_pdxNews.GetEntries();
		if (m_entries == null || m_entries.Count == 0)
		{
			return false;
		}
		SetupNextItem();
		m_scrollList.ScrollPosition = 0f;
		return true;
	}

	private void UpdateGui(float dt)
	{
		float num = m_scrollList.ScrollPosition + dt * 0.1f;
		if ((double)num > 1.5)
		{
			num = 0f;
			SetupNextItem();
		}
		m_scrollList.ScrollPosition = num;
	}

	private void SetupNextItem()
	{
		UIListItem component = m_item.GetComponent<UIListItem>();
		if (m_itemID < m_entries.Count)
		{
			component.Text = m_entries[m_itemID].m_title;
			m_currentUrl = m_entries[m_itemID].m_url;
		}
		m_itemID++;
		if (m_itemID >= m_entries.Count)
		{
			m_itemID = 0;
		}
	}

	private void OnItemSelected(IUIObject obj)
	{
		if (!string.IsNullOrEmpty(m_currentUrl))
		{
			if (m_gdpBackend != null)
			{
				m_gdpBackend.OpenWebUrl(m_currentUrl);
			}
			else
			{
				Application.OpenURL(m_currentUrl);
			}
		}
	}
}
