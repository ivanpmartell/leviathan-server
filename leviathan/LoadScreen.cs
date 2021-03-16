#define DEBUG
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LoadScreen
{
	private const float m_fadeTime = 0.5f;

	private GameObject m_gui;

	private GameObject m_guiCamera;

	private Material m_material;

	private SimpleSprite m_sprite;

	private UIButton m_bkg;

	private Texture m_defaultTexture;

	private float m_fadeTimer = -1f;

	public LoadScreen(GameObject guiCamera)
	{
		m_guiCamera = guiCamera;
		m_gui = GuiUtils.CreateGui("LoadScreen", m_guiCamera);
		GameObject gameObject = GuiUtils.FindChildOf(m_gui.transform, "BackgroundImg");
		DebugUtils.Assert(gameObject);
		m_bkg = GuiUtils.FindChildOf(m_gui.transform, "BackgroundWnd").GetComponent<UIButton>();
		m_sprite = gameObject.GetComponent<SimpleSprite>();
		m_material = gameObject.renderer.material;
		m_defaultTexture = m_material.mainTexture;
		m_gui.SetActiveRecursively(state: false);
	}

	public void Close()
	{
		Clear();
	}

	public void SetVisible(bool visible)
	{
		if (!(m_gui == null))
		{
			if (visible)
			{
				m_gui.SetActiveRecursively(state: true);
				m_sprite.SetColor(new Color(1f, 1f, 1f, 1f));
				m_bkg.SetColor(new Color(1f, 1f, 1f, 1f));
				m_fadeTimer = -1f;
			}
			else
			{
				m_fadeTimer = 0f;
			}
		}
	}

	public string GetRandomImage()
	{
		XmlDocument xmlDocument = Utils.LoadXml("loadscreens/manifest");
		DebugUtils.Assert(xmlDocument != null);
		List<string> list = new List<string>();
		for (XmlNode xmlNode = xmlDocument.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "image")
			{
				list.Add(xmlNode.FirstChild.Value);
			}
		}
		PLog.Log("Found " + list.Count + " loadscreens");
		if (list.Count > 0)
		{
			System.Random random = new System.Random();
			return list[random.Next(0, list.Count)];
		}
		return null;
	}

	public void SetImage(string name)
	{
		if (m_gui == null)
		{
			return;
		}
		if (name == string.Empty)
		{
			name = GetRandomImage();
			if (name == null)
			{
				name = "default";
			}
		}
		Texture texture = Resources.Load("loadscreens/" + name) as Texture;
		if (texture != null)
		{
			m_material.mainTexture = texture;
		}
		else
		{
			PLog.LogWarning("Missing loadscreen texture " + name);
		}
	}

	public void Update()
	{
		if (!(m_gui == null) && m_fadeTimer >= 0f)
		{
			m_fadeTimer += Time.deltaTime;
			if (m_fadeTimer > 0.5f)
			{
				m_gui.SetActiveRecursively(state: false);
				m_fadeTimer = -1f;
				Clear();
			}
			else
			{
				float a = 1f - m_fadeTimer / 0.5f;
				m_sprite.SetColor(new Color(1f, 1f, 1f, a));
				m_bkg.SetColor(new Color(1f, 1f, 1f, a));
			}
		}
	}

	private void Clear()
	{
		UnityEngine.Object.Destroy(m_gui);
		m_gui = null;
		m_material = null;
		m_sprite = null;
		m_bkg = null;
		m_defaultTexture = null;
	}
}
