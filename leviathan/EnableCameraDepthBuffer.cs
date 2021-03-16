using UnityEngine;

public class EnableCameraDepthBuffer : MonoBehaviour
{
	private void Start()
	{
		base.camera.depthTextureMode = DepthTextureMode.Depth;
	}
}
