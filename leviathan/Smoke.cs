using UnityEngine;

public class Smoke : MonoBehaviour
{
	private LineDrawer m_lineDrawer;

	private int m_lineType = -1;

	private float m_radius = 1f;

	private void OnTriggerStay(Collider other)
	{
		Section component = other.GetComponent<Section>();
		if (!component && other.transform.parent != null)
		{
			component = other.transform.parent.GetComponent<Section>();
		}
		if (component != null)
		{
			component.OnSmokeEnter();
		}
	}

	private void Awake()
	{
		m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
		m_lineType = m_lineDrawer.GetTypeID("smokeArea");
		m_radius = (base.collider as SphereCollider).radius;
	}

	private void Update()
	{
		m_lineDrawer.DrawXZCircle(base.transform.position, m_radius, 16, m_lineType, 1f);
	}
}
