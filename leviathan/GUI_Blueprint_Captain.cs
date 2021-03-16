#define DEBUG
using UnityEngine;

[AddComponentMenu("Scripts/Gui/GUI_Blueprint_Captain")]
public sealed class GUI_Blueprint_Captain : MonoBehaviour
{
	private const bool DEBUG = true;

	private UIListItem m_listItemComponent;

	private SimpleSprite m_simpleSpriteComponent;

	private bool m_hasInitialized;

	public float m_width = 100f;

	public float m_height = 100f;

	public string m_text = string.Empty;

	public Texture2D m_iconTexture;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!m_hasInitialized)
		{
			m_listItemComponent = base.gameObject.GetComponent<UIListItem>();
			DebugUtils.Assert(m_listItemComponent != null, "GUI_Captain script must be attached to a GameObject that has an IUIListObject");
			m_simpleSpriteComponent = base.gameObject.GetComponentInChildren<SimpleSprite>();
			DebugUtils.Assert(m_simpleSpriteComponent != null, "GUI_Captain script must be attached to a GameObject that has an SimpleSprite");
			Debug.Log($"{m_listItemComponent.Text}::Initialize() called !");
			SetSizes(m_width, m_height);
			SetTexts(m_text);
			SetIcon(m_iconTexture, 32f, 32f);
			m_hasInitialized = true;
		}
	}

	private void SetSizes(float width, float height)
	{
		if (m_listItemComponent != null)
		{
			m_listItemComponent.SetSize(width, height);
		}
	}

	private void SetTexts(string text)
	{
		if (!(m_listItemComponent != null))
		{
			return;
		}
		m_listItemComponent.Text = text;
		if (m_listItemComponent.stateLabels != null && m_listItemComponent.stateLabels.Length > 0)
		{
			for (int i = 0; i < m_listItemComponent.stateLabels.Length; i++)
			{
				m_listItemComponent.stateLabels[i] = text;
			}
		}
	}

	private void SetIcon(Texture2D iconTexture, float iconWidth, float iconHeight)
	{
		if (m_simpleSpriteComponent != null)
		{
			m_simpleSpriteComponent.SetTexture(iconTexture);
			m_simpleSpriteComponent.Setup(iconWidth, iconHeight, new Vector2(0f, iconHeight), new Vector2(iconWidth, iconHeight));
			m_simpleSpriteComponent.renderer.castShadows = false;
			m_simpleSpriteComponent.renderer.receiveShadows = false;
		}
	}

	public void OnTap()
	{
		Debug.Log($"{m_listItemComponent.Text}::OnTap() called !");
	}

	public void OnPress()
	{
		Debug.Log($"{m_listItemComponent.Text}::OnPress() called !");
	}

	public void OnRelease()
	{
		Debug.Log($"{m_listItemComponent.Text}::OnRelease() called !");
	}

	public void OnMove()
	{
		Debug.Log($"{m_listItemComponent.Text}::OnMove() called !");
	}
}
