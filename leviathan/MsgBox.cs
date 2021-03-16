using UnityEngine;

internal class MsgBox
{
	public enum Type
	{
		Ok,
		YesNo,
		OkCancel,
		TextOnly
	}

	public delegate void OkHandler();

	public delegate void CancelHandler();

	public delegate void YesHandler();

	public delegate void NoHandler();

	public OkHandler m_onOk;

	public CancelHandler m_onCancel;

	public YesHandler m_onYes;

	public NoHandler m_onNo;

	private Type m_type;

	private UIButton m_okButton;

	private UIButton m_cancelButton;

	private UIButton m_yesButton;

	private UIButton m_noButton;

	private GameObject m_gui;

	public MsgBox(GameObject guiCamera, Type type, string text, OkHandler okHandler, CancelHandler cancelHandler, YesHandler yesHandler, NoHandler noHandler)
	{
		m_type = type;
		m_onOk = okHandler;
		m_onCancel = cancelHandler;
		m_onYes = yesHandler;
		m_onNo = noHandler;
		m_gui = GuiUtils.CreateGui("MsgBox", guiCamera);
		m_gui.transform.FindChild("MsgBoxAnchor/Wnd/TextLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(text);
		m_okButton = m_gui.transform.FindChild("MsgBoxAnchor/Wnd/OkButton").GetComponent<UIButton>();
		m_cancelButton = m_gui.transform.FindChild("MsgBoxAnchor/Wnd/CancelButton").GetComponent<UIButton>();
		m_yesButton = m_gui.transform.FindChild("MsgBoxAnchor/Wnd/YesButton").GetComponent<UIButton>();
		m_noButton = m_gui.transform.FindChild("MsgBoxAnchor/Wnd/NoButton").GetComponent<UIButton>();
		m_okButton.SetValueChangedDelegate(OnOk);
		m_cancelButton.SetValueChangedDelegate(OnCancel);
		m_yesButton.SetValueChangedDelegate(OnYes);
		m_noButton.SetValueChangedDelegate(OnNo);
		m_okButton.gameObject.SetActiveRecursively(state: false);
		m_cancelButton.gameObject.SetActiveRecursively(state: false);
		m_yesButton.gameObject.SetActiveRecursively(state: false);
		m_noButton.gameObject.SetActiveRecursively(state: false);
		switch (type)
		{
		case Type.Ok:
			m_okButton.gameObject.SetActiveRecursively(state: true);
			break;
		case Type.OkCancel:
			m_okButton.gameObject.SetActiveRecursively(state: true);
			m_cancelButton.gameObject.SetActiveRecursively(state: true);
			break;
		case Type.YesNo:
			m_yesButton.gameObject.SetActiveRecursively(state: true);
			m_noButton.gameObject.SetActiveRecursively(state: true);
			break;
		case Type.TextOnly:
			break;
		}
	}

	public static MsgBox CreateOkMsgBox(GameObject guiCamera, string text, OkHandler okHandler)
	{
		return new MsgBox(guiCamera, Type.Ok, text, okHandler, null, null, null);
	}

	public static MsgBox CreateTextOnlyMsgBox(GameObject guiCamera, string text)
	{
		return new MsgBox(guiCamera, Type.TextOnly, text, null, null, null, null);
	}

	public static MsgBox CreateYesNoMsgBox(GameObject guiCamera, string text, YesHandler yesHandler, NoHandler noHandler)
	{
		return new MsgBox(guiCamera, Type.YesNo, text, null, null, yesHandler, noHandler);
	}

	public void Update()
	{
		if (m_type == Type.Ok && Input.GetKeyDown(KeyCode.Return))
		{
			Close();
			if (m_onOk != null)
			{
				m_onOk();
			}
		}
	}

	public void Close()
	{
		Object.Destroy(m_gui);
	}

	public void OnCancel(IUIObject obj)
	{
		Close();
		if (m_onCancel != null)
		{
			m_onCancel();
		}
	}

	public void OnOk(IUIObject obj)
	{
		Close();
		if (m_onOk != null)
		{
			m_onOk();
		}
	}

	public void OnYes(IUIObject obj)
	{
		Close();
		if (m_onYes != null)
		{
			m_onYes();
		}
	}

	public void OnNo(IUIObject obj)
	{
		Close();
		if (m_onNo != null)
		{
			m_onNo();
		}
	}
}
