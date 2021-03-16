using UnityEngine;

internal class ModuleButton
{
	public GameObject m_button;

	public GameObject m_disabledButton;

	public GameObject m_noammoIcon;

	public PackedSprite m_rechargeAnim;

	public SpriteText m_rechargeText;

	public SpriteText m_ammoText;

	public HPModule m_module;

	public float m_sortPos;

	public Vector3 m_point0;

	public Vector3 m_point1;

	public ModuleButton(HPModule module, GameObject guiCamera, bool localOwner, EZValueChangedDelegate onPressed, EZDragDropDelegate onDraged, bool canOrder)
	{
		m_button = GuiUtils.CreateGui("IngameGui/FlowerButton", guiCamera);
		m_module = module;
		m_button.GetComponent<ToolTip>().m_toolTip = Localize.instance.TranslateRecursive("$" + module.name + "_name");
		m_disabledButton = GuiUtils.FindChildOf(m_button, "DisabledButton");
		m_noammoIcon = GuiUtils.FindChildOf(m_button, "NoAmmo");
		m_rechargeAnim = GuiUtils.FindChildOf(m_button, "RechargeAnimation").GetComponent<PackedSprite>();
		m_rechargeText = GuiUtils.FindChildOfComponent<SpriteText>(m_button, "RechargeTextLabel");
		m_disabledButton.SetActiveRecursively(state: false);
		m_noammoIcon.SetActiveRecursively(state: false);
		GameObject gameObject = GuiUtils.FindChildOf(m_button, "StockEar");
		m_ammoText = GuiUtils.FindChildOf(gameObject, "StockValueLabel").GetComponent<SpriteText>();
		SpriteText component = GuiUtils.FindChildOf(m_button, "SizeLabel").GetComponent<SpriteText>();
		component.Text = module.GetAbbr();
		gameObject.SetActiveRecursively(state: false);
		Gun gun = module as Gun;
		if ((bool)gun && gun.GetMaxAmmo() > 0 && localOwner)
		{
			gameObject.SetActiveRecursively(state: true);
		}
		Texture2D gUITexture = module.m_GUITexture;
		UIButton component2 = m_button.GetComponent<UIButton>();
		GuiUtils.SetButtonImageSheet(component2, gUITexture);
		component2.SetValueChangedDelegate(onPressed);
		m_disabledButton.GetComponent<UIButton>().SetValueChangedDelegate(onPressed);
		if (canOrder)
		{
			component2.SetDragDropDelegate(onDraged);
		}
	}

	public bool MouseOver()
	{
		return GuiUtils.HasPointerRecursive(UIManager.instance, m_button);
	}

	public void SetCharge(float i, float time)
	{
		if (i < 0f || i >= 1f)
		{
			m_rechargeAnim.Hide(tf: true);
			m_rechargeText.Hide(tf: true);
			return;
		}
		m_rechargeAnim.Hide(tf: false);
		int frameCount = m_rechargeAnim.animations[0].GetFrameCount();
		m_rechargeAnim.SetFrame(0, (int)((float)frameCount * i));
		if (time >= 0f)
		{
			m_rechargeText.Hide(tf: false);
			m_rechargeText.Text = time.ToString("F1");
		}
		else
		{
			m_rechargeText.Hide(tf: true);
		}
	}
}
