#define DEBUG
using UnityEngine;

public class GenericTextInput : MonoBehaviour
{
	private delegate void InputTextChanged(string newValue);

	public delegate void InputTextCommit(string text);

	public delegate void InputTextCancel();

	private SpriteText lblTitle;

	private UIButton btn1;

	private UIButton btn2;

	private UITextField txtInput;

	private Object m_AdditionalData;

	private bool isValid;

	private bool m_allowEmptyInput;

	private InputTextChanged m_onInputChanged;

	private InputTextCommit m_onCommit;

	private InputTextCancel m_onCancel;

	public bool AllowEmptyInput
	{
		get
		{
			return m_allowEmptyInput;
		}
		set
		{
			m_allowEmptyInput = value;
		}
	}

	public Object AdditionalData
	{
		get
		{
			return m_AdditionalData;
		}
		set
		{
			m_AdditionalData = value;
		}
	}

	public string Text
	{
		get
		{
			return txtInput.GetComponent<UITextField>().Text;
		}
		set
		{
			txtInput.GetComponent<UITextField>().Text = value;
		}
	}

	public bool IsValid
	{
		get
		{
			return isValid;
		}
		private set
		{
			isValid = value;
		}
	}

	public void Initialize(string title, string btn1text, string btn2text, string textfieldText, InputTextCancel cancel, InputTextCommit commit)
	{
		ValidateComponents();
		m_onCancel = cancel;
		m_onCommit = commit;
		lblTitle.Text = Localize.instance.Translate(title);
		btn1.Text = Localize.instance.Translate(btn1text);
		btn2.Text = Localize.instance.Translate(btn2text);
		txtInput.Text = textfieldText;
		txtInput.allowClickCaretPlacement = false;
		if (!string.IsNullOrEmpty(textfieldText.Trim()))
		{
			txtInput.AddInputDelegate(TextFieldClicked);
		}
		txtInput.AddValueChangedDelegate(InputChanged);
		txtInput.SetCommitDelegate(OnTextCommit);
		btn1.SetValueChangedDelegate(OnCancel);
		btn2.SetValueChangedDelegate(OnOk);
		InputChanged(null);
		UIManager.instance.FocusObject = txtInput;
	}

	private void OnTextCommit(IKeyFocusable control)
	{
		if (m_onCommit != null)
		{
			m_onCommit(control.Content);
		}
	}

	private void OnOk(IUIObject obj)
	{
		if (m_onCommit != null)
		{
			m_onCommit(txtInput.Text);
		}
	}

	private void OnCancel(IUIObject obj)
	{
		if (m_onCancel != null)
		{
			m_onCancel();
		}
	}

	private void InputChanged(IUIObject obj)
	{
		string newValue = string.Empty;
		if (!m_allowEmptyInput && string.IsNullOrEmpty(txtInput.Text.Trim()))
		{
			btn2.controlIsEnabled = false;
		}
		else
		{
			btn2.controlIsEnabled = true;
			newValue = txtInput.Text;
		}
		if (m_onInputChanged != null)
		{
			m_onInputChanged(newValue);
		}
	}

	private void TextFieldClicked(ref POINTER_INFO ptr)
	{
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE || ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			txtInput.Text = string.Empty;
			txtInput.RemoveInputDelegate(TextFieldClicked);
		}
	}

	public void Hide()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
	}

	public void Show()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = true;
		}
	}

	private void ValidateComponents()
	{
		if (!IsValid)
		{
			DebugUtils.Assert(Validate_lblTitle(), "GenericTextInput failed to validate label named lblTitle !");
			DebugUtils.Assert(Validate_btn1(), "GenericTextInput failed to validate button named btn1 !");
			DebugUtils.Assert(Validate_btn2(), "GenericTextInput failed to validate button named btn2 !");
			DebugUtils.Assert(Validate_txtInput(), "GenericTextInput failed to validate textfield named txtInput !");
			IsValid = true;
		}
	}

	private bool Validate_lblTitle()
	{
		if (!ValidateTransform("lblTitle", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		lblTitle = go.GetComponent<SpriteText>();
		return lblTitle != null;
	}

	private bool Validate_btn1()
	{
		if (!ValidateTransform("ButtonsPanel/btn1", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btn1 = go.GetComponent<UIButton>();
		return btn1 != null;
	}

	private bool Validate_btn2()
	{
		if (!ValidateTransform("ButtonsPanel/btn2", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btn2 = go.GetComponent<UIButton>();
		return btn2 != null;
	}

	private bool Validate_txtInput()
	{
		if (!ValidateTransform("txtInput", out var go))
		{
			return false;
		}
		txtInput = go.GetComponent<UITextField>();
		return txtInput != null;
	}

	private bool ValidateTransform(string name, out GameObject go)
	{
		go = null;
		Transform transform = base.transform.FindChild(name);
		if (transform == null)
		{
			return false;
		}
		go = transform.gameObject;
		return true;
	}
}
