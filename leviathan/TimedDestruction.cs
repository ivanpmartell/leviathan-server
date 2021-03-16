using UnityEngine;

public class TimedDestruction : MonoBehaviour
{
	public float m_timeout = 1f;

	public bool m_detachChildren;

	private void Awake()
	{
		Invoke("DestroyNow", m_timeout);
	}

	private void DestroyNow()
	{
		if (m_detachChildren)
		{
			base.transform.DetachChildren();
		}
		Object.Destroy(base.gameObject);
	}
}
