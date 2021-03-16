using UnityEngine;

internal class RandomSFX : MonoBehaviour
{
	public enum PlayPhase
	{
		All,
		Planning,
		Simulating
	}

	public AudioClip[] m_audioClips = new AudioClip[0];

	public bool m_playOnAwake = true;

	public PlayPhase m_playInPhase;

	public float m_maxPitch = 1f;

	public float m_minPitch = 1f;

	public float m_maxVol = 1f;

	public float m_minVol = 1f;

	public float m_setPrioTime = -1f;

	public int m_setPrio = 128;

	public float m_setPrioTime2 = -1f;

	public int m_setPrio2 = 128;

	public float m_setPrioTime3 = 5f;

	public int m_setPrio3 = 255;

	public float m_maxDelay;

	public float m_minDelay;

	private float m_delay;

	private float m_time;

	public void Awake()
	{
		m_delay = Random.Range(m_minDelay, m_maxDelay);
	}

	public void Update()
	{
		m_time += Time.deltaTime;
		if (m_setPrioTime > 0f && m_time > m_delay + m_setPrioTime)
		{
			base.audio.priority = m_setPrio;
			m_setPrioTime = -1f;
		}
		if (m_setPrioTime2 > 0f && m_time > m_delay + m_setPrioTime2)
		{
			base.audio.priority = m_setPrio2;
			m_setPrioTime2 = -1f;
		}
		if (m_setPrioTime3 > 0f && m_time > m_delay + m_setPrioTime3)
		{
			base.audio.priority = m_setPrio3;
			m_setPrioTime3 = -1f;
		}
		if (!(m_delay >= 0f) || !(m_time >= m_delay))
		{
			return;
		}
		m_delay = -1f;
		if (base.audio != null && m_audioClips.Length > 0)
		{
			int num = Random.Range(0, m_audioClips.Length);
			base.audio.clip = m_audioClips[num];
			base.audio.pitch = Random.Range(m_minPitch, m_maxPitch);
			base.audio.volume = Random.Range(m_minVol, m_maxVol);
			if (m_playOnAwake && (m_playInPhase == PlayPhase.All || (m_playInPhase == PlayPhase.Planning && !NetObj.IsSimulating()) || (m_playInPhase == PlayPhase.Simulating && NetObj.IsSimulating())))
			{
				base.audio.Play();
			}
		}
	}
}
