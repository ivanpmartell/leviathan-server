using UnityEngine;

public class ShowOnPlatform : MonoBehaviour
{
	public bool m_visiblePc = true;

	public bool m_visibleIOS = true;

	public bool m_visibleAndroid = true;

	private void Start()
	{
		SetState();
	}

	private void FixedUpdate()
	{
	}

	private void Update()
	{
		SetState();
	}

	private void SetState()
	{
		bool flag = true;
		if (!m_visiblePc)
		{
			base.gameObject.SetActiveRecursively(state: false);
		}
	}
}
