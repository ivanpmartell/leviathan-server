#define DEBUG
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ConnectMenu
{
	private class ServerData
	{
		public string m_name = string.Empty;

		public string m_host = string.Empty;

		public int m_port;

		public bool m_dev;
	}

	public delegate void CreateAccountHandler(string host, int port, string userName, string password, string email);

	public delegate void LoginDelegatetHandler(string visualName, string host, int port, string userName, string password, string token);

	public delegate void RequestVerificationHandler(string host, int port, string userName);

	public delegate void ResetPasswordDelegate(string host, int port, string email, string token, string password);

	public delegate void SingleHandler();

	public LoginDelegatetHandler m_onConnect;

	public CreateAccountHandler m_onCreateAccount;

	public RequestVerificationHandler m_onRequestVerificationMail;

	public Action<string, int, string> m_onRequestResetPasswordCode;

	public ResetPasswordDelegate m_onResetPassword;

	public SingleHandler m_onSinglePlayer;

	public Action m_onExit;

	private UITextField m_nameField;

	private UITextField m_passwordField;

	private UIButton m_loginButton;

	private SpriteText m_serverNameLbl;

	private UIButton m_serverPrev;

	private UIButton m_serverNext;

	private UIPanel m_newAccountPanel;

	private UITextField m_createName;

	private UITextField m_createEmail;

	private UITextField m_createPwd1;

	private UITextField m_createPwd2;

	private UIButton m_createButton;

	private UITextField m_resetEmailField;

	private UITextField m_resetVerificationCode;

	private UITextField m_resetPwd1;

	private UITextField m_resetPwd2;

	private string m_selectedServer = string.Empty;

	private List<ServerData> m_servers = new List<ServerData>();

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private MsgBox m_msgBox;

	private GameObject m_tokenDialog;

	private GameObject m_resetPasswordDialog;

	private string m_tempResetEmail;

	private GameObject m_reqVerMailDialog;

	private MusicManager m_musicMan;

	private List<string> m_languages = new List<string>();

	private int m_selectedLanguage;

	private OptionsWindow m_optionsWindow;

	private NewsTicker m_newsTicker;

	private GDPBackend m_gdpBackend;

	public ConnectMenu(GameObject guiCamera, MusicManager musMan, PdxNews pdxNews, GDPBackend gdpBackend)
	{
		m_guiCamera = guiCamera;
		m_musicMan = musMan;
		m_gdpBackend = gdpBackend;
		string[] languages = Constants.m_languages;
		foreach (string text in languages)
		{
			m_languages.Add(Localize.instance.Translate(text));
		}
		SetupGui();
		LoadServerList();
		string @string = PlayerPrefs.GetString("LastHost");
		if (@string != string.Empty)
		{
			SetSelectedServer(@string);
		}
		m_newsTicker = new NewsTicker(pdxNews, gdpBackend, m_guiCamera);
		if (m_nameField.Text == string.Empty)
		{
			m_guiCamera.GetComponent<UIManager>().FocusObject = m_nameField;
		}
		else
		{
			m_guiCamera.GetComponent<UIManager>().FocusObject = m_passwordField;
		}
		m_musicMan.SetMusic("menu");
	}

	private void SetupGui()
	{
		if (m_gui != null)
		{
			UnityEngine.Object.Destroy(m_gui);
		}
		m_gui = GuiUtils.CreateGui("Login", m_guiCamera);
		DebugUtils.Assert(m_gui != null);
		m_loginButton = GuiUtils.FindChildOf(m_gui, "LoginButton").GetComponent<UIButton>();
		m_nameField = GuiUtils.FindChildOf(m_gui, "NameField").GetComponent<UITextField>();
		m_passwordField = GuiUtils.FindChildOf(m_gui, "PasswordField").GetComponent<UITextField>();
		m_loginButton.GetComponent<UIButton>().SetValueChangedDelegate(OnLoginPressed);
		GuiUtils.FindChildOf(m_gui, "OfflineButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOfflinePressed);
		GuiUtils.FindChildOf(m_gui, "ExitButton").GetComponent<UIButton>().SetValueChangedDelegate(OnExitPressed);
		GuiUtils.FindChildOf(m_gui, "OptionsButton").GetComponent<UIButton>().SetValueChangedDelegate(OnOptionsPressed);
		GuiUtils.FindChildOf(m_gui, "LangArrowLeftButton").GetComponent<UIActionBtn>().SetValueChangedDelegate(OnLanguageLeft);
		GuiUtils.FindChildOf(m_gui, "LangArrowRightButton").GetComponent<UIActionBtn>().SetValueChangedDelegate(OnLanguageRight);
		GuiUtils.FindChildOf(m_gui, "VersionLbl").GetComponent<SpriteText>().Text = Localize.instance.Translate("$version " + VersionInfo.GetFullVersionString());
		GuiUtils.FindChildOf(m_gui, "HelpButton").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(onHelp);
		m_serverNameLbl = GuiUtils.FindChildOfComponent<SpriteText>(m_gui, "ServerNameLbl");
		m_serverPrev = GuiUtils.FindChildOfComponent<UIButton>(m_gui, "ArrowLeftButton");
		m_serverNext = GuiUtils.FindChildOfComponent<UIButton>(m_gui, "ArrowRightButton");
		m_serverPrev.SetValueChangedDelegate(OnPrevServer);
		m_serverNext.SetValueChangedDelegate(OnNextServer);
		DebugUtils.Assert(m_serverNameLbl != null);
		m_newAccountPanel = GuiUtils.FindChildOf(m_gui, "NewAccountPanel").GetComponent<UIPanel>();
		m_createName = GuiUtils.FindChildOf(m_newAccountPanel.gameObject, "UsernameField").GetComponent<UITextField>();
		m_createEmail = GuiUtils.FindChildOf(m_newAccountPanel.gameObject, "EmailField").GetComponent<UITextField>();
		m_createPwd1 = GuiUtils.FindChildOf(m_newAccountPanel.gameObject, "PasswordField_reg").GetComponent<UITextField>();
		m_createPwd2 = GuiUtils.FindChildOf(m_newAccountPanel.gameObject, "PasswordRepeatField").GetComponent<UITextField>();
		m_createButton = GuiUtils.FindChildOfComponent<UIButton>(m_newAccountPanel.gameObject, "CreateAccountAcceptButton");
		m_createButton.SetValueChangedDelegate(OnCreateAccount);
		m_resetEmailField = GuiUtils.FindChildOfComponent<UITextField>(m_gui, "ResetEmailField");
		m_resetEmailField.SetCommitDelegate(OnRequestResetPasswordCodeFieldOk);
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "ResetAcceptButton").SetValueChangedDelegate(OnRequestResetPasswordCodeOk);
		GuiUtils.FindChildOfComponent<UIButton>(m_gui, "ResetCancelButton").SetValueChangedDelegate(OnRequestResetPasswordCodeCancel);
		UIButton uIButton = GuiUtils.FindChildOfComponent<UIButton>(m_gui, "BtnResendVerification");
		uIButton.SetValueChangedDelegate(OnRequestVerificationMail);
		m_nameField.SetCommitDelegate(OnNameEnter);
		m_passwordField.SetCommitDelegate(OnPwdEnter);
		m_nameField.Text = PlayerPrefs.GetString("LastUserName");

		SetupLanguageButtons();
	}

	public void Close()
	{
		if (m_tokenDialog != null)
		{
			UnityEngine.Object.Destroy(m_tokenDialog);
			m_tokenDialog = null;
		}
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		UnityEngine.Object.Destroy(m_gui);
		m_newsTicker.Close();
	}

	public void Update()
	{
		if (Utils.IsAndroidBack())
		{
			OnExitPressed(null);
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			UIManager component = m_guiCamera.GetComponent<UIManager>();
			if (component.FocusObject == m_nameField)
			{
				component.FocusObject = m_passwordField;
			}
			else if (component.FocusObject == m_passwordField)
			{
				component.FocusObject = m_nameField;
			}
			if (component.FocusObject == m_createEmail)
			{
				component.FocusObject = m_createName;
			}
			else if (component.FocusObject == m_createName)
			{
				component.FocusObject = m_createPwd1;
			}
			else if (component.FocusObject == m_createPwd1)
			{
				component.FocusObject = m_createPwd2;
			}
			else if (component.FocusObject == m_createPwd2)
			{
				component.FocusObject = m_createEmail;
			}
			if (m_resetPasswordDialog != null)
			{
				if (component.FocusObject == m_resetVerificationCode)
				{
					component.FocusObject = m_resetPwd1;
				}
				else if (component.FocusObject == m_resetPwd1)
				{
					component.FocusObject = m_resetPwd2;
				}
				else if (component.FocusObject == m_resetPwd2)
				{
					component.FocusObject = m_resetVerificationCode;
				}
			}
		}
		if (m_msgBox != null)
		{
			m_msgBox.Update();
		}
		m_newsTicker.Update(Time.deltaTime);
		m_loginButton.controlIsEnabled = m_nameField.Text.Length > 0 && m_passwordField.Text.Length > 0;
		UpdateCreateAccount();
	}

	private void UpdateCreateAccount()
	{
		if (m_newAccountPanel.gameObject.active)
		{
			bool controlIsEnabled = m_createEmail.text.Length != 0 && m_createName.Text.Length != 0 && m_createPwd1.Text.Length != 0 && m_createPwd2.Text.Length != 0;
			m_createButton.controlIsEnabled = controlIsEnabled;
		}
	}

	public void OnLoginFailed(ErrorCode errorCode)
	{
		switch (errorCode)
		{
		case ErrorCode.WrongUserPassword:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_incorrect", null);
			break;
		case ErrorCode.VersionMissmatch:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_versionmissmatch", null);
			break;
		case ErrorCode.UserNotVerified:
			m_tokenDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$login_entertoken"), string.Empty, OnTokenCancel, OnTokenOk);
			break;
		case ErrorCode.InvalidVerificationToken:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_invalidtoken", null);
			break;
		case ErrorCode.ServerFull:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_serverfull", null);
			break;
		case ErrorCode.AccountExist:
			break;
		}
	}

	private void OnTokenCancel()
	{
		UnityEngine.Object.Destroy(m_tokenDialog);
		m_tokenDialog = null;
	}

	private void OnTokenOk(string token)
	{
		if (token.Length != 0)
		{
			UnityEngine.Object.Destroy(m_tokenDialog);
			m_tokenDialog = null;
			Login(token);
		}
	}

	public void OnConnectFailed()
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_connectfail", null);
	}

	private void OnNameEnter(IKeyFocusable field)
	{
		if (m_passwordField.Text.Length == 0)
		{
			UIManager component = m_guiCamera.GetComponent<UIManager>();
			component.FocusObject = m_passwordField;
		}
		else
		{
			Login(string.Empty);
		}
	}

	private void OnPwdEnter(IKeyFocusable field)
	{
		Login(string.Empty);
	}

	private void OnLoginPressed(IUIObject obj)
	{
		Login(string.Empty);
	}

	private void Login(string token)
	{
		if (m_nameField.Text.Length > 0 && m_passwordField.Text.Length > 0)
		{
			ServerData serverData = GetServerData(m_selectedServer);
			if (serverData == null)
			{
				PLog.LogWarning("No server selected");
				return;
			}
			PlayerPrefs.SetString("LastHost", m_selectedServer);
			PlayerPrefs.SetString("LastUserName", m_nameField.Text);
			m_onConnect(serverData.m_name, serverData.m_host, serverData.m_port, m_nameField.Text, m_passwordField.Text, token);
		}
	}

	private void OnOfflinePressed(IUIObject obj)
	{
		m_onSinglePlayer();
	}

	private void OnExitPressed(IUIObject obj)
	{
		m_onExit();
	}

	private void OnOptionsPressed(IUIObject obj)
	{
		m_optionsWindow = new OptionsWindow(m_guiCamera, inGame: false);
	}

	private void OnCreateAccount(IUIObject obj)
	{
		switch (Utils.IsValidUsername(m_createName.Text))
		{
		case Utils.ValidationStatus.ToShort:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_usernametooshort", null);
			return;
		case Utils.ValidationStatus.InvalidCharacter:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_invalidusername", null);
			return;
		}
		if (!Utils.IsEmailAddress(m_createEmail.Text))
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_invalidemail", null);
			return;
		}
		switch (Utils.IsValidPassword(m_createPwd1.Text))
		{
		case Utils.ValidationStatus.ToShort:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_passwordtooshort", null);
			return;
		case Utils.ValidationStatus.InvalidCharacter:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_invalidpassword", null);
			return;
		}
		if (m_createPwd1.Text != m_createPwd2.Text)
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_passwordmissmatch", null);
			return;
		}
		ServerData serverData = GetServerData(m_selectedServer);
		if (serverData == null)
		{
			PLog.LogWarning("No server selected");
		}
		else
		{
			m_onCreateAccount(serverData.m_host, serverData.m_port, m_createName.Text, m_createPwd1.Text, m_createEmail.Text);
		}
	}

	public void OnCreateFailed(ErrorCode error)
	{
		PLog.LogWarning("Creation failed , error code:" + error);
		switch (error)
		{
		case ErrorCode.AccountExist:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_accountexist", null);
			break;
		case ErrorCode.VersionMissmatch:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$login_versionmissmatch", null);
			break;
		}
	}

	public void OnCreateSuccess()
	{
		PLog.Log("Account creation success");
		UIPanelManager uIPanelManager = GuiUtils.FindChildOfComponent<UIPanelManager>(m_gui, "AdminPanelMan");
		uIPanelManager.Dismiss();
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_accountcreated", OnCloseCreatedMsgbox);
	}

	private void OnCloseCreatedMsgbox()
	{
		GameObject gameObject = GuiUtils.FindChildOf(m_gui, "NewAccountPanel");
		gameObject.GetComponent<UIPanel>().Dismiss();
	}

	private void SetupLanguageButtons()
	{
		string language = Localize.instance.GetLanguage();
		for (int i = 0; i < m_languages.Count; i++)
		{
			if (m_languages[i] == language)
			{
				m_selectedLanguage = i;
				break;
			}
		}
		GuiUtils.FindChildOf(m_gui, "LanguageNameLbl").GetComponent<SpriteText>().Text = Localize.instance.Translate("$language_" + language);
		GuiUtils.FindChildOf(m_gui, "LangArrowLeftButton").GetComponent<UIButton>().controlIsEnabled = m_selectedLanguage > 0;
		GuiUtils.FindChildOf(m_gui, "LangArrowRightButton").GetComponent<UIButton>().controlIsEnabled = m_selectedLanguage < m_languages.Count - 1;
	}

	private void onHelp(IUIObject button)
	{
		ToolTipDisplay toolTip = ToolTipDisplay.GetToolTip(m_gui);
		if (!(toolTip == null))
		{
			if (button.gameObject.GetComponent<UIStateToggleBtn>().StateNum == 0)
			{
				toolTip.SetHelpMode(helpMode: false);
			}
			if (button.gameObject.GetComponent<UIStateToggleBtn>().StateNum == 1)
			{
				toolTip.SetHelpMode(helpMode: true);
			}
		}
	}

	private void OnLanguageLeft(IUIObject button)
	{
		if (m_selectedLanguage > 0)
		{
			m_selectedLanguage--;
			string text = m_languages[m_selectedLanguage];
			Localize.instance.SetLanguage(text);
			PlayerPrefs.SetString("Language", text);
			SetupGui();
			SetSelectedServer(m_selectedServer);
		}
	}

	private void OnLanguageRight(IUIObject button)
	{
		if (m_selectedLanguage < m_languages.Count - 1)
		{
			m_selectedLanguage++;
			string text = m_languages[m_selectedLanguage];
			Localize.instance.SetLanguage(text);
			PlayerPrefs.SetString("Language", text);
			SetupGui();
			SetSelectedServer(m_selectedServer);
		}
	}

	private int GetSelectedServerId(string name)
	{
		int num = 0;
		foreach (ServerData server in m_servers)
		{
			if (server.m_name == name)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private ServerData GetServerData(string name)
	{
		foreach (ServerData server in m_servers)
		{
			if (server.m_name == name)
			{
				return server;
			}
		}
		return null;
	}

	private void LoadServerList()
	{
		XmlDocument xmlDocument = Utils.LoadXml("serverlist");
		DebugUtils.Assert(xmlDocument != null);
		bool isDebugBuild = Debug.isDebugBuild;
		XmlNode xmlNode = xmlDocument.FirstChild.FirstChild;
		while (xmlNode != null)
		{
			if (xmlNode.Name == "server")
			{
				ServerData serverData = new ServerData();
				serverData.m_name = xmlNode.Attributes["name"].Value;
				serverData.m_host = xmlNode.Attributes["host"].Value;
				serverData.m_port = int.Parse(xmlNode.Attributes["port"].Value);
				if (xmlNode.Attributes["dev"] != null)
				{
					bool flag = bool.Parse(xmlNode.Attributes["dev"].Value);
					if ((flag && !isDebugBuild) || (!flag && isDebugBuild))
					{
						xmlNode = xmlNode.NextSibling;
						continue;
					}
				}
				else if (Debug.isDebugBuild)
				{
					continue;
				}
				PLog.Log("Server " + serverData.m_name + " " + serverData.m_host + " " + serverData.m_port);
				m_servers.Add(serverData);
			}
			xmlNode = xmlNode.NextSibling;
		}
		if (m_servers.Count > 0)
		{
			int index = 0;
			SetSelectedServer(m_servers[index].m_name);
		}
	}

	private void OnNextServer(IUIObject button)
	{
		int selectedServerId = GetSelectedServerId(m_selectedServer);
		selectedServerId++;
		if (selectedServerId >= m_servers.Count - 1)
		{
			selectedServerId = m_servers.Count - 1;
		}
		SetSelectedServer(m_servers[selectedServerId].m_name);
	}

	private void OnPrevServer(IUIObject button)
	{
		int selectedServerId = GetSelectedServerId(m_selectedServer);
		selectedServerId--;
		if (selectedServerId < 0)
		{
			selectedServerId = 0;
		}
		SetSelectedServer(m_servers[selectedServerId].m_name);
	}

	private void SetSelectedServer(string name)
	{
		PLog.LogWarning("Selecting " + name);
		int selectedServerId = GetSelectedServerId(name);
		if (selectedServerId >= 0)
		{
			m_selectedServer = name;
			m_serverNameLbl.Text = name;
			m_serverPrev.controlIsEnabled = selectedServerId != 0;
			m_serverNext.controlIsEnabled = selectedServerId != m_servers.Count - 1;
			m_serverNameLbl.Text = name;
		}
	}

	private void OnRequestResetPasswordCodeFieldOk(IKeyFocusable field)
	{
		RequestResetPasswordCode();
	}

	private void OnRequestResetPasswordCodeOk(IUIObject button)
	{
		RequestResetPasswordCode();
	}

	private void RequestResetPasswordCode()
	{
		string text = m_resetEmailField.Text;
		if (!Utils.IsEmailAddress(text))
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_invalidemail", null);
			return;
		}
		ServerData serverData = GetServerData(m_selectedServer);
		if (serverData == null)
		{
			PLog.LogWarning("No server selected");
			return;
		}
		m_tempResetEmail = text;
		m_onRequestResetPasswordCode(serverData.m_host, serverData.m_port, text);
		m_resetPasswordDialog = GuiUtils.CreateGui("GenericInputDialog_passwordReset", m_guiCamera);
		GuiUtils.FindChildOfComponent<UIButton>(m_resetPasswordDialog, "OkButton").SetValueChangedDelegate(OnResetPasswordOk);
		GuiUtils.FindChildOfComponent<UIButton>(m_resetPasswordDialog, "CancelButton").SetValueChangedDelegate(OnResetPasswordCancel);
		m_resetVerificationCode = GuiUtils.FindChildOfComponent<UITextField>(m_resetPasswordDialog, "txtInput_verification");
		m_resetPwd1 = GuiUtils.FindChildOfComponent<UITextField>(m_resetPasswordDialog, "txtInput_password");
		m_resetPwd2 = GuiUtils.FindChildOfComponent<UITextField>(m_resetPasswordDialog, "txtInput_passwordrepeat");
		UIManager component = m_guiCamera.GetComponent<UIManager>();
		component.FocusObject = m_resetVerificationCode;
		UIPanelManager uIPanelManager = GuiUtils.FindChildOfComponent<UIPanelManager>(m_gui, "AdminPanelMan");
		uIPanelManager.Dismiss();
	}

	private void OnRequestResetPasswordCodeCancel(IUIObject button)
	{
		m_resetEmailField.Text = string.Empty;
	}

	private void OnResetPasswordOk(IUIObject button)
	{
		if (m_resetVerificationCode.Text.Length == 0)
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_needverificationcode", null);
			return;
		}
		switch (Utils.IsValidPassword(m_resetPwd1.Text))
		{
		case Utils.ValidationStatus.ToShort:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_passwordtooshort", null);
			return;
		case Utils.ValidationStatus.InvalidCharacter:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_invalidpassword", null);
			return;
		}
		if (m_resetPwd1.Text != m_resetPwd2.Text)
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$createaccount_passwordmissmatch", null);
			return;
		}
		ServerData serverData = GetServerData(m_selectedServer);
		if (serverData == null)
		{
			PLog.LogWarning("No server selected");
		}
		else
		{
			m_onResetPassword(serverData.m_host, serverData.m_port, m_tempResetEmail, m_resetVerificationCode.Text, m_resetPwd1.Text);
		}
	}

	private void OnResetPasswordCancel(IUIObject button)
	{
		UnityEngine.Object.Destroy(m_resetPasswordDialog);
		m_resetPasswordDialog = null;
	}

	public void OnResetPasswordConfirmed()
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_passwordresetconfirmed", null);
		UnityEngine.Object.Destroy(m_resetPasswordDialog);
		m_resetPasswordDialog = null;
	}

	public void OnResetPasswordFail()
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$label_passwordresetfailed", null);
		UnityEngine.Object.Destroy(m_resetPasswordDialog);
		m_resetPasswordDialog = null;
	}

	private void OnRequestVerificationMail(IUIObject button)
	{
		m_reqVerMailDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$dialog_resendverificationcode"), string.Empty, OnReqVerMailCancel, OnReqVerMailOk);
	}

	private void OnReqVerMailCancel()
	{
		UnityEngine.Object.Destroy(m_reqVerMailDialog);
		m_reqVerMailDialog = null;
	}

	private void OnReqVerMailOk(string text)
	{
		RequestVerificationEmail(text);
	}

	private void RequestVerificationEmail(string email)
	{
		ServerData serverData = GetServerData(m_selectedServer);
		if (serverData == null)
		{
			PLog.LogWarning("No server selected");
		}
		else
		{
			m_onRequestVerificationMail(serverData.m_host, serverData.m_port, email);
		}
	}

	public void OnRequestVerificaionRespons(ErrorCode errorCode)
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		switch (errorCode)
		{
		case ErrorCode.NoError:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$dialog_resendverificationcode_success", null);
			UnityEngine.Object.Destroy(m_reqVerMailDialog);
			m_reqVerMailDialog = null;
			break;
		case ErrorCode.AccountDoesNotExist:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$dialog_resendverificationcode_fail_accountnotfound", null);
			break;
		case ErrorCode.UserAlreadyVerified:
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$dialog_resendverificationcode_fail_alreadyverified", null);
			UnityEngine.Object.Destroy(m_reqVerMailDialog);
			m_reqVerMailDialog = null;
			break;
		}
	}
}
