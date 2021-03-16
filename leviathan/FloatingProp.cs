using UnityEngine;

public class FloatingProp : MonoBehaviour
{
	private WaterSurface m_waterSurface;

	private float m_offset;

	public void Awake()
	{
		GameObject gameObject = GameObject.Find("WaterSurface");
		if (gameObject != null)
		{
			m_waterSurface = gameObject.GetComponent<WaterSurface>();
		}
		m_offset = base.transform.position.y;
	}

	private void FixedUpdate()
	{
		if (m_waterSurface != null)
		{
			Vector3 position = base.transform.position;
			position.y = m_waterSurface.GetWorldWaveHeight(position) + m_offset;
			base.transform.position = position;
		}
	}
}
