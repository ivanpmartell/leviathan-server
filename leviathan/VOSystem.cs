using System.Collections.Generic;
using UnityEngine;

public class VOSystem
{
	private class VOEventData
	{
		public VOCollection.VOEvent m_event;

		public bool m_played;
	}

	private static VOSystem m_instance;

	private string m_announcer = string.Empty;

	private float m_timeSinceAnnouncement = 1000f;

	private float m_minAnnouncementDelay = 2f;

	private GameObject m_lastSound;

	private Dictionary<string, VOEventData> m_events = new Dictionary<string, VOEventData>();

	public static VOSystem instance => m_instance;

	public VOSystem()
	{
		if (m_instance != null)
		{
			m_instance.Close();
		}
		m_instance = this;
	}

	public void Close()
	{
		m_instance = null;
		m_events.Clear();
	}

	public void SetAnnouncer(string name)
	{
		if (m_announcer == name)
		{
			return;
		}
		m_events.Clear();
		if (name == string.Empty)
		{
			return;
		}
		m_announcer = name;
		GameObject gameObject = Resources.Load("vo/" + name) as GameObject;
		VOCollection component = gameObject.GetComponent<VOCollection>();
		foreach (VOCollection.VOEvent voEvent in component.m_voEvents)
		{
			VOEventData vOEventData = new VOEventData();
			vOEventData.m_event = voEvent;
			m_events.Add(voEvent.m_name, vOEventData);
		}
		PLog.Log("Announcer set to " + name + "  events:" + m_events.Count);
	}

	public void ResetTurnflags()
	{
		foreach (KeyValuePair<string, VOEventData> @event in m_events)
		{
			@event.Value.m_played = false;
		}
	}

	public void DoEvent(string name)
	{
		if (m_timeSinceAnnouncement < m_minAnnouncementDelay || m_events.Count == 0)
		{
			return;
		}
		if (m_lastSound != null)
		{
			AudioSource component = m_lastSound.GetComponent<AudioSource>();
			if (component != null && component.isPlaying)
			{
				return;
			}
		}
		if (!m_events.TryGetValue(name, out var value))
		{
			PLog.LogWarning("Missing event " + name);
		}
		else if ((!value.m_event.m_oncePerTurn || !value.m_played) && !(value.m_event.m_effects == null))
		{
			value.m_played = true;
			PLog.Log("Doing event " + name);
			m_lastSound = Object.Instantiate(value.m_event.m_effects, Camera.main.transform.position, Quaternion.identity) as GameObject;
			m_timeSinceAnnouncement = 0f;
		}
	}

	public void Update(float dt)
	{
		m_timeSinceAnnouncement += dt;
	}
}
