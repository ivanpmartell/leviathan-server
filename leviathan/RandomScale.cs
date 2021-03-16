using UnityEngine;

public class RandomScale : MonoBehaviour
{
	public float m_min = 1f;

	public float m_max = 1.5f;

	public bool m_continuous = true;

	public float m_frequency = 0.1f;

	public bool m_resetRotation;

	private float m_timeOffset;

	private void Start()
	{
		float num = Random.Range(m_min, m_max);
		base.transform.localScale = new Vector3(num, num, num);
		m_timeOffset = Random.value * 10f;
	}

	private void Update()
	{
		float num = m_timeOffset + Time.time;
		float num2 = Mathf.Sin(num * m_frequency) * Mathf.Cos(num * m_frequency * 2.2532f);
		float num3 = m_min + (num2 * 0.5f + 0.5f) * (m_max - m_min);
		base.transform.localScale = new Vector3(num3, num3, num3);
		if (m_resetRotation)
		{
			base.transform.rotation = Quaternion.identity;
		}
	}
}
