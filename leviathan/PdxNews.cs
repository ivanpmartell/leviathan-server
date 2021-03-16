using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using MiniJSON;

public class PdxNews
{
	public class Entry
	{
		public string m_title;

		public string m_url;

		public long m_steamAppid;

		public long m_inGameStoreOffer;

		public string m_timeStamp;
	}

	private string m_baseUrl = "http://services.paradoxplaza.com/adam/feeds/";

	private string m_feedFile = "feed.json";

	private bool m_gotData;

	private WebClient m_client;

	private List<Entry> m_entries = new List<Entry>();

	public PdxNews(string gameName, bool live)
	{
		RequestFeed(live, gameName);
	}

	private string GetPlatformString()
	{
		return "steam";
	}

	private void RequestFeed(bool live, string gameName)
	{
		string platformString = GetPlatformString();
		string uriString = m_baseUrl + gameName + "-" + platformString + "/" + m_feedFile;
		Uri address = new Uri(uriString);
		m_client = new WebClient();
		m_client.DownloadStringCompleted += OnDownloaded;
		m_client.DownloadStringAsync(address);
	}

	private void OnDownloaded(object sender, DownloadStringCompletedEventArgs e)
	{
		m_entries.Clear();
		if (!e.Cancelled && e.Error == null)
		{
			string result = e.Result;
			IDictionary dictionary = (IDictionary)Json.Deserialize(result);
			string text = dictionary["result"] as string;
			if (text == "OK")
			{
				IList list = dictionary["entries"] as IList;
				foreach (IDictionary item in list)
				{
					Entry entry = new Entry();
					if (item.Contains("title"))
					{
						entry.m_title = item["title"] as string;
					}
					if (item.Contains("url"))
					{
						entry.m_url = item["url"] as string;
					}
					if (item.Contains("timestamp"))
					{
						entry.m_timeStamp = item["timestamp"] as string;
					}
					if (item.Contains("steam-appid"))
					{
						entry.m_steamAppid = (long)item["steam-appid"];
					}
					if (item.Contains("in-game-store-offer"))
					{
						entry.m_inGameStoreOffer = (long)item["in-game-store-offer"];
					}
					m_entries.Add(entry);
				}
			}
			else
			{
				PLog.LogWarning("Invalid result from ticker host");
			}
		}
		m_client = null;
		m_gotData = true;
	}

	public List<Entry> GetEntries()
	{
		if (m_gotData)
		{
			return m_entries;
		}
		return null;
	}
}
