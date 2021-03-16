using UnityEngine;

internal class AnchorButton
{
	private GameObject m_button;

	private UIButton m_disableButton;

	private UIButton m_enableButton;

	private PackedSprite m_rechargeAnim;

	private Ship m_ship;

	public AnchorButton(Ship ship, GameObject guiCamera, bool localOwner, bool canOrder)
	{
		m_ship = ship;
		m_button = GuiUtils.CreateGui("IngameGui/FlowerButtonAnchor", guiCamera);
		m_disableButton = GuiUtils.FindChildOf(m_button, "Disable").GetComponent<UIButton>();
		m_enableButton = GuiUtils.FindChildOf(m_button, "Enable").GetComponent<UIButton>();
		m_rechargeAnim = GuiUtils.FindChildOf(m_button, "RechargeAnimation").GetComponent<PackedSprite>();
		if (canOrder)
		{
			m_enableButton.GetComponent<UIButton>().SetValueChangedDelegate(OnToggleModePressed);
			m_disableButton.GetComponent<UIButton>().SetValueChangedDelegate(OnToggleModePressed);
		}
		UpdateStatus();
	}

	public void Close()
	{
		Object.Destroy(m_button);
	}

	public bool MouseOver()
	{
		return GuiUtils.HasPointerRecursive(UIManager.instance, m_button);
	}

	private void UpdateStatus()
	{
		if (m_ship.GetRequestedMaintenanceMode())
		{
			m_enableButton.gameObject.SetActiveRecursively(state: false);
			m_disableButton.gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			m_enableButton.gameObject.SetActiveRecursively(state: true);
			m_disableButton.gameObject.SetActiveRecursively(state: false);
		}
		SetCharge(m_ship.GetMaintenanceTimer());
	}

	public void UpdatePosition(float guiScale, Camera guiCamera, Camera gameCamera, ref float lowestScreenPos)
	{
		Vector3 pos = m_ship.transform.position + new Vector3(0f, m_ship.m_deckHeight, 0f);
		Vector3 position = GuiUtils.WorldToGuiPos(gameCamera, guiCamera, pos);
		m_button.transform.position = position;
		UpdateStatus();
	}

	private void OnToggleModePressed(IUIObject obj)
	{
		m_ship.SetRequestedMaintenanceMode(!m_ship.GetRequestedMaintenanceMode());
	}

	public void SetCharge(float i)
	{
		if (i < 0f || i >= 1f)
		{
			m_rechargeAnim.Hide(tf: true);
			return;
		}
		m_rechargeAnim.Hide(tf: false);
		int frameCount = m_rechargeAnim.animations[0].GetFrameCount();
		m_rechargeAnim.SetFrame(0, (int)((float)frameCount * i));
	}
}
