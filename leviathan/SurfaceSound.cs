using UnityEngine;

public class SurfaceSound : MonoBehaviour
{
	private void Update()
	{
		if (Camera.main != null)
		{
			Vector3 position = Camera.main.transform.position;
			position.y = 0f;
			base.transform.position = position;
		}
	}
}
