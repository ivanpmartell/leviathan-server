#define DEBUG
using UnityEngine;

public class StatusLight_Basic : MonoBehaviour
{
	public bool m_disableWhenOff;

	private bool m_isValid;

	public Texture2D m_OnTexture;

	public Texture2D m_OffTexture;

	private bool m_On = true;

	private SimpleSprite m_sprite;

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

	[SerializeField]
	public bool DisableWhenOff
	{
		get
		{
			return m_disableWhenOff;
		}
		set
		{
			m_disableWhenOff = value;
			if (m_disableWhenOff && !m_On)
			{
				m_sprite.Hide(tf: true);
			}
			else if (!m_disableWhenOff && !m_On)
			{
				m_sprite.Hide(tf: false);
			}
		}
	}

	public Texture2D CurrentTexture
	{
		get
		{
			return (!(m_sprite == null)) ? (m_sprite.renderer.material.mainTexture as Texture2D) : null;
		}
		set
		{
			if (!(m_sprite == null) && (!(CurrentTexture != null) || !(CurrentTexture == value)))
			{
				float width = m_sprite.width;
				float height = m_sprite.height;
				if (value == null)
				{
					m_sprite.Hide(tf: true);
					return;
				}
				m_sprite.Hide(tf: false);
				float num = value.width;
				float num2 = value.height;
				m_sprite.SetTexture(value);
				m_sprite.Setup(num, num2, new Vector2(0f, num2), new Vector2(num, num2));
				float x = width / (float)value.width;
				float y = height / (float)value.height;
				m_sprite.gameObject.transform.localScale = new Vector3(x, y, 1f);
				m_sprite.UpdateUVs();
			}
		}
	}

	public float ScaledHeight
	{
		get
		{
			if (CurrentTexture == null)
			{
				return 0f;
			}
			return (float)CurrentTexture.height * base.gameObject.transform.localScale.y;
		}
	}

	public float ScaledWidth
	{
		get
		{
			if (CurrentTexture == null)
			{
				return 0f;
			}
			return (float)CurrentTexture.width * base.gameObject.transform.localScale.x;
		}
	}

	public static StatusLight_Basic Create(GameObject guiCamera, string resourceName)
	{
		string text = "StatusLights/" + resourceName;
		GameObject gameObject = GuiUtils.CreateGui(text, guiCamera);
		DebugUtils.Assert(gameObject != null, "\"" + text + "\" failed to find prefab \"" + text + "\" !");
		StatusLight_Basic component = gameObject.GetComponent<StatusLight_Basic>();
		DebugUtils.Assert(component != null, "\"" + text + "\" failed to find a StatusLight_Basic-script in prefab \"" + text + "\" !");
		Transform obj = component.gameObject.transform;
		Vector3 zero = Vector3.zero;
		component.gameObject.transform.localPosition = zero;
		obj.position = zero;
		component.Initialize();
		return component;
	}

	internal void Initialize()
	{
		ValidateComponents();
		CurrentTexture = ((m_OffTexture == null) ? m_OnTexture : ((!m_disableWhenOff) ? m_OffTexture : m_OnTexture));
	}

	public void SetTint(Color color)
	{
		if (!(m_sprite == null))
		{
			m_sprite.Color = color;
		}
	}

	public void SetOnOff(bool onOff)
	{
		m_On = onOff;
		if (m_disableWhenOff && !m_On)
		{
			m_sprite.Hide(tf: true);
			return;
		}
		m_sprite.Hide(tf: false);
		CurrentTexture = ((!m_On) ? m_OffTexture : m_OnTexture);
	}

	private void ValidateComponents()
	{
		if (!m_isValid)
		{
			m_sprite = GetComponent<SimpleSprite>();
			DebugUtils.Assert(m_sprite != null, "StatusLight failed to validate it's SimpleSprite component !");
			DebugUtils.Assert(m_OnTexture != null, "StatusLight failed to validate. Has no ON-texture !");
			if (m_OffTexture == null)
			{
				m_disableWhenOff = true;
			}
			m_isValid = true;
		}
	}
}
