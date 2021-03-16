using UnityEngine;

public class SplashTrigger : MonoBehaviour
{
	public GameObject m_effectPrefabLow;

	public GameObject m_effectPrefabHigh;

	private float m_lastPos;

	private void Awake()
	{
		m_lastPos = base.transform.position.y;
	}

	private void Update()
	{
		float y = base.transform.position.y;
		if ((y >= 0f && m_lastPos < 0f) || (y < 0f && m_lastPos >= 0f))
		{
			Trigger();
		}
		m_lastPos = y;
	}

	private void Trigger()
	{
		GameObject effectPrefabHigh = m_effectPrefabHigh;
		if (!(effectPrefabHigh == null))
		{
			Vector3 position = base.transform.position;
			position.y = 0f;
			Object.Instantiate(effectPrefabHigh, position, Quaternion.identity);
		}
	}
}
