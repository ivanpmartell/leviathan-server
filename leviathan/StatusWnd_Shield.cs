#define DEBUG
using UnityEngine;

public class StatusWnd_Shield : StatusWnd_HPModule
{
	private const float m_iconSpacingX = 3f;

	private bool m_currentlyViewedAsFriend = true;

	private bool m_isValid;

	private GameObject m_guiCam;

	private HPModule m_shield;

	private GameObject m_gui;

	private SpriteText m_lblName;

	private SimpleSprite m_background;

	private UIProgressBar m_healthbar;

	private SpriteText m_lblHealth;

	private UIProgressBar m_energyBar;

	private SpriteText m_lblEnergy;

	private SpriteText m_lblStatus;

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

	public StatusWnd_Shield(HPModule shield, GameObject GUICam, bool friendly)
	{
		m_guiCam = GUICam;
		m_shield = shield;
		m_gui = GuiUtils.CreateGui("IngameGui/StatusWnd_Shield", m_guiCam);
		CurrentlyViewedAsFriend = friendly;
		ValidateComponents();
		Update();
	}

	public void Update()
	{
		if (m_shield.GetUnit().IsDead())
		{
			m_gui.SetActiveRecursively(state: false);
			return;
		}
		m_lblName.Text = m_shield.GetName();
		m_lblStatus.Text = Localize.instance.Translate(m_shield.GetStatusText());
		SetHealthPercentage(m_shield.GetHealth(), m_shield.GetMaxHealth());
		SetEnergy(m_shield.GetEnergy(), m_shield.GetMaxEnergy());
	}

	private void SetHealthPercentage(int health, int maxHealth)
	{
		m_lblHealth.Text = health.ToString();
		m_healthbar.Value = Mathf.Clamp((float)health / (float)maxHealth, 0f, 1f);
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

	public void SetEnergy(float energy, float maxEnergy)
	{
		m_lblEnergy.Text = energy.ToString("F0") + " / " + maxEnergy.ToString("F0");
		m_energyBar.Value = Mathf.Clamp(energy / maxEnergy, 0f, 1f);
	}

	private void ValidateComponents()
	{
		if (!m_isValid)
		{
			m_lblHealth = m_gui.transform.Find("lblHealth").GetComponent<SpriteText>();
			m_background = m_gui.GetComponent<SimpleSprite>();
			m_lblEnergy = GuiUtils.FindChildOf(m_gui, "lblEnergy").GetComponent<SpriteText>();
			m_energyBar = GuiUtils.FindChildOf(m_gui, "Progressbar_Energy").GetComponent<UIProgressBar>();
			m_healthbar = GuiUtils.FindChildOf(m_gui, "Progressbar_Health").GetComponent<UIProgressBar>();
			DebugUtils.Assert(m_background != null, "StatusWnd_Gun has no SimpleSprite-component to be used as background !");
			DebugUtils.Assert(Validate_NameLabel(), "StatusWnd_Gun failed to validate label named \"lblGunName\"");
			DebugUtils.Assert(Validate_StatusLabel(), "StatusWnd_Gun failed to validate label named \"lblGunStatus\"");
			m_isValid = true;
		}
	}

	private bool Validate_NameLabel()
	{
		if (!ValidateTransform("lblName", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_lblName = go.GetComponent<SpriteText>();
		return m_lblName != null;
	}

	private bool Validate_StatusLabel()
	{
		if (!ValidateTransform("lblStatus", out var go))
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		m_lblStatus = go.GetComponent<SpriteText>();
		return m_lblStatus != null;
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
}
