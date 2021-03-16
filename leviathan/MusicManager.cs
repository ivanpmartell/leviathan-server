#define DEBUG
using UnityEngine;

public class MusicManager
{
	private float m_fadeTimer = -1f;

	private float m_crossFadeTime = 1f;

	private AudioSource[] m_sources;

	private int m_primarySource;

	private float m_volume = 1f;

	private float m_volumeModifier = 0.25f;

	private string m_currentMusic = string.Empty;

	private static MusicManager m_instance;

	public static MusicManager instance => m_instance;

	public MusicManager(AudioSource[] sources)
	{
		DebugUtils.Assert(sources.Length == 2);
		m_sources = sources;
		AudioSource[] sources2 = m_sources;
		foreach (AudioSource audioSource in sources2)
		{
			audioSource.ignoreListenerVolume = true;
			audioSource.volume = 0f;
		}
		m_instance = this;
	}

	public void Close()
	{
		m_instance = null;
	}

	public void Update(float dt)
	{
		if (m_fadeTimer >= 0f)
		{
			m_fadeTimer += dt;
			float num = m_fadeTimer / m_crossFadeTime;
			if (num >= 1f)
			{
				m_fadeTimer = -1f;
				int primarySource = m_primarySource;
				m_primarySource = ((m_primarySource == 0) ? 1 : 0);
				m_sources[primarySource].volume = 0f;
				m_sources[m_primarySource].volume = m_volume * m_volumeModifier;
				m_sources[primarySource].Stop();
			}
			else
			{
				int num2 = ((m_primarySource == 0) ? 1 : 0);
				m_sources[num2].volume = num * m_volume * m_volumeModifier;
				m_sources[m_primarySource].volume = (1f - num) * m_volume * m_volumeModifier;
			}
		}
	}

	public void SetMusic(string name)
	{
		if (m_currentMusic == name)
		{
			return;
		}
		m_currentMusic = name;
		if (name == string.Empty)
		{
			SetMusic((AudioClip)null);
			return;
		}
		AudioClip audioClip = Resources.Load("music/" + name) as AudioClip;
		if (audioClip == null)
		{
			PLog.LogWarning("Missing music " + name);
		}
		else
		{
			SetMusic(audioClip);
		}
	}

	private void SetMusic(AudioClip clip)
	{
		m_fadeTimer = 0f;
		int num = ((m_primarySource == 0) ? 1 : 0);
		m_sources[num].clip = clip;
		if (clip != null)
		{
			m_sources[num].Play();
		}
		else
		{
			m_sources[num].Stop();
		}
	}

	public float GetVolume()
	{
		return m_volume;
	}

	public void SetVolume(float volume)
	{
		volume = Mathf.Clamp(volume, 0f, 1f);
		m_volume = volume;
		if (m_fadeTimer < 0f)
		{
			m_sources[m_primarySource].volume = m_volume * m_volumeModifier;
		}
	}
}
