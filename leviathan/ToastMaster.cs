using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class ToastMaster
{
	private const float m_stayTime = 8f;

	private const float m_fadeTime = 2f;

	private GameObject m_toastGui;

	private float m_time;

	private bool m_dismissed;

	private Queue<UserEvent> m_toastQueue = new Queue<UserEvent>();

	private GameObject m_guiCamera;

	private PTech.RPC m_rpc;

	private UserManClient m_userMan;

	public ToastMaster(PTech.RPC rpc, GameObject guiCamera, UserManClient userManClient)
	{
		m_rpc = rpc;
		m_guiCamera = guiCamera;
		m_userMan = userManClient;
		m_rpc.Register("Toast", RPC_Toast);
	}

	public void Close()
	{
		m_rpc.Unregister("Toast");
		if (m_toastGui != null)
		{
			Object.Destroy(m_toastGui);
		}
	}

	private void RPC_Toast(PTech.RPC rpc, List<object> args)
	{
		PLog.Log("got toast ");
		byte[] data = (byte[])args[0];
		UserEvent item = new UserEvent(data);
		m_toastQueue.Enqueue(item);
	}

	private void CreateToast(UserEvent ev)
	{
		m_time = 0f;
		string text = string.Empty;
		switch (ev.GetEventType())
		{
		case UserEvent.EventType.NewTurn:
			m_toastGui = GuiUtils.CreateGui("Toast/ToastNewTurn", m_guiCamera);
			text = "$toast_new_turn " + ev.GetGameName();
			break;
		case UserEvent.EventType.Achievement:
		{
			m_toastGui = GuiUtils.CreateGui("Toast/ToastAchievement", m_guiCamera);
			text = "$toast_achievement_unlocked $achievement_name" + ev.GetAchievementID();
			SimpleSprite sprite = GuiUtils.FindChildOfComponent<SimpleSprite>(m_toastGui, "ToastIcon");
			Texture2D achievementIconTexture = GuiUtils.GetAchievementIconTexture(ev.GetAchievementID(), unlocked: true);
			if (achievementIconTexture != null)
			{
				GuiUtils.SetImage(sprite, achievementIconTexture);
			}
			break;
		}
		case UserEvent.EventType.FriendRequest:
			m_toastGui = GuiUtils.CreateGui("Toast/ToastFriendRequest", m_guiCamera);
			text = "$toast_friend_request " + ev.GetFriendName();
			break;
		case UserEvent.EventType.FriendRequestAccepted:
			m_toastGui = GuiUtils.CreateGui("Toast/ToastFriendAccepted", m_guiCamera);
			text = ev.GetFriendName() + " $toast_accepted_friend";
			m_userMan.UnlockAchievement(3);
			break;
		case UserEvent.EventType.GameInvite:
			m_toastGui = GuiUtils.CreateGui("Toast/ToastInvite", m_guiCamera);
			text = "$toast_invite_pre " + ev.GetFriendName() + " $toast_invite_post " + ev.GetGameName();
			break;
		case UserEvent.EventType.ServerMessage:
			m_toastGui = GuiUtils.CreateGui("Toast/ToastMessage", m_guiCamera);
			text = ev.GetGameName();
			break;
		}
		m_toastGui.GetComponent<UIPanel>().BringIn();
		SpriteText component = GuiUtils.FindChildOf(m_toastGui, "ToastMessage").GetComponent<SpriteText>();
		component.Text = Localize.instance.Translate(text);
	}

	public void Update(float dt)
	{
		if (m_toastGui == null)
		{
			if (m_toastQueue.Count > 0)
			{
				UserEvent ev = m_toastQueue.Dequeue();
				CreateToast(ev);
			}
			return;
		}
		UIPanel component = m_toastGui.GetComponent<UIPanel>();
		m_time += dt;
		if (!m_dismissed)
		{
			if (m_time > 8f)
			{
				component.Dismiss();
				m_dismissed = true;
			}
		}
		else if (m_time > 10f)
		{
			Object.Destroy(m_toastGui);
			m_toastGui = null;
		}
	}
}
