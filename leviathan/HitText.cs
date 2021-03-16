using System;
using System.Collections.Generic;
using UnityEngine;

internal class HitText
{
	private class TextData
	{
		public int m_ownerID = -1;

		public SpriteText m_textElement;

		public GameObject m_guiElement;

		public Vector3 m_pos;

		public float m_time;

		public float m_offset;

		public float m_scale = 1f;

		public float m_textScale = 1f;

		public Color m_textColor = Color.white;

		public int m_damage = -1;

		public string m_baseText = string.Empty;

		public int m_hits;
	}

	private const float m_flyTime = 7f;

	private const float m_flySpeed = 0.5f;

	private const int m_maxTexts = 20;

	private List<TextData> m_texts = new List<TextData>();

	private static HitText m_instance;

	private Camera m_guiCamera;

	private GameObject m_textPrefabLarge;

	private GameObject m_textPrefabMedium;

	private GameObject m_textPrefabSmall;

	private bool m_visible = true;

	public static HitText instance => m_instance;

	public HitText(GameObject guiCamera)
	{
		m_instance = this;
		m_guiCamera = guiCamera.GetComponent<Camera>();
		m_textPrefabLarge = Resources.Load("gui/IngameGui/DmgTextLarge") as GameObject;
		m_textPrefabMedium = Resources.Load("gui/IngameGui/DmgTextMedium") as GameObject;
		m_textPrefabSmall = Resources.Load("gui/IngameGui/DmgTextSmall") as GameObject;
	}

	public void Close()
	{
		m_instance = null;
		Clear();
	}

	public void Clear()
	{
		foreach (TextData text in m_texts)
		{
			UnityEngine.Object.Destroy(text.m_guiElement);
		}
		m_texts.Clear();
	}

	public void SetVisible(bool visible)
	{
		if (m_visible != visible)
		{
			m_visible = visible;
			if (!m_visible)
			{
				Clear();
			}
		}
	}

	public void AddDmgText(int ownerID, Vector3 pos, string text, HitTextDef def)
	{
		if (m_visible)
		{
			text = Localize.instance.Translate(def.m_prefix + text + def.m_postfix);
			TextData textData = FindOldTextItem(ownerID, text);
			if (textData == null)
			{
				textData = CreateNewItem(ownerID, def, pos);
				textData.m_textElement.Text = text;
				textData.m_baseText = text;
			}
			textData.m_hits++;
			if (textData.m_hits > 1)
			{
				textData.m_textElement.Text = textData.m_baseText.ToString() + " x " + textData.m_hits;
			}
		}
	}

	public void AddDmgText(int ownerID, Vector3 pos, int dmg, HitTextDef def)
	{
		if (m_visible)
		{
			TextData textData = FindOldDamageItem(ownerID, def.m_color);
			if (textData == null)
			{
				textData = CreateNewItem(ownerID, def, pos);
			}
			textData.m_hits++;
			if (textData.m_damage == -1)
			{
				textData.m_damage = dmg;
				textData.m_textElement.Text = textData.m_damage.ToString();
			}
			else
			{
				textData.m_damage += dmg;
				textData.m_textElement.Text = textData.m_damage.ToString();
			}
		}
	}

	private TextData CreateNewItem(int ownerID, HitTextDef def, Vector3 pos)
	{
		TextData textData = new TextData();
		textData.m_ownerID = ownerID;
		textData.m_textColor = def.m_color;
		textData.m_pos = pos;
		GameObject gameObject = null;
		switch (def.m_fontSize)
		{
		case HitTextDef.FontSize.Large:
			gameObject = UnityEngine.Object.Instantiate(m_textPrefabLarge, pos, Quaternion.identity) as GameObject;
			break;
		case HitTextDef.FontSize.Medium:
			gameObject = UnityEngine.Object.Instantiate(m_textPrefabMedium, pos, Quaternion.identity) as GameObject;
			break;
		case HitTextDef.FontSize.Small:
			gameObject = UnityEngine.Object.Instantiate(m_textPrefabSmall, pos, Quaternion.identity) as GameObject;
			break;
		}
		textData.m_guiElement = gameObject;
		gameObject.transform.parent = m_guiCamera.transform;
		textData.m_textElement = gameObject.transform.FindChild("Text").GetComponent<SpriteText>();
		textData.m_textElement.SetColor(textData.m_textColor);
		textData.m_textElement.text = string.Empty;
		m_texts.Add(textData);
		return textData;
	}

	public void Update(float dt)
	{
		PurgeOldTexts();
		foreach (TextData text in m_texts)
		{
			text.m_time += dt;
		}
		for (int i = 0; i < m_texts.Count; i++)
		{
			if (m_texts[i].m_time > 7f)
			{
				UnityEngine.Object.Destroy(m_texts[i].m_guiElement);
				m_texts.RemoveAt(i);
				break;
			}
		}
	}

	private void PurgeOldTexts()
	{
		while (m_texts.Count > 20)
		{
			UnityEngine.Object.Destroy(m_texts[0].m_guiElement);
			m_texts.RemoveAt(0);
		}
	}

	public void LateUpdate(Camera camera)
	{
		UpdateScales(camera);
		Separate(camera, Time.deltaTime);
		foreach (TextData text in m_texts)
		{
			float num = text.m_time / 7f;
			Vector3 guiPos = GetGuiPos(camera, text);
			float num2 = 1f - num;
			text.m_offset += 0.5f * text.m_scale * num2;
			Color textColor = text.m_textColor;
			textColor.a = 1f - num * num * num;
			text.m_textElement.SetColor(textColor);
			text.m_textElement.transform.position = guiPos;
			text.m_textElement.transform.localScale = new Vector3(text.m_scale * text.m_textScale, text.m_scale * text.m_textScale, text.m_scale * text.m_textScale);
		}
	}

	private Vector3 GetGuiPos(Camera camera, TextData data)
	{
		Vector3 result = GuiUtils.WorldToGuiPos(camera, m_guiCamera, data.m_pos);
		result.z = data.m_textScale;
		result.y += data.m_offset * data.m_scale;
		return result;
	}

	private TextData FindOldDamageItem(int owner, Color textColor)
	{
		float num = 3.5f;
		foreach (TextData text in m_texts)
		{
			if (text.m_ownerID == owner && text.m_damage >= 0 && text.m_textColor == textColor && text.m_time < num)
			{
				return text;
			}
		}
		return null;
	}

	private TextData FindOldTextItem(int owner, string text)
	{
		float num = 3.5f;
		foreach (TextData text2 in m_texts)
		{
			if (text2.m_ownerID == owner && text2.m_damage == -1 && text2.m_baseText == text && text2.m_time < num)
			{
				return text2;
			}
		}
		return null;
	}

	private void Separate(Camera camera, float dt)
	{
		float num = 25f;
		float num2 = 40f;
		for (int i = 0; i < m_texts.Count; i++)
		{
			TextData textData = m_texts[i];
			Vector3 guiPos = GetGuiPos(camera, textData);
			for (int j = i + 1; j < m_texts.Count; j++)
			{
				TextData textData2 = m_texts[j];
				Vector3 guiPos2 = GetGuiPos(camera, textData2);
				float num3 = Vector3.Distance(guiPos, guiPos2);
				if (num3 < num)
				{
					if (guiPos.y > guiPos2.y)
					{
						textData.m_offset += dt * num2;
						guiPos = GetGuiPos(camera, textData);
					}
					else
					{
						textData2.m_offset += dt * num2;
					}
				}
			}
		}
	}

	private void UpdateScales(Camera camera)
	{
		float num = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f);
		foreach (TextData text in m_texts)
		{
			text.m_scale = Vector3.Distance(text.m_pos, camera.transform.position) * (1f / num);
			text.m_scale = Mathf.Clamp(text.m_scale, 0f, 1f);
		}
	}
}
