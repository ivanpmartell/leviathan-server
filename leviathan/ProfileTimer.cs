internal class ProfileTimer
{
	private static long m_averageInterval = 10000L;

	private long m_time;

	private bool m_started;

	private long m_totalTime;

	private int m_samples;

	private float m_average;

	private long m_updateTime;

	public ProfileTimer()
	{
		m_updateTime = Utils.GetTimeMS();
	}

	public void Start()
	{
		if (!m_started)
		{
			m_time = Utils.GetTimeMS();
			m_started = true;
		}
	}

	public void Stop()
	{
		if (m_started)
		{
			long timeMS = Utils.GetTimeMS();
			long num = timeMS - m_time;
			m_totalTime += num;
			m_samples++;
			m_started = false;
			if (timeMS - m_updateTime > m_averageInterval)
			{
				m_average = (float)m_totalTime / (float)m_samples;
				m_totalTime = 0L;
				m_samples = 0;
				m_updateTime = timeMS;
			}
		}
	}

	public float GetAverage()
	{
		return m_average;
	}
}
