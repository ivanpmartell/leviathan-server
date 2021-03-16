#define DEBUG
using UnityEngine;

public class StatusWnd_Gun : StatusWnd_HPModule
{
	private const float m_iconSpacingX = 3f;

	private bool m_currentlyViewedAsFriend = true;

	private bool m_isValid;

	private GameObject m_guiCam;

	private Gun m_gun;

	private GameObject m_gui;

	private SpriteText m_lblGunName;

	private SpriteText m_lblAmmo;

	private SimpleSprite m_background;

	private UIProgressBar m_healthbar;

	private SpriteText m_lblHealth;

	private SimpleSprite m_icon;

	private Transform m_indicationLightsList;

	private StatusLight_Basic m_status_repair;

	private StatusLight_Basic m_status_acid;

	private StatusLight_Basic m_status_electricity;

	private StatusLight_Basic m_status_fire;

	private StatusLight_Basic m_status_generalWarning;

	private StatusLight_Advanced m_status_ammo;

	private SpriteText m_lblGunStatus;

	public bool IsValid
	{
		get
		{
			return m_isValid;
		}
		private set
		{
			m_isValid = value;
		}
	}

	public string GunName
	{
		get
		{
			return (!(m_lblGunName == null)) ? m_lblGunName.Text : string.Empty;
		}
		private set
		{
			if (m_lblGunName != null)
			{
				m_lblGunName.Text = value;
			}
		}
	}

	public Texture2D Background
	{
		get
		{
			return (!(m_background == null)) ? (m_background.renderer.material.mainTexture as Texture2D) : null;
		}
		set
		{
			if (!(m_background == null))
			{
				float width = m_background.width;
				float height = m_background.height;
				m_background.SetTexture(value);
				float num = value.width;
				float num2 = value.height;
				m_background.Setup(num, num2, new Vector2(0f, num2), new Vector2(num, num2));
				m_background.gameObject.transform.localScale = new Vector3(width / (float)value.width, height / (float)value.height, 1f);
				m_background.UpdateUVs();
			}
		}
	}

	public bool CurrentlyViewedAsFriend
	{
		get
		{
			return m_currentlyViewedAsFriend;
		}
		private set
		{
			m_currentlyViewedAsFriend = value;
		}
	}

	public StatusWnd_Gun(Gun gun, GameObject GUICam, bool friend)
	{
		m_gun = gun;
		Initialize(GUICam, friendOrFoe: true);
	}

	private void Initialize(GameObject GUICam, bool friendOrFoe)
	{
		CurrentlyViewedAsFriend = friendOrFoe;
		m_guiCam = GUICam;
		m_gui = GuiUtils.CreateGui("IngameGui/StatusWnd_Gun", m_guiCam);
		ValidateComponents();
		LoadStatusLights(m_guiCam);
		Update();
	}

	public void Update()
	{
		if (m_gun.GetUnit().IsDead())
		{
			m_gui.SetActiveRecursively(state: false);
			return;
		}
		m_gui.SetActiveRecursively(state: true);
		m_lblGunName.Text = m_gun.GetName();
		m_lblGunStatus.Text = Localize.instance.Translate(m_gun.GetStatusText());
		SetIcon(m_gun.m_GUITexture);
		float num = (float)m_gun.GetHealth() / (float)m_gun.GetMaxHealth();
		if (num <= 0.25f)
		{
			m_status_generalWarning.SetOnOff(onOff: true);
		}
		else
		{
			m_status_generalWarning.SetOnOff(onOff: false);
		}
		SetHealthPercentage(m_gun.GetHealth(), m_gun.GetMaxHealth());
		int maxAmmo = m_gun.GetMaxAmmo();
		if (maxAmmo < 0)
		{
			m_lblAmmo.Text = ((int)m_gun.GetLoadedSalvo()).ToString();
			m_status_ammo.SetIcon_Percentage(100f);
		}
		else
		{
			int num2 = m_gun.GetAmmo() + (int)m_gun.GetLoadedSalvo();
			m_lblAmmo.Text = num2.ToString();
			float icon_Percentage = (float)num2 / (float)m_gun.GetMaxAmmo() * 100f;
			m_status_ammo.SetIcon_Percentage(icon_Percentage);
		}
		if (!m_currentlyViewedAsFriend)
		{
		}
	}

	public void Close()
	{
		Object.DestroyImmediate(m_gui);
	}

	public void SetTint(Color color)
	{
		if (!(m_background == null))
		{
			m_background.Color = color;
		}
	}

	public void TryChangeBackground(string textureUrl)
	{
		string o = $"StatusWnd_Gun failed to load background \"{textureUrl}\"";
		if (string.IsNullOrEmpty(textureUrl))
		{
			PLog.LogWarning(o);
			return;
		}
		Texture2D texture2D = Resources.Load(textureUrl) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning(o);
		}
		else
		{
			Background = texture2D;
		}
	}

	public void SetHealthPercentage(int health, int maxHealth)
	{
		m_lblHealth.Text = health.ToString();
		m_healthbar.Value = Mathf.Clamp((float)health / (float)maxHealth, 0f, 1f);
	}

	private void ValidateComponents()
	{
		if (!m_isValid)
		{
			m_lblHealth = m_gui.transform.Find("lblHealth").GetComponent<SpriteText>();
			m_background = m_gui.GetComponent<SimpleSprite>();
			DebugUtils.Assert(m_background != null, "StatusWnd_Gun has no SimpleSprite-component to be used as background !");
			DebugUtils.Assert(Validate_GunNameLabel(), "StatusWnd_Gun failed to validate label named \"lblGunName\"");
			DebugUtils.Assert(Validate_AmmoIcon(), "StatusWnd_Gun failed to validate StatusLight_Advanced named \"Ammo\"");
			DebugUtils.Assert(Validate_AmmoLabel(), "StatusWnd_Gun failed to validate label named \"lblAmmo\"");
			DebugUtils.Assert(Validate_StatusLabel(), "StatusWnd_Gun failed to validate label named \"lblGunStatus\"");
			DebugUtils.Assert(Validate_Icon(), "StatusWnd_Gun failed to validate SimpleSprite named \"Icon\"");
			DebugUtils.Assert(Validate_Health(), "StatusWnd_Gun failed to validate UIProgressBar named \"Progressbar_Health\"");
			DebugUtils.Assert(ValidateModifierIconList(), "StatusWnd_Gun failed to validate transform named \"ModifierIndications\"");
			m_isValid = true;
		}
	}

	private bool Validate_GunNameLabel()
	{
		if (!ValidateTransform("lblGunName", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_lblGunName = go.GetComponent<SpriteText>();
		return m_lblGunName != null;
	}

	private bool Validate_AmmoLabel()
	{
		if (!ValidateTransform("lblAmmo", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_lblAmmo = go.GetComponent<SpriteText>();
		return m_lblAmmo != null;
	}

	private bool Validate_StatusLabel()
	{
		if (!ValidateTransform("lblGunStatus", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_lblGunStatus = go.GetComponent<SpriteText>();
		return m_lblGunStatus != null;
	}

	private bool Validate_Icon()
	{
		if (!ValidateTransform("Icon", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_icon = go.GetComponent<SimpleSprite>();
		return m_icon != null;
	}

	private bool Validate_Health()
	{
		if (!ValidateTransform("Progressbar_Health", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_healthbar = go.GetComponent<UIProgressBar>();
		return m_healthbar != null;
	}

	private bool ValidateModifierIconList()
	{
		m_indicationLightsList = m_gui.transform.Find("ModifierIndications");
		return m_indicationLightsList != null;
	}

	private bool Validate_AmmoIcon()
	{
		if (!ValidateTransform("Ammo", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_status_ammo = go.GetComponent<StatusLight_Advanced>();
		if (m_status_ammo == null)
		{
			return false;
		}
		m_status_ammo.Initialize();
		return true;
	}

	private void LoadStatusLights(GameObject guiCamera)
	{
		float num = 0f;
		m_status_generalWarning = StatusLight_Basic.Create(guiCamera, "Warning");
		m_status_generalWarning.gameObject.transform.parent = m_gui.transform.Find("Icon");
		m_status_generalWarning.gameObject.transform.localPosition = new Vector3(-24f, 23f);
		m_status_generalWarning.DisableWhenOff = true;
		m_status_generalWarning.SetOnOff(onOff: false);
		m_status_acid = StatusLight_Basic.Create(guiCamera, "Acid");
		m_status_acid.DisableWhenOff = false;
		m_status_acid.gameObject.transform.parent = m_indicationLightsList;
		m_status_acid.gameObject.transform.localPosition = new Vector3(num, 0f, -1f);
		num += m_status_acid.ScaledWidth + 3f;
		m_status_repair = StatusLight_Basic.Create(guiCamera, "BeingRepaired");
		m_status_repair.DisableWhenOff = false;
		m_status_repair.gameObject.transform.parent = m_indicationLightsList;
		m_status_repair.gameObject.transform.localPosition = new Vector3(num, 0f, -1f);
		num += m_status_repair.ScaledWidth + 3f;
		m_status_electricity = StatusLight_Basic.Create(guiCamera, "Electricity");
		m_status_electricity.DisableWhenOff = false;
		m_status_electricity.gameObject.transform.parent = m_indicationLightsList;
		m_status_electricity.gameObject.transform.localPosition = new Vector3(num, 0f, -1f);
		num += m_status_electricity.ScaledWidth + 3f;
		m_status_fire = StatusLight_Basic.Create(guiCamera, "Fire");
		m_status_fire.DisableWhenOff = false;
		m_status_fire.gameObject.transform.parent = m_indicationLightsList;
		m_status_fire.gameObject.transform.localPosition = new Vector3(num, 0f, -1f);
	}

	private bool ValidateTransform(string name, out GameObject go)
	{
		go = null;
		Transform transform = m_gui.transform.FindChild(name);
		if (transform == null)
		{
			return false;
		}
		go = transform.gameObject;
		return true;
	}

	private void SetIcon(Texture2D value)
	{
		if (!(value == null))
		{
			m_icon.SetTexture(value);
			m_icon.Setup(m_icon.width, m_icon.height, new Vector2(0f, value.width), new Vector2(value.width, value.height));
		}
	}
}
