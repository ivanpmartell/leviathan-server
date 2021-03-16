using UnityEngine;

public class RandomRotation : MonoBehaviour
{
	private void Start()
	{
		base.transform.Rotate(new Vector3(0f, Random.value * 360f, 0f));
	}
}
