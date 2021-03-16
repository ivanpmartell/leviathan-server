#define DEBUG
using UnityEngine;

[AddComponentMenu("Scripts/Gui/LobbyMenu_Player")]
public class LobbyMenu_Player : MonoBehaviour
{
	private const string ChooseFleetStr_lbl = "$gamelobby_nofleet";

	private const string FreeSlotStr = "$gamelobby_freeslot";

	public PlayerRemoved m_playerRemovedDelegate;

	public OpenInvite m_onOpenInvite;

	private UIButton btnInvitePlayer;

	private UIButton btnRemove;

	private SpriteText lblPlayerName;

	private SpriteText lblFleetName;

	private SpriteText lblFleetValue;

	private SpriteText lblFleetValueText;

	private SpriteText lblReady;

	private SpriteText lblTeam;

	private SimpleSprite onlineStatusIcon;

	private SimpleSprite playerFlag;

	private LobbyPlayer m_player;

	private static Material[] m_staticOnlineMats;

	public LobbyMenu_Player()
	{
		if (m_staticOnlineMats == null)
		{
			m_staticOnlineMats = new Material[3]
			{
				Resources.Load("OnlineStatus/status_offline") as Material,
				Resources.Load("OnlineStatus/status_online") as Material,
				Resources.Load("OnlineStatus/status_present") as Material
			};
		}
	}

	public void Awake()
	{
		lblReady = base.gameObject.transform.FindChild("dialog_bg/lblReady").GetComponent<SpriteText>();
		lblTeam = base.gameObject.transform.FindChild("dialog_bg/lblTeam").GetComponent<SpriteText>();
		lblFleetValue = GuiUtils.FindChildOf(base.gameObject, "fleetValue").GetComponent<SpriteText>();
		lblFleetValueText = GuiUtils.FindChildOf(base.gameObject, "fleetValueText").GetComponent<SpriteText>();
		ValidateComponents();
		RegisterDelegatesToComponents();
	}

	public void Setup(LobbyPlayer player, LobbyPlayer watcher, bool visible, bool showTeam, bool noFleet, FleetSize maxFleetSize)
	{
		m_player = player;
		if (!visible)
		{
			base.gameObject.SetActiveRecursively(state: false);
			return;
		}
		bool admin = watcher.m_admin;
		bool watcherIsThis = m_player != null && watcher.m_name == m_player.m_name;
		bool thisIsAdmin = m_player != null && m_player.m_admin;
		if (player != null)
		{
			SetPlayer(noFleet, showTeam, maxFleetSize);
			SetViewMode(watcher, admin, watcherIsThis, thisIsAdmin, noFleet);
			return;
		}
		lblPlayerName.gameObject.SetActiveRecursively(state: false);
		lblFleetName.gameObject.SetActiveRecursively(state: false);
		lblFleetValueText.gameObject.SetActiveRecursively(state: false);
		lblFleetValue.gameObject.SetActiveRecursively(state: false);
		onlineStatusIcon.gameObject.SetActiveRecursively(state: false);
		lblReady.gameObject.SetActiveRecursively(state: false);
		lblTeam.gameObject.SetActiveRecursively(state: false);
		playerFlag.gameObject.SetActiveRecursively(state: false);
		lblFleetValueText.gameObject.SetActiveRecursively(state: false);
		btnInvitePlayer.gameObject.SetActiveRecursively(state: false);
		btnRemove.gameObject.SetActiveRecursively(state: false);
		if (admin)
		{
			btnInvitePlayer.gameObject.SetActiveRecursively(state: true);
		}
	}

	private void SetPlayer(bool noFleet, bool showTeam, FleetSize maxFleetSize)
	{
		DebugUtils.Assert(m_player != null);
		if (showTeam)
		{
			lblTeam.gameObject.SetActiveRecursively(state: true);
			lblTeam.Text = (m_player.m_team + 1).ToString();
		}
		else
		{
			lblTeam.gameObject.SetActiveRecursively(state: false);
		}
		lblPlayerName.Text = m_player.m_name;
		btnRemove.gameObject.SetActiveRecursively(state: true);
		if (noFleet)
		{
			lblFleetName.gameObject.SetActiveRecursively(state: false);
			lblFleetValue.gameObject.SetActiveRecursively(state: false);
			lblFleetValueText.gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			lblFleetName.gameObject.SetActiveRecursively(state: true);
			if (string.IsNullOrEmpty(m_player.m_fleet))
			{
				lblFleetName.Text = Localize.instance.Translate("$gamelobby_nofleet");
				lblFleetValue.gameObject.SetActiveRecursively(state: false);
				lblFleetValueText.gameObject.SetActiveRecursively(state: true);
			}
			else
			{
				lblFleetName.Text = m_player.m_fleet;
				lblFleetValue.Text = m_player.m_fleetValue.ToString();
				lblFleetValue.gameObject.SetActiveRecursively(state: true);
				lblFleetValueText.gameObject.SetActiveRecursively(state: true);
				if (maxFleetSize != null && !maxFleetSize.ValidSize(m_player.m_fleetValue))
				{
					lblFleetValue.SetColor(Color.red);
					lblFleetValueText.SetColor(Color.red);
				}
				else
				{
					lblFleetValue.SetColor(Color.white);
					lblFleetValueText.SetColor(Color.white);
				}
			}
		}
		playerFlag.gameObject.SetActiveRecursively(state: true);
		Texture2D flagTexture = GuiUtils.GetFlagTexture(m_player.m_flag);
		SetPlayerFlag(flagTexture);
		SetOnlineStatus(m_player.m_status);
	}

	private void SetPlayerFlag(Texture2D value)
	{
		DebugUtils.Assert(playerFlag != null);
		if (!(playerFlag.renderer.material.mainTexture == value))
		{
			playerFlag.SetTexture(value);
			float x = value.width;
			float y = value.height;
			playerFlag.Setup(playerFlag.width, playerFlag.height, new Vector2(0f, y), new Vector2(x, y));
		}
	}

	private void SetViewMode(LobbyPlayer watcher, bool watcherIsAdmin, bool watcherIsThis, bool thisIsAdmin, bool noFleet)
	{
		if (m_player == null)
		{
			return;
		}
		lblReady.gameObject.SetActiveRecursively(m_player.m_readyToStart);
		playerFlag.gameObject.SetActiveRecursively(state: true);
		lblFleetValueText.gameObject.SetActiveRecursively(state: true);
		lblPlayerName.gameObject.SetActiveRecursively(state: true);
		btnInvitePlayer.gameObject.SetActiveRecursively(state: false);
		if (noFleet)
		{
			lblFleetValueText.gameObject.SetActiveRecursively(state: false);
			lblFleetName.gameObject.SetActiveRecursively(state: false);
			lblFleetValue.gameObject.SetActiveRecursively(state: false);
		}
		else
		{
			lblFleetName.gameObject.SetActiveRecursively(state: true);
			if (string.IsNullOrEmpty(m_player.m_fleet))
			{
				lblFleetName.Text = Localize.instance.Translate("$gamelobby_nofleet");
				lblFleetValueText.gameObject.SetActiveRecursively(state: false);
				lblFleetValue.gameObject.SetActiveRecursively(state: false);
			}
			else
			{
				lblFleetName.Text = m_player.m_fleet;
				lblFleetValueText.gameObject.SetActiveRecursively(state: true);
				lblFleetValue.gameObject.SetActiveRecursively(state: true);
			}
		}
		if (watcherIsThis)
		{
			onlineStatusIcon.gameObject.SetActiveRecursively(state: false);
			btnRemove.gameObject.SetActiveRecursively(state: false);
		}
		else
		{
			onlineStatusIcon.gameObject.SetActiveRecursively(state: true);
			btnRemove.gameObject.SetActiveRecursively(watcherIsAdmin);
		}
	}

	private void SetOnlineStatus(PlayerPresenceStatus ps)
	{
		if (onlineStatusIcon == null)
		{
			return;
		}
		MeshRenderer component = onlineStatusIcon.gameObject.GetComponent<MeshRenderer>();
		if (!(component == null))
		{
			if (ps >= PlayerPresenceStatus.Offline && (int)ps <= m_staticOnlineMats.Length - 1)
			{
				component.material = m_staticOnlineMats[(int)ps];
			}
		}
	}

	private void RegisterDelegatesToComponents()
	{
		if (btnRemove != null)
		{
			btnRemove.SetValueChangedDelegate(OnRemoveClicked);
		}
		if (btnInvitePlayer != null)
		{
			btnInvitePlayer.SetValueChangedDelegate(OnInviteClicked);
		}
	}

	private void OnRemoveClicked(IUIObject obj)
	{
		PLog.Log("OnRemoveClicked()");
		if (m_playerRemovedDelegate != null)
		{
			m_playerRemovedDelegate(m_player);
		}
	}

	private void OnInviteClicked(IUIObject obj)
	{
		PLog.Log("OnInviteClicked()");
		if (m_onOpenInvite != null)
		{
			m_onOpenInvite(m_player);
		}
	}

	private void ValidateComponents()
	{
		Validate_InvitePlayerButton();
		Validate_FleetNameLabel();
		Validate_RemoveButton();
		Validate_PlayerNameLabel();
		Validate_StatusIcon();
		Validate_PlayerFlag();
	}

	private bool Validate_InvitePlayerButton()
	{
		if (!ValidateTransform("dialog_bg/btnInvitePlayer", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnInvitePlayer = go.GetComponent<UIButton>();
		return btnInvitePlayer != null;
	}

	private bool Validate_FleetNameLabel()
	{
		if (!ValidateTransform("dialog_bg/lblFleetName", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		lblFleetName = go.GetComponent<SpriteText>();
		return lblFleetName != null;
	}

	private bool Validate_RemoveButton()
	{
		if (!ValidateTransform("dialog_bg/btnRemove", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		btnRemove = go.GetComponent<UIButton>();
		return btnRemove != null;
	}

	private bool Validate_PlayerNameLabel()
	{
		if (!ValidateTransform("dialog_bg/lblPlayerName", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		lblPlayerName = go.GetComponent<SpriteText>();
		return lblPlayerName != null;
	}

	private bool Validate_StatusIcon()
	{
		if (!ValidateTransform("dialog_bg/onlineStatusIcon", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		onlineStatusIcon = go.GetComponent<SimpleSprite>();
		return onlineStatusIcon != null;
	}

	private bool Validate_PlayerFlag()
	{
		if (!ValidateTransform("dialog_bg/playerFlag", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		playerFlag = go.GetComponent<SimpleSprite>();
		return playerFlag != null;
	}

	private bool ValidateTransform(string name, out GameObject go)
	{
		go = null;
		Transform transform = base.gameObject.transform.FindChild(name);
		if (transform == null)
		{
			return false;
		}
		go = transform.gameObject;
		return true;
	}

	public LobbyPlayer GetPlayer()
	{
		return m_player;
	}
}
