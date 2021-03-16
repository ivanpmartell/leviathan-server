using System.Collections.Generic;
using PTech;
using UnityEngine;

public class MessageLog
{
	public enum TextPosition
	{
		Top,
		Middle,
		Bottom
	}

	public class Message
	{
		public string m_mainText;

		public string m_subText;

		public string m_prefab;

		public float m_displayTime;
	}

	public class MessageArea
	{
		public Queue<Message> m_Queue = new Queue<Message>();

		public float m_Timer;

		public GameObject m_dlg;

		public bool m_fading;

		public Vector3 m_position = default(Vector3);
	}

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private bool m_visible = true;

	private static MessageLog m_instance;

	private float m_fadeTime = 0.5f;

	private MessageArea m_areaTop = new MessageArea();

	private MessageArea m_areaMiddle = new MessageArea();

	private MessageArea m_areaBottom = new MessageArea();

	public static MessageLog instance => m_instance;

	public MessageLog(PTech.RPC rpc, GameObject guiCamera)
	{
		m_instance = this;
		Clear();
		if (!(guiCamera == null))
		{
			m_guiCamera = guiCamera;
			m_gui = GuiUtils.CreateGui("IngameGui/MessageLog", m_guiCamera);
			m_areaTop.m_dlg = GuiUtils.CreateGui("IngameGui/TurnMessage", m_guiCamera);
			m_areaTop.m_dlg.GetComponent<UIPanel>().Dismiss();
			m_areaTop.m_dlg.transform.position = new Vector3(0f, 375f, 0f);
			m_areaTop.m_position = new Vector3(0f, 375f, 0f);
			m_areaMiddle.m_dlg = GuiUtils.CreateGui("IngameGui/TurnMessage", m_guiCamera);
			m_areaMiddle.m_dlg.GetComponent<UIPanel>().Dismiss();
			m_areaMiddle.m_dlg.transform.position = new Vector3(0f, 0f, 0f);
			m_areaMiddle.m_position = new Vector3(0f, 0f, 0f);
			m_areaBottom.m_dlg = GuiUtils.CreateGui("IngameGui/TurnMessage", m_guiCamera);
			m_areaBottom.m_dlg.GetComponent<UIPanel>().Dismiss();
			m_areaBottom.m_dlg.transform.position = new Vector3(0f, -375f, 0f);
			m_areaBottom.m_position = new Vector3(0f, -375f, 0f);
			SetVisible(visible: false);
		}
	}

	public void Close()
	{
		Object.Destroy(m_gui);
		if ((bool)m_areaTop.m_dlg)
		{
			Object.Destroy(m_areaTop.m_dlg);
		}
		if ((bool)m_areaMiddle.m_dlg)
		{
			Object.Destroy(m_areaMiddle.m_dlg);
		}
		if ((bool)m_areaBottom.m_dlg)
		{
			Object.Destroy(m_areaBottom.m_dlg);
		}
		m_instance = null;
	}

	public void Clear()
	{
	}

	public void SetVisible(bool visible)
	{
		m_visible = visible;
		if (!m_visible)
		{
			m_gui.SetActiveRecursively(state: false);
			if ((bool)m_areaTop.m_dlg)
			{
				m_areaTop.m_dlg.SetActiveRecursively(state: false);
			}
			if ((bool)m_areaMiddle.m_dlg)
			{
				m_areaMiddle.m_dlg.SetActiveRecursively(state: false);
			}
			if ((bool)m_areaBottom.m_dlg)
			{
				m_areaBottom.m_dlg.SetActiveRecursively(state: false);
			}
		}
	}

	public void Update(List<ClientPlayer> players)
	{
	}

	private void SetMessage(MessageArea area, Message msg)
	{
		if ((bool)area.m_dlg)
		{
			Object.Destroy(area.m_dlg);
			area.m_dlg = null;
		}
		SpriteText spriteText = null;
		SpriteText spriteText2 = null;
		if (msg.m_prefab.Length == 0 || msg.m_prefab == "IngameGui/")
		{
			area.m_dlg = GuiUtils.CreateGui("IngameGui/ObjectiveMessage", m_guiCamera);
			spriteText = GuiUtils.FindChildOf(area.m_dlg.transform, "ObjectiveLabel").GetComponent<SpriteText>();
			spriteText2 = GuiUtils.FindChildOf(area.m_dlg.transform, "ObjectiveSubLabel").GetComponent<SpriteText>();
		}
		if (msg.m_prefab == "IngameGui/NewsflashMessage")
		{
			area.m_dlg = GuiUtils.CreateGui(msg.m_prefab, m_guiCamera);
			spriteText = GuiUtils.FindChildOf(area.m_dlg.transform, "ObjectiveLabel").GetComponent<SpriteText>();
		}
		if (msg.m_prefab == "IngameGui/ObjectiveDoneMessage")
		{
			area.m_dlg = GuiUtils.CreateGui(msg.m_prefab, m_guiCamera);
			spriteText = GuiUtils.FindChildOf(area.m_dlg.transform, "ObjectiveDoneLabel").GetComponent<SpriteText>();
			spriteText2 = GuiUtils.FindChildOf(area.m_dlg.transform, "ObjectiveDoneSubLabel").GetComponent<SpriteText>();
		}
		if (msg.m_prefab == "IngameGui/TurnMessage")
		{
			area.m_dlg = GuiUtils.CreateGui(msg.m_prefab, m_guiCamera);
			spriteText = GuiUtils.FindChildOf(area.m_dlg.transform, "TurnLabel").GetComponent<SpriteText>();
			spriteText2 = GuiUtils.FindChildOf(area.m_dlg.transform, "TurnSubLabel").GetComponent<SpriteText>();
		}
		spriteText.Text = msg.m_mainText;
		if ((bool)spriteText2)
		{
			spriteText2.Text = msg.m_subText;
		}
		area.m_dlg.GetComponent<UIPanel>().BringIn();
	}

	private void UpdateMessage(MessageArea area)
	{
		area.m_Timer -= Time.deltaTime;
		if (area.m_Timer >= m_fadeTime)
		{
			return;
		}
		if (!area.m_fading)
		{
			area.m_fading = true;
			area.m_dlg.GetComponent<UIPanel>().Dismiss();
		}
		if (area.m_Timer >= 0f || area.m_Queue.Count <= 0)
		{
			return;
		}
		Message message = area.m_Queue.Dequeue();
		area.m_Timer = message.m_displayTime + m_fadeTime;
		area.m_fading = false;
		if (area.m_dlg.GetComponent<UIPanel>().IsTransitioning)
		{
			EZTransition[] list = area.m_dlg.GetComponent<UIPanel>().Transitions.list;
			foreach (EZTransition eZTransition in list)
			{
				eZTransition.StopSafe();
			}
		}
		SetMessage(area, message);
	}

	public void Update()
	{
		UpdateMessage(m_areaTop);
		UpdateMessage(m_areaMiddle);
		UpdateMessage(m_areaBottom);
	}

	public void ShowMessage(TextPosition position, string maintext, string subtext, string prefab, float displayTime)
	{
		GameObject gameObject = GuiUtils.FindChildOf(m_guiCamera, "Dialog_Briefing(Clone)");
		if ((bool)gameObject)
		{
			PLog.Log("Skipping showmessage");
			return;
		}
		Message message = new Message();
		message.m_mainText = Localize.instance.Translate(maintext);
		message.m_subText = Localize.instance.Translate(subtext);
		message.m_prefab = "IngameGui/" + prefab;
		message.m_displayTime = displayTime;
		if (position == TextPosition.Top)
		{
			m_areaTop.m_Queue.Enqueue(message);
		}
		if (position == TextPosition.Middle)
		{
			m_areaMiddle.m_Queue.Enqueue(message);
		}
		if (position == TextPosition.Bottom)
		{
			m_areaBottom.m_Queue.Enqueue(message);
		}
	}
}
