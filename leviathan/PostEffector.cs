using System.Collections.Generic;
using UnityEngine;

public class PostEffector : MonoBehaviour
{
	private static bool m_fxaaDefault = true;

	private static bool m_bloomDefault = true;

	public MonoBehaviour m_fxaaEffect;

	public MonoBehaviour m_bloomEffect;

	private static bool m_first = true;

	private static bool m_fxaaEnabled = false;

	private static bool m_bloomEnabled = false;

	private static List<PostEffector> m_postEffectors = new List<PostEffector>();

	private void Start()
	{
		m_postEffectors.Add(this);
		if (m_first)
		{
			m_first = false;
			m_fxaaEnabled = PlayerPrefs.GetInt("fxaa", m_fxaaDefault ? 1 : 0) == 1;
			m_bloomEnabled = PlayerPrefs.GetInt("bloom", m_bloomDefault ? 1 : 0) == 1;
			PLog.Log("first " + m_fxaaEnabled + "  " + m_bloomEnabled);
		}
		UpdateEnabled();
	}

	private void OnDestroy()
	{
		m_postEffectors.Remove(this);
	}

	public static void SetFXAAEnabled(bool enabled)
	{
		m_fxaaEnabled = enabled;
		PlayerPrefs.SetInt("fxaa", enabled ? 1 : 0);
		UpdateEnabled();
	}

	public static void SetBloomEnabled(bool enabled)
	{
		m_bloomEnabled = enabled;
		PlayerPrefs.SetInt("bloom", enabled ? 1 : 0);
		UpdateEnabled();
	}

	public static bool IsFXAAEnabled()
	{
		return m_fxaaEnabled;
	}

	public static bool IsBloomEnabled()
	{
		return m_bloomEnabled;
	}

	private static void UpdateEnabled()
	{
		foreach (PostEffector postEffector in m_postEffectors)
		{
			if ((bool)postEffector.m_bloomEffect)
			{
				postEffector.m_bloomEffect.enabled = m_bloomEnabled;
			}
			if ((bool)postEffector.m_fxaaEffect)
			{
				postEffector.m_fxaaEffect.enabled = m_fxaaEnabled;
			}
		}
	}
}
