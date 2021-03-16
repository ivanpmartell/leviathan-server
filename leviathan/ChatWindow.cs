#define DEBUG
using System.Collections.Generic;
using UnityEngine;

public class ChatWindow : LogWindow
{
	public delegate void OnTextCommit(string text);

	public OnTextCommit m_OnTextCommit;

	private UITextField txtInput;

	public override void Initialize(GameObject guiCam, bool startVisible, List<string> messages, LogWindow_ScreenAlignment alignment)
	{
		base.Initialize(guiCam, startVisible, messages, alignment);
		txtInput.Text = string.Empty;
		txtInput.AddCommitDelegate(OnInputCommited);
	}

	private void OnInputCommited(IKeyFocusable control)
	{
		UITextField uITextField = control as UITextField;
		if (uITextField == null)
		{
			return;
		}
		string text = uITextField.Text;
		if (!string.IsNullOrEmpty(text))
		{
			if (m_OnTextCommit != null)
			{
				m_OnTextCommit(text);
			}
			txtInput.Text = string.Empty;
		}
	}

	protected override void LoadGUI()
	{
		if (m_gui == null)
		{
			m_gui = GuiUtils.CreateGui("LogDisplay/ChatWindow", m_guiCam);
		}
	}

	protected override void DoAdditionalValidation()
	{
		DebugUtils.Assert(Validate_TextInput(), "ChatWindow failed to validate label named UIScrollList named list");
	}

	private bool Validate_TextInput()
	{
		if (!ValidateTransform("txtInput", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		txtInput = go.GetComponent<UITextField>();
		return txtInput != null;
	}
}
