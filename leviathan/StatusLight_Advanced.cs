#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusLight_Advanced : MonoBehaviour
{
	public List<Texture2D> m_textures;

	private bool m_isValid;

	private int m_currentTextureIndex = -1;

	private SimpleSprite m_icon;

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

	public Texture2D CurrentTexture
	{
		get
		{
			return (!(m_icon == null)) ? (m_icon.renderer.material.mainTexture as Texture2D) : null;
		}
		set
		{
			if (m_icon == null)
			{
				return;
			}
			if (value == null)
			{
				base.gameObject.SetActiveRecursively(state: false);
				return;
			}
			base.gameObject.SetActiveRecursively(state: true);
			float width = m_icon.width;
			float height = m_icon.height;
			m_icon.SetTexture(value);
			float num = value.width;
			float num2 = value.height;
			m_icon.Setup(num, num2, new Vector2(0f, num2), new Vector2(num, num2));
			float num3 = width / (float)value.width;
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			float num4 = height / (float)value.height;
			if (num4 < 0f)
			{
				num4 = 0f;
			}
			m_icon.gameObject.transform.localScale = new Vector3(num3, num4, 1f);
			m_icon.UpdateCamera();
			m_icon.UpdateUVs();
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

	public static StatusLight_Advanced Create(GameObject guiCamera, string resourceName)
	{
		GameObject gameObject = GuiUtils.CreateGui("StatusLights/" + resourceName, guiCamera);
		StatusLight_Advanced component = gameObject.GetComponent<StatusLight_Advanced>();
		component.transform.position = Vector3.zero;
		component.Initialize();
		return component;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Initialize()
	{
		ValidateComponents();
	}

	public void SetTint(Color color)
	{
		if (!(m_icon == null))
		{
			m_icon.Color = color;
		}
	}

	public void SetIcon(int index)
	{
		if (!(m_icon == null))
		{
			if (m_textures == null || index < 0 || index > m_textures.Count)
			{
				m_icon.Hide(tf: true);
			}
			m_icon.Hide(tf: false);
			if (index != m_currentTextureIndex)
			{
				m_currentTextureIndex = index;
				CurrentTexture = m_textures[m_currentTextureIndex];
			}
		}
	}

	public void SetIcon_Percentage(float percentage)
	{
		if (!(m_icon == null) && m_textures != null && m_textures.Count != 0)
		{
			double num = (double)((percentage > 100f) ? 100f : ((!(percentage < 0f)) ? percentage : 0f)) / 100.0;
			int num2 = (int)Math.Ceiling(num * (double)(m_textures.Count - 1));
			m_icon.Hide(tf: false);
			if (num2 != m_currentTextureIndex)
			{
				m_currentTextureIndex = num2;
				CurrentTexture = m_textures[m_currentTextureIndex];
			}
		}
	}

	private void ValidateComponents()
	{
		if (!m_isValid)
		{
			m_icon = base.gameObject.GetComponent<SimpleSprite>();
			DebugUtils.Assert(m_icon != null, "StatusLight_Advanced failed to validate it's SimpleSprite component !");
			DebugUtils.Assert(m_textures != null && m_textures.Count > 0, "StatusLight_Advanced failed to validate. Has no textures !");
			m_isValid = true;
		}
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
}
