using UnityEngine;

public class WSEffect : MonoBehaviour
{
	public float m_timeout = 1f;

	public bool m_detachChildren;

	private void Awake()
	{
		Invoke("DestroyNow", m_timeout);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
		}
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
