using UnityEngine;

internal class LogMsg_Text : LogMsg
{
	private SpriteText lblMsg;

	public LogMsg_Text(GameObject guiCam, string message)
	{
		m_gui = GuiUtils.CreateGui("LogDisplay/LogMsg_Text", guiCam);
		m_listItemComponent = m_gui.GetComponent<UIListItem>();
		lblMsg = m_gui.transform.Find("lblMsg").gameObject.GetComponent<SpriteText>();
		message = message.Trim();
		lblMsg.Text = message;
	}
}
