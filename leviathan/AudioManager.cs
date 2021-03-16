using UnityEngine;

internal class AudioManager
{
	private const float m_soundEnableFadein = 1f;

	private static AudioManager m_instance;

	private float m_enableSoundDelay = -1f;

	private float m_currentVolume = 1f;

	private float m_volume = 1f;

	public static AudioManager instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new AudioManager();
			}
			return m_instance;
		}
	}

	public static void ResetInstance()
	{
		m_instance = null;
	}

	public void Update(float dt)
	{
		if (m_enableSoundDelay > 0f)
		{
			m_enableSoundDelay -= Mathf.Min(0.05f, dt);
			float num = Mathf.Clamp(1f - m_enableSoundDelay / 1f, 0f, 1f);
			if (m_currentVolume != num)
			{
				m_currentVolume = num;
				AudioListener.volume = num * m_volume;
			}
		}
	}

	public void SetSFXEnabled(bool enabled)
	{
		if (enabled)
		{
			m_enableSoundDelay = 1f;
			return;
		}
		m_currentVolume = 0f;
		AudioListener.volume = 0f;
		m_enableSoundDelay = -1f;
	}

	public void SetVolume(float volume)
	{
		m_volume = volume;
		AudioListener.volume = m_currentVolume * m_volume;
	}

	public float GetVolume()
	{
		return m_volume;
	}
}
