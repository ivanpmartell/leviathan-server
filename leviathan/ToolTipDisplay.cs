using UnityEngine;

public class ToolTipDisplay : MonoBehaviour
{
	private string m_message = string.Empty;

	private float m_showTimer = -1f;

	private float m_hideTimer = -1f;

	private Vector3 m_position = default(Vector3);

	private GameObject m_gameObject;

	private SpriteText _tooltipText;

	private bool m_helpModeOn;

	private Vector3 m_from = default(Vector3);

	private Vector3 m_to = default(Vector3);

	private int tooltipHalfWidth = 128;

	private int tooltipHalfHeight = 32;

	private int screenHalfWidth = 512;

	private int screenHalfHeight = 384;

	private void Start()
	{
		_tooltipText = base.transform.GetChild(0).GetComponent<SpriteText>();
		SimpleSprite component = GetComponent<SimpleSprite>();
		tooltipHalfWidth = (int)component.width / 2;
		tooltipHalfHeight = (int)component.height / 2;
		screenHalfWidth = (int)_tooltipText.RenderCamera.GetScreenWidth() / 2;
		screenHalfHeight = (int)_tooltipText.RenderCamera.GetScreenHeight() / 2;
	}

	private void LimitToScreen()
	{
		if (m_position.x - (float)tooltipHalfWidth < (float)(-screenHalfWidth))
		{
			m_position.x = -screenHalfWidth + tooltipHalfWidth;
		}
		if (m_position.x + (float)tooltipHalfWidth > (float)screenHalfWidth)
		{
			m_position.x = screenHalfWidth - tooltipHalfWidth;
		}
		if (m_position.y - (float)tooltipHalfHeight < (float)(-screenHalfHeight))
		{
			m_position.y = m_from.y + (float)(tooltipHalfHeight * 2);
		}
		if (m_position.y + (float)tooltipHalfHeight > (float)screenHalfHeight)
		{
			m_position.y = screenHalfHeight - tooltipHalfHeight;
		}
	}

	public void SetupToolTip(GameObject go, string text, Vector3 from, Vector3 to)
	{
		if (!(go == m_gameObject))
		{
			m_from = from;
			m_to = to;
			m_message = text;
			m_position = to;
			m_position.z = -80f;
			m_position.y -= tooltipHalfHeight;
			LimitToScreen();
			m_showTimer = 1f;
			m_hideTimer = -1f;
			m_gameObject = go;
		}
	}

	public void NoDelay()
	{
		m_showTimer = 0f;
	}

	public void StopToolTip(GameObject go)
	{
		if (m_gameObject == go)
		{
			Hide();
		}
		m_gameObject = null;
	}

	public void StopToolTip()
	{
		Hide();
		m_gameObject = null;
	}

	private void Show()
	{
		m_position.x = Mathf.Floor(m_position.x);
		m_position.y = Mathf.Floor(m_position.y);
		m_position.z = Mathf.Floor(m_position.z);
		base.transform.transform.position = m_position;
		_tooltipText.Text = m_message;
		m_hideTimer = 4f;
	}

	private void Hide()
	{
		base.transform.position = new Vector3(0f, 5000f, 0f);
		_tooltipText.Text = "***";
		m_showTimer = -1f;
		m_hideTimer = -1f;
	}

	private void FixedUpdate()
	{
		if (m_showTimer >= 0f)
		{
			m_showTimer -= Time.fixedDeltaTime;
			if (m_showTimer <= 0f)
			{
				Show();
			}
		}
		if (m_hideTimer >= 0f)
		{
			m_hideTimer -= Time.fixedDeltaTime;
			if (m_hideTimer <= 0f)
			{
				Hide();
			}
		}
	}

	public void SetHelpMode(bool helpMode)
	{
		m_helpModeOn = helpMode;
	}

	public bool GetHelpMode()
	{
		return m_helpModeOn;
	}

	public static ToolTipDisplay GetToolTip(GameObject src)
	{
		GameObject gameObject = GuiUtils.FindParent(src.transform, "GuiCamera");
		if (gameObject == null)
		{
			return null;
		}
		GameObject gameObject2 = GuiUtils.FindChildOf(gameObject, "Tooltip(Clone)");
		if (gameObject2 == null)
		{
			gameObject2 = GuiUtils.CreateGui("dialogs/Tooltip", gameObject);
			gameObject2.transform.position = new Vector3(0f, 5000f, -80f);
		}
		return gameObject2.GetComponent<ToolTipDisplay>();
	}

	public bool IsOnTouch()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return true;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return true;
		}
		return false;
	}
}
