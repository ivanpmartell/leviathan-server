using UnityEngine;

public class CamShaker : MonoBehaviour
{
	public float m_intensity = 1f;

	public bool m_triggerOnAwake;

	public float m_delay;

	public void Update()
	{
		if (m_triggerOnAwake)
		{
			m_delay -= Time.deltaTime;
			if (m_delay <= 0f)
			{
				Trigger();
				m_triggerOnAwake = false;
			}
		}
	}

	public void Trigger()
	{
		Camera camera = Camera.main;
		if (camera != null)
		{
			GameCamera component = camera.GetComponent<GameCamera>();
			if (component != null)
			{
				component.AddShake(base.transform.position, m_intensity);
			}
		}
	}
}
