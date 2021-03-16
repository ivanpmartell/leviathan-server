using UnityEngine;

public class ScreenCamera : MonoBehaviour
{
	public bool m_disableFog = true;

	public int m_minHeight = 720;

	public int m_maxHeightPC = 1080;

	public int m_maxHeightTablet = 720;

	public Texture2D[] m_fontTextures;

	private bool m_fogStatus;

	private void Awake()
	{
		int num = Screen.height;
		if (num < m_minHeight)
		{
			num = m_minHeight;
		}
		if (num > m_maxHeightPC)
		{
			num = m_maxHeightPC;
		}
		base.camera.orthographicSize = num / 2;
		bool scaled = num != Screen.height;
		SetupFontFiltering(scaled);
	}

	private void SetupFontFiltering(bool scaled)
	{
		Texture2D[] fontTextures = m_fontTextures;
		foreach (Texture2D texture2D in fontTextures)
		{
			if (scaled)
			{
				texture2D.filterMode = FilterMode.Bilinear;
			}
			else
			{
				texture2D.filterMode = FilterMode.Point;
			}
		}
	}

	private void OnPreRender()
	{
		if (m_disableFog)
		{
			m_fogStatus = RenderSettings.fog;
			RenderSettings.fog = false;
		}
	}

	private void OnPostRender()
	{
		if (m_disableFog)
		{
			RenderSettings.fog = m_fogStatus;
		}
	}
}
